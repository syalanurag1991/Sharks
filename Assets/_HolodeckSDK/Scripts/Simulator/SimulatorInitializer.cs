//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;

namespace Vulcan
{
    public class SimulatorInitializer : MonoBehaviour
    {
        [SerializeField] Camera _simulatorCamera;
        [SerializeField] GameObject _simulatorDome;

        int _simulatorLayer;

        private void Awake()
        {
            _simulatorLayer = LayerMask.NameToLayer(Constants.SIMULATOR_LAYER);

            _simulatorCamera.cullingMask = (1 << _simulatorLayer);
            _simulatorDome.layer = _simulatorLayer;
        }

        private void Start()
        {
            VulcanUtils.SetLayerRecursively(gameObject, _simulatorLayer);
        }
    }
}