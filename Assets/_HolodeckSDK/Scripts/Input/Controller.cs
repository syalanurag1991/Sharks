using System.Collections.Generic;
using UnityEngine;

namespace Vulcan
{
    public abstract class Controller
    {
        public enum InputChannel
        {
            Trigger = 0,
            Grip = 1,
            TouchPad = 2,
        }

        public enum InputAxis
        {
            Axis0 = 0,
            Axis1 = 1,
            Axis2 = 2,
            Axis3 = 3,
            Axis4 = 4,
        }

        private Dictionary<InputChannel, bool> _inputChannelStateLastFrame = new Dictionary<InputChannel, bool>();
        private Dictionary<InputChannel, bool> _inputChannelStateThisFrame = new Dictionary<InputChannel, bool>();

        protected int _domeLayer = -1;
        protected int _simulatorLayer = -1;

        protected int _controllerId = -1;
        protected int _rigId = -1;
        protected HolodeckCameraRig _registeredRig;

        protected GameObject _calibrationParent;
        protected GameObject _referenceTransform;
        
        protected ControllerProjection _cachedProjection = null;
        protected VulcanCanvasRaycaster _canvasRaycaster;

        protected bool _isTracker;

        protected Controller()
        {
            _domeLayer = 1 << LayerMask.NameToLayer(Constants.COLLIDER_LAYER);
            _simulatorLayer = 1 << LayerMask.NameToLayer(Constants.SIMULATOR_LAYER);
        }

        /// <summary>
        /// Get whether this controller is a tracker or not
        /// </summary>
        public bool IsTracker
        {
            get { return _isTracker; }
        }

        /// <summary>
        /// Get the id of this controller
        /// </summary>
        public int ControllerId
        {
            get { return _controllerId; }
        }

        /// <summary>
        /// Get the id of the associated rig
        /// </summary>
        public int RigId
        {
            get { return _rigId; }
        }

        /// <summary>
        /// Get the absolute position of the Controller in world space
        /// </summary>
        /// <returns>Vector3 position in world space</returns>
        public abstract Vector3 GetWorldSpacePosition();

        /// <summary>
        /// Get the absolute rotation of the Controller in world space
        /// </summary>
        /// <returns>Quaternion rotation in world space</returns>
        public abstract Quaternion GetWorldSpaceRotation();

        /// <summary>
        /// Get the ControllerProjection of this controller
        /// </summary>
        /// <returns>ControllerProjection object containing the inteded ray and an array of RaycastHit objects</returns>
        public abstract ControllerProjection GetControllerProjection(float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers);

        /// <summary>
        /// Get the present press value of the given InputChannel from 0 to 1
        /// </summary>
        /// <returns>0 to 1 depending on the extent to which the given channel is depressed</returns>
        public abstract float GetInputChannelButtonState(InputChannel channel);

        /// <summary>
        /// Get the present press value of the axis
        /// </summary>
        /// <returns>0 to 1 depending on the extent to which the given channel is depressed</returns>
        public abstract Vector2 GetInputAxisState(InputAxis channel);

        /// <summary>
        /// Get the descriptive name of this controller
        /// </summary>
        /// <returns></returns>
        public abstract string GetDescriptiveName();

        /// <summary>
        /// Get whether the given InputChannel is currently pressed
        /// </summary>
        /// <returns>True if the InputChannel is currently pressed</returns>
        public bool GetInputChannel(InputChannel channel)
        {
            return GetInputChannelButtonState(channel) > 0.5;
        }

        /// <summary>
        /// Check if the InputChannel was pressed this frame
        /// </summary>
        /// <returns>true during the frame that the InputChannel is pressed</returns>
        public bool GetInputChannelDown(InputChannel channel)
        {
            bool thisFrame = false;
            bool lastFrame = false;
            _inputChannelStateLastFrame.TryGetValue(channel, out lastFrame);
            _inputChannelStateThisFrame.TryGetValue(channel, out thisFrame);
            return thisFrame && !lastFrame;
        }

        /// <summary>
        /// Check if the InputChannel was released this frame
        /// </summary>
        /// <returns>true during the frame that the InputChannel is released</returns>
        public bool GetInputChannelUp(InputChannel channel)
        {
            bool thisFrame = false;
            bool lastFrame = false;
            _inputChannelStateLastFrame.TryGetValue(channel, out lastFrame);
            _inputChannelStateThisFrame.TryGetValue(channel, out thisFrame);
            return !thisFrame && lastFrame;
        }

        public virtual void Destroy()
        {
            if (_referenceTransform != null)
            {
                GameObject.Destroy(_referenceTransform);
                _referenceTransform = null;
            }
        }

        public virtual void Update()
        {
            foreach (InputChannel channel in System.Enum.GetValues(typeof(InputChannel)))
            {
                bool v = false;
                _inputChannelStateThisFrame.TryGetValue(channel, out v);
                _inputChannelStateLastFrame[channel] = v;
                _inputChannelStateThisFrame[channel] = GetInputChannel(channel);
            }
            
            _cachedProjection = null;
        }

        public virtual void LateUpdate() { }

        /// <summary>
        /// Get the trigger press amount from 0 to 1
        /// </summary>
        /// <returns>0 to 1 depending on the extent to which trigger is depressed</returns>
        public float GetTriggerState()
        {
            return GetInputChannelButtonState(InputChannel.Trigger);
        }

        /// <summary>
        /// Check if the trigger is held down
        /// </summary>
        /// <returns>True if the trigger is pressed</returns>
        public bool GetTrigger()
        {
            return GetInputChannel(InputChannel.Trigger);
        }

        /// <summary>
        /// Check if the trigger was pressed this frame
        /// </summary>
        /// <returns>true during the frame that the trigger is pressed</returns>
        public bool GetTriggerDown()
        {
            return GetInputChannelDown(InputChannel.Trigger);
        }

        /// <summary>
        /// Check if the trigger was released this frame
        /// </summary>
        /// <returns>true during the frame that the trigger is released</returns>
        public bool GetTriggerUp()
        {
            return GetInputChannelUp(InputChannel.Trigger);
        }

        /// <summary>
        /// Trigger a haptic plus for specified duration
        /// </summary>
        public virtual void TriggerHapticPulse(ushort durationMicroSeconds = 2500) { }

        /// <summary>
        /// Specify which HolodeckCameraRig this controller should be associated with
        /// </summary>
        /// <param name="hcr"></param>
        public virtual void SetTargetRig(HolodeckCameraRig hcr)
        {
            if (hcr == null)
            {
                Debug.LogErrorFormat("[Controller] Attempting to set an invalid rig for controller {0}", _controllerId);
                return;
            }

            SetRigId(hcr.RigId);
        }

        /// <summary>
        /// Specify which HolodeckCameraRig this controller should be associated with by id
        /// </summary>
        /// <param name="id"></param>
        public virtual void SetRigId(int id)
        {
            if (id < 0 || id > HolodeckInitializer.Instance.HolodeckCameraRigs.Length)
            {
                Debug.LogError("[Controller] Unable to register controller to rig " + id);
                return;
            }

            _registeredRig = HolodeckInitializer.Instance.HolodeckCameraRigs[id];
            if (_registeredRig == null)
            {
                Debug.LogError("[Controller] Rig not registered to id " + id);
                return;
            }

            _rigId = id;

            if (_referenceTransform == null)
            {
                _referenceTransform = new GameObject(GetDescriptiveName() + "_ReferenceTransform");
            }
            _referenceTransform.transform.parent = _registeredRig.ControllerReference.transform;
        }
    }
}