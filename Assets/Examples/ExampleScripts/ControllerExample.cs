using Vulcan;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControllerExample : MonoBehaviour
{
    struct ControllerObject
    {
        public Controller controller;
        public GameObject controllerRootObject;
        public GameObject controllerRenderObject;
        public GameObject reticle;
        public bool active;
    }

    private Color[] _controllerColors;
    private ControllerObject[] _controllers;

    [SerializeField] private GameObject _reticlePrefab;
    [SerializeField] private bool _useReticle;

    private int clickCount = 0;
    public void HandleUIPress(Button context)
    {
        if (context != null)
        {
            clickCount++;
            Text text = context.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = string.Format("Pressed {0} times", clickCount);
            }
        }
    }

    private void Awake()
    {
        _controllerColors = new Color[8];
        _controllerColors[0] = Color.red;
        _controllerColors[1] = Color.green;
        _controllerColors[2] = Color.blue;
        _controllerColors[3] = Color.white;
        _controllerColors[4] = Color.yellow;
        _controllerColors[5] = Color.magenta;
        _controllerColors[6] = Color.grey;
        _controllerColors[7] = Color.cyan;

        _controllers = new ControllerObject[8];

        GameObject controllerHolder = new GameObject("HolodeckControllers");

        for (int i = 0; i < 8; i++)
        {
            // Set up render object
            _controllers[i].controllerRootObject = new GameObject("Controller_" + i);

            GameObject renderObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            renderObject.transform.parent = _controllers[i].controllerRootObject.transform;
            renderObject.transform.localRotation = Quaternion.identity;
            renderObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            renderObject.transform.position = _controllers[i].controllerRootObject.transform.position + 1.5f * _controllers[i].controllerRootObject.transform.forward;
            renderObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            _controllers[i].controllerRenderObject = renderObject;

            renderObject.GetComponent<Renderer>().material.color = _controllerColors[i];

            if (_reticlePrefab != null)
            {
                GameObject reticle = Instantiate(_reticlePrefab);
                reticle.transform.parent = controllerHolder.transform;
                reticle.name = ("ControllerReticle_" + i);

                reticle.GetComponent<Renderer>().material.color = _controllerColors[i];

                _controllers[i].reticle = reticle;
            }

            _controllers[i].controllerRootObject.SetActive(false);
            _controllers[i].reticle.SetActive(false);

            // Mark not active
            _controllers[i].active = false;
            _controllers[i].controllerRootObject.transform.parent = controllerHolder.transform;
        }
    }

    void Start()
    {
        if (ControllerManager.Instance == null)
        {
            Debug.LogWarning("[ControllerExample] Input is not ready. Ensure the HolodeckInitializer is set to use input.");
            return;
        }
        ControllerManager.Instance.RegisterForControllerNotifications(OnControllerActivated, OnControllerDeactivated);
    }

    void InsertController(Controller controller)
    {
        HolodeckCameraRig hcr;
        if (_controllers.Length > controller.ControllerId)
        {
            if (_controllers[controller.ControllerId].controller == null)
            {
                _controllers[controller.ControllerId].controller = controller;
                _controllers[controller.ControllerId].controllerRootObject.SetActive(true);
                _controllers[controller.ControllerId].controllerRenderObject.SetActive(!_useReticle);
                _controllers[controller.ControllerId].reticle.SetActive(_useReticle);
                _controllers[controller.ControllerId].active = true;

                hcr = HolodeckInitializer.Instance.HolodeckCameraRigs[0];
                _controllers[controller.ControllerId].reticle.layer = hcr.RigLayer;

                // Explicit Rig Target Example
                //_controllers[i].SetTargetRig(hcr);
            }
            else
            {
                Debug.LogErrorFormat(this, "[ControllerExample] Controller {0} has become activated, but already appears to be in the list and is currently active", controller.GetDescriptiveName());
            }
        }
        else
        {
            Debug.LogErrorFormat(this, "[ControllerExample] Controller {0} has become activated, but there are not enough controller objects to use this controller", controller.GetDescriptiveName());
        }
    }

    void RemoveController(Controller controller)
    {
        if (_controllers != null)
        {
            bool found = false;
            for (int i = 0; i < _controllers.Length; i++)
            {
                if (_controllers[i].controller == controller)
                {
                    if (_controllers[i].active)
                    {
                        _controllers[i].active = false;
                        _controllers[i].controllerRootObject.SetActive(false);
                        _controllers[i].controllerRenderObject.SetActive(false);
                        _controllers[i].reticle.SetActive(false);
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                Debug.LogErrorFormat(this, "[ControllerExample] Controller {0} has become deactivated, but is is not being tracked currently", controller.GetDescriptiveName());
            }
        }
    }

    void OnControllerActivated(Controller controller)
    {
        InsertController(controller);
    }

    void OnControllerDeactivated(Controller controller)
    {
        RemoveController(controller);
    }

    void LateUpdate()
    {
        float reticleNormalOffset = 0.05f;
        if (_controllers != null)
        {
            for (int i = 0; i < _controllers.Length; i++)
            {
                if (_controllers[i].active)
                {
                    // TODO: Fix this for multiple cameras and offsets
                    _controllers[i].controllerRootObject.transform.position = _controllers[i].controller.GetWorldSpacePosition();
                    _controllers[i].controllerRootObject.transform.rotation = _controllers[i].controller.GetWorldSpaceRotation();

                    // Reticle projection example
                    if (_useReticle && _controllers[i].reticle != null)
                    {
                        if (!_controllers[i].reticle.activeSelf)
                        {
                            _controllers[i].controllerRenderObject.SetActive(false);
                            _controllers[i].reticle.SetActive(true);
                        }

                        // In-progress
                        RaycastHit rh = new RaycastHit();
                        ControllerProjection cp = _controllers[i].controller.GetControllerProjection();
                        if (cp == null)
                        {
                            continue;
                        }

                        if (cp.RaycastHits.Length > 0)
                        {
                            // closest
                            //rh.distance = float.MaxValue;

                            // furthest
                            rh.distance = 0.0f;

                            for (int j = 0; j < cp.RaycastHits.Length; ++j)
                            {
                                // closest
                                //if (cp._raycastHits[j].distance < rh.distance)
                                //{
                                //    rh = cp._raycastHits[j];
                                //}

                                // furthest
                                if (cp.RaycastHits[j].distance > rh.distance)
                                {
                                    rh = cp.RaycastHits[j];
                                    Renderer renderer = rh.transform.GetComponent<Renderer>();
                                    if (renderer != null)
                                    {
                                        if (_controllers[i].controller.GetTriggerDown())
                                        {
                                            if (renderer.material.color == Color.green)
                                            {
                                                renderer.material.color = Color.red;
                                            }
                                            else
                                            {
                                                renderer.material.color = Color.green;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            rh = cp.HolodeckHit;
                        }

                        if (rh.collider != null)
                        {
                            Vector3 n = -rh.normal;
                            _controllers[i].reticle.transform.position = rh.point - n * reticleNormalOffset;
                            _controllers[i].reticle.transform.rotation = Quaternion.LookRotation(n);
                        }
                    }
                    else
                    {
                        if (!_controllers[i].controllerRenderObject.activeSelf)
                        {
                            _controllers[i].controllerRenderObject.SetActive(true);
                            if (_controllers[i].reticle != null)
                            {
                                _controllers[i].reticle.SetActive(false);
                            }
                        }
                    }

                    //
                    // Reload the scene to verify that controllers function properly between scenes
                    //
                    if (_controllers[i].controller.GetInputChannelUp(Controller.InputChannel.TouchPad))
                    {
                        Scene activeScene = SceneManager.GetActiveScene();
                        SceneManager.LoadScene(activeScene.name);
                    }

                    if (_controllers[i].controller.GetTrigger())
                    {
                        _controllers[i].controller.TriggerHapticPulse(2500);
                    }

                    Vector2 axis0 = _controllers[i].controller.GetInputAxisState(Controller.InputAxis.Axis0);
                    if (Mathf.Abs(axis0.y) > 0.05f)
                    {
                        HolodeckInitializer.Instance.HolodeckCameraRigs[i].transform.position += Time.deltaTime * 0.5f *
                                                                                                 HolodeckInitializer.Instance.HolodeckCameraRigs[i].transform.forward;
                    }
                }
            }
        }
    }
}
