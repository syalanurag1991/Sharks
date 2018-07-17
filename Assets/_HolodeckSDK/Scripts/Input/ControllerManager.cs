//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Vulcan
{
    public delegate void OnControllerActivated(Controller controller);
    public delegate void OnControllerDeactivated(Controller controller);

    public enum ControllerCalibrationState
    {
        NOT_RUNNING = 0,
        WAITING_FOR_START_COMMAND = 1,
        WAITING_FOR_CONFIRMATION_COMMAND = 2,
        SAVING = 3,
        RUNNING = 4
    }

    public class ControllerProjection
    {
        public RaycastHit HolodeckHit;
        public RaycastHit[] RaycastHits;
        public Ray Ray;

        // used as an out and ref property
        public List<GameObject> CanvasOverObjects;
    }

    [Serializable]
    public class CalibrationData
    {
        public Vector3 _calibrationPositionVector3;
        public Quaternion _calibrationRotationQuat;
    }

    public class ControllerManager : MonoSingleton<ControllerManager>
    {
        protected List<Controller> _controllers;

        protected ControllerCalibrationState _calibrationState = ControllerCalibrationState.NOT_RUNNING;

        protected CalibrationData _calibrationData;
        protected GameObject _calibrationParent;

        protected bool _isCalibrated = false;
        public bool IsCalibrated { get { return _isCalibrated; } }

        protected override void Awake()
        {
            base.Awake();

            _controllers = new List<Controller>();

            _calibrationData = LoadCalibrationData();
            if (_calibrationData != null)
            {
                if (_calibrationParent == null)
                {
                    _calibrationParent = new GameObject("CalibrationParent");
                    _calibrationParent.transform.parent = transform;
                }

                _calibrationParent.transform.position = _calibrationData._calibrationPositionVector3;
                _calibrationParent.transform.rotation = _calibrationData._calibrationRotationQuat;

                _isCalibrated = true;

                Debug.LogFormat("[ControllerManager] ControllerManager has been calibrated from a data file.  Recalibrate if controllers seem wrong");
            }
        }

        private CalibrationData LoadCalibrationData()
        {
            CalibrationData data;
            if (File.Exists(Constants.GLOBAL_CALIBRATION_FILE))
            {
                string path;
                string fileContents;
                try
                {
                    string overridePath = SettingsManager.Instance.settings.HolodeckSettings.steamVRControllerCalibrationFileOverridePath;
                    if (!string.IsNullOrEmpty(overridePath))
                    {
                        path = overridePath;
                        Debug.LogFormat("[ControllerManager] Attempting to load controller calibration data from override file: {0}", overridePath);
                    }
                    else
                    {
                        path = Constants.GLOBAL_CALIBRATION_FILE;
                        Debug.LogFormat("[ControllerManager] Attempting to load controller calibration data from global file: {0}", Constants.GLOBAL_CALIBRATION_FILE);
                    }
                    fileContents = File.ReadAllText(path);
                }
                catch (Exception e)
                {
                    fileContents = null;
                    Debug.LogFormat("[ControllerManager] Failed to read file. Controllers are currently uncalibrated. Error: {0}", e.Message);
                }

                if (fileContents != null)
                {
                    try
                    {
                        data = JsonUtility.FromJson<CalibrationData>(fileContents);
                    }
                    catch (Exception e)
                    {
                        data = null;
                        Debug.LogFormat("[ControllerManager] Failed to read file {0}. Controllers are currently uncalibrated.  Error: {1}", Constants.GLOBAL_CALIBRATION_FILE, e.Message);
                    }
                }
                else
                {
                    // The error would been printed above.
                    data = null;
                }
            }
            else
            {
                data = null;
                Debug.LogFormat("[ControllerManager] {0} file does not exist or app doesn't have permission to open in.  Controllers are currently uncalibrated", Constants.GLOBAL_CALIBRATION_FILE);
            }
            return data;
        }

        /// <summary>
        /// Save the calibration based on the specified controller index
        /// </summary>
        /// <param name="referenceControllerIdx"></param>
        public void Calibrate(int referenceControllerIdx)
        {
            Debug.LogFormat("[ControllerManager] Calibrating");

            _calibrationParent.transform.position = SteamVR_Controller.Input(referenceControllerIdx).transform.pos;
            _calibrationParent.transform.rotation = SteamVR_Controller.Input(referenceControllerIdx).transform.rot;

            StoreCalibrationData();
        }

        private void StoreCalibrationData()
        {
            if (_calibrationParent != null)
            {
                CalibrationData calData = new CalibrationData();
                calData._calibrationPositionVector3 = _calibrationParent.transform.position;
                calData._calibrationRotationQuat = _calibrationParent.transform.rotation;

                try
                {
                    string json = JsonUtility.ToJson(calData);

                    string overridePath = SettingsManager.Instance.settings.HolodeckSettings.steamVRControllerCalibrationFileOverridePath;
                    if (!string.IsNullOrEmpty(json))
                    {
                        string path;
                        if (!string.IsNullOrEmpty(overridePath))
                        {
                            path = overridePath;
                            Debug.LogFormat("[ControllerManager] Attempting to store calibration data to override file: {0}", overridePath);
                        }
                        else
                        {
                            path = Constants.GLOBAL_CALIBRATION_FILE;
                            Debug.LogFormat("[ControllerManager] Attempting to store calibration data to global file: {0}", Constants.GLOBAL_CALIBRATION_FILE);
                        }
                        File.WriteAllText(path, json);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("[ControllerManager] Failed to persist calibration data to json with error {0}", e.Message);
                }
            }
            else
            {
                Debug.LogErrorFormat("[ControllerManager] _calibrationParent object does not exist, controllers are probably not calibrated. NOT writing Calibration data to file");
            }
        }

        /// <summary>
        /// Send the current calibration command
        /// </summary>
        public void SendCalibratonCommand()
        {
            if (_calibrationState == ControllerCalibrationState.NOT_RUNNING)
            {
                StartCoroutine(CalibrationRoutine());

                _calibrationState = ControllerCalibrationState.RUNNING;
            }

            if (_calibrationState == ControllerCalibrationState.WAITING_FOR_CONFIRMATION_COMMAND)
            {
                _calibrationState = ControllerCalibrationState.SAVING;
            }
        }

        private IEnumerator CalibrationRoutine()
        {
            while (true)
            {
                _calibrationState = ControllerCalibrationState.WAITING_FOR_START_COMMAND;
                while (_calibrationState != ControllerCalibrationState.RUNNING)
                {
                    yield return null;
                }
                yield return null;

                HolodeckCameraRig[] rigs = HolodeckInitializer.Instance.HolodeckCameraRigs;
                HolodeckCameraRig rig;
                for (int i = 0; i < rigs.Length; ++i)
                {
                    rig = rigs[i];
                    rig.ShowCalibrationTarget();
                }

                if (_controllers != null && _controllers.Count > 0)
                {
                    int controllerIdx = -1;
                    while (controllerIdx == -1)
                    {
                        for (int i = 0; i < _controllers.Count; i++)
                        {
                            if (_controllers[i].GetTrigger())
                            {
                                controllerIdx = _controllers[i].ControllerId;
                                break;
                            }
                            yield return null;
                        }
                    }

                    // Only calibrate against the first registered rig;
                    rig = rigs[0];
                    rig.SetCalibrationTargetReady();

                    _calibrationState = ControllerCalibrationState.WAITING_FOR_CONFIRMATION_COMMAND;
                    while (_calibrationState == ControllerCalibrationState.WAITING_FOR_CONFIRMATION_COMMAND)
                    {
                        yield return null;
                    }
                    yield return null;

                    Calibrate(controllerIdx);

                    for (int i = 0; i < rigs.Length; ++i)
                    {
                        rig = rigs[i];
                        rig.HideCalibrationTarget();
                    }
                    yield return null;
                }
                else
                {
                    Debug.LogErrorFormat("[ControllerManager] Calibration Routine activated but no controllers are present");
                }

                yield return null;

                _calibrationState = ControllerCalibrationState.NOT_RUNNING;
            }
        }

        /// <summary>
        /// Get the number of currently registered controllers
        /// </summary>
        /// <returns></returns>
        public virtual int GetNumControllers()
        {
            return _controllers != null ? _controllers.Count : 0;
        }

        /// <summary>
        /// Get the current calibration state
        /// </summary>
        /// <returns></returns>
        public virtual ControllerCalibrationState GetCalibrationState()
        {
            return _calibrationState;
        }

        protected virtual void Update()
        {
            if (_controllers != null)
            {
                for (int i = 0; i < _controllers.Count; i++)
                {
                    _controllers[i].Update();
                }
            }
        }

        protected virtual void LateUpdate()
        {
            if (_controllers != null)
            {
                for (int i = 0; i < _controllers.Count; i++)
                {
                    _controllers[i].LateUpdate();
                }
            }
        }

        protected virtual void Destroy()
        {
            if (_controllers != null && _controllers.Count > 0)
            {
                foreach (Controller c in _controllers)
                {
                    c.Destroy();
                }

                _controllers.Clear();
                _controllers = null;
            }
        }

        /// <summary>
        /// Request a Controller object by id
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual Controller GetController(int index)
        {
            Controller controller;
            if (index > -1)
            {
                if (_controllers != null && _controllers.Count > 0)
                {
                    if (index < _controllers.Count)
                    {
                        controller = _controllers[index];
                    }
                    else
                    {
                        controller = null;
                        Debug.LogErrorFormat("[ControllerManager] index {0} is out of randge of active controllers", index);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("[ControllerManager] Controller List is Empty");
                    controller = null;
                }
            }
            else
            {
                Debug.LogErrorFormat("[ControllerManager] Invalid Index Paramerter {0} passed to GetController", index);
                controller = null;
            }
            return controller;
        }

        /// <summary>
        /// Method used to initialize systems related to a specific ControllerManager
        /// </summary>
        public virtual void Initialize()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Method used to register for events related to a specific ControllerManager
        /// </summary>
        /// <param name="onControllerActivated"></param>
        /// <param name="onControllerDeactivated"></param>
        public virtual void RegisterForControllerNotifications(OnControllerActivated onControllerActivated,
            OnControllerDeactivated onControllerDeactivated)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Method used to dereigster for events related to a specific ControllerManager
        /// </summary>
        /// <param name="onControllerActivated"></param>
        /// <param name="onControllerDeactivated"></param>
        public virtual void DeregisterForControllerNotifications(OnControllerActivated onControllerActivated,
            OnControllerDeactivated onControllerDeactivated)
        {
            throw new System.NotImplementedException();
        }
    }
}