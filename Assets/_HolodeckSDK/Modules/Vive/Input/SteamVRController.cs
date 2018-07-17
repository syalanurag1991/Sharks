//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

namespace Vulcan
{
    public class SteamVRController : Controller
    {
        private GameObject _steamVRtrackedObject;
        private SteamVR_TrackedObject _trackedController;

        public static SteamVRController Create(int id, GameObject calibrationParent, bool isTracker)
        {
            SteamVRController svrc = new SteamVRController(id, calibrationParent, isTracker);
            return svrc;
        }

        protected SteamVRController(int id, GameObject calibrationParent, bool isTracker) : base()
        {
            _controllerId = id;
            _isTracker = isTracker;
            _calibrationParent = calibrationParent;

            // create SteamVR_TrackedObject behind the scene to provide us with lower-latency motion tracking
            _steamVRtrackedObject = new GameObject(GetDescriptiveName() + "_TrackedObject");
            _canvasRaycaster = _steamVRtrackedObject.AddComponent<VulcanCanvasRaycaster>();

            _trackedController = _steamVRtrackedObject.AddComponent<SteamVR_TrackedObject>();
            _trackedController.SetDeviceIndex(id);

            // Default Controller to first HolodeckCameraRig
            SetRigId(0);
        }

        public override void Destroy()
        {
            base.Destroy();

            if (_steamVRtrackedObject != null)
            {
                GameObject.Destroy(_steamVRtrackedObject);
                _steamVRtrackedObject = null;
            }
        }

        public override string GetDescriptiveName()
        {
            return "SteamVRController(idx: " + ControllerId + " type: (Controller)";
        }

        private EVRButtonId InputChannelToEVRButtonId(InputChannel channel)
        {
            switch (channel)
            {
                case InputChannel.Trigger:
                    return EVRButtonId.k_EButton_SteamVR_Trigger;
                case InputChannel.Grip:
                    return EVRButtonId.k_EButton_Grip;
                case InputChannel.TouchPad:
                    return EVRButtonId.k_EButton_SteamVR_Touchpad;
                default:
                    return EVRButtonId.k_EButton_SteamVR_Trigger;
            }
        }

        private EVRButtonId InputAxisToEVRButtonId(InputAxis channel)
        {
            switch (channel)
            {
                case InputAxis.Axis0:
                    return EVRButtonId.k_EButton_Axis0;
                case InputAxis.Axis1:
                    return EVRButtonId.k_EButton_Axis1;
                case InputAxis.Axis2:
                    return EVRButtonId.k_EButton_Axis2;
                case InputAxis.Axis3:
                    return EVRButtonId.k_EButton_Axis3;
                case InputAxis.Axis4:
                    return EVRButtonId.k_EButton_Axis4;
                default:
                    return EVRButtonId.k_EButton_Axis0;
            }
        }

        public override float GetInputChannelButtonState(InputChannel channel)
        {
            float retVal = SteamVR_Controller.Input(_controllerId).GetPress(InputChannelToEVRButtonId(channel)) ? 1.0f : 0.0f;
            return retVal;
        }

        public override Vector2 GetInputAxisState(InputAxis channel)
        {
            Vector2 retVal = SteamVR_Controller.Input(_controllerId).GetAxis(InputAxisToEVRButtonId(channel));
            return retVal;
        }

        private Vector3 GetHolodeckCameraRelativePosition()
        {
            Vector3 pos = _steamVRtrackedObject.transform.position;
            if (ControllerManager.Instance.IsCalibrated)
            {
                pos = _calibrationParent.transform.InverseTransformPoint(pos);
            }
            return pos;
        }

        private Quaternion GetHolodeckCameraRelativeRotation()
        {
            Quaternion rot = _steamVRtrackedObject.transform.rotation;
            if (ControllerManager.Instance.IsCalibrated)
            {
                rot = Quaternion.Inverse(_calibrationParent.transform.rotation) * rot;
            }
            return rot;
        }

        public override Vector3 GetWorldSpacePosition()
        {
            HolodeckCameraRig cam = _registeredRig;
            if (cam == null)
            {
                return Vector3.zero;
            }

            Vector3 camRelativePosition = GetHolodeckCameraRelativePosition();
            Vector3 position;
            if (cam != null)
            {
                _referenceTransform.transform.localPosition = camRelativePosition;
                position = _referenceTransform.transform.position;
            }
            else
            {
                position = Vector3.zero;
                Debug.LogErrorFormat("[SteamVRController] HolodeckCameraRig is not instantiated.  Controller world space position will be inaccurate");
            }
            return position;
        }

        public override Quaternion GetWorldSpaceRotation()
        {
            HolodeckCameraRig cam = _registeredRig;
            if (cam == null)
            {
                return Quaternion.identity;
            }

            Quaternion rotation;
            Quaternion camRelativeRotation = GetHolodeckCameraRelativeRotation();
            if (cam != null)
            {
                _referenceTransform.transform.localRotation = camRelativeRotation;
                rotation = _referenceTransform.transform.rotation;
            }
            else
            {
                rotation = Quaternion.identity;
                Debug.LogErrorFormat("[SteamVRController] HolodeckCameraRig is not instantiated.  Controller world space rotation will be inaccurate");
            }

            return rotation;
        }

        Dictionary<GameObject, PointerEventData> eventDataDictionary = new Dictionary<GameObject, PointerEventData>();
        void ProcessEventSystemEvents(ControllerProjection projection)
        {
            GameObject canvasOverGO;
            PointerEventData ped;

            for (int i = 0; i < projection.CanvasOverObjects.Count; ++i)
            {
                canvasOverGO = projection.CanvasOverObjects[i];
                if (!eventDataDictionary.ContainsKey(canvasOverGO))
                {
                    eventDataDictionary[canvasOverGO] = new PointerEventData(EventSystem.current);
                }

                ped = eventDataDictionary[canvasOverGO];
                if (GetTriggerDown())
                {
                    ped.eligibleForClick = true;
                    ped.pointerPress = projection.CanvasOverObjects[i];

                    ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.pointerDownHandler);
                    continue;
                }

                if (GetTriggerUp())
                {
                    if (ped.eligibleForClick && ped.pointerPress == projection.CanvasOverObjects[i])
                    {
                        ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.pointerClickHandler);
                    }
                    else
                    {
                        ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.pointerUpHandler);
                    }

                    ped.eligibleForClick = false;
                    ped.pointerPress = null;
                    continue;
                }

                if (ped.pointerEnter == null)
                {
                    ped.eligibleForClick = false;
                    ped.pointerEnter = projection.CanvasOverObjects[i];
                    ped.pointerPress = null;

                    ExecuteEvents.Execute(ped.pointerEnter, ped, ExecuteEvents.pointerEnterHandler);
                }
            }

            foreach (var pair in eventDataDictionary)
            {
                if (projection.CanvasOverObjects.Contains(pair.Key))
                {
                    continue;
                }

                ped = pair.Value;
                if (ped.pointerEnter != null)
                {
                    ExecuteEvents.Execute(ped.pointerEnter, ped, ExecuteEvents.pointerExitHandler);

                    ped.eligibleForClick = false;
                    ped.pointerEnter = null;
                    ped.pointerPress = null;
                }
            }
        }

        public override void TriggerHapticPulse(ushort durationMicroSeconds = 2500)
        {
            SteamVR_Controller.Input(_controllerId).TriggerHapticPulse(durationMicroSeconds);
        }

        public override ControllerProjection GetControllerProjection(float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            if (_cachedProjection != null)
            {
                return _cachedProjection;
            }

            if (_registeredRig == null)
            {
                Debug.LogError("[SteamVRController] Controller is not registered to a rig correctly.");
                return null;
            }

            ControllerProjection projection = new ControllerProjection();
            Vector3 direction = GetWorldSpaceRotation() * Vector3.forward;

            RaycastHit rh;

            Ray hdRay = new Ray(GetWorldSpacePosition(), direction);
            _registeredRig.RaycastAgainstHolodeck(hdRay, out rh);

            layerMask &= ~_domeLayer;
            layerMask &= ~_simulatorLayer;

            projection.HolodeckHit = rh;
            projection.Ray = new Ray(_registeredRig.transform.position, rh.point - _registeredRig.transform.position);
            projection.RaycastHits = Physics.RaycastAll(projection.Ray, maxDistance, layerMask);

            projection.CanvasOverObjects = new List<GameObject>();

            _canvasRaycaster.Raycast(projection.Ray, ref projection.CanvasOverObjects);
            _cachedProjection = projection;

            ProcessEventSystemEvents(projection);

            return projection;
        }
    }
}
