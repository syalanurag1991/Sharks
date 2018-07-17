//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using System.Collections.Generic;

namespace Vulcan
{
    [RequireComponent(typeof(Camera))]
    public class CubeToEqHook : MonoSingleton<CubeToEqHook>
    {
        public struct RegistrationBundle
        {
            public RegistrationBundle(int rigId = -1, int rigLayer = -1)
            {
                _rigId = rigId;
                _rigLayer = rigLayer;
            }

            public int _rigId;
            public int _rigLayer;
        }

        private const int MAX_RIGS = 4;

        private static List<PanoramaRig> _registeredRigs = new List<PanoramaRig>();
        private static Dictionary<int, CustomRenderTexture> _cubemapRTs = new Dictionary<int, CustomRenderTexture>();
        private static int[] _registeredLayers = new int[MAX_RIGS];

        private static int GetNextAvailableLayer()
        {
            int null_layer = -1;
            for (int i = 31; i > 7; --i)
            {
                if (string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    bool isRegistered = false;
                    for (int j = 0; j < MAX_RIGS; ++j)
                    {
                        if (_registeredLayers[j] == i)
                        {
                            isRegistered = true;
                            break;
                        }
                    }

                    if (isRegistered)
                    {
                        continue;
                    }
                    else
                    {
                        return i;
                    }
                }
            }

            return null_layer;
        }

        [SerializeField] private Renderer _targetRenderer;
        [SerializeField] private Camera _targetCamera;

        private CubemapArray _cubemapArray;
        private Material _targetMaterial;
        private int _resolution;
        private int _depthBuffer;
        private int _screensVisible = 0;

        protected override void Awake()
        {
            base.Awake();

            if (_targetRenderer == null)
            {
                Debug.LogErrorFormat(this, "[CubeToEqHook] _targetRenderer is not set.");
                return;
            }

            if (_targetCamera == null)
            {
                Debug.LogErrorFormat(this, "[CubeToEqHook] _targetCamera is not set.");
                return;
            }

            int warpRenderLayer = LayerMask.NameToLayer(Constants.WARP_RENDER_LAYER);

            _targetMaterial = _targetRenderer != null ? _targetRenderer.material : null;
            _targetRenderer.gameObject.layer = warpRenderLayer;
            _targetCamera.cullingMask = (1 << warpRenderLayer);
        }

        public CustomRenderTexture GetCubemapForId(int id)
        {
            if (_cubemapRTs.ContainsKey(id))
            {
                return _cubemapRTs[id];
            }

            return null;
        }

        public CubemapArray GetCubeMapArray()
        {
            return _cubemapArray;
        }

        public int GetLayerForRig(int rig)
        {
            if (rig < 0 || rig >= _registeredLayers.Length)
            {
                return -1;
            }

            return _registeredLayers[rig];
        }

        public void SetResolution(int resolution, int depthBuffer)
        {
            _resolution = resolution;
            _depthBuffer = depthBuffer;

            ResetRTs(resolution, depthBuffer);
        }

        public void SetScreenCount(int count)
        {
            if (_targetMaterial != null)
            {
                _targetMaterial.SetFloat("_NumCubesInUse", count);
            }

            _screensVisible = count;
            for (int i = 0; i < _registeredRigs.Count; ++i)
            {
                _registeredRigs[i].gameObject.SetActive(i < _screensVisible);
            }
        }

        public RegistrationBundle RegisterRig(PanoramaRig rig)
        {
            RegistrationBundle rb = new RegistrationBundle();
            if (_registeredRigs.Contains(rig))
            {
                return rb;
            }

            if (_registeredRigs.Count >= MAX_RIGS)
            {
                Debug.LogErrorFormat("[CubeToEqHook] Unable to register more rigs. There is currently a maximum of {0} HolodeckCameraRigs.", MAX_RIGS);
                return rb;
            }

            _registeredRigs.Add(rig);
            rig.gameObject.SetActive(_registeredRigs.Count <= _screensVisible);

            rb._rigId = _registeredRigs.Count - 1;
            rb._rigLayer = GetNextAvailableLayer();

            _registeredLayers[rb._rigId] = rb._rigLayer;

            ResetRTs(_resolution, _depthBuffer);

            return rb;
        }

        public void DeregisterRig(PanoramaRig rig)
        {
            if (!_registeredRigs.Contains(rig))
            {
                return;
            }

            _registeredRigs.Remove(rig);
        }

        public void ResetExcludeLayers()
        {
            // Only handle exclude layers if you have more than one HolodeckCameraRig
            if (_registeredRigs.Count < 2)
            {
                return;
            }

            for (int i = 0; i < _registeredRigs.Count; ++i)
            {
                _registeredRigs[i].UpdateCameraExcludeLayers(_registeredLayers);
            }
        }

        private void ResetRTs(int resolution, int depthBuffer)
        {
            CustomRenderTexture cubemapRT;
            for (int i = 0; i < _cubemapRTs.Count; ++i)
            {
                cubemapRT = _cubemapRTs[i];
                cubemapRT.Release();
            }
            _cubemapRTs.Clear();

            if (_registeredRigs.Count == 0)
            {
                return;
            }

            _cubemapArray = new CubemapArray(resolution, _registeredRigs.Count, TextureFormat.ARGB32, false, true);
            _cubemapArray.name = "CubemapArray";

            for (int i = 0; i < _registeredRigs.Count; ++i)
            {
                cubemapRT = new CustomRenderTexture(resolution, resolution, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                cubemapRT.name = "Cubemap_" + i.ToString();
                cubemapRT.dimension = UnityEngine.Rendering.TextureDimension.Cube;

                // TODO: This crashes when not set by the editor for some reason in 2017.2 / Fixed in 2017.3
                cubemapRT.initializationMode = CustomRenderTextureUpdateMode.OnLoad;
                cubemapRT.updateMode = CustomRenderTextureUpdateMode.Realtime;

                cubemapRT.depth = depthBuffer;
                cubemapRT.Create();

                _cubemapRTs.Add(i, cubemapRT);

                _registeredRigs[i].UpdateBlitters(cubemapRT, _cubemapArray);
                _targetMaterial.SetTexture("_MainTex_" + i.ToString(), cubemapRT);
            }

            _targetMaterial.SetFloat("_VerticalMask", HolodeckInitializer.Instance.SettingsManager.settings.DisplaySettings.verticalMasking);
            _targetMaterial.SetTexture("_MainTexCubes", _cubemapArray);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            for (int i = 0; i < _cubemapRTs.Count; ++i)
            {
                _cubemapRTs[i].Release();
            }
            _cubemapRTs.Clear();

            _registeredRigs.Clear();
        }
    }
}