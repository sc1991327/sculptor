using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cubiquity;

public enum ControlPanel { empty, main, color, replay, load, high };
public enum OptModePanel { sculptor, paint, replay, mirror };
public enum InfoPanel { empty, start, info};
public enum OptState { create, delete, smooth, paint };
public enum OptShape { cube, sphere, capsule, cylinder };
public enum DrawPos { left, right, twice };
public enum HandOpt { singleOpt, pairOpt, voxelWorldOpt };

public class HandBehaviour : MonoBehaviour {

    public GameObject BasicProceduralVolume = null;

    public GameObject leftHandAnchor = null;
    public GameObject rightHandAnchor = null;

    private TrackAnchor trackAnchor;
    private RecordBehaviour recordBehaviour;
    private HandMenuControl handMenuControl;

    private TerrainVolume terrainVolume;
    private ProceduralTerrainVolume proceduralTerrainVolume;
    private BoundIndicator boundIndicator;

    private MaterialSet emptyMaterialSet;
    private MaterialSet colorMaterialSet;

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
    private InfoPanel activeInfoPanel;
    private OptState activeState;
    private OptShape activeShape;
    private DrawPos activeDrawPos;
    private HandOpt activeHandOpt;

    private float buttonPreTime = 0.0f;
    private float ButtonTimeControlSingle = 0.3f;
    private float markTime;

    private int optRange = 4;
    private Vector3 tempDrawPosScaled;
    private Vector3 tempDrawRotate;
    private Vector3 tempDrawScale;

    private Vector3 rightRotateEuler;
    private Vector3 leftRotateEuler;

    private Vector3 leftChildPos = new Vector3(0, 0, 1);
    private Vector3 rightChildPos = new Vector3(0, 0, 1);

    private Color colorChose = Color.white;

    private float appStartTime;

    private bool checkOptContinueState = false;
    private bool checkPreOptContinueState = false;

    private float replayStartTime = 0.0f;

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

        terrainVolume = BasicProceduralVolume.GetComponent<TerrainVolume>();
        proceduralTerrainVolume = BasicProceduralVolume.GetComponent<ProceduralTerrainVolume>();
        boundIndicator = proceduralTerrainVolume.gameObject.GetComponent<BoundIndicator>();

        if (leftHandAnchor == null || rightHandAnchor == null || BasicProceduralVolume == null)
        {
            Debug.LogError("Please assign the GameObject first.");
        }
        if (terrainVolume == null)
        {
            Debug.LogError("This 'BasicProceduralVolume' script should be attached to a game object with a TerrainVolume component");
        }

        VoxelWorldTransform = terrainVolume.transform;

        trackAnchor = GetComponent<TrackAnchor>();
        recordBehaviour = GetComponent<RecordBehaviour>();
        handMenuControl = GetComponent<HandMenuControl>();

        // empty
        emptyMaterialSet = new MaterialSet();
        emptyMaterialSet.weights[3] = 0;
        emptyMaterialSet.weights[2] = 0;
        emptyMaterialSet.weights[1] = 0;
        emptyMaterialSet.weights[0] = 0;

        // color control
        colorMaterialSet = new MaterialSet();
        colorMaterialSet.weights[3] = 0;    // black
        colorMaterialSet.weights[2] = 255;  // b
        colorMaterialSet.weights[1] = 255;  // g
        colorMaterialSet.weights[0] = 255;  // r

        activePanel = ControlPanel.empty;
        activePanelContinue = false;
        activeOptModePanel = OptModePanel.sculptor;
        activeState = OptState.create;

        markTime = Time.time;
        activeInfoPanel = InfoPanel.start;

        activeShape = OptShape.sphere;
        activeHandOpt = HandOpt.singleOpt;

        rightRotateEuler = new Vector3(0, 0, 0);
        leftRotateEuler = new Vector3(0, 0, 0);

        VoxelWorldCenterPos = terrainVolume.transform.position;
        VoxelWorldLeftHandPos = new Vector3(0, 0, 0);
    }
	
    private bool IsHandInVolume(bool leftHand = true)
    {
        if(proceduralTerrainVolume == null)
        {
            return false;
        }

        Vector3 handPos = leftHand ? trackAnchor.GetLeftChildPosition() : trackAnchor.GetRightChildPosition();

        //todo: worldSpace
        return (
                handPos.x <= proceduralTerrainVolume.planetRadius * VoxelWorldTransform.localScale.x && handPos.x >= -proceduralTerrainVolume.planetRadius * VoxelWorldTransform.localScale.x && 
                handPos.y <= proceduralTerrainVolume.planetRadius * VoxelWorldTransform.localScale.y && handPos.y >= -proceduralTerrainVolume.planetRadius * VoxelWorldTransform.localScale.y &&
                handPos.z <= proceduralTerrainVolume.planetRadius * VoxelWorldTransform.localScale.z && handPos.z >= -proceduralTerrainVolume.planetRadius * VoxelWorldTransform.localScale.z);
    }

	// Update is called once per frame
	void Update () {

        // Bail out if we're not attached to a terrain.
        if (terrainVolume == null)
        {
            return;
        }

        // show the begining HMD info
        if (activeInfoPanel != InfoPanel.empty && (Time.time - markTime) > 5)
        {
            activeInfoPanel = InfoPanel.empty;
            markTime = Time.time;
        }


        if(IsHandInVolume(true) && IsHandInVolume(false))
        {
            boundIndicator.Hide();
        }
        else
        {
            boundIndicator.Show();
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
        int tempTouchID = handMenuControl.GetTouchID();
        Debug.Log("TouchID:" + tempTouchID);
        switch (tempTouchID)
        {
            case 0:
                // color choose panel
                activePanel = ControlPanel.color;
                break;
            case 1:
                // sculptor mode
                activeOptModePanel = OptModePanel.sculptor;
                break;
            case 2:
                // paint mode
                activeOptModePanel = OptModePanel.paint;
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
                //Replay file choose panel
                activePanel = ControlPanel.replay;
                break;
            case 7:
                //High operators mode choose panel
                activePanel = ControlPanel.high;
                break;
            case 8:
                //Save
                SaveVDBFile();
                activePanel = ControlPanel.empty;
                activePanelContinue = true;
                break;
            case 9:
                //Load files choose panel
                activePanel = ControlPanel.load;
                break;
        }
    }

    private void colorPanelHandleOVRInput()
    {
        // chose color
        int tempTouchID = handMenuControl.GetTouchID();
        if (activePanel == ControlPanel.color && tempTouchID >= 0)
        {
            Color tempcolor = handMenuControl.colorColorList[tempTouchID];

            if (colorChose != tempcolor)
            {
                float temptotal = tempcolor.r + tempcolor.g + tempcolor.b;
                if (temptotal == 0)
                {
                    colorMaterialSet.weights[3] = 255;    // black
                    colorMaterialSet.weights[2] = 0;  // b
                    colorMaterialSet.weights[1] = 0;  // g
                    colorMaterialSet.weights[0] = 0;  // r
                }
                else
                {
                    colorMaterialSet.weights[3] = 0;    // black
                    colorMaterialSet.weights[2] = (byte)(int)(254 * (tempcolor.b / temptotal));  // b
                    colorMaterialSet.weights[1] = (byte)(int)(254 * (tempcolor.g / temptotal));  // g
                    colorMaterialSet.weights[0] = (byte)(int)(254 * (tempcolor.r / temptotal));  // r
                }
                //Debug.Log("ColorMaterial: " + colorMaterialSet.weights[0] + ", " + colorMaterialSet.weights[1] + ", " + colorMaterialSet.weights[2] + ", " + colorMaterialSet.weights[3]);
                colorChose = tempcolor;
            }
        }

    }

    private void replayPanelHandleOVRInput()
    {
        // execute once for each touch

        int tempTouchID = handMenuControl.GetTouchID();
        if (tempTouchID >= 0 && tempTouchID < recordBehaviour.recordFileNames.Count)
        {
            recordBehaviour.ReadJsonFile(recordBehaviour.recordFileNames[tempTouchID]);

            activeOptModePanel = OptModePanel.replay;
            replayStartTime = Time.time;

            activePanel = ControlPanel.empty;
        }
    }

    private void loadPanelHandleOVRInput()
    {
        int tempTouchID = handMenuControl.GetTouchID();
        if (tempTouchID >= 0 && tempTouchID < recordBehaviour.loadFileNames.Count)
        {
            proceduralTerrainVolume.LoadVDBFile(recordBehaviour.loadFileNames[tempTouchID]);

            activePanel = ControlPanel.empty;
        }
    }

    private void highPanelHandleOVRInput()
    {
        int tempTouchID = handMenuControl.GetTouchID();
        Debug.Log("TouchID:" + tempTouchID);
        switch (tempTouchID)
        {
            case 0:
                // mirror sculptor panel
                activeOptModePanel = OptModePanel.mirror;
                break;
        }
    }

    private void sculptorOptModePanelHandleOVRInput()
    {
        checkOptContinueState = false;

        if (Axis1D_LT > 0 && Axis1D_RT > 0)
        {
            // global position/rotation/scaling
            if (activeHandOpt != HandOpt.voxelWorldOpt)
            {
                // first
                VoxelWorldCenterPos = terrainVolume.transform.position;
                VoxelWorldLeftHandPos = leftChildPosition;

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

                terrainVolume.transform.position = VoxelWorldCenterPos + (leftChildPosition - VoxelWorldLeftHandPos);
                terrainVolume.transform.rotation = Quaternion.FromToRotation(VoxelWorldPreAngleDir, VoxelWorldNowAngleDir) * VoxelWorldBasicAngle;
                terrainVolume.transform.localScale = VoxelWorldBasicScale * (VoxelWorldNowScale / VoxelWorldPreScale);
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
                StateHandleOVRInput(DrawPos.twice, false);
                buttonPreTime = Time.time;
            }

            // one hand operator
            activeHandOpt = HandOpt.singleOpt;
            if (activeShape != OptShape.cube && activeShape != OptShape.sphere)
            {
                activeShape = OptShape.sphere;
            }

            if (Axis1D_LB > 0 && Axis1D_LT > 0 && optRange < 10)
            {
                activeShape = OptShape.sphere;
                activeState = OptState.smooth;
                StateHandleOVRInput(DrawPos.left, false);
            }
            else if (Axis1D_LB > 0)
            {
                activeState = OptState.create;
                StateHandleOVRInput(DrawPos.left, false);
            }
            else if (Axis1D_LT > 0)
            {
                activeState = OptState.delete;
                StateHandleOVRInput(DrawPos.left, false);
            }

            if (Axis1D_RB > 0 && Axis1D_RT > 0 && optRange < 10)
            {
                activeState = OptState.smooth;
                StateHandleOVRInput(DrawPos.right, false);
            }
            else if (Axis1D_RB > 0)
            {
                activeState = OptState.create;
                StateHandleOVRInput(DrawPos.right, false);
            }
            else if (Axis1D_RT > 0)
            {
                activeState = OptState.delete;
                StateHandleOVRInput(DrawPos.right, false);
            }

            // size
            if ((Axis2D_LB_Up || Axis2D_RB_Up) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
            {
                if (optRange < 10)
                {
                    optRange += 2;
                }
                buttonPreTime = Time.time;
            }
            if ((Axis2D_LB_Down || Axis2D_RB_Down) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
            {
                if (optRange >= 4)
                {
                    optRange -= 2;
                }
                buttonPreTime = Time.time;
            }

            // shape
            if ((Axis2D_LB_Left || Axis2D_RB_Left || Axis2D_LB_Right || Axis2D_RB_Right) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
            {
                if (activeShape == OptShape.cube)
                {
                    activeShape = OptShape.sphere;
                }
                else
                {
                    activeShape = OptShape.cube;
                }
                buttonPreTime = Time.time;
            }

            //if (Axis2D_L.y >= 0)
            //{
            //    leftChildPos.z = Axis2D_L.y * 20;
            //}
            //if (Axis2D_R.y >= 0)
            //{
            //    rightChildPos.z = Axis2D_R.y * 20;
            //}
        }

        if (checkPreOptContinueState == true && checkOptContinueState == false)
        {
            recordBehaviour.NewDo();
        }
        checkPreOptContinueState = checkOptContinueState;
    }

    private void paintOptModePanelHandleOVRInput()
    {
        if (Axis1D_LB > 0)
        {
            activeState = OptState.paint;
            StateHandleOVRInput(DrawPos.left, false);
        }
        else if (Axis1D_RB > 0)
        {
            activeState = OptState.paint;
            StateHandleOVRInput(DrawPos.right, false);
        }

    }

    private void mirrorOptModePanelHandleOVRInput()
    {
        // mirror only support one hand operator
        activeHandOpt = HandOpt.singleOpt;
        if (activeShape != OptShape.cube && activeShape != OptShape.sphere)
        {
            activeShape = OptShape.sphere;
        }

        if (Axis1D_LB > 0 && Axis1D_LT > 0 && optRange < 10)
        {
            activeShape = OptShape.sphere;
            activeState = OptState.smooth;
            StateHandleOVRInput(DrawPos.left, true);
        }
        else if (Axis1D_LB > 0)
        {
            activeState = OptState.create;
            StateHandleOVRInput(DrawPos.left, true);
        }
        else if (Axis1D_LT > 0)
        {
            activeState = OptState.delete;
            StateHandleOVRInput(DrawPos.left, true);
        }

        if (Axis1D_RB > 0 && Axis1D_RT > 0 && optRange < 10)
        {
            activeState = OptState.smooth;
            StateHandleOVRInput(DrawPos.right, true);
        }
        else if (Axis1D_RB > 0)
        {
            activeState = OptState.create;
            StateHandleOVRInput(DrawPos.right, true);
        }
        else if (Axis1D_RT > 0)
        {
            activeState = OptState.delete;
            StateHandleOVRInput(DrawPos.right, true);
        }

        // size
        if ((Axis2D_LB_Up || Axis2D_RB_Up) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            if (optRange < 10)
            {
                optRange += 2;
            }
            buttonPreTime = Time.time;
        }
        if ((Axis2D_LB_Down || Axis2D_RB_Down) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            if (optRange >= 4)
            {
                optRange -= 2;
            }
            buttonPreTime = Time.time;
        }

        // shape
        if ((Axis2D_LB_Left || Axis2D_RB_Left || Axis2D_LB_Right || Axis2D_RB_Right) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            if (activeShape == OptShape.cube)
            {
                activeShape = OptShape.sphere;
            }
            else
            {
                activeShape = OptShape.cube;
            }
            buttonPreTime = Time.time;
        }
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

                    VoxelSetting(Pos, RotateEuler, materialSet, range, optshape, activeMirror);
                }
                else
                {
                    Vector3 Pos = new Vector3(tempVSO.PosX, tempVSO.PosY, tempVSO.PosZ);
                    Vector3i range = new Vector3i(tempVSO.RangeX, tempVSO.RangeY, tempVSO.RangeZ);
                    bool activeMirror = tempVSO.ActiveMirror;

                    VoxelSmoothing(Pos, range, activeMirror);
                }

                recordBehaviour.ReplayVoxelStore.RemoveAt(0);
            }
        }
    }

    private void HandleOVRInput()
    {
        // Axis2D
        Axis2D_L = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Axis2D_R = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        Axis2D_LB_Center = OVRInput.Get(OVRInput.Button.PrimaryThumbstick);
        Axis2D_LB_Left = OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft);
        Axis2D_LB_Right = OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight);
        Axis2D_LB_Up = OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp);
        Axis2D_LB_Down = OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown);

        Axis2D_RB_Center = OVRInput.Get(OVRInput.Button.SecondaryThumbstick);
        Axis2D_RB_Left = OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft);
        Axis2D_RB_Right = OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight);
        Axis2D_RB_Up = OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp);
        Axis2D_RB_Down = OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown);

        // Axis1D
        Axis1D_LB = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch);
        Axis1D_LT = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch);

        Axis1D_RB = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch);
        Axis1D_RT = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch);

        // Button
        Button_A = OVRInput.Get(OVRInput.Button.One);
        Button_B = OVRInput.Get(OVRInput.Button.Two);
        Button_X = OVRInput.Get(OVRInput.Button.Three);
        Button_Y = OVRInput.Get(OVRInput.Button.Four);

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
            activeInfoPanel = InfoPanel.info;
            buttonPreTime = Time.time;
        }

        if (Button_Y && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activeDrawPos = DrawPos.left;
            activeInfoPanel = InfoPanel.info;
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
                colorPanelHandleOVRInput();
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
            case OptModePanel.paint:
                paintOptModePanelHandleOVRInput();
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

    private void StateHandleOVRInput(DrawPos drawPos, bool activeMirror)
    {
        checkOptContinueState = true;

        if (drawPos == DrawPos.left)
        {
            tempDrawPosScaled = leftChildPositionScaled;
            tempDrawRotate = leftRotateEuler;
            tempDrawScale = new Vector3(optRange / 2, optRange / 2, optRange / 2);
        }
        else if (drawPos == DrawPos.right)
        {
            tempDrawPosScaled = rightChildPositionScaled;
            tempDrawRotate = rightRotateEuler;
            tempDrawScale = new Vector3(optRange / 2, optRange / 2, optRange / 2);
        }
        else
        {
            tempDrawPosScaled = twiceChildPositionScale;
            tempDrawRotate = new Vector3(0, 0, 0);
            Vector3 tempTwiceScale = trackAnchor.GetTwiceChildLocalScale() / 2;
            tempDrawScale = (new Vector3(tempTwiceScale.x / VoxelWorldTransform.localScale.x, tempTwiceScale.y / VoxelWorldTransform.localScale.y, tempTwiceScale.z / VoxelWorldTransform.localScale.z));
        }

        switch (activeState)
        {
            case OptState.create:
                CreateVoxels(tempDrawPosScaled, tempDrawRotate, colorMaterialSet, (Vector3i)tempDrawScale, activeShape, activeMirror);
                break;
            case OptState.delete:
                DestroyVoxels(tempDrawPosScaled, tempDrawRotate, (Vector3i)tempDrawScale, activeShape, activeMirror);
                break;
            case OptState.smooth:
                SmoothVoxels(tempDrawPosScaled, (Vector3i)tempDrawScale, activeMirror);
                break;
            case OptState.paint:
                PaintVoxels(tempDrawPosScaled, 0, 5, 1, 0);
                break;
        }
    }

    /*
    private void testHandleOVRInput()
    {
        // please see https://developer.oculus.com/documentation/game-engines/latest/concepts/unity-ovrinput/ to known the mapping

        // Axis2D
        Axis2D_L = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Axis2D_R = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        Axis2D_LB_Center = OVRInput.Get(OVRInput.Button.PrimaryThumbstick);
        Axis2D_LB_Left = OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft);
        Axis2D_LB_Right = OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight);
        Axis2D_LB_Up = OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp);
        Axis2D_LB_Down = OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown);

        Axis2D_RB_Center = OVRInput.Get(OVRInput.Button.SecondaryThumbstick);
        Axis2D_RB_Left = OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft);
        Axis2D_RB_Right = OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight);
        Axis2D_RB_Up = OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp);
        Axis2D_RB_Down = OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown);

        // Axis1D
        Axis1D_LB = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch);
        Axis1D_LT = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch);

        Axis1D_RB = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch);
        Axis1D_RT = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch);

        // Button
        Button_A = OVRInput.Get(OVRInput.Button.One);
        Button_B = OVRInput.Get(OVRInput.Button.Two);
        Button_X = OVRInput.Get(OVRInput.Button.Three);
        Button_Y = OVRInput.Get(OVRInput.Button.Four);

        if (Axis1D_LB > 0)
        {
            colorMaterialSet.weights[2] = 255;
            colorMaterialSet.weights[1] = 0;
            colorMaterialSet.weights[0] = 0;
            CreateVoxels((Vector3i)leftPosition, colorMaterialSet, drawRange / 2, -1);
        }

        if (Axis1D_RB > 0)
        {
            colorMaterialSet.weights[2] = 0;
            colorMaterialSet.weights[1] = 255;
            colorMaterialSet.weights[0] = 0;
            CreateVoxels((Vector3i)rightPosition, colorMaterialSet, smoothRange / 2, 1);
        }

        if (Axis1D_LT > 0)
        {
            DestroyVoxels((Vector3i)leftPosition, drawRange / 2, -1);
        }

        if (Axis1D_RT > 0)
        {
            SmoothVoxels((Vector3i)rightPosition, smoothRange / 2);
        }

        if (Button_A)
        {
            if(smoothRange > 2 && preButtonState == false)
            {
                smoothRange -= 2;
                //OVRInput.SetControllerVibration(1.0f, 1.0f, OVRInput.Controller.RTouch);
            }
        }

        if (Button_B)
        {
            if (smoothRange < 10 && preButtonState == false)
            {
                smoothRange += 2;
                //OVRInput.SetControllerVibration(1.0f, 1.0f, OVRInput.Controller.RTouch);
            }
        }

        if (Button_X)
        {
            if (drawRange > 2 && preButtonState == false)
            {
                drawRange -= 2;
                //OVRInput.SetControllerVibration(1.0f, 1.0f, OVRInput.Controller.LTouch);
            }
        }

        if (Button_Y)
        {
            if (drawRange < 10 && preButtonState == false)
            {
                drawRange += 2;
                //OVRInput.SetControllerVibration(1.0f, 1.0f, OVRInput.Controller.LTouch);
            }
        }

        // the end to record the state
        if (Button_A || Button_B || Button_X || Button_Y)
        {
            preButtonState = true;
        }
        else
        {
            preButtonState = false;
        }

    }
    */

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

    private void VoxelSmoothing(Vector3 pos, Vector3i range, bool activeMirror)
    {
        Transform mirrorTrans = trackAnchor.GetMirrorPlaneTransform();
        Vector3 mirrorAnchorPoint0 = trackAnchor.GetMirrorAnchorPoint0();
        Vector3 mirrorAnchorPoint1 = trackAnchor.GetMirrorAnchorPoint1();
        Vector3 mirrorAnchorPoint2 = trackAnchor.GetMirrorAnchorPoint2();

        Region vRegion = new Region((int)pos.x - range.x, (int)pos.y - range.y, (int)pos.z - range.z, (int)pos.x + range.x, (int)pos.y + range.y, (int)pos.z + range.z);
        for (int tempX = vRegion.lowerCorner.x; tempX <= vRegion.upperCorner.x; ++tempX)
        {
            for (int tempY = vRegion.lowerCorner.y; tempY <= vRegion.upperCorner.y; ++tempY)
            {
                for (int tempZ = vRegion.lowerCorner.z; tempZ <= vRegion.upperCorner.z; ++tempZ)
                {
                    // only support 4 material channel
                    MaterialSet tempMaterialSet = terrainVolume.data.GetVoxel(tempX, tempY, tempZ);
                    for (uint tempM = 0; tempM < 4; tempM++)
                    {
                        int originalMaterialWeight = tempMaterialSet.weights[tempM];

                        int sum = 0;
                        sum += terrainVolume.data.GetVoxel(tempX, tempY, tempZ).weights[tempM];
                        sum += terrainVolume.data.GetVoxel(tempX + 1, tempY, tempZ).weights[tempM];
                        sum += terrainVolume.data.GetVoxel(tempX - 1, tempY, tempZ).weights[tempM];
                        sum += terrainVolume.data.GetVoxel(tempX, tempY + 1, tempZ).weights[tempM];
                        sum += terrainVolume.data.GetVoxel(tempX, tempY - 1, tempZ).weights[tempM];
                        sum += terrainVolume.data.GetVoxel(tempX, tempY, tempZ + 1).weights[tempM];
                        sum += terrainVolume.data.GetVoxel(tempX, tempY, tempZ - 1).weights[tempM];

                        int avg = (int)((float)(sum) / 7.0f + 0.5f);
                        avg = Mathf.Clamp(avg, 0, 255);
                        tempMaterialSet.weights[tempM] = (byte)avg;
                    }

                    MaterialSet tempOld = terrainVolume.data.GetVoxel(tempX, tempY, tempZ);
                    if (!CompareMaterialSet(tempMaterialSet, tempOld))
                    {
                        recordBehaviour.PushOperator(new VoxelOpt(new Vector3i(tempX, tempY, tempZ), tempMaterialSet, tempOld));
                        terrainVolume.data.SetVoxel(tempX, tempY, tempZ, tempMaterialSet);
                    }
                    //terrainVolume.data.SetVoxel(tempX, tempY, tempZ, tempMaterialSet);
                }
            }
        }

        if (activeMirror)
        {
            Vector3 tempmpos = (CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, pos));
            VoxelSmoothing(tempmpos, range, false);
        }

    }

    private void VoxelSetting(Vector3 Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape, bool activeMirror)
    {
        int xPos = (int)Pos.x;
        int yPos = (int)Pos.y;
        int zPos = (int)Pos.z;

        int rangeX2 = range.x * range.x;
        int rangeY2 = range.y * range.y;
        int rangeZ2 = range.z * range.z;

        Transform mirrorTrans = trackAnchor.GetMirrorPlaneTransform();
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
                            Vector3 temp = RotatePointAroundPivot(new Vector3(x, y, z),new Vector3(xPos, yPos, zPos), RotateEuler);
                            temp = VoxelWorldTransform.InverseTransformPoint(temp) * VoxelWorldTransform.localScale.x;

                            Vector3i tempi = (Vector3i)(temp);
                            MaterialSet tempOld = terrainVolume.data.GetVoxel(tempi.x, tempi.y, tempi.z);
                            if (!CompareMaterialSet(materialSet, tempOld))
                            {
                                recordBehaviour.PushOperator(new VoxelOpt(tempi, materialSet, tempOld));
                                terrainVolume.data.SetVoxel(tempi.x, tempi.y, tempi.z, materialSet);
                            }

                            if (activeMirror)
                            {
                                Vector3i tempmi = (Vector3i)(CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, temp));
                                MaterialSet tempmOld = terrainVolume.data.GetVoxel(tempmi.x, tempmi.y, tempmi.z);
                                if (!CompareMaterialSet(materialSet, tempmOld))
                                {
                                    recordBehaviour.PushOperator(new VoxelOpt(tempmi, materialSet, tempmOld));
                                    terrainVolume.data.SetVoxel(tempmi.x, tempmi.y, tempmi.z, materialSet);
                                }
                            }
                        }
                    }
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
                                Vector3 temp = RotatePointAroundPivot(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler);
                                temp = VoxelWorldTransform.InverseTransformPoint(temp) * VoxelWorldTransform.localScale.x;
                                Vector3i tempi = (Vector3i)(temp);

                                MaterialSet tempOld = terrainVolume.data.GetVoxel(tempi.x, tempi.y, tempi.z);
                                if (!CompareMaterialSet(materialSet, tempOld))
                                {
                                    recordBehaviour.PushOperator(new VoxelOpt(tempi, materialSet, tempOld));
                                    terrainVolume.data.SetVoxel(tempi.x, tempi.y, tempi.z, materialSet);
                                }

                                if (activeMirror)
                                {
                                    Vector3i tempmi = (Vector3i)(CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, temp));
                                    MaterialSet tempmOld = terrainVolume.data.GetVoxel(tempmi.x, tempmi.y, tempmi.z);
                                    if (!CompareMaterialSet(materialSet, tempmOld))
                                    {
                                        recordBehaviour.PushOperator(new VoxelOpt(tempmi, materialSet, tempmOld));
                                        terrainVolume.data.SetVoxel(tempmi.x, tempmi.y, tempmi.z, materialSet);
                                    }
                                }
                            }
                        }
                    }
                }
                VoxelSmoothing(new Vector3(xPos, yPos, zPos), range, activeMirror);
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
                                Vector3 temp = RotatePointAroundPivot(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler);
                                temp = VoxelWorldTransform.InverseTransformPoint(temp) * VoxelWorldTransform.localScale.x;
                                Vector3i tempi = (Vector3i)(temp);

                                MaterialSet tempOld = terrainVolume.data.GetVoxel(tempi.x, tempi.y, tempi.z);
                                if (!CompareMaterialSet(materialSet, tempOld))
                                {
                                    recordBehaviour.PushOperator(new VoxelOpt(tempi, materialSet, tempOld));
                                    terrainVolume.data.SetVoxel(tempi.x, tempi.y, tempi.z, materialSet);
                                }

                                if (activeMirror)
                                {
                                    Vector3i tempmi = (Vector3i)(CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, temp));
                                    MaterialSet tempmOld = terrainVolume.data.GetVoxel(tempmi.x, tempmi.y, tempmi.z);
                                    if (!CompareMaterialSet(materialSet, tempmOld))
                                    {
                                        recordBehaviour.PushOperator(new VoxelOpt(tempmi, materialSet, tempmOld));
                                        terrainVolume.data.SetVoxel(tempmi.x, tempmi.y, tempmi.z, materialSet);
                                    }
                                }
                            }
                        }
                    }
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
                                Vector3 temp = RotatePointAroundPivot(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler);
                                temp = VoxelWorldTransform.InverseTransformPoint(temp) * VoxelWorldTransform.localScale.x;
                                Vector3i tempi = (Vector3i)(temp);

                                MaterialSet tempOld = terrainVolume.data.GetVoxel(tempi.x, tempi.y, tempi.z);
                                if (!CompareMaterialSet(materialSet, tempOld))
                                {
                                    recordBehaviour.PushOperator(new VoxelOpt(tempi, materialSet, tempOld));
                                    terrainVolume.data.SetVoxel(tempi.x, tempi.y, tempi.z, materialSet);
                                }

                                if (activeMirror)
                                {
                                    Vector3i tempmi = (Vector3i)(CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, temp));
                                    MaterialSet tempmOld = terrainVolume.data.GetVoxel(tempmi.x, tempmi.y, tempmi.z);
                                    if (!CompareMaterialSet(materialSet, tempmOld))
                                    {
                                        recordBehaviour.PushOperator(new VoxelOpt(tempmi, materialSet, tempmOld));
                                        terrainVolume.data.SetVoxel(tempmi.x, tempmi.y, tempmi.z, materialSet);
                                    }
                                }
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
                                Vector3 temp = RotatePointAroundPivot(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler);
                                temp = VoxelWorldTransform.InverseTransformPoint(temp) * VoxelWorldTransform.localScale.x;
                                Vector3i tempi = (Vector3i)(temp);

                                MaterialSet tempOld = terrainVolume.data.GetVoxel(tempi.x, tempi.y, tempi.z);
                                if (!CompareMaterialSet(materialSet, tempOld))
                                {
                                    recordBehaviour.PushOperator(new VoxelOpt(tempi, materialSet, tempOld));
                                    terrainVolume.data.SetVoxel(tempi.x, tempi.y, tempi.z, materialSet);
                                }

                                if (activeMirror)
                                {
                                    Vector3i tempmi = (Vector3i)(CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, temp));
                                    MaterialSet tempmOld = terrainVolume.data.GetVoxel(tempmi.x, tempmi.y, tempmi.z);
                                    if (!CompareMaterialSet(materialSet, tempmOld))
                                    {
                                        recordBehaviour.PushOperator(new VoxelOpt(tempmi, materialSet, tempmOld));
                                        terrainVolume.data.SetVoxel(tempmi.x, tempmi.y, tempmi.z, materialSet);
                                    }
                                }
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
                                Vector3 temp = RotatePointAroundPivot(new Vector3(x, y, z), new Vector3(xPos, yPos, zPos), RotateEuler);
                                temp = VoxelWorldTransform.InverseTransformPoint(temp) * VoxelWorldTransform.localScale.x;
                                Vector3i tempi = (Vector3i)(temp);

                                MaterialSet tempOld = terrainVolume.data.GetVoxel(tempi.x, tempi.y, tempi.z);
                                if (!CompareMaterialSet(materialSet, tempOld))
                                {
                                    recordBehaviour.PushOperator(new VoxelOpt(tempi, materialSet, tempOld));
                                    terrainVolume.data.SetVoxel(tempi.x, tempi.y, tempi.z, materialSet);
                                }

                                if (activeMirror)
                                {
                                    Vector3i tempmi = (Vector3i)(CalcMirrorPos(mirrorAnchorPoint0, mirrorAnchorPoint1, mirrorAnchorPoint2, temp));
                                    MaterialSet tempmOld = terrainVolume.data.GetVoxel(tempmi.x, tempmi.y, tempmi.z);
                                    if (!CompareMaterialSet(materialSet, tempmOld))
                                    {
                                        recordBehaviour.PushOperator(new VoxelOpt(tempmi, materialSet, tempmOld));
                                        terrainVolume.data.SetVoxel(tempmi.x, tempmi.y, tempmi.z, materialSet);
                                    }
                                }
                            }
                        }
                    }
                }
                break;
        }

    }

    private void DestroyVoxels(Vector3 Pos, Vector3 RotateEular, Vector3i range, OptShape optshape, bool activeMirror)
    {
        MaterialSet emptyMaterialSet = new MaterialSet();
        recordBehaviour.WriteJsonFile(Pos, RotateEular, emptyMaterialSet, range, optshape, Time.time - appStartTime, activeMirror);
        VoxelSetting(Pos, RotateEular, emptyMaterialSet, range, optshape, activeMirror);
    }

    private void CreateVoxels(Vector3 Pos, Vector3 RotateEular, MaterialSet materialSet, Vector3i range, OptShape optshape, bool activeMirror)
    {
        recordBehaviour.WriteJsonFile(Pos, RotateEular, materialSet, range, optshape, Time.time - appStartTime, activeMirror);
        VoxelSetting(Pos, RotateEular, materialSet, range, optshape, activeMirror);
    }

    private void SmoothVoxels(Vector3 Pos, Vector3i range, bool activeMirror)
    {
        Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
        recordBehaviour.WriteJsonFileSmooth(tempPos, range, Time.time - appStartTime, activeMirror);
        //TerrainVolumeEditor.BlurTerrainVolume(terrainVolume, new Region(tempPosi.x - range.x, tempPosi.y - range.y, tempPosi.z - range.z, tempPosi.x + range.x, tempPosi.y + range.y, tempPosi.z + range.z));
        VoxelSmoothing(tempPos, range, activeMirror);
    }

    private void PaintVoxels(Vector3 Pos, float brushInnerRadius, float brushOuterRadius, float amount, uint materialIndex)
    {
        Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
        Vector3i tempPosi = (Vector3i)tempPos;
        TerrainVolumeEditor.PaintTerrainVolume(terrainVolume, Pos.x, Pos.y, Pos.z, brushInnerRadius, brushOuterRadius, amount, materialIndex);
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
    }

    public int GetOptRange()
    {
        return optRange;
    }

    public ControlPanel GetActivePanel()
    {
        return activePanel;
    }

    public InfoPanel GetActiveInfoPanel()
    {
        return activeInfoPanel;
    }

    public OptState GetActiveState()
    {
        return activeState;
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
}