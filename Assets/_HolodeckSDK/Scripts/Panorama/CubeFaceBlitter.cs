//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;

namespace Vulcan
{
    [RequireComponent(typeof(Camera))]
    public class CubeFaceBlitter : MonoBehaviour
    {
        [SerializeField] private Material _blitMaterial;
        [SerializeField] private CubemapFace _targetFace;

        private CustomRenderTexture _cubemapRT;
        private Camera _localCamera;
        private Material _localMaterial;

        // TODO: Support array based setting
        private CubemapArray _cubemapArray;
        private int _rigId;

        private void Start()
        {
            _localCamera = GetComponent<Camera>();
            if (_localCamera == null || _blitMaterial == null)
            {
                Debug.LogWarning("[CubeFaceBlitter] Camera or Material not set.");
                return;
            }

            _localCamera.enabled = false;
            _localMaterial = new Material(_blitMaterial);
        }

        public void SetRigId(int id)
        {
            _rigId = id;
        }

        public void UpdateCubemapRT(CustomRenderTexture crt, CubemapArray cubemapArray)
        {
            _cubemapRT = crt;
            _cubemapArray = cubemapArray;
        }

        private void LateUpdate()
        {
            _localCamera.aspect = 1.0f;
            _localCamera.fieldOfView = 90f;

            _localCamera.Render();
        }

        // TODO: Move this stuff into a central controller instead of once per camera
        // This could probably be a command buffer on a single camera
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.SetRenderTarget(_cubemapRT, 0, _targetFace);
            Graphics.Blit(_localCamera.activeTexture, _localMaterial);
            Graphics.Blit(source, destination);

            // TODO: Shader would get trimmer if this would work
            if (_cubemapArray != null && _rigId > -1)
            {
                //Graphics.CopyTexture(_cubemapRT, 0, 0, _cubemapArray, _rigId, 0);
            }
        }
    }
}