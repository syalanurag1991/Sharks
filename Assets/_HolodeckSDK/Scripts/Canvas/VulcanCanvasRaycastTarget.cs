//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using UnityEngine.EventSystems;

namespace Vulcan
{
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class VulcanCanvasRaycastTarget : UIBehaviour
    {
        private Canvas _canvas;
        [SerializeField] private bool _ignoreReversedGraphics = true;

        public virtual Canvas Canvas { get { return _canvas; } }

        public bool IgnoreReversedGraphics { get { return _ignoreReversedGraphics; } set { _ignoreReversedGraphics = value; } }

        protected override void Awake()
        {
            base.Awake();
            _canvas = GetComponent<Canvas>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            VulcanCanvasRaycaster.AddTarget(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            VulcanCanvasRaycaster.RemoveTarget(this);
        }
    }
}