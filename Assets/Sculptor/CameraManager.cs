using UnityEngine;
using System.Collections;
using UnityEngine.VR;
using System.Collections.Generic;
using Valve.VR;
using UnityEditor;

public class VirtualOpt
{
    // Axis2D
    public bool Axis2D_LB_Center;
    public bool Axis2D_LB_Left;
    public bool Axis2D_LB_Right;
    public bool Axis2D_LB_Up;
    public bool Axis2D_LB_Down;

    public bool Axis2D_RB_Center;
    public bool Axis2D_RB_Left;
    public bool Axis2D_RB_Right;
    public bool Axis2D_RB_Up;
    public bool Axis2D_RB_Down;

    // Axis1D
    public float Axis1D_LB;
    public float Axis1D_LT;

    public float Axis1D_RB;
    public float Axis1D_RT;

    // Button
    public bool Button_A;
    public bool Button_B;
    public bool Button_X;
    public bool Button_Y;

    public VirtualOpt()
    {
        Axis2D_LB_Center = false;
        Axis2D_LB_Down = false;
        Axis2D_LB_Left = false;
        Axis2D_LB_Right = false;
        Axis2D_LB_Up = false;

        Axis2D_RB_Center = false;
        Axis2D_RB_Down = false;
        Axis2D_RB_Left = false;
        Axis2D_RB_Right = false;
        Axis2D_RB_Up = false;

        Axis1D_LB = 0;
        Axis1D_LT = 0;
        Axis1D_RB = 0;
        Axis1D_RT = 0;

        Button_A = false;
        Button_B = false;
        Button_X = false;
        Button_Y = false;
    }
}

public enum VRMode
{
    None,
    SteamVR,
    OculusVR,
}

public class CameraManager : MonoBehaviour {

    public GameObject OculusCamera;
    public GameObject SteamCamera;

    private VRMode vrMode = VRMode.None;

    private VirtualOpt vOpt;

    List<int> controllerIndices = new List<int>();

    // cached roles - may or may not be connected
    private int leftIndex;
    private int rightIndex;

    EVRButtonId[] buttonIds = new EVRButtonId[] {
        EVRButtonId.k_EButton_ApplicationMenu,
        EVRButtonId.k_EButton_SteamVR_Trigger,
        EVRButtonId.k_EButton_Axis4,
        EVRButtonId.k_EButton_Axis3,
        EVRButtonId.k_EButton_Axis2,
        EVRButtonId.k_EButton_Axis1,
        EVRButtonId.k_EButton_Axis0
    };

    EVRButtonId[] axisIds = new EVRButtonId[] {
        EVRButtonId.k_EButton_SteamVR_Touchpad,
        EVRButtonId.k_EButton_SteamVR_Trigger
    };

    // Use this for initialization
    void Awake () {

        if (OculusCamera.activeSelf && SteamCamera.activeSelf)
        {
            EditorUtility.DisplayDialog("ERROR", "Place Only Set Active One VR Device Camera Object!", "OK");
            Application.Quit();
        }
        if (OculusCamera.activeSelf)
        {
            // Use oculus first
            vrMode = VRMode.OculusVR;
            SteamVR.enabled = false;
            UnityEngine.VR.VRSettings.enabled = true;
        }
        else
        {
            SteamVR.enabled = true;
            if (SteamVR.enabled)
            {
                vrMode = VRMode.SteamVR;
                UnityEngine.VR.VRSettings.enabled = false;
            }
        }
	}

    private void OnDeviceConnected(params object[] args)
    {
        switch (vrMode)
        {
            case VRMode.SteamVR:
                leftIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
                rightIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
                SteamDeviceConnected(args);
                break;

            case VRMode.OculusVR:

                break;

            case VRMode.None:

                break;
        }

        
    }

    void OnEnable()
    {
        switch (vrMode)
        {
            case VRMode.SteamVR:
                SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);
                break;

            case VRMode.OculusVR:
                break;

            case VRMode.None:
                break;
        }
    }

    void OnDisable()
    {

        switch (vrMode)
        {
            case VRMode.SteamVR:
                SteamVR_Utils.Event.Remove("device_connected", OnDeviceConnected);
                break;

            case VRMode.OculusVR:
                break;

            case VRMode.None:
                break;
        }
    }

    // Update is called once per frame
    void Update () {

        switch (vrMode)
        {
            case VRMode.SteamVR:
                GetSteamOpt();
                break;

            case VRMode.OculusVR:
                GetOculusOpt();
                break;

            case VRMode.None:
                break;
        }

    }

    private void GetOculusOpt()
    {
        VirtualOpt tempVirtualOpt = new VirtualOpt();

        // Axis2D
        tempVirtualOpt.Axis2D_LB_Center = OVRInput.Get(OVRInput.Button.PrimaryThumbstick);
        tempVirtualOpt.Axis2D_LB_Left = OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft);
        tempVirtualOpt.Axis2D_LB_Right = OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight);
        tempVirtualOpt.Axis2D_LB_Up = OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp);
        tempVirtualOpt.Axis2D_LB_Down = OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown);

        tempVirtualOpt.Axis2D_RB_Center = OVRInput.Get(OVRInput.Button.SecondaryThumbstick);
        tempVirtualOpt.Axis2D_RB_Left = OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft);
        tempVirtualOpt.Axis2D_RB_Right = OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight);
        tempVirtualOpt.Axis2D_RB_Up = OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp);
        tempVirtualOpt.Axis2D_RB_Down = OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown);

        // Axis1D
        tempVirtualOpt.Axis1D_LB = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch);
        tempVirtualOpt.Axis1D_LT = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch);

        tempVirtualOpt.Axis1D_RB = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch);
        tempVirtualOpt.Axis1D_RT = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch);

        // Button
        tempVirtualOpt.Button_A = OVRInput.Get(OVRInput.Button.One);
        tempVirtualOpt.Button_B = OVRInput.Get(OVRInput.Button.Two);
        tempVirtualOpt.Button_X = OVRInput.Get(OVRInput.Button.Three);
        tempVirtualOpt.Button_Y = OVRInput.Get(OVRInput.Button.Four);

        vOpt = tempVirtualOpt;
    }

    private void GetSteamOpt()
    {
        VirtualOpt tempVirtualOpt = new VirtualOpt();

        foreach (var index in controllerIndices)
        {
            var deviceHand = SteamVR_Controller.Input(index);

            if (index == leftIndex)
            {
                // leftHand

                if (deviceHand.GetPress(EVRButtonId.k_EButton_Axis0))
                {
                    Vector2 axis = deviceHand.GetAxis(EVRButtonId.k_EButton_Axis0);
                    float maxaxis = Mathf.Max(Mathf.Abs(axis.x), Mathf.Abs(axis.y));

                    if (maxaxis < 0.5f){tempVirtualOpt.Axis2D_LB_Center = true;}
                    else if(maxaxis == -axis.x){tempVirtualOpt.Axis2D_LB_Left = true;}
                    else if (maxaxis == axis.x) {tempVirtualOpt.Axis2D_LB_Right = true;}
                    else if (maxaxis == -axis.y){tempVirtualOpt.Axis2D_LB_Down = true;}
                    else{tempVirtualOpt.Axis2D_LB_Up = true;}
                }

                if (deviceHand.GetPress(EVRButtonId.k_EButton_ApplicationMenu)) { tempVirtualOpt.Button_X = true; }
                if (deviceHand.GetPress(EVRButtonId.k_EButton_SteamVR_Trigger)) { tempVirtualOpt.Axis1D_LB = 1; }
                if (deviceHand.GetPress(EVRButtonId.k_EButton_Grip)) { tempVirtualOpt.Axis1D_LT = 1; }
            }
            else
            {
                // rightHand

                if (deviceHand.GetPress(EVRButtonId.k_EButton_Axis0))
                {
                    Vector2 axis = deviceHand.GetAxis(EVRButtonId.k_EButton_Axis0);
                    float maxaxis = Mathf.Max(Mathf.Abs(axis.x), Mathf.Abs(axis.y));

                    if (maxaxis < 0.5f) { tempVirtualOpt.Axis2D_RB_Center = true; }
                    else if (maxaxis == -axis.x) { tempVirtualOpt.Axis2D_RB_Left = true; }
                    else if (maxaxis == axis.x) { tempVirtualOpt.Axis2D_RB_Right = true; }
                    else if (maxaxis == -axis.y) { tempVirtualOpt.Axis2D_RB_Down = true; }
                    else { tempVirtualOpt.Axis2D_RB_Up = true; }
                }

                if (deviceHand.GetPress(EVRButtonId.k_EButton_ApplicationMenu)) { tempVirtualOpt.Button_A = true; }
                if (deviceHand.GetPress(EVRButtonId.k_EButton_SteamVR_Trigger)) { tempVirtualOpt.Axis1D_RB = 1; }
                if (deviceHand.GetPress(EVRButtonId.k_EButton_Grip)) { tempVirtualOpt.Axis1D_RT = 1; }
            }
        }

        vOpt = tempVirtualOpt;
    }

    private void SteamDeviceConnected(params object[] args)
    {
        var index = (int)args[0];

        var system = OpenVR.System;
        if (system == null || system.GetTrackedDeviceClass((uint)index) != ETrackedDeviceClass.Controller)
            return;

        var connected = (bool)args[1];
        if (connected)
        {
            Debug.Log(string.Format("Controller {0} connected.", index));
            PrintControllerStatus(index);
            controllerIndices.Add(index);
        }
        else
        {
            Debug.Log(string.Format("Controller {0} disconnected.", index));
            PrintControllerStatus(index);
            controllerIndices.Remove(index);
        }
    }

    void PrintControllerStatus(int index)
    {
        var device = SteamVR_Controller.Input(index);
        Debug.Log("index: " + device.index);
        Debug.Log("connected: " + device.connected);
        Debug.Log("hasTracking: " + device.hasTracking);
        Debug.Log("outOfRange: " + device.outOfRange);
        Debug.Log("calibrating: " + device.calibrating);
        Debug.Log("uninitialized: " + device.uninitialized);
        Debug.Log("pos: " + device.transform.pos);
        Debug.Log("rot: " + device.transform.rot.eulerAngles);
        Debug.Log("velocity: " + device.velocity);
        Debug.Log("angularVelocity: " + device.angularVelocity);
        Debug.Log((leftIndex == rightIndex) ? "first" : (leftIndex == index) ? "left" : "right");
    }

    public VRMode GetVRMode()
    {
        return vrMode;
    }

    public VirtualOpt GetVirtualOpt()
    {
        return vOpt;
    }
}
