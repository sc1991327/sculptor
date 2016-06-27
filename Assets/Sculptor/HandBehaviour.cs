using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cubiquity;

public enum ControlPanel { empty, main, color, replay, load, high };
public enum OptModePanel { sculptor, network, rotate, replay, mirror };
public enum InfoPanel { empty, start, info};
public enum OptState { create, delete, smooth, paint };
public enum OptShape { cube, sphere, capsule, cylinder };
public enum DrawPos { left, right, twice };
public enum HandOpt { singleOpt, pairOpt, voxelWorldOpt, voxelWorldSingleOpt };

public class HandBehaviour : MonoBehaviour {

    public GameObject networkManagerObj = null;
    public GameObject cameraManagerObj = null;
    public GameObject BasicProceduralVolume = null;

    public GameObject leftHandAnchor = null;
    public GameObject rightHandAnchor = null;

    private TrackAnchor trackAnchor;
    private RecordBehaviour recordBehaviour;
    private HandMenuObjectControl handMenuObjectControl;

    private NetManagerUDP networkManager;
    private CameraManager cameraManager;

    private TerrainVolume terrainVolume;
    private ProceduralTerrainVolume proceduralTerrainVolume;

    private MaterialSet emptyMaterialSet;
    private MaterialSet colorMaterialSetLeft;
    private MaterialSet colorMaterialSetRight;

    private Transform VoxelWorldTransform;

    private Vector3 rightChildPosition = new Vector3(0, 0, 0);
    private Vector3 rightChildPositionScaled = new Vector3(0, 0, 0);

    private Vector3 leftChildPosition = new Vector3(0, 0, 0);
    private Vector3 leftChildPositionScaled = new Vector3(0, 0, 0);

    private Vector3 twiceChildPosition = new Vector3(0, 0, 0);
    private Vector3 twiceChildPositionScale = new Vector3(0, 0, 0);

    private Vector3 VoxelWorldCenterPos;
    private Vector3 VoxelWorldLeftHandPos;

    private Vector3 VoxelWorldPreAngleDir;
    private Vector3 VoxelWorldNowAngleDir;
    private Quaternion VoxelWorldBasicAngle;

    private Vector3 VoxelWorldBasicScale;
    private float VoxelWorldPreScale;
    private float VoxelWorldNowScale;

    private ControlPanel activePanel;
    private bool activePanelContinue;
    private OptModePanel activeOptModePanel;
    private OptState activeStateLeft;
    private OptState activeStateRight;
    private OptShape activeShape;
    private DrawPos activeDrawPos;
    private HandOpt activeHandOpt;

    private int activeInfoPanelTimes;

    private float buttonPreTime = 0.0f;
    private float ButtonTimeControlSingle = 0.3f;

    private int optRangeLeft = 6;
    private int optRangeRight = 6;
    private int optRangeSingleHandMax = 10;
    private int optRangeSingleHandMin = 4;

    private Vector3 rightRotateEuler;
    private Vector3 leftRotateEuler;

    private Vector3 leftChildPos = new Vector3(0, 0, 2);
    private Vector3 rightChildPos = new Vector3(0, 0, 2);

    private Color colorChoseLeft = new Color(0.25f, 0.25f, 0.5f);
    private Color colorChoseRight = new Color(0.25f, 0.25f, 0.5f);

    private float appStartTime;

    private bool checkOptContinueState = false;
    private bool checkPreOptContinueState = false;

    private int singleHandOptModeLeft = 10000;
    private int singleHandOptModeRight = 10000;

    private int CoroutineRange = 10000;

    private float replayStartTime = 0.0f;

    private float rotateSpeed = 60;
    private float preRotateTime;

    private float preOptTime;
    private float preOptRate = 0.05f;
    private bool preOptState = false;
    private Vector3 preOptPos;
    private Vector3 preNetOptPos;

    private bool breakTwiceHand = false;

    // Compute shader

    private int CBMaxSize = 32;
    private ComputeBuffer CBIn;
    private ComputeBuffer CBOut;
    public ComputeShader CSCreate;
    public ComputeShader CSSmooth;

    // -- OVRInput Info

    // Axis2D
    Vector2 Axis2D_L;
    Vector2 Axis2D_R;

    bool Axis2D_LB_Center;
    bool Axis2D_LB_Left;
    bool Axis2D_LB_Right;
    bool Axis2D_LB_Up;
    bool Axis2D_LB_Down;

    bool Axis2D_RB_Center;
    bool Axis2D_RB_Left;
    bool Axis2D_RB_Right;
    bool Axis2D_RB_Up;
    bool Axis2D_RB_Down;

    // Axis1D
    float Axis1D_LB;
    float Axis1D_LT;

    float Axis1D_RB;
    float Axis1D_RT;

    // Button
    bool Button_A;
    bool Button_B;
    bool Button_X;
    bool Button_Y;

    // Use this for initialization
    void Start () {

        appStartTime = Time.time;
        preRotateTime = Time.time;
        preOptTime = Time.time;

        networkManager = networkManagerObj.GetComponent<NetManagerUDP>();
        cameraManager = cameraManagerObj.GetComponent<CameraManager>();

        terrainVolume = BasicProceduralVolume.GetComponent<TerrainVolume>();
        proceduralTerrainVolume = BasicProceduralVolume.GetComponent<ProceduralTerrainVolume>();

        if (leftHandAnchor == null || rightHandAnchor == null || BasicProceduralVolume == null || cameraManager == null || networkManager == null)
        {
            Debug.LogError("Please assign the GameObject first.");
        }
        if (terrainVolume == null || cameraManager == null)
        {
            Debug.LogError("This script should be attached to a game object with a right component");
        }

        VoxelWorldTransform = terrainVolume.transform;

        trackAnchor = GetComponent<TrackAnchor>();
        recordBehaviour = GetComponent<RecordBehaviour>();
        handMenuObjectControl = GetComponent<HandMenuObjectControl>();

        // empty
        emptyMaterialSet = new MaterialSet();
        emptyMaterialSet.weights[3] = 0;
        emptyMaterialSet.weights[2] = 0;
        emptyMaterialSet.weights[1] = 0;
        emptyMaterialSet.weights[0] = 0;

        // color control
        colorMaterialSetLeft = new MaterialSet();
        colorMaterialSetLeft.weights[3] = 127;    // light
        colorMaterialSetLeft.weights[2] = 64;  // b
        colorMaterialSetLeft.weights[1] = 32;  // g
        colorMaterialSetLeft.weights[0] = 32;  // r

        colorMaterialSetRight = new MaterialSet();
        colorMaterialSetRight.weights[3] = 127;    // light
        colorMaterialSetRight.weights[2] = 64;  // b
        colorMaterialSetRight.weights[1] = 32;  // g
        colorMaterialSetRight.weights[0] = 32;  // r

        activePanel = ControlPanel.empty;
        activePanelContinue = false;
        activeOptModePanel = OptModePanel.sculptor;
        activeStateLeft = OptState.create;
        activeStateRight = OptState.create;

        activeInfoPanelTimes = 0;

        activeShape = OptShape.sphere;
        activeHandOpt = HandOpt.singleOpt;

        rightRotateEuler = new Vector3(0, 0, 0);
        leftRotateEuler = new Vector3(0, 0, 0);

        VoxelWorldCenterPos = terrainVolume.transform.position;
        VoxelWorldLeftHandPos = new Vector3(0, 0, 0);

        CBIn = new ComputeBuffer(CBMaxSize * CBMaxSize * CBMaxSize, sizeof(int), ComputeBufferType.Default);
        CBOut = new ComputeBuffer(CBMaxSize * CBMaxSize * CBMaxSize, sizeof(int), ComputeBufferType.Default);
    }

	// Update is called once per frame
	void Update () {

        // Bail out if we're not attached to a terrain.
        if (terrainVolume == null)
        {
            return;
        }

        leftChildPosition = trackAnchor.GetLeftChildPosition() - VoxelWorldTransform.position;
        leftChildPositionScaled = (new Vector3(leftChildPosition.x / VoxelWorldTransform.localScale.x, leftChildPosition.y / VoxelWorldTransform.localScale.y, leftChildPosition.z / VoxelWorldTransform.localScale.z));

        rightChildPosition = trackAnchor.GetRightChildPosition() - VoxelWorldTransform.position;
        rightChildPositionScaled = (new Vector3(rightChildPosition.x / VoxelWorldTransform.localScale.x, rightChildPosition.y / VoxelWorldTransform.localScale.y, rightChildPosition.z / VoxelWorldTransform.localScale.z));

        twiceChildPosition = trackAnchor.GetTwiceChildPosition() - VoxelWorldTransform.position;
        twiceChildPositionScale = (new Vector3(twiceChildPosition.x / VoxelWorldTransform.localScale.x, twiceChildPosition.y / VoxelWorldTransform.localScale.y, twiceChildPosition.z / VoxelWorldTransform.localScale.z));

        leftRotateEuler = leftHandAnchor.transform.rotation.eulerAngles;
        rightRotateEuler = rightHandAnchor.transform.rotation.eulerAngles;

        HandleKeyBoardInput();
        HandleOVRInput();
    }

    void OnDestroy()
    {

        CBIn.Release();
        CBOut.Release();

    }

    private void HandleKeyBoardInput()
    {
        // P - screen shot

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveVDBFile();
        }
    }


    private void emptyPanelHandleOVRInput()
    {

    }

    private void mainPanelHandleOVRInput()
    {
        int tempTouchID = handMenuObjectControl.GetTouchID();
        //Debug.Log("TouchID:" + tempTouchID);
        switch (tempTouchID)
        {
            case 0:
                // color choose panel
                activePanel = ControlPanel.color;
                break;
            case 1:
                //High operators mode choose panel
                activePanel = ControlPanel.high;
                break;
            case 2:
                //Replay file choose panel
                activePanel = ControlPanel.replay;
                break;
            case 3:
                // Undo
                recordBehaviour.UnDo();
                activePanel = ControlPanel.empty;
                activePanelContinue = true;
                break;
            case 4:
                // Redo
                recordBehaviour.ReDo();
                activePanel = ControlPanel.empty;
                activePanelContinue = true;
                break;
            case 5:
                //Restart
                RestartTerrainVolumeData();
                activePanel = ControlPanel.empty;
                activePanelContinue = true;
                break;
            case 6:
                //Save
                SaveVDBFile();
                activePanel = ControlPanel.empty;
                activePanelContinue = true;
                break;
            case 7:
                //Load files choose panel
                activePanel = ControlPanel.load;
                break;
        }
    }

    private void colorPanelHandleOVRInput(ref MaterialSet colormaterialset, ref Color handcolor)
    {
        // chose color
        int tempTouchID = handMenuObjectControl.GetTouchID();
        if (activePanel == ControlPanel.color && tempTouchID >= 0)
        {
            Color tempcolor = handMenuObjectControl.colorColorList[tempTouchID];

            if (handcolor != tempcolor)
            {
                float temptotal = tempcolor.r + tempcolor.g + tempcolor.b;
                colormaterialset.weights[3] = (byte)(int)Mathf.Clamp(255 - Mathf.Max(tempcolor.r, tempcolor.g, tempcolor.b) * 196, 7, 247);  // light
                float colortotal = 255 - colormaterialset.weights[3];
                colormaterialset.weights[2] = (byte)(int)(colortotal * (tempcolor.b / temptotal));  // b
                colormaterialset.weights[1] = (byte)(int)(colortotal * (tempcolor.g / temptotal));  // g
                colormaterialset.weights[0] = (byte)(int)(colortotal - colormaterialset.weights[2] - colormaterialset.weights[1]);  // r
                handcolor = tempcolor;
            }
        }
    }

    private void replayPanelHandleOVRInput()
    {
        // execute once for each touch

        int tempTouchID = handMenuObjectControl.GetTouchID();
        if (tempTouchID >= 0 && tempTouchID < recordBehaviour.recordFileNames.Count)
        {
            recordBehaviour.ReadJsonFile(recordBehaviour.recordFileNames[tempTouchID]);

            VoxelStoreObj tempVSO = recordBehaviour.ReplayVoxelStore[0];

            activeOptModePanel = OptModePanel.replay;
            replayStartTime = Time.time - tempVSO.Time;

            activePanel = ControlPanel.empty;
            activePanelContinue = true;
        }
    }

    private void loadPanelHandleOVRInput()
    {
        int tempTouchID = handMenuObjectControl.GetTouchID();
        if (tempTouchID >= 0 && tempTouchID < recordBehaviour.loadFileNames.Count)
        {
            proceduralTerrainVolume.LoadVDBFile(recordBehaviour.loadFileNames[tempTouchID]);

            activePanel = ControlPanel.empty;
            activePanelContinue = true;
        }
    }

    private void highPanelHandleOVRInput()
    {
        int tempTouchID = handMenuObjectControl.GetTouchID();
        //Debug.Log("TouchID:" + tempTouchID);
        switch (tempTouchID)
        {
            case 0:
                activeOptModePanel = OptModePanel.sculptor;
                break;
            case 1:
                activeOptModePanel = OptModePanel.mirror;
                break;
            case 2:
                activeOptModePanel = OptModePanel.rotate;
                preRotateTime = Time.time;
                break;
            case 3:
                activeOptModePanel = OptModePanel.network;
                break;
        }
        if (tempTouchID >= 0)
        {
            activePanel = ControlPanel.empty;
            activePanelContinue = true;
        }
    }

    private void HandleButtonInSculptor(bool activeMirror)
    {
        float ButtonFilter = 0.8f;

        // mirror only support one hand operator
        activeHandOpt = HandOpt.singleOpt;
        if (activeShape != OptShape.sphere)
        {
            activeShape = OptShape.sphere;
        }

        if (Axis1D_LB > ButtonFilter && breakTwiceHand == false)
        {
            StateHandleOVRInput(DrawPos.left, activeStateLeft, colorMaterialSetLeft, activeMirror, true);
            preOptState = true;
        }

        if (Axis1D_RB > ButtonFilter && breakTwiceHand == false)
        {
            StateHandleOVRInput(DrawPos.right, activeStateRight, colorMaterialSetRight, activeMirror, true);
            preOptState = true;
        }
        
        if (Axis1D_LB <= ButtonFilter && Axis1D_RB <= ButtonFilter)
        {
            preOptState = false;
            breakTwiceHand = false;
        }

        // size
        if ((Axis2D_LB_Up) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            if (optRangeLeft < optRangeSingleHandMax)
            {
                optRangeLeft += 2;
            }
            buttonPreTime = Time.time;
        }
        else if ((Axis2D_RB_Up) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            if (optRangeRight < optRangeSingleHandMax)
            {
                optRangeRight += 2;
            }
            buttonPreTime = Time.time;
        }
        else if ((Axis2D_LB_Down) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            if (optRangeLeft > optRangeSingleHandMin)
            {
                optRangeLeft -= 2;
            }
            buttonPreTime = Time.time;
        }
        else if ((Axis2D_RB_Down) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            if (optRangeRight > optRangeSingleHandMin)
            {
                optRangeRight -= 2;
            }
            buttonPreTime = Time.time;
        }

        // shape
        if (Axis2D_LB_Left && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            singleHandOptModeLeft--;
            buttonPreTime = Time.time;
        }
        else if (Axis2D_RB_Left && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            singleHandOptModeRight--;
            buttonPreTime = Time.time;
        }
        else if (Axis2D_LB_Right && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            singleHandOptModeLeft++;
            buttonPreTime = Time.time;
        }
        else if (Axis2D_RB_Right && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            singleHandOptModeRight++;
            buttonPreTime = Time.time;
        }
    }

    private OptState SwitchOptState(int modenum, OptState tempOptState)
    {
        switch (Mathf.Abs(modenum) % 4)
        {
            case 0:
                tempOptState = OptState.create;
                break;
            case 1:
                tempOptState = OptState.delete;
                break;
            case 2:
                tempOptState = OptState.smooth;
                break;
            case 3:
                tempOptState = OptState.paint;
                break;
        }
        return tempOptState;
    }

    private void sculptorOptModePanelHandleOVRInput()
    {
        checkOptContinueState = false;

        activeStateLeft = SwitchOptState(singleHandOptModeLeft, activeStateLeft);
        activeStateRight = SwitchOptState(singleHandOptModeRight, activeStateRight);

        if (Axis1D_LT > 0 && Axis1D_RT > 0)
        {
            // global position/rotation/scaling
            if (activeHandOpt != HandOpt.voxelWorldOpt)
            {
                // first
                //VoxelWorldCenterPos = terrainVolume.transform.position;
                //VoxelWorldLeftHandPos = leftChildPosition;

                VoxelWorldBasicAngle = terrainVolume.transform.rotation;
                VoxelWorldPreAngleDir = rightChildPosition - leftChildPosition;

                VoxelWorldBasicScale = terrainVolume.transform.localScale;
                VoxelWorldPreScale = Vector3.Distance(rightChildPosition, leftChildPosition);
            }
            else
            {
                // continue
                VoxelWorldNowAngleDir = rightChildPosition - leftChildPosition;
                VoxelWorldNowScale = Vector3.Distance(rightChildPosition, leftChildPosition);

                //terrainVolume.transform.position = VoxelWorldCenterPos + (leftChildPosition - VoxelWorldLeftHandPos);
                terrainVolume.transform.rotation = Quaternion.FromToRotation(VoxelWorldPreAngleDir, VoxelWorldNowAngleDir) * VoxelWorldBasicAngle;
                terrainVolume.transform.localScale = VoxelWorldBasicScale * ( Mathf.Clamp(VoxelWorldNowScale / VoxelWorldPreScale, 0.2f, 5));
            }
            activeHandOpt = HandOpt.voxelWorldOpt;
        }
        else if (Axis1D_LT > 0)
        {
            // global position/rotation/scaling
            if (activeHandOpt != HandOpt.voxelWorldSingleOpt)
            {
                // first
                VoxelWorldCenterPos = terrainVolume.transform.position;
                VoxelWorldLeftHandPos = leftChildPosition;
            }
            else
            {
                terrainVolume.transform.position = VoxelWorldCenterPos + (leftChildPosition - VoxelWorldLeftHandPos);
            }
            activeHandOpt = HandOpt.voxelWorldSingleOpt;
        }
        else if (Axis1D_RT > 0)
        {
            // global position/rotation/scaling
            if (activeHandOpt != HandOpt.voxelWorldSingleOpt)
            {
                // first
                VoxelWorldCenterPos = terrainVolume.transform.position;
                VoxelWorldLeftHandPos = rightChildPosition;
            }
            else
            {
                terrainVolume.transform.position = VoxelWorldCenterPos + (rightChildPosition - VoxelWorldLeftHandPos);
            }
            activeHandOpt = HandOpt.voxelWorldSingleOpt;
        }
        else if (Axis1D_LB > 0 && Axis1D_RB > 0)
        {
            // two hand operator
            activeHandOpt = HandOpt.pairOpt;
            if ((Axis2D_LB_Right || Axis2D_RB_Right) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
            {
                if (activeShape >= OptShape.cylinder)
                {
                    activeShape = OptShape.cube;
                }
                else
                {
                    activeShape++;
                }
                buttonPreTime = Time.time;
            }
            else if ((Axis2D_LB_Left || Axis2D_RB_Left) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
            {
                if (activeShape <= OptShape.cube)
                {
                    activeShape = OptShape.cylinder;
                }
                else
                {
                    activeShape--;
                }
                buttonPreTime = Time.time;
            }
        }
        else
        {

            // draw two hand result
            if (activeHandOpt == HandOpt.pairOpt)
            {
                StateHandleOVRInput(DrawPos.twice, OptState.create, colorMaterialSetRight, false, false);
                buttonPreTime = Time.time;
                preOptState = false;
                breakTwiceHand = true;
            }

            // one hand operator
            HandleButtonInSculptor(false);

        }

        if (checkPreOptContinueState == true && checkOptContinueState == false)
        {
            recordBehaviour.NewDo();
        }
        checkPreOptContinueState = checkOptContinueState;
    }

    private void rotateOptModePanelHandleOVRInput()
    {
        checkOptContinueState = false;

        activeStateLeft = SwitchOptState(singleHandOptModeLeft, activeStateLeft);
        activeStateRight = SwitchOptState(singleHandOptModeRight, activeStateRight);

        // rotate
        float rotateValue = Time.time - preRotateTime;
        terrainVolume.transform.Rotate(0, rotateValue * rotateSpeed, 0);
        preRotateTime += rotateValue;

        // only one hand operator
        HandleButtonInSculptor(false);

        if (checkPreOptContinueState == true && checkOptContinueState == false)
        {
            recordBehaviour.NewDo();
        }
        checkPreOptContinueState = checkOptContinueState;
    }

    private void networkOptModePanelHandleOVRInput()
    {
        checkOptContinueState = false;

        activeStateLeft = SwitchOptState(singleHandOptModeLeft, activeStateLeft);
        activeStateRight = SwitchOptState(singleHandOptModeRight, activeStateRight);

        // only one hand operator
        HandleButtonInSculptor(false);

        if (checkPreOptContinueState == true && checkOptContinueState == false)
        {
            recordBehaviour.NewDo();
        }
        checkPreOptContinueState = checkOptContinueState;
    }

    private void mirrorOptModePanelHandleOVRInput()
    {
        checkOptContinueState = false;

        activeStateLeft = SwitchOptState(singleHandOptModeLeft, activeStateLeft);
        activeStateRight = SwitchOptState(singleHandOptModeRight, activeStateRight);

        if (Axis1D_LT > 0 && Axis1D_RT > 0)
        {
            // global position/rotation/scaling
            if (activeHandOpt != HandOpt.voxelWorldOpt)
            {
                // first
                VoxelWorldCenterPos = trackAnchor.GetMirrorPlaneTransform().transform.position;
                VoxelWorldLeftHandPos = leftChildPosition;

                VoxelWorldBasicAngle = trackAnchor.GetMirrorPlaneTransform().transform.rotation;
                VoxelWorldPreAngleDir = rightChildPosition - leftChildPosition;
            }
            else
            {
                // continue
                VoxelWorldNowAngleDir = rightChildPosition - leftChildPosition;

                Vector3 temppos = VoxelWorldCenterPos + (leftChildPosition - VoxelWorldLeftHandPos);
                Quaternion temprot = Quaternion.FromToRotation(VoxelWorldPreAngleDir, VoxelWorldNowAngleDir) * VoxelWorldBasicAngle;
                trackAnchor.SetMirrorPlaneTransform(temppos, temprot);
            }
            activeHandOpt = HandOpt.voxelWorldOpt;
        }
        else if (Axis1D_LB > 0 && Axis1D_RB > 0)
        {
            // two hand operator
            activeHandOpt = HandOpt.pairOpt;
            if ((Axis2D_LB_Right || Axis2D_RB_Right) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
            {
                if (activeShape >= OptShape.cylinder)
                {
                    activeShape = OptShape.cube;
                }
                else
                {
                    activeShape++;
                }
                buttonPreTime = Time.time;
            }
            else if ((Axis2D_LB_Left || Axis2D_RB_Left) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
            {
                if (activeShape <= OptShape.cube)
                {
                    activeShape = OptShape.cylinder;
                }
                else
                {
                    activeShape--;
                }
                buttonPreTime = Time.time;
            }
        }
        else
        {
            // draw two hand result
            if (activeHandOpt == HandOpt.pairOpt)
            {
                StateHandleOVRInput(DrawPos.twice, OptState.create, colorMaterialSetRight, true, false);
                buttonPreTime = Time.time;
            }

            // one hand operator
            HandleButtonInSculptor(true);
        }

        if (checkPreOptContinueState == true && checkOptContinueState == false)
        {
            recordBehaviour.NewDo();
        }
        checkPreOptContinueState = checkOptContinueState;
    }

    private void replayOptModePanelHandleOVRInput()
    {
        if (recordBehaviour.ReplayVoxelStore.Count > 0)
        {
            VoxelStoreObj tempVSO = recordBehaviour.ReplayVoxelStore[0];

            float relatedTime = Time.time - replayStartTime;
            if (relatedTime > tempVSO.Time)
            {
                if (tempVSO.Type == 1)
                {
                    Vector3 Pos = new Vector3(tempVSO.PosX, tempVSO.PosY, tempVSO.PosZ);
                    Vector3 RotateEuler = new Vector3(tempVSO.RotateEulerX, tempVSO.RotateEulerY, tempVSO.RotateEulerZ);
                    MaterialSet materialSet = new MaterialSet();
                    materialSet.weights[0] = (byte)tempVSO.MaterialWeight0;
                    materialSet.weights[1] = (byte)tempVSO.MaterialWeight1;
                    materialSet.weights[2] = (byte)tempVSO.MaterialWeight2;
                    materialSet.weights[3] = (byte)tempVSO.MaterialWeight3;
                    Vector3i range = new Vector3i(tempVSO.RangeX, tempVSO.RangeY, tempVSO.RangeZ);
                    OptShape optshape = (OptShape)tempVSO.Optshape;
                    bool activeMirror = tempVSO.ActiveMirror;

                    VoxelSettingGPU(Pos, RotateEuler, materialSet, range, optshape, activeMirror);
                    //StartCoroutine(VoxelSetting(Pos, RotateEuler, materialSet, range, optshape, activeMirror));
                }
                else if (tempVSO.Type == 2)
                {
                    Vector3 Pos = new Vector3(tempVSO.PosX, tempVSO.PosY, tempVSO.PosZ);
                    Vector3i range = new Vector3i(tempVSO.RangeX, tempVSO.RangeY, tempVSO.RangeZ);
                    bool activeMirror = tempVSO.ActiveMirror;

                    VoxelSmoothingGPU(Pos, range, activeMirror);
                    //StartCoroutine(VoxelSmoothing(Pos, range, activeMirror));
                }
                else
                {
                    Vector3 Pos = new Vector3(tempVSO.PosX, tempVSO.PosY, tempVSO.PosZ);
                    MaterialSet materialSet = new MaterialSet();
                    materialSet.weights[0] = (byte)tempVSO.MaterialWeight0;
                    materialSet.weights[1] = (byte)tempVSO.MaterialWeight1;
                    materialSet.weights[2] = (byte)tempVSO.MaterialWeight2;
                    materialSet.weights[3] = (byte)tempVSO.MaterialWeight3;
                    float brushInnerRadius = tempVSO.RangeX;
                    float brushOuterRadius = tempVSO.RangeY;
                    float amount = tempVSO.RangeZ;
                    bool activeMirror = tempVSO.ActiveMirror;

                    VoxelPainting(Pos, new Vector3i((int)brushOuterRadius, (int)brushOuterRadius, (int)brushOuterRadius), materialSet, activeMirror);
                }

                recordBehaviour.ReplayVoxelStore.RemoveAt(0);
            }
        }
    }

    private void HandleOVRInput()
    {
        VirtualOpt vOpt = new VirtualOpt();
        vOpt = cameraManager.GetVirtualOpt();

        // Axis2D
        Axis2D_LB_Center = vOpt.Axis2D_LB_Center;
        Axis2D_LB_Left = vOpt.Axis2D_LB_Left;
        Axis2D_LB_Right = vOpt.Axis2D_LB_Right;
        Axis2D_LB_Up = vOpt.Axis2D_LB_Up;
        Axis2D_LB_Down = vOpt.Axis2D_LB_Down;

        Axis2D_RB_Center = vOpt.Axis2D_RB_Center;
        Axis2D_RB_Left = vOpt.Axis2D_RB_Left;
        Axis2D_RB_Right = vOpt.Axis2D_RB_Right;
        Axis2D_RB_Up = vOpt.Axis2D_RB_Up;
        Axis2D_RB_Down = vOpt.Axis2D_RB_Down;

        // Axis1D
        Axis1D_LB = vOpt.Axis1D_LB;
        Axis1D_LT = vOpt.Axis1D_LT;

        Axis1D_RB = vOpt.Axis1D_RB;
        Axis1D_RT = vOpt.Axis1D_RT;

        // Button
        Button_A = vOpt.Button_A;
        Button_B = vOpt.Button_B;
        Button_X = vOpt.Button_X;
        Button_Y = vOpt.Button_Y;

        if (Button_A)
        {
            activeDrawPos = DrawPos.right;
            if (activePanel == ControlPanel.empty && activePanelContinue == false)
            {
                activePanel = ControlPanel.main;
            }
        }
        else if (Button_X)
        {
            activeDrawPos = DrawPos.left;
            if (activePanel == ControlPanel.empty && activePanelContinue == false)
            {
                activePanel = ControlPanel.main;
            }
        }
        else
        {
            activePanelContinue = false;
            activePanel = ControlPanel.empty;
        }

        if (Button_B && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activeDrawPos = DrawPos.right;
            activeInfoPanelTimes++;
            buttonPreTime = Time.time;
        }

        if (Button_Y && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activeDrawPos = DrawPos.left;
            activeInfoPanelTimes++;
            buttonPreTime = Time.time;
        }

        // menu opt
        switch (activePanel)
        {
            case ControlPanel.empty:
                emptyPanelHandleOVRInput();
                break;
            case ControlPanel.main:
                mainPanelHandleOVRInput();
                break;
            case ControlPanel.color:
                if (activeDrawPos == DrawPos.left)
                {
                    colorPanelHandleOVRInput(ref colorMaterialSetLeft, ref colorChoseLeft);
                }
                else
                {
                    colorPanelHandleOVRInput(ref colorMaterialSetRight, ref colorChoseRight);
                }
                break;
            case ControlPanel.replay:
                replayPanelHandleOVRInput();
                break;
            case ControlPanel.load:
                loadPanelHandleOVRInput();
                break;
            case ControlPanel.high:
                highPanelHandleOVRInput();
                break;
        }

        // scene opt
        switch (activeOptModePanel)
        {
            case OptModePanel.sculptor:
                sculptorOptModePanelHandleOVRInput();
                break;
            case OptModePanel.rotate:
                rotateOptModePanelHandleOVRInput();
                break;
            case OptModePanel.network:
                networkOptModePanelHandleOVRInput();
                break;
            case OptModePanel.mirror:
                mirrorOptModePanelHandleOVRInput();
                break;
            case OptModePanel.replay:
                replayOptModePanelHandleOVRInput();
                break;
        }

        //Debug.Log("activePanel: " + activePanel + " activeState: " + activeState);

    }

    private void SingleStateHandleOVRInput(OptState optState, Vector3 tempDrawPosScaled, Vector3 tempDrawRotate, Vector3 tempDrawScale, MaterialSet colorMaterial, bool activeMirror, bool useGPU)
    {
        switch (optState)
        {
            case OptState.create:
                CreateVoxels(tempDrawPosScaled, tempDrawRotate, colorMaterial, (Vector3i)tempDrawScale, activeShape, activeMirror, useGPU);
                break;
            case OptState.delete:
                DestroyVoxels(tempDrawPosScaled, tempDrawRotate, (Vector3i)tempDrawScale, activeShape, activeMirror, useGPU);
                break;
            case OptState.smooth:
                // Set independent smooth only use CPU
                SmoothVoxels(tempDrawPosScaled, (Vector3i)tempDrawScale, activeMirror, false);
                break;
            case OptState.paint:
                PaintVoxels(tempDrawPosScaled, colorMaterial, (Vector3i)tempDrawScale, 1, activeMirror);
                break;
        }

        // network operator
        if (activeOptModePanel == OptModePanel.network)
        {
            switch (optState)
            {
                case OptState.create:
                    networkManager.SendOptMessage(tempDrawPosScaled, tempDrawRotate, colorMaterial, (Vector3i)tempDrawScale, activeShape, preOptState, activeMirror);
                    break;
                case OptState.delete:
                    MaterialSet emptyMaterialSet = new MaterialSet();
                    networkManager.SendOptMessage(tempDrawPosScaled, tempDrawRotate, emptyMaterialSet, (Vector3i)tempDrawScale, activeShape, preOptState, activeMirror);
                    break;
                case OptState.smooth:
                    networkManager.SendOptMessage(tempDrawPosScaled, (Vector3i)tempDrawScale, preOptState, activeMirror);
                    break;
                case OptState.paint:
                    networkManager.SendOptMessage(tempDrawPosScaled, (Vector3i)tempDrawScale, colorMaterial, preOptState, activeMirror);
                    break;
            }
        }

    }

    private void StateHandleOVRInput(DrawPos drawPos, OptState optState, MaterialSet colorMaterial, bool activeMirror, bool useGPU)
    {
        checkOptContinueState = true;

        Vector3 tempDrawPosScaled;
        Vector3 tempDrawRotate;
        Vector3 tempDrawScale;

        float nowOptTime = Time.time;
        if (nowOptTime - preOptTime > preOptRate)
        {
            if (drawPos == DrawPos.left)
            {
                tempDrawPosScaled = leftChildPositionScaled;
                tempDrawPosScaled = VoxelWorldTransform.InverseTransformPoint(tempDrawPosScaled) * VoxelWorldTransform.localScale.x;
                tempDrawRotate = leftRotateEuler;
                tempDrawScale = new Vector3(optRangeLeft / 2, optRangeLeft / 2, optRangeLeft / 2);
            }
            else if (drawPos == DrawPos.right)
            {
                tempDrawPosScaled = rightChildPositionScaled;
                tempDrawPosScaled = VoxelWorldTransform.InverseTransformPoint(tempDrawPosScaled) * VoxelWorldTransform.localScale.x;
                tempDrawRotate = rightRotateEuler;
                tempDrawScale = new Vector3(optRangeRight / 2, optRangeRight / 2, optRangeRight / 2);
            }
            else
            {
                tempDrawPosScaled = twiceChildPositionScale;
                tempDrawRotate = new Vector3(0, 0, 0);
                Vector3 tempTwiceScale = trackAnchor.GetTwiceChildLocalScale() / 2;
                tempDrawScale = (new Vector3(tempTwiceScale.x / VoxelWorldTransform.localScale.x, tempTwiceScale.y / VoxelWorldTransform.localScale.y, tempTwiceScale.z / VoxelWorldTransform.localScale.z));
            }

            // local operator
            if (drawPos == DrawPos.twice)
            {
                SingleStateHandleOVRInput(optState, tempDrawPosScaled, tempDrawRotate, tempDrawScale, colorMaterial, activeMirror, useGPU);
            }
            else if (preOptState)
            {
                List<Vector3> tempDraw = calcDiffPos(preOptPos, tempDrawPosScaled, tempDrawScale);
                if (tempDraw.Count > 0)
                {
                    foreach (Vector3 temp in tempDraw)
                    {
                        SingleStateHandleOVRInput(optState, temp, tempDrawRotate, tempDrawScale, colorMaterial, activeMirror, useGPU);
                    }
                    preOptPos = tempDrawPosScaled;
                }
                //else if (activeOptModePanel == OptModePanel.rotate)
                //{
                //    SingleStateHandleOVRInput(optState, tempDrawPosScaled, tempDrawRotate, tempDrawScale, colorMaterial, activeMirror, useGPU);
                //    preOptPos = tempDrawPosScaled;
                //}
            }
            else
            {
                SingleStateHandleOVRInput(optState, tempDrawPosScaled, tempDrawRotate, tempDrawScale, colorMaterial, activeMirror, useGPU);
                preOptPos = tempDrawPosScaled;
            }

            nowOptTime = preOptTime;
        }
    }

    private bool CompareMaterialSet(MaterialSet ms1, MaterialSet ms2)
    {
        if (ms1.weights[0] != ms2.weights[0])
        {
            return false;
        }
        else if (ms1.weights[1] != ms2.weights[1])
        {
            return false;
        }
        else if (ms1.weights[2] != ms2.weights[2])
        {
            return false;
        }
        else if (ms1.weights[3] != ms2.weights[3])
        {
            return false;
        }
        return true;
    }

    private Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 v)
    {
        planeNormal.Normalize();
        float distance = -Vector3.Dot(planeNormal.normalized, v);
        return v + planeNormal * distance;
    }

    private Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
    {
        planeNormal.Normalize();
        float distance = -Vector3.Dot(planeNormal.normalized, (point - planePoint));
        return point + planeNormal* distance;
    }

    private Vector3 CalcMirrorPos(Vector3 planePoint0, Vector3 planePoint1, Vector3 planePoint2, Vector3 point)
    {

        Plane tempp = new Plane(planePoint0, planePoint1, planePoint2);
        float tempdis = tempp.GetDistanceToPoint(point);
        Vector3 tempm = point - 2 * tempp.normal * tempdis;
        return tempm;
    }

    private float SingleVoxelHandling(Vector3 nowPos, Vector3 cPos, Vector3 RotateEuler, MaterialSet materialSet)
    {
        float dismax = 0;

        Vector3 temp1 = RotatePointAroundPivot(nowPos, cPos, RotateEuler);
        Vector3 temp2 = VoxelWorldTransform.InverseTransformPoint(temp1) * VoxelWorldTransform.localScale.x;
        Vector3i tempi = (Vector3i)(temp2);

        dismax = Vector3.Distance(temp1, cPos);

        MaterialSet tempOld = terrainVolume.data.GetVoxel(tempi.x, tempi.y, tempi.z);
        if (!CompareMaterialSet(materialSet, tempOld))
        {
            recordBehaviour.PushOperator(new VoxelOpt(tempi, materialSet, tempOld));
            terrainVolume.data.SetVoxel(tempi.x, tempi.y, tempi.z, materialSet);
        }

        return dismax;
    }

    private void SingleVoxelSmoothHanding(MaterialSet[,,] voxelHandleRegion, Vector3i regionLowerPos, int tempX, int tempY, int tempZ)
    {
        int x = regionLowerPos.x + tempX;
        int y = regionLowerPos.y + tempY;
        int z = regionLowerPos.z + tempZ;

        MaterialSet tempMaterialSet = voxelHandleRegion[tempX, tempY, tempZ];
        MaterialSet tempMaterialSet1 = voxelHandleRegion[tempX + 1, tempY, tempZ];
        MaterialSet tempMaterialSet2 = voxelHandleRegion[tempX - 1, tempY, tempZ];
        MaterialSet tempMaterialSet3 = voxelHandleRegion[tempX, tempY + 1, tempZ];
        MaterialSet tempMaterialSet4 = voxelHandleRegion[tempX, tempY - 1, tempZ];
        MaterialSet tempMaterialSet5 = voxelHandleRegion[tempX, tempY, tempZ + 1];
        MaterialSet tempMaterialSet6 = voxelHandleRegion[tempX, tempY, tempZ - 1];
        MaterialSet tempMaterialSetEnd = new MaterialSet();

        for (uint tempM = 0; tempM < 4; tempM++)
        {
            int sum = 0;
            sum += tempMaterialSet.weights[tempM];
            sum += tempMaterialSet1.weights[tempM];
            sum += tempMaterialSet2.weights[tempM];
            sum += tempMaterialSet3.weights[tempM];
            sum += tempMaterialSet4.weights[tempM];
            sum += tempMaterialSet5.weights[tempM];
            sum += tempMaterialSet6.weights[tempM];

            int avg = (int)((float)(sum) / 7.0f + 0.5f);
            avg = Mathf.Clamp(avg, 0, 255);
            tempMaterialSetEnd.weights[tempM] = (byte)avg;
        }

        recordBehaviour.PushOperator(new VoxelOpt(new Vector3i(x, y, z), tempMaterialSetEnd, tempMaterialSet));
        terrainVolume.data.SetVoxel(x, y, z, tempMaterialSetEnd);
    }

    private void SingleVoxelPaintHanding(int tempX, int tempY, int tempZ, MaterialSet materialset, float distSquared)
    {
        MaterialSet tempOld = terrainVolume.data.GetVoxel(tempX, tempY, tempZ);
        if (!CompareMaterialSet(materialset, tempOld))
        {
            int totalmold = tempOld.weights[0] + tempOld.weights[1] + tempOld.weights[2] + tempOld.weights[3];
            int totalmset = materialset.weights[0] + materialset.weights[1] + materialset.weights[2] + materialset.weights[3];

            MaterialSet mnew = new MaterialSet();
            float temp0 = ((distSquared) * (tempOld.weights[0]) + (1 - distSquared) * (materialset.weights[0]));
            float temp1 = ((distSquared) * (tempOld.weights[1]) + (1 - distSquared) * (materialset.weights[1]));
            float temp2 = ((distSquared) * (tempOld.weights[2]) + (1 - distSquared) * (materialset.weights[2]));
            float temp3 = ((distSquared) * (tempOld.weights[3]) + (1 - distSquared) * (materialset.weights[3]));
            float totaltemp = temp0 + temp1 + temp2 + temp3;

            mnew.weights[0] = (byte)(int)(temp0 * totalmold / totaltemp);
            mnew.weights[1] = (byte)(int)(temp1 * totalmold / totaltemp);
            mnew.weights[2] = (byte)(int)(temp2 * totalmold / totaltemp);
            mnew.weights[3] = (byte)(int)(totalmold - (mnew.weights[0] + mnew.weights[1] + mnew.weights[2]));

            recordBehaviour.PushOperator(new VoxelOpt(new Vector3i(tempX, tempY, tempZ), materialset, tempOld));
            terrainVolume.data.SetVoxel(tempX, tempY, tempZ, mnew);
        }
    }

    private void VoxelSettingGPU(Vector3 Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape, bool activeMirror)
    {
        // Only support one hand operator!
        if (range.x != range.y && range.x != range.z)
        {
            return;
        }

        int r = range.x;
        int r2 = range.x * range.x;

        Vector3 tempPos = Pos;
        //Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
        Region vRegion = new Region((int)tempPos.x - r, (int)tempPos.y - r, (int)tempPos.z - r, (int)tempPos.x + r, (int)tempPos.y + r, (int)tempPos.z + r);

        Vector3i regionLowerPos = new Vector3i(vRegion.lowerCorner.x - 1, vRegion.lowerCorner.y - 1, vRegion.lowerCorner.z - 1);
        Vector3i regionUpperPos = new Vector3i(vRegion.upperCorner.x + 1, vRegion.upperCorner.y + 1, vRegion.upperCorner.z + 1);

        // obtain operator data.
        int rsize = regionUpperPos.x - regionLowerPos.x + 1;
        int[,,] voxelHandleRegionIn = new int[rsize, rsize, rsize * 4];
        int[,,] voxelHandleRegionOut = new int[rsize, rsize, rsize * 4];

        for (int tempX = 0; tempX < rsize; ++tempX)
        {
            for (int tempY = 0; tempY < rsize; ++tempY)
            {
                for (int tempZ = 0; tempZ < rsize; ++tempZ)
                {
                    MaterialSet tempM = terrainVolume.data.GetVoxel(regionLowerPos.x + tempX, regionLowerPos.y + tempY, regionLowerPos.z + tempZ);
                    voxelHandleRegionIn[tempX, tempY, tempZ * 4] = tempM.weights[0];
                    voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 1] = tempM.weights[1];
                    voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 2] = tempM.weights[2];
                    voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 3] = tempM.weights[3];
                }
            }
        }
        CBIn.SetData(voxelHandleRegionIn);

        // create GPU
        CSCreate.SetVector("colorSet", new Vector4(materialSet.weights[0], materialSet.weights[1], materialSet.weights[2], materialSet.weights[3]));
        CSCreate.SetVector("centerPos", new Vector3(rsize / 2, rsize / 2, rsize / 2));
        CSCreate.SetFloat("rangePow2", r2);
        CSCreate.SetInt("range", rsize);
        CSCreate.SetBuffer(0, "bufferIn", CBIn);
        CSCreate.SetBuffer(0, "bufferOut", CBOut);
        CSCreate.Dispatch(0, CBOut.count / 256, 1, 1);

        CBOut.GetData(voxelHandleRegionOut);

        // smooth GPU
        CSSmooth.SetInt("range", rsize);
        CSSmooth.SetBuffer(0, "bufferIn", CBOut);
        CSSmooth.SetBuffer(0, "bufferOut", CBIn);
        CSSmooth.Dispatch(0, CBIn.count / 256, 1, 1);

        CBIn.GetData(voxelHandleRegionOut);
        for (int tempX = 0; tempX < rsize; ++tempX)
        {
            for (int tempY = 0; tempY < rsize; ++tempY)
            {
                for (int tempZ = 0; tempZ < rsize; ++tempZ)
                {
                    MaterialSet tempMOld = new MaterialSet();
                    tempMOld.weights[0] = (byte)voxelHandleRegionIn[tempX, tempY, tempZ * 4];
                    tempMOld.weights[1] = (byte)voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 1];
                    tempMOld.weights[2] = (byte)voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 2];
                    tempMOld.weights[3] = (byte)voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 3];

                    MaterialSet tempMNew = new MaterialSet();
                    tempMNew.weights[0] = (byte)voxelHandleRegionOut[tempX, tempY, tempZ * 4];
                    tempMNew.weights[1] = (byte)voxelHandleRegionOut[tempX, tempY, tempZ * 4 + 1];
                    tempMNew.weights[2] = (byte)voxelHandleRegionOut[tempX, tempY, tempZ * 4 + 2];
                    tempMNew.weights[3] = (byte)voxelHandleRegionOut[tempX, tempY, tempZ * 4 + 3];

                    Vector3i tp = new Vector3i(regionLowerPos.x + tempX, regionLowerPos.y + tempY, regionLowerPos.z + tempZ);
                    recordBehaviour.PushOperator(new VoxelOpt(tp, tempMNew, tempMOld));
                    terrainVolume.data.SetVoxel(tp.x, tp.y, tp.z, tempMNew);
                }
            }
        }

        if (activeMirror)
        {
            Vector3 mirrorAnchorPoint0 = trackAnchor.GetMirrorAnchorPoint0();
            Vector3 mirrorAnchorPoint1 = trackAnchor.GetMirrorAnchorPoint1();
            Vector3 mirrorAnchorPoint2 = trackAnchor.GetMirrorAnchorPoint2();
            Vector3 tempmpos = (CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, Pos));
            VoxelSettingGPU(tempmpos, RotateEuler, materialSet, range, optshape, false);
        }
    }

    private void VoxelSmoothingGPU(Vector3 pos, Vector3i range, bool activeMirror)
    {
        // Only support one hand operator!
        if (range.x != range.y && range.x != range.z)
        {
            return;
        }

        int r = range.x;

        Vector3 tempPos = pos;
        //Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(pos) * VoxelWorldTransform.localScale.x;
        Region vRegion = new Region((int)tempPos.x - r, (int)tempPos.y - r, (int)tempPos.z - r, (int)tempPos.x + r, (int)tempPos.y + r, (int)tempPos.z + r);

        Vector3i regionLowerPos = new Vector3i(vRegion.lowerCorner.x - 1, vRegion.lowerCorner.y - 1, vRegion.lowerCorner.z - 1);
        Vector3i regionUpperPos = new Vector3i(vRegion.upperCorner.x + 1, vRegion.upperCorner.y + 1, vRegion.upperCorner.z + 1);

        // obtain operator data.
        int rsize = regionUpperPos.x - regionLowerPos.x + 1;
        int[,,] voxelHandleRegionIn = new int[rsize, rsize, rsize * 4];
        int[,,] voxelHandleRegionOut = new int[rsize, rsize, rsize * 4];

        for (int tempX = 0; tempX < rsize; ++tempX)
        {
            for (int tempY = 0; tempY < rsize; ++tempY)
            {
                for (int tempZ = 0; tempZ < rsize; ++tempZ)
                {
                    MaterialSet tempM = terrainVolume.data.GetVoxel(regionLowerPos.x + tempX, regionLowerPos.y + tempY, regionLowerPos.z + tempZ);
                    voxelHandleRegionIn[tempX, tempY, tempZ * 4] = tempM.weights[0];
                    voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 1] = tempM.weights[1];
                    voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 2] = tempM.weights[2];
                    voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 3] = tempM.weights[3];
                }
            }
        }
        CBIn.SetData(voxelHandleRegionIn);

        // calculate GPU
        CSSmooth.SetInt("range", rsize);
        CSSmooth.SetBuffer(0, "bufferIn", CBIn);
        CSSmooth.SetBuffer(0, "bufferOut", CBOut);
        CSSmooth.Dispatch(0, CBOut.count / 256, 1, 1);

        CBOut.GetData(voxelHandleRegionOut);
        for (int tempX = 0; tempX < rsize; ++tempX)
        {
            for (int tempY = 0; tempY < rsize; ++tempY)
            {
                for (int tempZ = 0; tempZ < rsize; ++tempZ)
                {
                    MaterialSet tempMOld = new MaterialSet();
                    tempMOld.weights[0] = (byte)voxelHandleRegionIn[tempX, tempY, tempZ * 4];
                    tempMOld.weights[1] = (byte)voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 1];
                    tempMOld.weights[2] = (byte)voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 2];
                    tempMOld.weights[3] = (byte)voxelHandleRegionIn[tempX, tempY, tempZ * 4 + 3];

                    MaterialSet tempMNew = new MaterialSet();
                    tempMNew.weights[0] = (byte)voxelHandleRegionOut[tempX, tempY, tempZ * 4];
                    tempMNew.weights[1] = (byte)voxelHandleRegionOut[tempX, tempY, tempZ * 4 + 1];
                    tempMNew.weights[2] = (byte)voxelHandleRegionOut[tempX, tempY, tempZ * 4 + 2];
                    tempMNew.weights[3] = (byte)voxelHandleRegionOut[tempX, tempY, tempZ * 4 + 3];

                    Vector3i tp = new Vector3i(regionLowerPos.x + tempX, regionLowerPos.y + tempY, regionLowerPos.z + tempZ);
                    recordBehaviour.PushOperator(new VoxelOpt(tp, tempMNew, tempMOld));
                    terrainVolume.data.SetVoxel(tp.x, tp.y, tp.z, tempMNew);
                }
            }
        }

        if (activeMirror)
        {
            Vector3 mirrorAnchorPoint0 = trackAnchor.GetMirrorAnchorPoint0();
            Vector3 mirrorAnchorPoint1 = trackAnchor.GetMirrorAnchorPoint1();
            Vector3 mirrorAnchorPoint2 = trackAnchor.GetMirrorAnchorPoint2();
            Vector3 tempmpos = (CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, pos));
            VoxelSmoothingGPU(tempmpos, range, false);
        }
    }

    IEnumerator VoxelSmoothing(Vector3 pos, Vector3i range, bool activeMirror)
    {
        Vector3 tempPos = pos;
        //Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(pos) * VoxelWorldTransform.localScale.x;
        Region vRegion = new Region((int)tempPos.x - range.x, (int)tempPos.y - range.y, (int)tempPos.z - range.z, (int)tempPos.x + range.x, (int)tempPos.y + range.y, (int)tempPos.z + range.z);

        Vector3i regionLowerPos = new Vector3i(vRegion.lowerCorner.x - 1, vRegion.lowerCorner.y - 1, vRegion.lowerCorner.z - 1);
        Vector3i regionUpperPos = new Vector3i(vRegion.upperCorner.x + 1, vRegion.upperCorner.y + 1, vRegion.upperCorner.z + 1);

        // obtain operator data.
        int rsizex = regionUpperPos.x - regionLowerPos.x + 1;
        int rsizey = regionUpperPos.y - regionLowerPos.y + 1;
        int rsizez = regionUpperPos.z - regionLowerPos.z + 1;
        MaterialSet[,,] voxelHandleRegion = new MaterialSet[rsizex, rsizey, rsizez];
        for (int tempX = 0; tempX < rsizex; ++tempX)
        {
            for (int tempY = 0; tempY < rsizey; ++tempY)
            {
                for (int tempZ = 0; tempZ < rsizez; ++tempZ)
                {
                    voxelHandleRegion[tempX, tempY, tempZ] = terrainVolume.data.GetVoxel(regionLowerPos.x + tempX, regionLowerPos.y + tempY, regionLowerPos.z + tempZ);
                }
            }
        }

        // calculate
        int itimes = 0;
        for (int tempX = 0; tempX < rsizex - 2; ++tempX)
        {
            for (int tempY = 0; tempY < rsizey - 2; ++tempY)
            {
                for (int tempZ = 0; tempZ < rsizez - 2; ++tempZ)
                {
                    SingleVoxelSmoothHanding(voxelHandleRegion, regionLowerPos, tempX + 1, tempY + 1, tempZ + 1);
                    itimes++;
                    if (itimes % CoroutineRange == 0)
                        yield return null;
                }
            }
        }

        if (activeMirror)
        {
            Vector3 mirrorAnchorPoint0 = trackAnchor.GetMirrorAnchorPoint0();
            Vector3 mirrorAnchorPoint1 = trackAnchor.GetMirrorAnchorPoint1();
            Vector3 mirrorAnchorPoint2 = trackAnchor.GetMirrorAnchorPoint2();
            Vector3 tempmpos = (CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, pos));
            StartCoroutine(VoxelSmoothing(tempmpos, range, false));
        }

    }

    IEnumerator VoxelSetting(Vector3 Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape, bool activeMirror)
    {
        int xPos = (int)Pos.x;
        int yPos = (int)Pos.y;
        int zPos = (int)Pos.z;

        int rangeX2 = range.x * range.x;
        int rangeY2 = range.y * range.y;
        int rangeZ2 = range.z * range.z;

        int adsrange = 1;
        int dismax = Mathf.Max(range.x, range.y, range.z);

        Vector3 mirrorAnchorPoint0 = trackAnchor.GetMirrorAnchorPoint0();
        Vector3 mirrorAnchorPoint1 = trackAnchor.GetMirrorAnchorPoint1();
        Vector3 mirrorAnchorPoint2 = trackAnchor.GetMirrorAnchorPoint2();

        switch (optshape)
        {
            case OptShape.cube:
                for (int z = zPos - range.z; z < zPos + range.z; z++)
                {
                    for (int y = yPos - range.y; y < yPos + range.y; y++)
                    {
                        for (int x = xPos - range.x; x < xPos + range.x; x++)
                        {
                            dismax = (int)SingleVoxelHandling(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler, materialSet);
                        }
                    }
                }

                dismax += adsrange;
                for ( int i=0; i < Mathf.Clamp(dismax / 3, 2, 6); i++ )
                {
                    //Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(new Vector3(xPos, yPos, zPos)) * VoxelWorldTransform.localScale.x;
                    //Vector3i tempPosi = new Vector3i(tempPos);
                    //TerrainVolumeEditor.BlurTerrainVolume(terrainVolume, new Region(tempPosi.x - dismax, tempPosi.y - dismax, tempPosi.z - dismax, tempPosi.x + dismax, tempPosi.y + dismax, tempPosi.z + dismax));

                    Pos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
                    StartCoroutine(VoxelSmoothing(Pos, new Vector3i(dismax, dismax, dismax), activeMirror));
                    yield return null;
                }
                break;

            case OptShape.sphere:
                for (int z = zPos - range.z; z < zPos + range.z; z++)
                {
                    for (int y = yPos - range.y; y < yPos + range.y; y++)
                    {
                        for (int x = xPos - range.x; x < xPos + range.x; x++)
                        {
                            float xDistance = x - xPos;
                            float yDistance = y - yPos;
                            float zDistance = z - zPos;

                            float distSquared = xDistance * xDistance / rangeX2 + yDistance * yDistance / rangeY2 + zDistance * zDistance / rangeZ2;
                            if (distSquared < 1)
                            {
                                SingleVoxelHandling(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler, materialSet);
                            }
                        }
                    }
                }

                int itimes = (range.x + range.y + range.z) / 3 > optRangeSingleHandMax ? 3 : 1;
                for (int i = 0; i < itimes; i++)
                {
                    int rmax = Mathf.Max(range.x + 1, range.y + 1, range.z + 1);
                    Pos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
                    StartCoroutine(VoxelSmoothing(Pos, new Vector3i(rmax, rmax, rmax), activeMirror));
                    yield return null;
                }

                break;

            case OptShape.cylinder:
                for (int z = zPos - range.z; z < zPos + range.z; z++)
                {
                    for (int y = yPos - range.y * 2; y < yPos + range.y * 2; y++)
                    {
                        for (int x = xPos - range.x; x < xPos + range.x; x++)
                        {
                            float xDistance = x - xPos;
                            float yDistance = y - yPos;
                            float zDistance = z - zPos;

                            float distSquared = xDistance * xDistance / rangeX2 + zDistance * zDistance / rangeZ2;
                            if (distSquared < 1)
                            {
                                dismax = (int)SingleVoxelHandling(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler, materialSet);
                            }
                        }
                    }
                }

                dismax += adsrange;
                for (int i = 0; i < Mathf.Clamp((range.x + range.y + range.z) / 6, 3, 6); i++)
                {
                    Pos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
                    StartCoroutine(VoxelSmoothing(Pos, new Vector3i(dismax, dismax, dismax), activeMirror));
                    yield return null;
                }

                break;

            case OptShape.capsule:
                for (int z = zPos - range.z; z < zPos + range.z; z++)
                {
                    for (int y = yPos - range.y; y < yPos + range.y; y++)
                    {
                        for (int x = xPos - range.x; x < xPos + range.x; x++)
                        {
                            float xDistance = x - xPos;
                            float yDistance = y - yPos;
                            float zDistance = z - zPos;

                            float distSquared = xDistance * xDistance / rangeX2 + zDistance * zDistance / rangeZ2;
                            if (distSquared < 1)
                            {
                                SingleVoxelHandling(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler, materialSet);
                            }
                        }
                    }
                }

                int upxPos = (int)Pos.x;
                int upyPos = (int)Pos.y + range.y;
                int upzPos = (int)Pos.z;
                for (int z = upzPos - range.z; z < upzPos + range.z; z++)
                {
                    for (int y = upyPos; y < upyPos + range.y; y++)
                    {
                        for (int x = upxPos - range.x; x < upxPos + range.x; x++)
                        {
                            float xDistance = x - upxPos;
                            float yDistance = y - upyPos;
                            float zDistance = z - upzPos;

                            float distSquared = xDistance * xDistance / rangeX2 + yDistance * yDistance / rangeY2 + zDistance * zDistance / rangeZ2;
                            if (distSquared < 1)
                            {
                                SingleVoxelHandling(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler, materialSet);

                            }
                        }
                    }
                }

                int downxPos = (int)Pos.x;
                int downyPos = (int)Pos.y - range.y;
                int downzPos = (int)Pos.z;
                for (int z = downzPos - range.z; z < downzPos + range.z; z++)
                {
                    for (int y = downyPos - range.z; y < downyPos; y++)
                    {
                        for (int x = downxPos - range.z; x < downxPos + range.z; x++)
                        {
                            float xDistance = x - downxPos;
                            float yDistance = y - downyPos;
                            float zDistance = z - downzPos;

                            float distSquared = xDistance * xDistance / rangeX2 + yDistance * yDistance / rangeY2 + zDistance * zDistance / rangeZ2;
                            if (distSquared < 1)
                            {
                                SingleVoxelHandling(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler, materialSet);
                            }
                        }
                    }
                }

                dismax = Mathf.Max(range.y * 2, dismax);
                dismax += adsrange;
                for (int i = 0; i < Mathf.Clamp((range.x + range.y + range.z) / 6, 3, 6); i++)
                {
                    Pos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
                    StartCoroutine(VoxelSmoothing(Pos, new Vector3i(dismax, dismax, dismax), activeMirror));
                    yield return null;
                }
                break;
        }

        if (activeMirror)
        {
            Vector3 tempmpos = (CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, Pos));
            StartCoroutine(VoxelSetting(tempmpos, RotateEuler, materialSet, range, optshape, false));
        }

    }

    private void VoxelPainting(Vector3 pos, Vector3i range, MaterialSet materialset, bool activeMirror)
    {
        Vector3 tempPos = pos;
        //Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(pos) * VoxelWorldTransform.localScale.x;
        Vector3i tempPosi = (Vector3i)tempPos;

        int xPos = tempPosi.x;
        int yPos = tempPosi.y;
        int zPos = tempPosi.z;

        int rangeX2 = range.x * range.x;
        int rangeY2 = range.y * range.y;
        int rangeZ2 = range.z * range.z;

        for (int z = zPos - range.z; z < zPos + range.z; z++)
        {
            for (int y = yPos - range.y; y < yPos + range.y; y++)
            {
                for (int x = xPos - range.x; x < xPos + range.x; x++)
                {
                    float xDistance = x - xPos;
                    float yDistance = y - yPos;
                    float zDistance = z - zPos;

                    float distSquared = xDistance * xDistance / rangeX2 + yDistance * yDistance / rangeY2 + zDistance * zDistance / rangeZ2;
                    if (distSquared < 1)
                    {
                        SingleVoxelPaintHanding(x, y, z, materialset, distSquared);
                    }
                }
            }
        }

        if (activeMirror)
        {
            Vector3 mirrorAnchorPoint0 = trackAnchor.GetMirrorAnchorPoint0();
            Vector3 mirrorAnchorPoint1 = trackAnchor.GetMirrorAnchorPoint1();
            Vector3 mirrorAnchorPoint2 = trackAnchor.GetMirrorAnchorPoint2();
            Vector3 tempmpos = (CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, pos));
            VoxelPainting(tempmpos, range, materialset, false);
        }
    }

    private void DestroyVoxels(Vector3 Pos, Vector3 RotateEular, Vector3i range, OptShape optshape, bool activeMirror, bool useGPU)
    {
        MaterialSet emptyMaterialSet = new MaterialSet();
        recordBehaviour.WriteJsonFile(Pos, RotateEular, emptyMaterialSet, range, optshape, Time.time - appStartTime, activeMirror);
        if (useGPU)
        {
            VoxelSettingGPU(Pos, RotateEular, emptyMaterialSet, range, optshape, activeMirror);
        }
        else
        {
            StartCoroutine(VoxelSetting(Pos, RotateEular, emptyMaterialSet, range, optshape, activeMirror));
        }
    }

    private void CreateVoxels(Vector3 Pos, Vector3 RotateEular, MaterialSet materialSet, Vector3i range, OptShape optshape, bool activeMirror, bool useGPU)
    {
        recordBehaviour.WriteJsonFile(Pos, RotateEular, materialSet, range, optshape, Time.time - appStartTime, activeMirror);
        if (useGPU)
        {
            VoxelSettingGPU(Pos, RotateEular, materialSet, range, optshape, activeMirror);
        }
        else
        {
            StartCoroutine(VoxelSetting(Pos, RotateEular, materialSet, range, optshape, activeMirror));
        }
    }

    private void SmoothVoxels(Vector3 Pos, Vector3i range, bool activeMirror, bool useGPU)
    {
        recordBehaviour.WriteJsonFileSmooth(Pos, range, Time.time - appStartTime, activeMirror);
        if (useGPU)
        {
            VoxelSmoothingGPU(Pos, range, activeMirror);
        }
        else
        {
            StartCoroutine(VoxelSmoothing(Pos, range, activeMirror));
        }
    }

    private void PaintVoxels(Vector3 Pos, MaterialSet materialSet, Vector3i range, float amount, bool activeMirror)
    {
        recordBehaviour.WriteJsonFilePaint(Pos, materialSet, range, amount, Time.time - appStartTime, activeMirror);
        VoxelPainting(Pos, range, materialSet, activeMirror);
    }

    private void RestartTerrainVolumeData()
    {
        ProceduralTerrainVolume tempbpv = BasicProceduralVolume.GetComponent<ProceduralTerrainVolume>();
        tempbpv.ProceduralVoxelVR();
        recordBehaviour.ClearAll();
        Debug.Log("Voxel database has been restarted.");
    }

    private void SaveVDBFile()
    {
        terrainVolume.data.CommitChanges();
        Debug.Log("Voxel database has been saved.");
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it

        //float tempx = point.x - pivot.x;
        //float tempy = point.y - pivot.y;
        //float tempz = point.z - pivot.z;

        //float Sx = Mathf.Sin(angles.x);
        //float Sy = Mathf.Sin(angles.y);
        //float Sz = Mathf.Sin(angles.z);
        //float Cx = Mathf.Cos(angles.x);
        //float Cy = Mathf.Cos(angles.y);
        //float Cz = Mathf.Cos(angles.z);

        /* ZXY */
        //float m00 = Cy * Cz - Sx * Sy * Sz;
        //float m01 = -Cx * Sz;
        //float m02 = Cz * Sy + Cy * Sx * Sz;
        //float m10 = Cz * Sx * Sy + Cy * Sz;
        //float m11 = Cx * Cz;
        //float m12 = -Cy * Cz * Sx + Sy * Sz;
        //float m20 = -Cx * Sy;
        //float m21 = Sx;
        //float m22 = Cx * Cy;

        /* XYZ */
        //float m00 = Cy * Cz;
        //float m01 = -Cy * Sz;
        //float m02 = Sy;
        //float m10 = Cz * Sx * Sy + Cx * Sz;
        //float m11 = Cx * Cz - Sx * Sy * Sz;
        //float m12 = -Cy * Sx;
        //float m20 = -Cx * Cz * Sy + Sx * Sz;
        //float m21 = Cz * Sx + Cx * Sy * Sz;
        //float m22 = Cx * Cy;

        //float tempz1 = m00 * tempx + m01 * tempy + m02 * tempz;
        //float tempx1 = m10 * tempx + m11 * tempy + m12 * tempz;
        //float tempy1 = m20 * tempx + m21 * tempy + m22 * tempz;

        //return new Vector3(tempx1 + pivot.x, tempy1 + pivot.y, tempz1 + pivot.z);
    }

    public List<Vector3> calcDiffPos(Vector3 prePos, Vector3 nowPos, Vector3 scale)
    {
        List<Vector3> reList = new List<Vector3>();

        Vector3 Diff = nowPos - prePos;
        float MaxDiff = Mathf.Max(Mathf.Abs(Diff.x), Mathf.Abs(Diff.y), Mathf.Abs(Diff.z));
        float MaxScale = Mathf.Max(scale.x, scale.y, scale.z) / 2;
        float dnum = MaxDiff / MaxScale - 1;

        for (float lpos = 0; lpos < dnum; lpos++)
        {
            float tempx = prePos.x + Diff.x * (lpos / dnum);
            float tempy = prePos.y + Diff.y * (lpos / dnum);
            float tempz = prePos.z + Diff.z * (lpos / dnum);
            Vector3 temp = new Vector3(tempx, tempy, tempz);
            reList.Add(temp);
        }

        return reList;
    }

    public int GetActiveInfoPanelTimes()
    {
        return activeInfoPanelTimes;
    }

    public int GetOptRangeLeft()
    {
        return optRangeLeft;
    }

    public int GetOptRangeRight()
    {
        return optRangeRight;
    }

    public ControlPanel GetActivePanel()
    {
        return activePanel;
    }

    public OptState GetActiveStateLeft()
    {
        return activeStateLeft;
    }

    public OptState GetActiveStateRight()
    {
        return activeStateRight;
    }

    public OptShape GetActiveShape()
    {
        return activeShape;
    }

    public OptModePanel GetActiveOptModePanel()
    {
        return activeOptModePanel;
    }

    public float GetLeftChildPosZ()
    {
        return leftChildPos.z;
    }

    public float GetRightChildPosZ()
    {
        return rightChildPos.z;
    }

    public DrawPos GetActiveDrawPos()
    {
        return activeDrawPos;
    }

    public HandOpt GetActiveHandOpt()
    {
        return activeHandOpt;
    }

    public float Angle360(Vector3 v1, Vector3 v2, Vector3 n)
    {
        //  Acute angle [0,180]
        float angle = Vector3.Angle(v1, v2);

        //  -Acute angle [180,-179]
        float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(v1, v2)));
        float signed_angle = angle * sign;

        //  360 angle
        return (signed_angle + 180) % 360;
    }

    public void NetVoxelSetting(Vector3 Pos, Vector3 RotateEular, MaterialSet materialSet, Vector3i range, OptShape optshape, bool calcContinue, bool activeMirror)
    {
        if (calcContinue)
        {
            List<Vector3> tempDraw = calcDiffPos(preNetOptPos, Pos, (Vector3)range);
            if (tempDraw.Count > 0)
            {
                foreach (Vector3 temp in tempDraw)
                {
                    VoxelSettingGPU(temp, RotateEular, materialSet, range, optshape, activeMirror);
                    //StartCoroutine(VoxelSetting(temp, RotateEular, materialSet, range, optshape, activeMirror));
                }
                preNetOptPos = Pos;
            }
        }
        else
        {
            VoxelSettingGPU(Pos, RotateEular, materialSet, range, optshape, activeMirror);
            //StartCoroutine(VoxelSetting(Pos, RotateEular, materialSet, range, optshape, activeMirror));
            preNetOptPos = Pos;
        }
    }
    
    public void NetVoxelSmoothing(Vector3 Pos, Vector3i range, bool calcContinue, bool activeMirror)
    {
        if (calcContinue)
        {
            List<Vector3> tempDraw = calcDiffPos(preNetOptPos, Pos, (Vector3)range);
            if (tempDraw.Count > 0)
            {
                foreach (Vector3 temp in tempDraw)
                {
                    VoxelSmoothingGPU(temp, range, activeMirror);
                    //StartCoroutine(VoxelSmoothing(temp, range, activeMirror));
                }
                preNetOptPos = Pos;
            }
        }
        else
        {
            VoxelSmoothingGPU(Pos, range, activeMirror);
            //StartCoroutine(VoxelSmoothing(Pos, range, activeMirror));
            preNetOptPos = Pos;
        }
    }

    public void NetVoxelPainting(Vector3 Pos, MaterialSet materialSet, Vector3i range, bool calcContinue, bool activeMirror)
    {
        if (calcContinue)
        {
            List<Vector3> tempDraw = calcDiffPos(preNetOptPos, Pos, (Vector3)range);
            if (tempDraw.Count > 0)
            {
                foreach (Vector3 temp in tempDraw)
                {
                    VoxelPainting(temp, range, materialSet, activeMirror);
                }
                preNetOptPos = Pos;
            }
        }
        else
        {
            VoxelPainting(Pos, range, materialSet, activeMirror);
            preNetOptPos = Pos;
        }
    }

    public Color GetColorChoseLeft()
    {
        return colorChoseLeft;
    }

    public Color GetColorChoseRight()
    {
        return colorChoseRight;
    }
}