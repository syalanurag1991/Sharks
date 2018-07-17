//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vulcan
{
    [DisallowMultipleComponent]
    public class VulcanCanvasRaycaster : MonoBehaviour
    {
        private static readonly List<VulcanCanvasRaycastTarget> _canvases = new List<VulcanCanvasRaycastTarget>();

        private static Camera _raycastCamera;

        public static void AddTarget(VulcanCanvasRaycastTarget obj) { if (_canvases.Contains(obj)) return; _canvases.Add(obj); }

        public static void RemoveTarget(VulcanCanvasRaycastTarget obj) { if (!_canvases.Contains(obj)) return; _canvases.Remove(obj); }

        public static Vector2 ScreenCenterPoint { get { return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f); } }

        public Camera EventCamera
        {
            get
            {
                if (_raycastCamera == null)
                {
                    var go = new GameObject(name + "EventCamera");
                    go.SetActive(false);

                    EventSystem es = null;
                    if (HolodeckInitializer.Instance.EventSystem != null)
                    {
                        es = HolodeckInitializer.Instance.EventSystem.GetComponent<EventSystem>();
                    }

                    if (es == null)
                    {
                        es = EventSystem.current;
                    }

                    go.transform.SetParent(es.transform, false);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    _raycastCamera = go.AddComponent<Camera>();
                    _raycastCamera.clearFlags = CameraClearFlags.Nothing;
                    _raycastCamera.cullingMask = 0;
                    _raycastCamera.orthographic = true;
                    _raycastCamera.orthographicSize = 1;
                    _raycastCamera.useOcclusionCulling = false;
                    _raycastCamera.stereoTargetEye = StereoTargetEyeMask.None;
                }

                return _raycastCamera;
            }
        }

        public void Raycast(Ray ray, ref List<GameObject> overGOs, float distance = Mathf.Infinity)
        {
            // align the event camera
            EventCamera.transform.position = ray.origin;

            // check to ensure we are not being given a bunk ray, can happen if controllers deregister but game code still passes in ray
            if (ray.direction != Vector3.zero)
            {
                EventCamera.transform.rotation = Quaternion.LookRotation(ray.direction, transform.up);
            }

            for (int i = _canvases.Count - 1; i >= 0; --i)
            {
                var target = _canvases[i];
                if (target == null || !target.enabled) { continue; }
                overGOs.AddRange(Raycast(target.Canvas, target.IgnoreReversedGraphics, ray, EventCamera, distance));
            }
        }

        public static List<GameObject> Raycast(Canvas canvas, bool ignoreReversedGraphics, Ray ray, Camera eventCamera, float distance = Mathf.Infinity)
        {
            List<GameObject> raycastObjects = null;
            if (canvas == null) { return raycastObjects; }

            var screenCenterPoint = ScreenCenterPoint;
            var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            raycastObjects = new List<GameObject>();
            for (int i = 0; i < graphics.Count; ++i)
            {
                var graphic = graphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget) { continue; }

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, screenCenterPoint, eventCamera)) { continue; }

                if (ignoreReversedGraphics && Vector3.Dot(ray.direction, graphic.transform.forward) <= 0f) { continue; }

                if (!graphic.Raycast(screenCenterPoint, eventCamera)) { continue; }

                float dist;
                new Plane(graphic.transform.forward, graphic.transform.position).Raycast(ray, out dist);
                if (dist > distance) { continue; }

                // TODO: Sort this list of all objects
                var currentOverGo = graphic.gameObject;
                raycastObjects.Add(currentOverGo);
            }

            return raycastObjects;
        }
    }
}
