using UnityEngine;
using System.Collections;
using UnityEngine.VR;
using System.Collections.Generic;
using Valve.VR;

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

public enum SwipeMode { up, down, left, right };

public class CameraManager : MonoBehaviour {

    public GameObject OculusCamera;
    public GameObject SteamCamera;

    public GameObject SteamLeftHandController;
    private SteamVR_TrackedObject trackedobj;

    private VRMode vrMode = VRMode.None;

    private VirtualOpt vOpt;

    List<int> controllerIndices = new List<int>();

    // swipe
    public float mMinSwipeDist = 0.2f;
    public float mMinVelocity = 1.0f;
    public float mAngleRange = 45;

    private readonly Vector2 mXAxis = new Vector2(1, 0);
    private readonly Vector2 mYAxis = new Vector2(0, 1);

    private bool mTrackingSwipe;
    private bool mCheckSwipe;
    private float mSwipeStartTime;
    private Vector2 mStartPosition;
    private Vector2 mEndPosition;


    // cached roles - may or may not be connected

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
            trackedobj = SteamLeftHandController.GetComponent<SteamVR_TrackedObject>();
        }
	}

    private void OnDeviceConnected(params object[] args)
    {
        switch (vrMode)
        {
            case VRMode.SteamVR:
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

        // Haptic Example
        // OVRInput.SetControllerVibration(...);

        vOpt = tempVirtualOpt;
    }

    private void GetSteamOpt()
    {
        VirtualOpt tempVirtualOpt = new VirtualOpt();

        foreach (var index in controllerIndices)
        {
            var deviceHand = SteamVR_Controller.Input(index);

            if (index == (int)trackedobj.GetDeviceIndex())
            {
                // leftHand

                //if (deviceHand.GetPress(EVRButtonId.k_EButton_Axis0))
                //{
                //    Vector2 axis = deviceHand.GetAxis(EVRButtonId.k_EButton_Axis0);
                //    float maxaxis = Mathf.Max(Mathf.Abs(axis.x), Mathf.Abs(axis.y));

                //    //if (maxaxis < 0.5f){tempVirtualOpt.Axis2D_LB_Center = true;}
                //    if (maxaxis < 0.5f) { tempVirtualOpt.Button_Y = true; }
                //    else if(maxaxis == -axis.x){tempVirtualOpt.Axis2D_LB_Left = true;}
                //    else if (maxaxis == axis.x) {tempVirtualOpt.Axis2D_LB_Right = true;}
                //    else if (maxaxis == -axis.y){tempVirtualOpt.Axis2D_LB_Down = true;}
                //    else{tempVirtualOpt.Axis2D_LB_Up = true;}
                //}

                if (deviceHand.GetPressDown(EVRButtonId.k_EButton_Axis0)){ tempVirtualOpt.Button_Y = true; }

                int tempAxis = CheckViveSwipe(index);
                if (tempAxis == 1) { tempVirtualOpt.Axis2D_LB_Left = true; }
                if (tempAxis == 2) { tempVirtualOpt.Axis2D_LB_Right = true; }
                if (tempAxis == 3) { tempVirtualOpt.Axis2D_LB_Down = true; }
                if (tempAxis == 4) { tempVirtualOpt.Axis2D_LB_Up = true; }

                if (deviceHand.GetPress(EVRButtonId.k_EButton_ApplicationMenu)) { tempVirtualOpt.Button_X = true; }
                if (deviceHand.GetPress(EVRButtonId.k_EButton_SteamVR_Trigger)) { tempVirtualOpt.Axis1D_LB = 1; }
                if (deviceHand.GetPress(EVRButtonId.k_EButton_Grip)) { tempVirtualOpt.Axis1D_LT = 1; }
            }
            else
            {
                // rightHand

                //if (deviceHand.GetPress(EVRButtonId.k_EButton_Axis0))
                //{
                //    Vector2 axis = deviceHand.GetAxis(EVRButtonId.k_EButton_Axis0);
                //    float maxaxis = Mathf.Max(Mathf.Abs(axis.x), Mathf.Abs(axis.y));

                //    //if (maxaxis < 0.5f) { tempVirtualOpt.Axis2D_RB_Center = true; }
                //    if (maxaxis < 0.5f) { tempVirtualOpt.Button_B = true; }
                //    else if (maxaxis == -axis.x) { tempVirtualOpt.Axis2D_RB_Left = true; }
                //    else if (maxaxis == axis.x) { tempVirtualOpt.Axis2D_RB_Right = true; }
                //    else if (maxaxis == -axis.y) { tempVirtualOpt.Axis2D_RB_Down = true; }
                //    else { tempVirtualOpt.Axis2D_RB_Up = true; }
                //}

                if (deviceHand.GetPressDown(EVRButtonId.k_EButton_Axis0)) { tempVirtualOpt.Button_B = true; }

                int tempAxis = CheckViveSwipe(index);
                if (tempAxis == 1) { tempVirtualOpt.Axis2D_RB_Left = true; }
                else if (tempAxis == 2) { tempVirtualOpt.Axis2D_RB_Right = true; }
                else if (tempAxis == 3) { tempVirtualOpt.Axis2D_RB_Down = true; }
                else if (tempAxis == 4) { tempVirtualOpt.Axis2D_RB_Up = true; }

                if (deviceHand.GetPress(EVRButtonId.k_EButton_ApplicationMenu)) { tempVirtualOpt.Button_A = true; }
                if (deviceHand.GetPress(EVRButtonId.k_EButton_SteamVR_Trigger)) { tempVirtualOpt.Axis1D_RB = 1; }
                if (deviceHand.GetPress(EVRButtonId.k_EButton_Grip)) { tempVirtualOpt.Axis1D_RT = 1; }

                // Haptic Example
                // if (deviceHand.GetPress(EVRButtonId.k_EButton_SteamVR_Trigger)) { tempVirtualOpt.Axis1D_RB = 1;  deviceHand.TriggerHapticPulse(500); }
            }
        }

        vOpt = tempVirtualOpt;
    }

    private int CheckViveSwipe(int index)
    {
        var deviceHand = SteamVR_Controller.Input(index);

        // Touch down, possible chance for a swipe
        if (deviceHand.GetTouchDown(EVRButtonId.k_EButton_Axis0))
        {
            // Record start time and position
            mTrackingSwipe = true;
            mSwipeStartTime = Time.time;
            mStartPosition = new Vector2(deviceHand.GetAxis(EVRButtonId.k_EButton_Axis0).x, deviceHand.GetAxis(EVRButtonId.k_EButton_Axis0).y);
        }
        // Touch up, possible chance for a swipe
        else if (deviceHand.GetTouchUp(EVRButtonId.k_EButton_Axis0))
        {
            mTrackingSwipe = false;
            mCheckSwipe = true;
        }
        // Touching, obtain pos.
        else if (mTrackingSwipe)
        {
            mEndPosition = new Vector2(deviceHand.GetAxis(EVRButtonId.k_EButton_Axis0).x, deviceHand.GetAxis(EVRButtonId.k_EButton_Axis0).y);
        }

        // check and active swipt
        if (mCheckSwipe)
        {
            mCheckSwipe = false;

            float deltaTime = Time.time - mSwipeStartTime;
            Vector2 swipeVector = mEndPosition - mStartPosition;
            float velocity = swipeVector.magnitude / deltaTime;

            if (velocity > mMinVelocity && swipeVector.magnitude > mMinSwipeDist)
            {
                // if the swipe has enough velocity and enough distance

                swipeVector.Normalize();

                float angleOfSwipe = Vector2.Dot(swipeVector, mXAxis);
                angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;

                // Detect left and right swipe
                if (angleOfSwipe < mAngleRange)
                {
                    OnSwipeRight();
                    return 2;
                }
                else if ((180.0f - angleOfSwipe) < mAngleRange)
                {
                    OnSwipeLeft();
                    return 1;
                }
                else
                {
                    // Detect top and bottom swipe
                    angleOfSwipe = Vector2.Dot(swipeVector, mYAxis);
                    angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                    if (angleOfSwipe < mAngleRange)
                    {
                        OnSwipeTop();
                        return 4;
                    }
                    else if ((180.0f - angleOfSwipe) < mAngleRange)
                    {
                        OnSwipeBottom();
                        return 3;
                    }
                }
            }

        }

        return -1;
    }

    private void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
    }

    private void OnSwipeRight()
    {
        Debug.Log("Swipe right");
    }

    private void OnSwipeTop()
    {
        Debug.Log("Swipe Top");
    }

    private void OnSwipeBottom()
    {
        Debug.Log("Swipe Bottom");
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
            //PrintControllerStatus(index);
            controllerIndices.Add(index);
        }
        else
        {
            Debug.Log(string.Format("Controller {0} disconnected.", index));
            //PrintControllerStatus(index);
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
