//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using UnityEngine.UI;

namespace Vulcan
{
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField] private bool _shouldAllowDebug = true;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private float _updateInterval = 0.5f;

        private int _frameCount = 0; // Frames drawn over the interval
        private float _frameTime;

        private Text _label;
        private string _targetDisplayText;

        void Start()
        {
            _label = GetComponent<Text>();
            if (_label == null)
            {
                Debug.LogError("[FPSDisplay] Attached to a component with no label.");
            }

            if (_canvas != null)
            {
                _targetDisplayText = "Display: " + _canvas.targetDisplay;
            }
            else
            {
                _targetDisplayText = null;
            }

            _frameTime = _updateInterval;
            _label.enabled = false;
        }

        void Update()
        {
            _frameTime += Time.unscaledDeltaTime;
            ++_frameCount;

            if (_shouldAllowDebug)
            {
                if (Input.GetKeyUp(KeyCode.F))
                {
                    _label.enabled = !_label.enabled;
                }
            }

            if (_frameTime >= _updateInterval)
            {
                // display two fractional digits (f2 format)
                float fps = _frameCount / _frameTime;
                Color c = Color.red;
                if (fps > 30)
                {
                    c = Color.yellow;
                    if (fps > 50)
                    {
                        c = Color.green;
                    }
                }

                string cashex = ColorUtility.ToHtmlStringRGB(c);

                if (!string.IsNullOrEmpty(_targetDisplayText))
                {
                    _label.text = System.String.Format("{0} -- <color=#{1}>{2:F2} FPS</color>", _targetDisplayText, cashex, fps);
                }
                else
                {
                    _label.text = System.String.Format("<color=#{0}>{1:F2} FPS</color>", cashex, fps);
                }

                _frameTime = 0.0f;
                _frameCount = 0;
            }
        }
    }
}