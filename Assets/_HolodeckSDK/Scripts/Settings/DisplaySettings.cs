//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System;

namespace Vulcan
{
    /// <summary>
    /// Interface for the Settings class to implement to ensure that any class relying on this
    /// Settings class can get it.
    /// </summary>
    public interface IDisplaySettings
    {
        DisplaySettings DisplaySettings { get; }
    }

    [Serializable]
    public class WarpCameraToDisplayMap
    {
        public int warpMap0TargetDisplay = 2;
        public int warpMap1TargetDisplay = 3;
        public int warpMap2TargetDisplay = 4;
        public int warpMap3TargetDisplay = 5;
    }

    [Serializable]
    public class DisplaySettings
    {
        public WarpCameraToDisplayMap warpCameraToDisplayMap;
        public float cameraSetupDelay;
        public string mapOverridePath;
        public float verticalMasking;
    }
}