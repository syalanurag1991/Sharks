//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

using UnityEngine;
using Vulcan;

/*
 * TODOs
 *      Use same base class for controller registration that the example does
 *      Share similar registration methods
 */
public class SimulatorControllers : MonoBehaviour
{
    struct ControllerObject
    {
        public Controller controller;
        public GameObject controllerRootObject;
        public GameObject controllerRenderObject;
        public bool active;
    }

    private Color[] _controllerColors;
    private ControllerObject[] _controllers;

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

        GameObject controllerHolder = new GameObject("HolodeckSimulatorControllers");
        controllerHolder.transform.parent = transform;
        controllerHolder.transform.position = Vector3.zero;
        controllerHolder.transform.rotation = Quaternion.identity;

        for (int i = 0; i < 8; i++)
        {
            _controllers[i].controllerRootObject = new GameObject("Controller_" + i);

            GameObject renderObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            renderObject.transform.parent = _controllers[i].controllerRootObject.transform;
            renderObject.transform.localRotation = Quaternion.identity;
            renderObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            renderObject.transform.localPosition = Vector3.zero;
            renderObject.layer = LayerMask.NameToLayer(Constants.SIMULATOR_LAYER);
            _controllers[i].controllerRenderObject = renderObject;

            renderObject.GetComponent<Renderer>().material.color = _controllerColors[i];
            _controllers[i].controllerRootObject.SetActive(false);

            // Mark not active
            _controllers[i].active = false;
            _controllers[i].controllerRootObject.transform.parent = controllerHolder.transform;
        }
    }

    void Start()
    {
        if (ControllerManager.Instance == null)
        {
            Debug.LogWarning("[SimulatorControllers] Input is not ready. Ensure the HolodeckInitializer is set to use input.");
            return;
        }
        ControllerManager.Instance.RegisterForControllerNotifications(OnControllerActivated, OnControllerDeactivated);
    }

    void InsertController(Controller controller)
    {
        if (_controllers.Length > controller.ControllerId)
        {
            if (_controllers[controller.ControllerId].controller == null)
            {

                _controllers[controller.ControllerId].controller = controller;
                _controllers[controller.ControllerId].controllerRootObject.SetActive(true);
                _controllers[controller.ControllerId].active = true;
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
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                Debug.LogErrorFormat(this, "[SimulatorControllers] Controller {0} has become deactivated, but is is not being tracked currently", controller.GetDescriptiveName());
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

    private Vector3 GetWorldPositionRelativeToSimulator(Controller c)
    {
        int rigId = c.RigId;
        if (rigId == -1 || HolodeckInitializer.Instance.HolodeckCameraRigs.Length <= rigId)
        {
            Debug.LogWarningFormat(this, "[SimulatorControllers] Controllers have not been registered to a HolodeckCameraRig.");
            return Vector3.zero;
        }

        Vector3 localToHD = HolodeckInitializer.Instance.HolodeckCameraRigs[rigId].transform.InverseTransformPoint(c.GetWorldSpacePosition());
        return transform.TransformPoint(localToHD);
    }

    private void LateUpdate()
    {
        if (_controllers != null)
        {
            for (int i = 0; i < _controllers.Length; i++)
            {
                if (_controllers[i].active)
                {
                    _controllers[i].controllerRootObject.transform.position = GetWorldPositionRelativeToSimulator(_controllers[i].controller);
                    _controllers[i].controllerRootObject.transform.rotation = _controllers[i].controller.GetWorldSpaceRotation();
                }
            }
        }
    }
}
