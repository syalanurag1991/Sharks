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
    public interface IHolodeckSettings
    {
        HolodeckSettings HolodeckSettings { get; }
    }

    [Serializable]
    public class HolodeckSettings
    {
        public string steamVRControllerCalibrationFileOverridePath = "";
        public bool useSimulator = false;
    }
}