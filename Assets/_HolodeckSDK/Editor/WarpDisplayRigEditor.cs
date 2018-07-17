//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vulcan
{
    [CustomEditor(typeof(WarpDisplayRig))]
    public class WarpDisplayRigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WarpDisplayRig rig = target as WarpDisplayRig;

            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Clear PersistentData Maps"))
            {
                EXRLoader.ClearCachedEXRs();
            }
            UpdateDragAndDrop(rig);
        }

        private void UpdateDragAndDrop(WarpDisplayRig rig)
        {
            Color cachedColor = GUI.contentColor;
            Event currentEvent = Event.current;
            Rect exrDropArea = GUILayoutUtility.GetRect(0.0f, 80.0f, GUILayout.ExpandWidth(true));
            Rect blendDropArea = GUILayoutUtility.GetRect(0.0f, 80.0f, GUILayout.ExpandWidth(true));
            Rect textArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(exrDropArea, "Drop EXR Maps Here");
            GUI.Box(blendDropArea, "Drop Blend Maps Here");

            int exrCount = rig.GetEXRMapCount();
            if (exrCount > 0)
            {
                if ( rig.GetBlendMapCount() != exrCount )
                {
                    GUI.contentColor = Color.red;
                    GUI.Label(textArea, "EXP Map count must match Blend Map Count");
                    GUI.contentColor = cachedColor;
                }
            }
            else
            {
                GUI.contentColor = Color.red;
                GUI.Label(textArea, "Please add EXR Maps");
                GUI.contentColor = cachedColor;
            }
            
            if ( currentEvent.type == EventType.DragUpdated )
            {
                // Check for exr maps
                if (exrDropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }

                if (blendDropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
            }
            else if (currentEvent.type == EventType.DragPerform) 
            {
                // in the box?
                if (exrDropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.AcceptDrag();

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        string current = DragAndDrop.paths[i];
                        if (!string.IsNullOrEmpty(current))
                        {
                            string file = Path.GetFileNameWithoutExtension(current);
                            rig.AddExrFileName(file, WarpDisplayRig.FileType.EXR);
                        }
                    }
                }

                if (blendDropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.AcceptDrag();

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        string current = DragAndDrop.paths[i];
                        if (!string.IsNullOrEmpty(current))
                        {
                            string file = Path.GetFileName(current);
                            rig.AddExrFileName(file, WarpDisplayRig.FileType.BLEND);
                        }
                    }
                }

                // Eliminate nulls that may be in the list
                rig.CleanupFileNameList(WarpDisplayRig.FileType.EXR);
                rig.CleanupFileNameList(WarpDisplayRig.FileType.BLEND);
            }
        }
    }
}
