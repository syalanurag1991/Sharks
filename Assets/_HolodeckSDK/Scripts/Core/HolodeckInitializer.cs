//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;

namespace Vulcan
{
    public class HolodeckInitializer : MonoSingleton<HolodeckInitializer>
    {
        public enum RigType
        {
            BASE = 0,
            COMPOSITE = 1
        }

        private const string SIMULATOR_ASSET = "Simulator/HolodeckSimulator";

        [SerializeField] [Range(0, 10)] private int _panoramaQuality = 7;
        [SerializeField] [Range(1, 4)] private int _screensToShow = 1;
        [SerializeField] private bool _useControllers = false;
        [SerializeField] private bool _allowCollisions = false;
        [SerializeField] private bool _useSimulator = false;
        [SerializeField] private RigType _rigStyle = RigType.BASE;
        [SerializeField] private GameObject _warpDisplayRigPrefab = null;

        protected SettingsManager _settingsManager;
        protected SteamVR_Render _steamVRRender;
        protected GameObject _utilitiesHolder;
        protected GameObject _simulator;
        protected GameObject _eventSystem;

        protected GameObject _warpDisplayRigGameObject;
        protected HolodeckCameraRig[] _holodeckCameraRigs;
        protected Vector3[] _cameraRigForwards;

        public SettingsManager SettingsManager
        {
            get { return _settingsManager; }
        }

        public int PanoramaQuality
        {
            get { return _panoramaQuality; }
            set
            {
                if (_panoramaQuality != value)
                {
                    _panoramaQuality = value;
                    if (Application.isPlaying)
                    {
                        int rtResolution = Constants.GetResolutionFromQuality(_panoramaQuality);
                        CubeToEqHook.Instance.SetResolution(rtResolution, 24);
                    }
                }
            }
        }

        public int ScreensToShow
        {
            get { return _screensToShow; }
            set
            {
                if (_screensToShow != value)
                {
                    _screensToShow = value;
                    if (Application.isPlaying)
                    {
                        UpdateRigForwards();
                        CubeToEqHook.Instance.SetScreenCount(_screensToShow);
                    }
                }
            }
        }

        public HolodeckCameraRig[] HolodeckCameraRigs
        {
            get { return _holodeckCameraRigs; }
        }

        public GameObject EventSystem
        {
            get { return _eventSystem; }
        }

#if UNITY_EDITOR
        /// <summary>
        /// This accessor is only used by editor scripts and should not be used at runtime.
        /// </summary>
        public bool UseControllers
        {
            get { return _useControllers; }
            set { _useControllers = value; }
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// This accessor is only used by editor scripts and should not be used at runtime.
        /// </summary>
        public bool UseSimulator
        {
            get { return _useSimulator; }
            set { _useSimulator = value; }
        }
#endif

        /// <summary>
        /// This accessor is only used by editor scripts and should not be used at runtime.
        /// </summary>
        public RigType CameraRigType
        {
            get { return _rigStyle; }
#if UNITY_EDITOR
            set { _rigStyle = value; }
#endif
        }

        /// <summary>
        /// This accessor is only used by editor scripts and should not be used at runtime.
        /// </summary>
        public bool AllowCollisions
        {
            get { return _allowCollisions; }
#if UNITY_EDITOR
            set { _allowCollisions = value; }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// This accessor is only used by editor scripts and should not be used at runtime.
        /// </summary>
        public GameObject WarpRigPrefab
        {
            get { return _warpDisplayRigPrefab; }
            set { _warpDisplayRigPrefab = value; }
        }
#endif

        protected override void Awake()
        {
            base.Awake();

#if NET_4_6
            _settingsManager = InitSettings();

#if !UNITY_EDITOR
            _useSimulator = _settingsManager.settings.HolodeckSettings.useSimulator;
#endif

            _utilitiesHolder = new GameObject("HolodeckUtilities");
            if (_warpDisplayRigPrefab != null)
            {
                _warpDisplayRigGameObject = Instantiate(_warpDisplayRigPrefab, _utilitiesHolder.transform, true);
                _warpDisplayRigGameObject.name = _warpDisplayRigPrefab.name;
            }

            if (_useSimulator)
            {
                _simulator = (GameObject)Instantiate(Resources.Load(SIMULATOR_ASSET));
                _simulator.transform.parent = _utilitiesHolder.transform;
            }

            UnityEngine.XR.XRSettings.enabled = true;
            SteamVR_Render.pauseRendering = !_useSimulator;

            int rtResolution = Constants.GetResolutionFromQuality(_panoramaQuality);
            GameObject unwrappedRig = (GameObject)Instantiate(Resources.Load(Constants.PANORAMA_ASSET_PATH + Constants.UNWRAPPED_RIG), _utilitiesHolder.transform);
            unwrappedRig.name = Constants.UNWRAPPED_RIG;

            CubeToEqHook.Instance.SetResolution(rtResolution, 24);
            CubeToEqHook.Instance.SetScreenCount(_screensToShow);

            _holodeckCameraRigs = FindObjectsOfType<HolodeckCameraRig>();
            for (int i = 0; i < _holodeckCameraRigs.Length; ++i)
            {
                _holodeckCameraRigs[i].Initialize();
            }

            if (_useControllers)
            {
                _eventSystem = new GameObject("HolodeckEventSystem");
                _eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                _eventSystem.transform.parent = _utilitiesHolder.transform;

                SteamVRControllerManager input = gameObject.AddComponent<SteamVRControllerManager>();
                input.Initialize();

                _steamVRRender = gameObject.AddComponent<SteamVR_Render>();
                _steamVRRender.pauseGameWhenDashboardIsVisible = false;
                _steamVRRender.trackingSpace = Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding;
                _steamVRRender.enabled = true;
            }

            UpdateRigForwards();
#else
            Debug.LogError("[HolodeckInitializer] The Holodeck SDK requires the use of .NET 4.6.");
#endif
        }

        void UpdateRigForwards()
        {
            Vector3[] forwards = new Vector3[4] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
            switch (_screensToShow)
            {
                case 2:
                    forwards[0] = Vector3.left;
                    forwards[1] = Vector3.right;
                    break;
                case 3:
                    forwards[0] = Quaternion.AngleAxis(-120f, Vector3.up) * Vector3.forward;
                    forwards[2] = Quaternion.AngleAxis(120f, Vector3.up) * Vector3.forward;
                    break;
                case 4:
                    forwards[0] = Quaternion.AngleAxis(-135, Vector3.up) * Vector3.forward;
                    forwards[1] = Quaternion.AngleAxis(-45f, Vector3.up) * Vector3.forward;
                    forwards[2] = Quaternion.AngleAxis(45f, Vector3.up) * Vector3.forward;
                    forwards[3] = Quaternion.AngleAxis(135f, Vector3.up) * Vector3.forward;
                    break;
            }

            _cameraRigForwards = forwards;
            for (int i = 0; i < _screensToShow; ++i)
            {
                if (i >= _holodeckCameraRigs.Length)
                {
                    break;
                }

                _holodeckCameraRigs[i].CameraRigForward = forwards[i];
            }
        }

        SettingsManager InitSettings()
        {
            SettingsManager sm;
            sm = SettingsManager.Instance;
            sm.ReadFromFile();
            return sm;
        }
    }
}