//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vulcan
{
    [RequireComponent(typeof(DisplayScript))]
    public class WarpDisplayRig : MonoBehaviour
    {
        public enum FileType
        {
            EXR = 0,
            BLEND = 1
        }

        [SerializeField] private Material _warpMaterial;
        [SerializeField] private string _warpTextureShaderProperty = "_WarpTex";
        [SerializeField] private string _blendTextureShaderProperty = "_BlendTex";
        [SerializeField] private int _textureWidth = 4096;
        [SerializeField] private int _textureHeight = 2160;
        [SerializeField] List<string> _exrMapFileNames;
        [SerializeField] List<string> _blendMapFileNames;
        [SerializeField] CameraClearFlags _clearFlags = CameraClearFlags.Skybox;

        private List<Camera> _displayCameras;
        private List<GameObject> _renderSurfaces;
        private List<Vector3> _renderSurfacesWorldPositions;
        private List<int> _renderSurfaceIndicies;
        private List<Texture2D> _loadedBlendMaps;
        private List<Material> _warpMaterials;

        public List<Material> WarpMaterials
        {
            get { return _warpMaterials; }
            private set { }
        }

        public List<Camera> DisplayCameras
        {
            get { return _displayCameras; }
            private set { }
        }

        void Start()
        {
            if (DisplayScript.Instance == null)
            {
                Debug.LogError("[WarpDisplayRig] No DisplayScript available. Unable to load screen mappings.");
                return;
            }

            StartCoroutine(Initialize());
        }

        private static bool IsValidPath(string path)
        {
            bool isValid;
            if (!string.IsNullOrEmpty(path))
            {
                // Override map path provided, check that it exists
                isValid = Directory.Exists(path);
            }
            else
            {
                isValid = false;
            }
            return isValid;
        }

        private IEnumerator Initialize()
        {
            string pathToMaps;
            if (IsValidPath(DisplayScript.Instance.settings.mapOverridePath))
            {
                pathToMaps = DisplayScript.Instance.settings.mapOverridePath;
            }
            else
            {
                // override path was noth provided or does not exist, use global
                if (IsValidPath(Constants.GLOBAL_MAPS_PATH))
                {
                    pathToMaps = Constants.GLOBAL_MAPS_PATH;
                }
                else
                {
                    pathToMaps = null;
                    Debug.LogErrorFormat(this, "[WarpDisplayRig] Global Map Path does not exist.  Please install warp and blend maps to {0}", Constants.GLOBAL_MAPS_PATH);
                }
            }

            if (!string.IsNullOrEmpty(pathToMaps))
            {
                _loadedBlendMaps = new List<Texture2D>();
                for (int i = 0; i < _blendMapFileNames.Count; ++i)
                {
                    string filename = _blendMapFileNames[i];
                    string path = Path.Combine(pathToMaps, filename);
                    path = "file://" + path;

                    using (WWW www = new WWW(path))
                    {
                        while (!www.isDone)
                        {
                            yield return www;
                        }

                        Texture2D loadedBlendMap = www.texture;
                        _loadedBlendMaps.Add(loadedBlendMap);
                    }
                }

                int renderLayer = LayerMask.NameToLayer(Constants.WARP_RENDER_LAYER);
                if (_warpMaterial != null)
                {
                    if (!string.IsNullOrEmpty(_warpTextureShaderProperty) &&
                        !string.IsNullOrEmpty(_blendTextureShaderProperty))
                    {
                        // 
                        // Check all the exr Inputs,  currently we expect 4 textures, but we should support any number
                        //
                        if (_exrMapFileNames != null && _exrMapFileNames.Count > 0)
                        {
                            if (_blendMapFileNames != null && _blendMapFileNames.Count == _exrMapFileNames.Count)
                            {
                                _displayCameras = new List<Camera>();
                                _renderSurfaces = new List<GameObject>();
                                _renderSurfacesWorldPositions = new List<Vector3>();
                                _renderSurfaceIndicies = new List<int>();
                                _warpMaterials = new List<Material>();

                                for (int i = 0; i < _exrMapFileNames.Count; i++)
                                {
                                    if (_exrMapFileNames[i] != null && _blendMapFileNames[i] != null)
                                    {
                                        //
                                        // Create and configure the current camera
                                        // 
                                        GameObject camObject = new GameObject();
                                        Camera currentCamera = camObject.AddComponent<Camera>();
                                        _displayCameras.Add(currentCamera);
                                        currentCamera.name = Constants.WARP_CAMERA_PREFIX + i;
                                        currentCamera.transform.parent = transform;
                                        currentCamera.orthographic = true;
                                        currentCamera.orthographicSize = 1.0f;
                                        currentCamera.nearClipPlane = 0.5f;
                                        currentCamera.farClipPlane = 1.5f;
                                        currentCamera.targetDisplay = i;
                                        currentCamera.stereoTargetEye = StereoTargetEyeMask.None;
                                        currentCamera.clearFlags = _clearFlags;
                                        currentCamera.backgroundColor = Color.black;

                                        //
                                        // Put in a reserved layer so nothing else renders even if world objects overlap
                                        //
                                        currentCamera.cullingMask = 1 << renderLayer;

                                        // Calculate the height and width
                                        float height = currentCamera.orthographicSize * 2;
                                        float width = height * Constants.ASPECT_RATIO;

                                        // Move each cameras over so they don't overlap, and add a little bias to prevent overlap due to floating point error
                                        int x, y;
                                        x = i % 2;
                                        y = i / 2;
                                        currentCamera.transform.localPosition =
                                            (
                                                ((x * width * Vector3.right) + (x * 0.1f * Vector3.right)) +
                                                ((y * height * Vector3.down) + (y * 0.1f * Vector3.down))
                                            );

                                        //
                                        // Create and configure the current quad
                                        //
                                        GameObject currentQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                                        MeshCollider mc = currentQuad.GetComponent<MeshCollider>();
                                        Destroy(mc);

                                        currentQuad.name = "WarpDisplaySurface_" + i;
                                        _renderSurfaces.Add(currentQuad);

                                        // Position the quad directly infront of the matching camera
                                        currentQuad.transform.parent = currentCamera.transform;
                                        currentQuad.transform.localPosition = Vector3.zero;
                                        currentQuad.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);

                                        currentQuad.transform.localScale = new Vector3(width, height, 1.0f);
                                        currentQuad.transform.localPosition += currentQuad.transform.parent.forward;

                                        //
                                        // Apply the render layer
                                        //
                                        currentQuad.layer = renderLayer;

                                        _renderSurfacesWorldPositions.Add(currentQuad.transform.position);
                                        _renderSurfaceIndicies.Add(i);

                                        //
                                        // Set the material
                                        // NOTE: The exr loader script sets the shader texture based on an expected texture property
                                        //
                                        Renderer currentRenderer = currentQuad.GetComponent<Renderer>();
                                        currentRenderer.material = _warpMaterial;

                                        //
                                        // Load the EXR
                                        //

                                        Texture2D decodedExr = EXRLoader.DecodeEXR(_exrMapFileNames[i], _textureWidth, _textureHeight, pathToMaps);
                                        if (decodedExr != null)
                                        {
                                            //
                                            // Update the renderer
                                            //
                                            currentRenderer.material.SetTexture(_warpTextureShaderProperty, decodedExr);
                                            currentRenderer.material.SetTexture(_blendTextureShaderProperty,
                                                _loadedBlendMaps[i]);
                                        }
                                        else
                                        {
                                            Debug.LogErrorFormat(this,
                                                "[WarpDisplayRig] Failed to decode EXR at index {0}.  Can't continue",
                                                i);
                                        }

                                        _warpMaterials.Add(currentRenderer.material);
                                    }
                                    else
                                    {
                                        Debug.LogErrorFormat(this,
                                            "[WarpDisplayRig] Exr map and/or Blend Map at index [i] is null, can't continue");
                                        yield break;
                                    }
                                }

                                //
                                // Enable the additional status camera, if possible
                                //
                                AddDisplayZeroStatusCamera();
                            }
                            else
                            {
                                Debug.LogErrorFormat(this,
                                    "[WarpDisplayRig] Blend maps list much match size of EXR list, can't continue");
                            }
                        }
                        else
                        {
                            Debug.LogErrorFormat(this, "[WarpDisplayRig] Exr maps list is empty, can't continue");
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat(this,
                            "[WarpDisplayRig] Warp and/or Blend shader properties are not set, can't continue.  Expecting '{0}' and '{1}'",
                            _warpTextureShaderProperty, _blendTextureShaderProperty);
                    }
                }
                else
                {
                    Debug.LogErrorFormat(this, "[WarpDisplayRig] Warp material not supplied, can't continue");
                }
            }

            DisplayScript.Instance.Reset();
        }

        private void AddDisplayZeroStatusCamera()
        {
            //
            // If display zero is not used for rendering use it for status instead
            //
            if (DisplayScript.Instance != null && !DisplayScript.Instance.DisplayZeroUsedForRendering)
            {
                float xMin = _displayCameras[0].transform.position.x;
                float xMax = _displayCameras[0].transform.position.x;
                float yMin = _displayCameras[0].transform.position.y;
                float yMax = _displayCameras[0].transform.position.y;

                // find the center of the display cameras
                for (int i = 1; i < _displayCameras.Count; i++)
                {
                    xMin = Mathf.Min(_displayCameras[i].transform.position.x, xMin);
                    xMax = Mathf.Max(_displayCameras[i].transform.position.x, xMax);
                    yMin = Mathf.Min(_displayCameras[i].transform.position.y, yMin);
                    yMax = Mathf.Max(_displayCameras[i].transform.position.y, yMax);
                }

                Vector3 position = new Vector3(xMin + 0.5f * (xMax - xMin), yMin + 0.5f * (yMax - yMin), _displayCameras[0].transform.position.z);
                GameObject debugCameraObject = new GameObject("HolodeckOperatorDisplay");
                debugCameraObject.transform.position = position;
                debugCameraObject.transform.parent = transform;

                Camera camera = debugCameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 3.0f;
                camera.nearClipPlane = 0.5f;
                camera.farClipPlane = 1.5f;
                camera.targetDisplay = 0;
                camera.stereoTargetEye = StereoTargetEyeMask.None;
                camera.cullingMask = 1 << LayerMask.NameToLayer(Constants.WARP_RENDER_LAYER);
                camera.clearFlags = CameraClearFlags.Color;

                OperatorControlScreen ocs = camera.gameObject.AddComponent<OperatorControlScreen>();
                ocs.WarpDisplayRig = this;
                ocs.TargetCamera = camera;

                //
                // Soft background
                //
                camera.backgroundColor = new Color32(180, 180, 180, 255);
            }
        }

#if UNITY_EDITOR
        public void AddExrFileName(string fileName, FileType type)
        {
            List<string> targetList = (type == FileType.EXR) ? _exrMapFileNames : _blendMapFileNames;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (targetList == null)
                {
                    targetList = new List<string>();
                }
                targetList.Add(fileName);
            }
        }

        public void CleanupFileNameList(FileType type)
        {
            List<string> targetList = (type == FileType.EXR) ? _exrMapFileNames : _blendMapFileNames;
            if (targetList != null)
            {
                for (int i = targetList.Count - 1; i > -1; i--)
                {
                    if (string.IsNullOrEmpty(targetList[i]))
                    {
                        targetList.RemoveAt(i);
                    }
                }
            }
        }

        public int GetEXRMapCount()
        {
            int count;
            if (_exrMapFileNames != null)
            {
                count = _exrMapFileNames.Count;
            }
            else
            {
                count = 0;
            }
            return count;
        }

        public int GetBlendMapCount()
        {
            int count;
            if (_blendMapFileNames != null)
            {
                count = _blendMapFileNames.Count;
            }
            else
            {
                count = 0;
            }
            return count;
        }
#endif
    }
}