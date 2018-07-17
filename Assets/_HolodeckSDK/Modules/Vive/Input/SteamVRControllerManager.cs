//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Vulcan
{
    public class SteamVRControllerManager : ControllerManager
    {
        private OnControllerActivated _onControllerActivated;
        private OnControllerDeactivated _onControllerDeactivated;

        private List<int> _activatedTrackedControllerIndices;
        private CVRSystem _system;

        public override void Initialize()
        {
            _system = OpenVR.System;
            if (_system == null)
            {
                Debug.LogErrorFormat("[SteamVRControllerManager] OpenVR System is not initialize");
                return;
            }

            QueueActivatedTrackedControllerNotifications();

            SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);

            Debug.Log("[SteamVRControllerManager] Initialized");
        }

        public override void RegisterForControllerNotifications(OnControllerActivated onControllerActivated,
            OnControllerDeactivated onControllerDeactivated)
        {
            if (onControllerActivated != null)
            {
                _onControllerActivated += onControllerActivated;
            }
            else
            {
                Debug.LogErrorFormat("[SteamVRControllerManager] Unable to register null delegate for controller notifications.", this);
            }

            if (onControllerDeactivated != null)
            {
                _onControllerDeactivated += onControllerDeactivated;
            }
            else
            {
                Debug.LogErrorFormat("[SteamVRControllerManager] Unable to register null delegate for controller notifications.", this);
            }
        }

        public override void DeregisterForControllerNotifications(OnControllerActivated onControllerActivated,
            OnControllerDeactivated onControllerDeactivated)
        {
            if (onControllerActivated != null)
            {
                _onControllerActivated -= onControllerActivated;
            }
            else
            {
                Debug.LogErrorFormat("[SteamVRControllerManager] Unable to deregister null delegate for controller notifications.", this);
            }

            if (onControllerDeactivated != null)
            {
                _onControllerDeactivated -= onControllerDeactivated;
            }
            else
            {
                Debug.LogErrorFormat("[SteamVRControllerManager] Unable to deregister null delegate for controller notifications.", this);
            }
        }

        private void OnDeviceConnected(int index, bool connected)
        {
            if (_system != null)
            {
                ETrackedDeviceClass deviceClass = _system.GetTrackedDeviceClass((uint)index);

                // TODO: Modify this to keep lists of other device types
                if (deviceClass == ETrackedDeviceClass.Controller || deviceClass == ETrackedDeviceClass.GenericTracker)
                {
                    PrintControllerStatus(index);
                    if (connected)
                    {
                        bool alreadyTracked = false;
                        for (int i = 0; i < _controllers.Count; i++)
                        {
                            if (_controllers[i].ControllerId == index)
                            {
                                alreadyTracked = true;
                                break;
                            }
                        }

                        if (!alreadyTracked)
                        {
                            Debug.Log(string.Format("[ControllerInput] OnTrackingAcquired {0}: ", index));
                            PrintControllerStatus(index);

                            bool isTracker = (deviceClass == ETrackedDeviceClass.GenericTracker);
                            SteamVRController c = SteamVRController.Create(index, _calibrationParent, isTracker);
                            _controllers.Add(c);

                            _onControllerActivated?.Invoke(c);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("ControllerInput] OnTrackingLost {0}: ", index);
                        PrintControllerStatus(index);
                        if (_controllers != null)
                        {
                            for (int i = 0; i < _controllers.Count; i++)
                            {
                                SteamVRController c = (SteamVRController)_controllers[i];
                                if (c.ControllerId == index)
                                {
                                    _onControllerDeactivated?.Invoke(c);
                                    _controllers.Remove(c);

                                    c.Destroy();

                                    // Break immediately since we are changing the list size
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("[SteamVRControllerManager] OpenVR system not initialized");
            }
        }

        private void QueueActivatedTrackedControllerNotifications()
        {
            uint[] controllerIndices = new uint[8];
            uint[] trackerIndices = new uint[8];
            uint controllerCount = _system.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.Controller, controllerIndices, 0);
            uint trackerCount = _system.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.GenericTracker, trackerIndices, 0);
            uint total = controllerCount + trackerCount;

            if (total > 0)
            {
                if (_activatedTrackedControllerIndices == null)
                {
                    _activatedTrackedControllerIndices = new List<int>();
                }

                for (int i = 0; i < controllerCount; i++)
                {
                    _activatedTrackedControllerIndices.Add((int)controllerIndices[i]);
                }

                for (int i = 0; i < trackerCount; i++)
                {
                    _activatedTrackedControllerIndices.Add((int)trackerIndices[i]);
                }
            }
        }

        private void SendActivatedTrackedControllerNotifications()
        {
            if (_activatedTrackedControllerIndices != null)
            {
                for (int i = _activatedTrackedControllerIndices.Count - 1; i > -1; i--)
                {
                    OnDeviceConnected(_activatedTrackedControllerIndices[i], true);
                    _activatedTrackedControllerIndices.RemoveAt(i);
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            SendActivatedTrackedControllerNotifications();
        }

        protected override void Destroy()
        {
            base.Destroy();

            SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
        }

        private void PrintControllerStatus(int index)
        {
            var device = SteamVR_Controller.Input(index);
            string statusMessage = string.Format("Controller Status:\nIndex: {0} \nConnected: {1} \nHasTracking: {2} \nOutOfRange:{3} \nCalibrating:{4} \nUninitialized: {5}",
                                                       device.index,
                                                       device.connected,
                                                       device.hasTracking,
                                                       device.outOfRange,
                                                       device.calibrating,
                                                       device.uninitialized);

            Debug.LogFormat("[SteamVRControllerManager] {0}", statusMessage);
        }
    }
}
