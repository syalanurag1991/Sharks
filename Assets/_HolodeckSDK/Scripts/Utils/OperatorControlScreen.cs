//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Vulcan;

public class OperatorControlScreen : MonoBehaviour
{
    private ControllerManager _controllerManager;
    private SettingsManager _settingsManager;
    private WarpDisplayRig _warpDisplayRig;
    private Camera _targetCamera;

    private Rect _buttonRect = new Rect(25, 50, 250, 100);
    private Vector2 _labelSize = new Vector2(350, 100);

    public WarpDisplayRig WarpDisplayRig
    {
        get { return _warpDisplayRig; }
        set { _warpDisplayRig = value; }
    }

    public Camera TargetCamera
    {
        get { return _targetCamera; }
        set { _targetCamera = value; }
    }

    IEnumerator Start()
    {
        while (ControllerManager.Instance == null)
        {
            yield return null;
        }

        _controllerManager = ControllerManager.Instance;
        _settingsManager = SettingsManager.Instance;
    }

    void OnGUI()
    {
        GUI.skin.textArea.fontSize = 20;

        DrawButtons();
        DrawWarpInformation();
    }

    void DrawButtons()
    {
        bool buttonEnabled;
        string calibrationButtonMessage;
        ControllerCalibrationState state = _controllerManager == null ? ControllerCalibrationState.NOT_RUNNING : _controllerManager.GetCalibrationState();
        if (state == ControllerCalibrationState.NOT_RUNNING || state == ControllerCalibrationState.WAITING_FOR_START_COMMAND)
        {
            buttonEnabled = true;
            calibrationButtonMessage = "Start Controller Calibration";
        }
        else if (state == ControllerCalibrationState.RUNNING)
        {
            buttonEnabled = false;
            calibrationButtonMessage = "Waiting For Controller Selection";
        }
        else if (state == ControllerCalibrationState.WAITING_FOR_CONFIRMATION_COMMAND)
        {
            buttonEnabled = true;
            calibrationButtonMessage = "Finish Controller Calibration";
        }
        else
        {
            buttonEnabled = false;
            calibrationButtonMessage = "Unknown State {0}:" + state.ToString();
        }

        bool cachedGUIEnabledState = GUI.enabled;
        GUI.enabled = buttonEnabled;

        if (GUI.Button(_buttonRect, calibrationButtonMessage))
        {
            _controllerManager.SendCalibratonCommand();
        }

        GUI.enabled = cachedGUIEnabledState;
        Rect newRect = _buttonRect;
        newRect.position += new Vector2(0.0f, _buttonRect.height + 10);

        if (GUI.Button(newRect, "ShutDown"))
        {
            Application.Quit();
        }
    }

    void DrawWarpInformation()
    {
        List<Camera> displays = _warpDisplayRig.DisplayCameras;
        Vector3 screenLocation;
        Rect labelRect = new Rect(Vector2.zero, _labelSize);

        for (int i = 0; i < displays.Count; ++i)
        {
            screenLocation = TargetCamera.WorldToScreenPoint(displays[i].transform.position);
            screenLocation.y = Screen.height - screenLocation.y;

            labelRect.center = screenLocation;

            int displayMapping = IntToDisplayProperty(i);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " + displays[i].name);
            sb.AppendLine("Target Display: " + (displayMapping + 1));

            if (Display.displays.Length > displayMapping && displayMapping > -1)
            {
                Display d = Display.displays[displayMapping];
                sb.AppendLine("Resolution / Refresh: " + d.renderingWidth + " x " + d.renderingHeight + " / " + Constants.DISPLAY_REFRESH_RATE);
            }

            GUI.TextArea(labelRect, sb.ToString());
        }
    }

    int IntToDisplayProperty(int display)
    {
        int ret = -1;
        if (_settingsManager == null)
        {
            return ret;
        }

        switch (display)
        {
            case 0:
                ret = _settingsManager.settings.DisplaySettings.warpCameraToDisplayMap.warpMap0TargetDisplay;
                break;

            case 1:
                ret = _settingsManager.settings.DisplaySettings.warpCameraToDisplayMap.warpMap1TargetDisplay;
                break;

            case 2:
                ret = _settingsManager.settings.DisplaySettings.warpCameraToDisplayMap.warpMap2TargetDisplay;
                break;

            case 3:
                ret = _settingsManager.settings.DisplaySettings.warpCameraToDisplayMap.warpMap3TargetDisplay;
                break;

        }

        return ret;
    }
}
