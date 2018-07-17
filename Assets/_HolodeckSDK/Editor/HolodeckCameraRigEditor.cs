//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEditor;
using UnityEngine;

namespace Vulcan
{
    [CustomEditor(typeof(HolodeckCameraRig))]
    public class HolodeckCameraRigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            HolodeckCameraRig holodeckCameraRig = (HolodeckCameraRig)target;

            GUILayout.BeginVertical("box");
            {
                if (Application.isPlaying)
                {
                    GUILayout.Label(string.Format("HolodeckCameraRig registered with id of {0}", holodeckCameraRig.RigId));
                    GUILayout.Label(string.Format("HolodeckCameraRig targeting layer of {0}", holodeckCameraRig.RigLayer));
                }
                else
                {
                    GUILayout.Label("HolodeckCameraRig registration occurs runtime.");
                }
            }
            GUILayout.EndVertical();
        }
    }
}