//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using System;

namespace Vulcan
{
    public class DisplayScript : MonoSingleton<DisplayScript>
    {
        [HideInInspector] public DisplaySettings settings;

        private bool _setupCompleted = false;
        public bool SetupCompleted
        {
            get { return _setupCompleted; }
        }

        private bool _displayZeroUsedForRendering = false;
        public bool DisplayZeroUsedForRendering
        {
            get { return _displayZeroUsedForRendering; }
        }

        protected override void Awake()
        {
            base.Awake();

            // This script should only be called once at the start of the application.
            // If it has already been activated as indicated by the static variable _setupCompleted,
            // then just skip the camera setup.
            if (!_setupCompleted)
            {
                // Get the display settings from the SettingsManager singleton.
                IDisplaySettings ds = SettingsManager.Instance.settings;
                settings = ds.DisplaySettings;

#if !UNITY_EDITOR
                if (!SettingsManager.Instance.settings.HolodeckSettings.useSimulator)
#endif
                {
                    // Activate all the configured and available displays.
                    ActivateDisplays();
                }

                _setupCompleted = true;
                Reset();
            }
        }

        private bool ValidateWarpCamToDisplayMap()
        {
            bool valid = true;

#if !UNITY_EDITOR
            if (!SettingsManager.Instance.settings.HolodeckSettings.useSimulator)
            {
                if ( Display.displays.Length < 4 )
                {
                    valid = false;
                    Debug.LogErrorFormat(this, "[DisplayScript] The attached Display Adapter doesn't have enough displays, at least 4 are required. Only {0} detected", Display.displays.Length);
                }

                int highestDisplayIndexCount = Display.displays.Length - 1;
                if (settings.warpCameraToDisplayMap.warpMap0TargetDisplay < 0 || settings.warpCameraToDisplayMap.warpMap0TargetDisplay > highestDisplayIndexCount )
                {
                    valid = false;
                    Debug.LogErrorFormat(this, "[DisplayScript] warpMap0TargetDisplay value {0} is out of range.  Max is {1}", settings.warpCameraToDisplayMap.warpMap0TargetDisplay, highestDisplayIndexCount);
                }

                if (settings.warpCameraToDisplayMap.warpMap1TargetDisplay < 0 || settings.warpCameraToDisplayMap.warpMap1TargetDisplay > highestDisplayIndexCount)
                {
                    valid = false;
                    Debug.LogErrorFormat(this, "[DisplayScript] warpMap1TargetDisplay value {0} is out of range.  Max is {1}", settings.warpCameraToDisplayMap.warpMap1TargetDisplay, highestDisplayIndexCount);
                }

                if (settings.warpCameraToDisplayMap.warpMap2TargetDisplay < 0 || settings.warpCameraToDisplayMap.warpMap2TargetDisplay > highestDisplayIndexCount)
                {
                    valid = false;
                    Debug.LogErrorFormat(this, "[DisplayScript] warpMap2TargetDisplay value {0} is out of range.  Max is {1}", settings.warpCameraToDisplayMap.warpMap2TargetDisplay, highestDisplayIndexCount);
                }

                if (settings.warpCameraToDisplayMap.warpMap3TargetDisplay < 0 || settings.warpCameraToDisplayMap.warpMap3TargetDisplay > highestDisplayIndexCount)
                {
                    valid = false;
                    Debug.LogErrorFormat(this, "[DisplayScript] warpMap3TargetDisplay value {0} is out of range.  Max is {1}", settings.warpCameraToDisplayMap.warpMap3TargetDisplay, highestDisplayIndexCount);
                }
            }
#endif
            return valid;
        }

        /// <summary>
        /// Activate all the configured and available displays.
        /// </summary>
        void ActivateDisplays()
        {
            if (!ValidateWarpCamToDisplayMap())
            {
                Debug.LogAssertionFormat(this, "[DisplayScript] WarpCameraToDisplayScript failed validation, incorrect rendering and/or exceptions likely incoming");
            }
            // Display.displays[0] is the primary, default display and is always ON.
            // If a warp map is assigned to it we enable 3 others, if not we enable 4 others and it is left blank

            if (Display.displays.Length > 4)
            {
                if (settings.warpCameraToDisplayMap.warpMap0TargetDisplay != 0 &&
                    settings.warpCameraToDisplayMap.warpMap1TargetDisplay != 0 &&
                    settings.warpCameraToDisplayMap.warpMap2TargetDisplay != 0 &&
                    settings.warpCameraToDisplayMap.warpMap3TargetDisplay != 0)
                {
                    // No  warp camera is mapped to display zero
                    // Enable it so it won't get enabled again later
                    Display.displays[0].Activate(4096, 2160, 60);
                    _displayZeroUsedForRendering = false;
                }
                else
                {
                    _displayZeroUsedForRendering = true;
                }
            }
            else
            {
                //
                // There are only 4 displays or fewer on this box, therefore display 0 must be used for rendering
                //
                _displayZeroUsedForRendering = true;
            }

            if (settings.warpCameraToDisplayMap.warpMap0TargetDisplay >= 0 && settings.warpCameraToDisplayMap.warpMap0TargetDisplay < Display.displays.Length)
            {
                Display.displays[settings.warpCameraToDisplayMap.warpMap0TargetDisplay].Activate(Constants.DISPLAY_WIDTH, Constants.DISPLAY_HEIGHT, Constants.DISPLAY_REFRESH_RATE);
            }
            else
            {
                //
                // In editor this will always fail because there is only ever 1 display, so don't spam the console in editor
                //
#if !UNITY_EDITOR
                Debug.LogErrorFormat(this, "[DisplayScript] warpMap0TargetDisplay is {0}, not a activating to prevent out of range exception", settings.warpCameraToDisplayMap.warpMap0TargetDisplay);
#endif
            }

            if (settings.warpCameraToDisplayMap.warpMap1TargetDisplay >= 0 && settings.warpCameraToDisplayMap.warpMap1TargetDisplay < Display.displays.Length)
            {
                Display.displays[settings.warpCameraToDisplayMap.warpMap1TargetDisplay].Activate(Constants.DISPLAY_WIDTH, Constants.DISPLAY_HEIGHT, Constants.DISPLAY_REFRESH_RATE);
            }
            else
            {
                //
                // In editor this will always fail because there is only ever 1 display, so don't spam the console in editor
                //
#if !UNITY_EDITOR
                Debug.LogErrorFormat(this, "[DisplayScript] warpMap1TargetDisplay is {0}, not a activating to prevent out of range exception", settings.warpCameraToDisplayMap.warpMap1TargetDisplay);
#endif
            }

            if (settings.warpCameraToDisplayMap.warpMap2TargetDisplay >= 0 && settings.warpCameraToDisplayMap.warpMap2TargetDisplay < Display.displays.Length)
            {
                Display.displays[settings.warpCameraToDisplayMap.warpMap2TargetDisplay].Activate(Constants.DISPLAY_WIDTH, Constants.DISPLAY_HEIGHT, Constants.DISPLAY_REFRESH_RATE);
            }
            else
            {
                //
                // In editor this will always fail because there is only ever 1 display, so don't spam the console in editor
                //
#if !UNITY_EDITOR
                Debug.LogErrorFormat(this, "[DisplayScript] warpMap2TargetDisplay is {0}, not a activating to prevent out of range exception", settings.warpCameraToDisplayMap.warpMap2TargetDisplay);
#endif
            }

            if (settings.warpCameraToDisplayMap.warpMap3TargetDisplay >= 0 && settings.warpCameraToDisplayMap.warpMap3TargetDisplay < Display.displays.Length)
            {
                Display.displays[settings.warpCameraToDisplayMap.warpMap3TargetDisplay].Activate(Constants.DISPLAY_WIDTH, Constants.DISPLAY_HEIGHT, Constants.DISPLAY_REFRESH_RATE);
            }
            else
            {
                //
                // In editor this will always fail because there is only ever 1 display, so don't spam the console in editor
                //
#if !UNITY_EDITOR
                Debug.LogErrorFormat(this, "[DisplayScript] warpMap3TargetDisplay is {0}, not a activating to prevent out of range exception", settings.warpCameraToDisplayMap.warpMap3TargetDisplay);
#endif
            }
        }

        /// <summary>
        /// Setup the appropriate cameras for the chopped up equirectangular panorama view.
        /// </summary>
        void SetupCameras()
        {
            int count = Camera.allCameras.Length;
            if (settings != null)
            {
                foreach (Camera cam in Camera.allCameras)
                {
                    if (String.Equals(cam.name, Constants.WARP_CAMERA_PREFIX + "0", StringComparison.Ordinal))
                    {
                        cam.targetDisplay = settings.warpCameraToDisplayMap.warpMap0TargetDisplay;
                    }
                    else if (String.Equals(cam.name, Constants.WARP_CAMERA_PREFIX + "1", StringComparison.Ordinal))
                    {
                        cam.targetDisplay = settings.warpCameraToDisplayMap.warpMap1TargetDisplay;
                    }
                    else if (String.Equals(cam.name, Constants.WARP_CAMERA_PREFIX + "2", StringComparison.Ordinal))
                    {
                        cam.targetDisplay = settings.warpCameraToDisplayMap.warpMap2TargetDisplay;
                    }
                    else if (String.Equals(cam.name, Constants.WARP_CAMERA_PREFIX + "3", StringComparison.Ordinal))
                    {
                        cam.targetDisplay = settings.warpCameraToDisplayMap.warpMap3TargetDisplay;
                    }
                    else
                    {
                        if (cam.name.Contains(Constants.WARP_CAMERA_PREFIX))
                        {
                            Debug.LogErrorFormat(this, "[DisplayScript] Found WarpCam {0} that is out of known range", cam.name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method to reset the camera setup based on configured time delay.
        /// </summary>
        public void Reset()
        {
            if (settings != null)
            {
                Invoke("SetupCameras", settings.cameraSetupDelay);
            }
        }
    }
}