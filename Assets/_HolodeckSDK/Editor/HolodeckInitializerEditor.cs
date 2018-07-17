//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using System.IO;

namespace Vulcan
{
    [CustomEditor(typeof(HolodeckInitializer))]
    public class HolodeckInitializerEditor : Editor
    {
        const string CONTROLLER_ASSET_PATH = "Options/ControllerReference.prefab";
        const string COLLISION_ASSET_PATH = "Options/CollisionModel.prefab";
        const string COMPOSITE_RIG_PATH = "Rigs/CompositeWarpDisplayRig.prefab";
        const string BASE_RIG_PATH = "Rigs/WarpDisplayRig.prefab";

        GameObject _baseRigPrefab;
        GameObject _compositeRigPrefab;
        GameObject _collisionPrefab;
        GameObject _controllerReferencePrefab;

        string GetResourcePath()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            path = Path.GetDirectoryName(path);
            return path.Substring(0, path.Length - "Editor".Length) + "Prefabs/";
        }

        void OnEnable()
        {
            _baseRigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetResourcePath() + BASE_RIG_PATH);
            _compositeRigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetResourcePath() + COMPOSITE_RIG_PATH);
            _collisionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetResourcePath() + COLLISION_ASSET_PATH);
            _controllerReferencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetResourcePath() + CONTROLLER_ASSET_PATH);
        }

        public override void OnInspectorGUI()
        {
            HolodeckInitializer initializer = (HolodeckInitializer)target;
            HolodeckCameraRig[] hcrs = FindObjectsOfType<HolodeckCameraRig>();

            GUILayout.BeginVertical("box");
            {
                initializer.PanoramaQuality = EditorGUILayout.IntSlider("Quality", initializer.PanoramaQuality, 1, 10);
                int size = Mathf.Clamp(Constants.BASE_RT_MULTIPLIER * (1 << initializer.PanoramaQuality), 1, 8192);
                GUILayout.Label("One cube side is: " + size + "x" + size);

                initializer.ScreensToShow = EditorGUILayout.IntSlider("Screens to Show", initializer.ScreensToShow, 1, 4);

                if (GUILayout.Button(initializer.UseSimulator ? "Disable Simulator" : "Enable Simulator"))
                {
                    initializer.UseSimulator = !initializer.UseSimulator;
                }

                if (GUILayout.Button(initializer.UseControllers ? "Disable Controllers" : "Enable Controllers"))
                {
                    initializer.UseControllers = !initializer.UseControllers;
                    
                    for (int i = 0; i < hcrs.Length; ++i)
                    {
                        if (initializer.UseControllers)
                        {
                            GameObject cr = Instantiate(_controllerReferencePrefab, hcrs[i].transform);
                            cr.name = _controllerReferencePrefab.name;

                            hcrs[i].ControllerReference = cr;
                        }
                        else if (hcrs[i].ControllerReference != null)
                        {
                            DestroyImmediate(hcrs[i].ControllerReference);
                        }
                    }
                }

                if (GUILayout.Button((initializer.AllowCollisions) ? "Remove Collision Models" : "Add Collision Models"))
                {
                    initializer.AllowCollisions = !initializer.AllowCollisions;

                    for (int i = 0; i < hcrs.Length; ++i)
                    {
                        if (initializer.AllowCollisions)
                        {
                            GameObject co = Instantiate(_collisionPrefab, hcrs[i].transform);
                            VulcanUtils.SetLayerRecursively(co, LayerMask.NameToLayer(Constants.COLLIDER_LAYER));
                            co.name = _collisionPrefab.name;

                            hcrs[i].CollisionModel = co;
                        }
                        else if (hcrs[i].CollisionModel != null)
                        {
                            DestroyImmediate(hcrs[i].CollisionModel);
                        }
                    }
                }

                string[] rigOptions = { "Basic Rig", "Composite Rig" };
                initializer.CameraRigType = (HolodeckInitializer.RigType)GUILayout.Toolbar((int)initializer.CameraRigType, rigOptions);
                initializer.WarpRigPrefab = initializer.CameraRigType == 0 ? _baseRigPrefab : _compositeRigPrefab;
            }
            GUILayout.EndVertical();

            Camera targetCamera;
            for (int i = 0; i < hcrs.Length; ++i)
            {
                targetCamera = hcrs[i].GetComponent<Camera>();
                if (targetCamera == null)
                {
                    continue;
                }

                if (initializer.CameraRigType == HolodeckInitializer.RigType.COMPOSITE &&
                     (targetCamera.backgroundColor != Color.clear || (targetCamera.clearFlags != CameraClearFlags.Color && targetCamera.clearFlags != CameraClearFlags.SolidColor)))

                {
                    targetCamera.clearFlags = CameraClearFlags.SolidColor;
                    targetCamera.backgroundColor = Color.clear;
                    Debug.LogWarning("[HolodeckInitializer] Composite camera rig can not run with your camera clear flags. Setting to SolidColor.");
                }
            }   

            if (!Application.isPlaying && GUI.changed)
            {
                EditorUtility.SetDirty(initializer);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
}
