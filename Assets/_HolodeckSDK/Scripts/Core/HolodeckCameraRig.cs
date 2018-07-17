//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;

namespace Vulcan
{
    [RequireComponent(typeof(Camera))]
    public class HolodeckCameraRig : MonoBehaviour
    {
        private Camera _unityCamera;
        private PanoramaRig _panoramaRig;

        private Vector3 _cameraRigForward;

        [SerializeField] private GameObject _controllerReference;
        [SerializeField] private Renderer _controllerCalibrationTarget;
        [SerializeField] private GameObject _collisionModel;
        [SerializeField] private Collider[] _colliders;

        public Camera UnityCamera
        {
            get { return _unityCamera; }
        }

        public PanoramaRig PanoramaRig
        {
            get { return _panoramaRig; }
        }

        public int RigId
        {
            get { return _panoramaRig == null ? -1 : _panoramaRig.GetRigId(); }
        }

        public int RigLayer
        {
            get { return _panoramaRig == null ? -1 : _panoramaRig.GetRigLayer(); }
        }

        public Vector3 CameraRigForward
        {
            get { return _cameraRigForward; }
            set { _cameraRigForward = value; }
        }

        public GameObject ControllerReference
        {
            get { return _controllerReference; }
#if UNITY_EDITOR
            set { _controllerReference = value; }
#endif
        }
        
        public GameObject CollisionModel
        {
            get { return _collisionModel; }
#if UNITY_EDITOR
            set { _collisionModel = value; }
#endif
        }

        public void Initialize()
        {
            if (_unityCamera == null)
            {
                _unityCamera = GetComponent<Camera>();
                if (UnityCamera == null)
                {
                    Debug.LogErrorFormat(this, "[HolodeckCameraRig] No camera was provided or exists on {0}.  Unable to Initialize", name);
                    return;
                }
            }

            GameObject prgo = (GameObject)Instantiate(Resources.Load(Constants.PANORAMA_ASSET_PATH + Constants.PANORAMA_RIG_NAME), transform, false);
            _panoramaRig = prgo.GetComponent<PanoramaRig>();
            _panoramaRig.CopyCameraProperties(_unityCamera);

            if (_controllerReference != null)
            {
                _controllerCalibrationTarget = _controllerReference.GetComponentInChildren<Renderer>();
            }

            if (_collisionModel != null)
            {
                VulcanUtils.SetLayerRecursively(_collisionModel, LayerMask.NameToLayer(Constants.COLLIDER_LAYER));
                _colliders = _collisionModel.GetComponentsInChildren<Collider>();
            }
        }

        private void Start()
        {
            HideCalibrationTarget();
        }

        public void ShowCalibrationTarget()
        {
            if (_controllerCalibrationTarget != null)
            {
                _controllerCalibrationTarget.enabled = true;
                Renderer renderer = _controllerCalibrationTarget.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }
            else
            {
                Debug.LogErrorFormat(this, "[HolodeckCamera] Controller Calibration Target not set");
            }
        }

        public void SetCalibrationTargetReady()
        {
            if (_controllerCalibrationTarget != null)
            {
                Renderer renderer = _controllerCalibrationTarget.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.green;
                }
            }
            else
            {
                Debug.LogErrorFormat(this, "[HolodeckCamera] Controller Calibration Target not set");
            }
        }

        public void HideCalibrationTarget()
        {
            if (_controllerCalibrationTarget != null)
            {
                _controllerCalibrationTarget.enabled = false;
            }
        }

        public bool RaycastAgainstHolodeck(Transform source, out RaycastHit rayHit)
        {
            Ray ray = new Ray(source.position, source.forward);
            return RaycastAgainstHolodeck(ray, out rayHit);
        }

        public bool RaycastAgainstHolodeck(Ray ray, out RaycastHit rayHit)
        {
            rayHit = new RaycastHit();

            bool didContact = false;
            for (int i = 0; i < _colliders.Length; ++i)
            {
                didContact = _colliders[i].Raycast(ray, out rayHit, Mathf.Infinity);
                if (didContact)
                {
                    break;
                }
            }
            return didContact;
        }
    }
}