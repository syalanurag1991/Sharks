//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace Vulcan
{
    public class PanoramaRig : MonoBehaviour
    {
        public enum CameraDirection
        {
            FRONT,
            BACK,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        [SerializeField] private Transform _front;
        [SerializeField] private Transform _back;
        [SerializeField] private Transform _left;
        [SerializeField] private Transform _right;
        [SerializeField] private Transform _up;
        [SerializeField] private Transform _down;

        private Dictionary<CameraDirection, Camera> _cameras;
        private int _warpRenderLayer;
        private int _simulatorLayer;

        private CubeToEqHook.RegistrationBundle _rigRegistration;

        private void Awake()
        {
            _warpRenderLayer = LayerMask.NameToLayer(Constants.WARP_RENDER_LAYER);
            _simulatorLayer = LayerMask.NameToLayer(Constants.SIMULATOR_LAYER);

            _cameras = new Dictionary<CameraDirection, Camera>();

            _rigRegistration = CubeToEqHook.Instance.RegisterRig(this);
            gameObject.name = Constants.PANORAMA_RIG_NAME + "_" + _rigRegistration._rigId;

            UpdateBlitters(CubeToEqHook.Instance.GetCubemapForId(_rigRegistration._rigId), CubeToEqHook.Instance.GetCubeMapArray());
        }

        public int GetRigLayer()
        {
            return _rigRegistration._rigLayer;
        }

        public int GetRigId()
        {
            return _rigRegistration._rigId;
        }

        public void UpdateCameraRigForward(Vector3 forward)
        {
            Transform t;
            foreach (CameraDirection d in System.Enum.GetValues(typeof(CameraDirection)))
            {
                t = GetTransform(d);
                if (t == null)
                {
                    continue;
                }

                t.parent = null;
            }

            transform.forward = Vector3.zero;

            foreach (CameraDirection d in System.Enum.GetValues(typeof(CameraDirection)))
            {
                t = GetTransform(d);
                if (t == null)
                {
                    continue;
                }

                t.parent = transform;
            }
        }

        public void UpdateBlitters(CustomRenderTexture cubemapRT, CubemapArray cubemapArray)
        {
            Transform t;
            CubeFaceBlitter cfb;
            foreach (CameraDirection d in System.Enum.GetValues(typeof(CameraDirection)))
            {
                t = GetTransform(d);
                if (t == null)
                {
                    continue;
                }

                cfb = t.GetComponent<CubeFaceBlitter>();
                cfb.SetRigId(_rigRegistration._rigId);
                cfb.UpdateCubemapRT(cubemapRT, cubemapArray);
            }
        }

        public void CopyCameraProperties(Camera baseCamera)
        {
            Camera c;
            foreach (CameraDirection d in System.Enum.GetValues(typeof(CameraDirection)))
            {
                c = GetCamera(d);
                if (c == null)
                {
                    continue;
                }

                if (XRDevice.isPresent)
                {
                    XRDevice.DisableAutoXRCameraTracking(c, true);
                }

                CopyCamera(baseCamera, c);
            }

            CubeToEqHook.Instance.ResetExcludeLayers();
        }

        public void UpdateCameraExcludeLayers(int[] excludeLayers)
        {
            if (_rigRegistration._rigLayer == 0)
            {
                return;
            }

            Camera c;
            foreach (CameraDirection d in System.Enum.GetValues(typeof(CameraDirection)))
            {
                c = GetCamera(d);
                if (c == null)
                {
                    continue;
                }

                if (XRDevice.isPresent)
                {
                    XRDevice.DisableAutoXRCameraTracking(c, true);
                }

                UpdateExcludeLayers(c, excludeLayers);
            }
        }

        public Camera GetCamera(CameraDirection d)
        {
            Camera c;

            if (_cameras.ContainsKey(d))
            {
                c = _cameras[d];
            }
            else
            {
                Transform t = GetTransform(d);
                c = t.GetComponent<Camera>();
                if (c != null)
                {
                    _cameras.Add(d, c);
                }
            }

            return c;
        }

        public Transform GetTransform(CameraDirection d)
        {
            Transform ret = null;
            switch (d)
            {
                case CameraDirection.FRONT:
                    ret = _front;
                    break;

                case CameraDirection.BACK:
                    ret = _back;
                    break;

                case CameraDirection.LEFT:
                    ret = _left;
                    break;

                case CameraDirection.RIGHT:
                    ret = _right;
                    break;

                case CameraDirection.UP:
                    ret = _up;
                    break;

                case CameraDirection.DOWN:
                    ret = _down;
                    break;
            }

            return ret;
        }

        private void UpdateExcludeLayers(Camera c, int[] excludeLayers)
        {
            int adjustedCullingMask = c.cullingMask;
            for (int i = 0; i < excludeLayers.Length; ++i)
            {
                if (excludeLayers[i] > 0 && excludeLayers[i] != _rigRegistration._rigLayer)
                {
                    adjustedCullingMask &= ~(1 << excludeLayers[i]);
                }
            }

            c.cullingMask = adjustedCullingMask;
        }


        private void CopyCamera(Camera source, Camera destination)
        {
            source.stereoTargetEye = StereoTargetEyeMask.None;

            int adjustedCullingMask = source.cullingMask;
            adjustedCullingMask &= ~(1 << _warpRenderLayer);
            adjustedCullingMask &= ~(1 << _simulatorLayer);

            destination.clearFlags = source.clearFlags;
            destination.backgroundColor = source.backgroundColor;
            destination.cullingMask = adjustedCullingMask;
            destination.orthographic = source.orthographic;
            destination.nearClipPlane = source.nearClipPlane;
            destination.farClipPlane = source.farClipPlane;
            destination.depth = source.depth;
            destination.renderingPath = source.renderingPath;
            destination.targetTexture = source.targetTexture;
            destination.rect = source.rect;
            destination.useOcclusionCulling = source.useOcclusionCulling;
            destination.allowHDR = source.allowHDR;
            destination.allowMSAA = source.allowMSAA;
            destination.stereoSeparation = source.stereoSeparation;
            destination.stereoConvergence = source.stereoConvergence;
            destination.targetDisplay = source.targetDisplay;
            destination.stereoTargetEye = source.stereoTargetEye;
            destination.fieldOfView = source.fieldOfView;
        }
    }
}