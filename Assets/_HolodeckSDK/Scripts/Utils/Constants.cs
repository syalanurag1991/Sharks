//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;

namespace Vulcan
{
    public class Constants
    {
        public const string WARP_RENDER_LAYER = "VulcanReservedWarpDisplay";
        public const string COLLIDER_LAYER = "VulcanDomeCollider";
        public const string SIMULATOR_LAYER = "VulcanSimulator";
        public const float ASPECT_RATIO = 1.8962963f;
        public const int DISPLAY_WIDTH = 4096;
        public const int DISPLAY_HEIGHT = 2160;
        public const int DISPLAY_REFRESH_RATE = 60;
        public const int BASE_RT_MULTIPLIER = 8;
        public const string WARP_CAMERA_PREFIX = "WarpDisplayCamera_";
        
        public const string PANORAMA_ASSET_PATH = "Panorama/";
        public const string UNWRAPPED_RIG = "SimpleUnwrappedRig";
        public const string PANORAMA_RIG_NAME = "SimplePanoramaRig";
        public const string SIMULATOR_ASSET = "Simulator/HolodeckSimulator";
        
        public const string GLOBAL_CALIBRATION_FILE = @"c:\Vulcan\SteamVRControllerCalibration.json";
        public const string GLOBAL_MAPS_PATH = @"c:\Vulcan\GlobalMaps\";

        public static int GetResolutionFromQuality(int quality)
        {
            return Mathf.Clamp(BASE_RT_MULTIPLIER * (1 << quality), 1, 8192);
        }
    }
}