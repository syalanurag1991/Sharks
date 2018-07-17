 //  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Vulcan
{
    [InitializeOnLoad]
    public class HolodeckEditorWindow : EditorWindow
    {
#if UNITY_EDITOR
        const string USER_LAYER_PREFIX = "User Layer ";
        const string FACTORY_ASSET_PATH = "Assets/_HolodeckSDK/Prefabs/HolodeckInitializer.prefab";

        static bool isShowing = false;

        static HolodeckEditorWindow window;
        static HolodeckEditorWindow()
        {
            EditorApplication.update += Update;
        }

        [MenuItem("Vulcan/Add HolodeckInitializer to Scene", false, 100)]
        static void AddHolodeckToScene()
        {
            Object[] sceneAssets = FindObjectsOfType(typeof(HolodeckInitializer));
            if (sceneAssets.Length > 0)
            {
                Debug.LogWarning("[HolodeckEditor] Unable to add Holodeck to scene. It already exists.");
                return;
            }

            Object holodeckAsset = (GameObject)AssetDatabase.LoadMainAssetAtPath(FACTORY_ASSET_PATH);
            if (holodeckAsset == null)
            {
                Debug.LogError("[HolodeckEditor] Unable to add Holodeck to scene. Factory prefab not available.");
                return;
            }

            GameObject holodeck = GameObject.Instantiate(holodeckAsset) as GameObject;
            holodeck.name = holodeckAsset.name;
        }

        [MenuItem("Vulcan/Add HolodeckCameraRig to Scene", false, 101)]
        static void CreateHolodeckCameraRig()
        {
            GameObject holodeckCameraRig = new GameObject("HolodeckCameraRig");
            holodeckCameraRig.AddComponent<HolodeckCameraRig>();
        }

        [MenuItem("Vulcan/Generate Starter Holodeck Scene", false, 1)]
        static void GenerateHolodeckScene()
        {
            if (EditorUtility.DisplayDialog("Generate Scene", "Generating a new scene will close your current scene.", "Okay", "Cancel"))
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AddHolodeckToScene();
                CreateHolodeckCameraRig();
            }
        }

        static bool CheckHolodeckLayers()
        {
            int warpRenderLayer = LayerMask.NameToLayer(Constants.WARP_RENDER_LAYER);
            int collisionLayer = LayerMask.NameToLayer(Constants.COLLIDER_LAYER);
            int simulatorLayer = LayerMask.NameToLayer(Constants.SIMULATOR_LAYER);
            
            return warpRenderLayer != -1 && collisionLayer != -1 && simulatorLayer != -1;
        }

        static bool CheckHolodeckColliderLayers()
        {
            bool isCorrect = true;

            int collisionLayer = LayerMask.NameToLayer(Constants.COLLIDER_LAYER);
            HolodeckCameraRig[] rigs = FindObjectsOfType<HolodeckCameraRig>();
            for (int i = 0; i < rigs.Length; ++i)
            {
                if (rigs[i].CollisionModel != null && rigs[i].CollisionModel.layer != collisionLayer)
                {
                    isCorrect = false;
                    break;
                }
            }

            return isCorrect;
        }

        static int GetNextAvailableLayer()
        {
            for (int i = 31; i > 7; --i)
            {
                if (string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    return i;
                }
            }

            Debug.LogWarning("[HolodeckEditor] All layers in use. Holodeck layers can not be added.");
            return -1;
        }

        static void SetLayer(int index, string name)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                Debug.LogWarning("[HolodeckEditor] Unable to setup layers.  This version of Unity is not supported.");
                return;
            }

            SerializedProperty sp = layers.GetArrayElementAtIndex(index);
            sp.stringValue = name;

            tagManager.ApplyModifiedProperties();
        }

        [MenuItem("Vulcan/Setup Holodeck Layers", false, 102)]
        static void SetupHolodeckLayers()
        {
            int warpRenderLayer = LayerMask.NameToLayer(Constants.WARP_RENDER_LAYER);
            int colliderLayer = LayerMask.NameToLayer(Constants.COLLIDER_LAYER);
            int simulatorLayer = LayerMask.NameToLayer(Constants.SIMULATOR_LAYER);

            if (warpRenderLayer == -1)
            {
                warpRenderLayer = GetNextAvailableLayer();
                SetLayer(warpRenderLayer, Constants.WARP_RENDER_LAYER);
            }

            if (colliderLayer == -1)
            {
                colliderLayer = GetNextAvailableLayer();
                SetLayer(colliderLayer, Constants.COLLIDER_LAYER);
            }

            if (simulatorLayer == -1)
            {
                simulatorLayer = GetNextAvailableLayer();
                SetLayer(simulatorLayer, Constants.SIMULATOR_LAYER);
            }

            string debugLayerString = (Constants.WARP_RENDER_LAYER + " is set to " + warpRenderLayer + " / " + Constants.COLLIDER_LAYER + " is set to " + colliderLayer + " / " + Constants.SIMULATOR_LAYER + " is set to " + simulatorLayer);
            if (warpRenderLayer == -1 || colliderLayer == -1 || simulatorLayer == -1)
            {
                Debug.Log("[HolodeckEditor] One or more Holodeck layers have failed to set. " + debugLayerString);
            }
            else
            {
                Debug.Log("[HolodeckEditor] Holodeck layers have been set. " + debugLayerString);
            }
        }

        private void OnEnable()
        {
            isShowing = true;
            EditorApplication.update -= Update;
        }

        private void OnDisable()
        {
            isShowing = false;
            EditorApplication.update += Update;
        }

        static void Update()
        {
            bool shouldShow = false;            
            if (!CheckHolodeckLayers())
            {
                shouldShow = true;
                if (!isShowing)
                {
                    Debug.LogWarning("[HolodeckEditor] Holodeck layers are not properly set and may produce rendering or collision artifacts. Please initialize layers using Vulcan > Setup Holodeck Layers");
                }
            }
            else if (!CheckHolodeckColliderLayers())
            {
                shouldShow = true;
            }

            if (PlayerSettings.scriptingRuntimeVersion != ScriptingRuntimeVersion.Latest)
            {
                shouldShow = true;
            }
            
            if (shouldShow)
            {
                window = GetWindow<HolodeckEditorWindow>(true);
                window.minSize = new Vector2(320, 300);
            }
        }

        bool requiresRestart = false;
        public void OnGUI()
        {
            int warpRenderLayer = LayerMask.NameToLayer(Constants.WARP_RENDER_LAYER);
            int colliderLayer = LayerMask.NameToLayer(Constants.COLLIDER_LAYER);
            int simulatorLayer = LayerMask.NameToLayer(Constants.SIMULATOR_LAYER);

            bool runtimeSetCorrectly = PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest;
            bool warpLayerSetCorrectly = warpRenderLayer != -1;
            bool colliderLayerSetCorrectly = colliderLayer != -1;
            bool simulatorLayerSetCorrectly = simulatorLayer != -1;

            bool colliderLayersSetCorrectly = true;
            HolodeckCameraRig[] rigs = FindObjectsOfType<HolodeckCameraRig>();
            for (int i = 0; i < rigs.Length; ++i)
            {
                if (rigs[i].CollisionModel != null && rigs[i].CollisionModel.layer != colliderLayer)
                {
                    colliderLayersSetCorrectly = false;
                }
            }

            if (!runtimeSetCorrectly || !warpLayerSetCorrectly || !colliderLayerSetCorrectly || !simulatorLayerSetCorrectly || !colliderLayersSetCorrectly)
            {
                EditorGUILayout.HelpBox("HolodeckSDK is not setup correctly.", MessageType.Warning);
            }
            else if (requiresRestart)
            {
                EditorGUILayout.HelpBox("Editor requires restart.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("HolodeckSDK is ready.", MessageType.Info);
            }

            GUILayout.Label(string.Format("ScriptingRuntimeVersion is {0}", PlayerSettings.scriptingRuntimeVersion));
            if (!runtimeSetCorrectly)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Set to Latest"))
                    {
                        PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
                        Debug.LogWarning("[HolodeckEditor] Holodeck requires .NET 4.6. Forcing player settings to use the latest runtime. Please restart your editor.");
                        EditorUtility.DisplayDialog("Incorrect Runtime Version", "HolodeckSDK requires .NET 4.6.\nPlease restart your editor.", "Okay");

                        requiresRestart = true;
                    }
                }
                GUILayout.EndHorizontal();
            }

            SetOrUpdateLayerByGUI(Constants.WARP_RENDER_LAYER, warpRenderLayer, warpLayerSetCorrectly);
            SetOrUpdateLayerByGUI(Constants.COLLIDER_LAYER, colliderLayer, colliderLayerSetCorrectly);
            SetOrUpdateLayerByGUI(Constants.SIMULATOR_LAYER, simulatorLayer, simulatorLayerSetCorrectly);

            if (colliderLayer != -1 && !colliderLayersSetCorrectly)
            {
                GUILayout.Label("Holodeck Collision Models are on incorrect layers.");
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Force Holodeck Colliders to Layer"))
                    {
                        for (int i = 0; i < rigs.Length; ++i)
                        {
                            if (rigs[i].CollisionModel != null && rigs[i].CollisionModel.layer != colliderLayer)
                            {
                                rigs[i].CollisionModel.layer = colliderLayer;
                            }
                        }
                    }
                }
            }
        }

        private void SetOrUpdateLayerByGUI(string layerName, int layer, bool isCorrect)
        {
            GUILayout.Label(string.Format("Layer [{0}] is {1}", layerName, layer));
            if (!isCorrect)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Assign Available Layer"))
                    {
                        layer = GetNextAvailableLayer();
                        SetLayer(layer, layerName);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
#endif
    }
}