using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cubiquity;

public enum ControlPanel { empty, main, state, shape, color, readfile };
public enum InfoPanel { empty, start, info};
public enum OptState { create, delete, smooth };
public enum OptShape { cube, sphere, capsule, cylinder };
public enum DrawPos {left, right, twice };
public enum HandOpt { singleOpt, pairOpt, voxelWorldOpt };

public class HandBehaviour : MonoBehaviour {

    public GameObject BasicProceduralVolume = null;

    public GameObject leftHandAnchor = null;
    public GameObject rightHandAnchor = null;

    private TrackAnchor trackAnchor;
    private RecordBehaviour recordBehaviour;

    private TerrainVolume terrainVolume;
    private ProceduralTerrainVolume proceduralTerrainVolume;
    private BoundIndicator boundIndicator;

    private MaterialSet emptyMaterialSet;
    private MaterialSet colorMaterialSet;

    private Transform VoxelWorldTransform;
    private Matrix4x4 VoxelWorldMatrix;

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
        colorMaterialSet.weights[1] = 0;  // g
        colorMaterialSet.weights[0] = 0;  // r

        activePanel = ControlPanel.empty;
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

        Color tempcolor = trackAnchor.GetColorChose();
        if (colorChose != tempcolor)
        {
            float temptotal = colorChose.r + colorChose.g + colorChose.b;
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
                colorMaterialSet.weights[2] = (byte)(int)(254 * (colorChose.b / temptotal));  // b
                colorMaterialSet.weights[1] = (byte)(int)(254 * (colorChose.g / temptotal));  // g
                colorMaterialSet.weights[0] = (byte)(int)(254 * (colorChose.r / temptotal));  // r
            }
            Debug.Log("ColorMaterial: " + colorMaterialSet.weights[0] + ", " + colorMaterialSet.weights[1] + ", " + colorMaterialSet.weights[2] + ", " + colorMaterialSet.weights[3]);
            colorChose = tempcolor;
        }

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
        checkOptContinueState = false;

        if (Axis1D_LT >0 && Axis1D_RT > 0)
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
            if((Axis2D_LB_Right || Axis2D_RB_Right) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
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
            else if((Axis2D_LB_Left || Axis2D_RB_Left) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
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
                StateHandleOVRInput(DrawPos.twice);
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
                StateHandleOVRInput(DrawPos.left);
            }
            else if (Axis1D_LB > 0)
            {
                activeState = OptState.create;
                StateHandleOVRInput(DrawPos.left);
            }
            else if (Axis1D_LT > 0)
            {
                activeState = OptState.delete;
                StateHandleOVRInput(DrawPos.left);
            }

            if (Axis1D_RB > 0 && Axis1D_RT > 0 && optRange < 10)
            {
                activeState = OptState.smooth;
                StateHandleOVRInput(DrawPos.right);
            }
            else if (Axis1D_RB > 0)
            {
                activeState = OptState.create;
                StateHandleOVRInput(DrawPos.right);
            }
            else if (Axis1D_RT > 0)
            {
                activeState = OptState.delete;
                StateHandleOVRInput(DrawPos.right);
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

    private void mainPanelHandleOVRInput()
    {
        if ((Axis2D_RB_Left || Axis2D_LB_Left) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activePanel = ControlPanel.shape;
            buttonPreTime = Time.time;
        }
        if ((Axis2D_RB_Right || Axis2D_LB_Right) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activePanel = ControlPanel.color;
            buttonPreTime = Time.time;
        }
        if ((Axis2D_RB_Up || Axis2D_LB_Up) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activePanel = ControlPanel.state;
            buttonPreTime = Time.time;
        }
        if (((Axis2D_RB_Down || Axis2D_LB_Down) || (Axis2D_RB_Center || Axis2D_LB_Center)) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activePanel = ControlPanel.empty;
            buttonPreTime = Time.time;
        }
    }

    private void statePanelHandleOVRInput()
    {
        if ((Axis2D_RB_Left || Axis2D_LB_Left) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activeState = OptState.delete;
            activePanel = ControlPanel.empty;
            buttonPreTime = Time.time;
        }
        if ((Axis2D_RB_Right || Axis2D_LB_Right) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activeState = OptState.smooth;
            activePanel = ControlPanel.empty;
            buttonPreTime = Time.time;
        }
        if ((Axis2D_RB_Up || Axis2D_LB_Up) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activeState = OptState.create;
            activePanel = ControlPanel.empty;
            buttonPreTime = Time.time;
        }
        if (((Axis2D_RB_Down || Axis2D_LB_Down) || (Axis2D_RB_Center || Axis2D_LB_Center)) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activePanel = ControlPanel.empty;
            buttonPreTime = Time.time;
        }
    }

    private void shapePanelHandleOVRInput()
    {
        if ((Axis2D_RB_Left || Axis2D_LB_Left) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
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
        if ((Axis2D_RB_Right || Axis2D_LB_Right) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
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
        if ((Axis2D_RB_Up || Axis2D_LB_Up) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            rightChildPos.z = 0;
            optRange += 2;
            buttonPreTime = Time.time;
        }
        if ((Axis2D_RB_Down || Axis2D_LB_Down) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            rightChildPos.z = 0;
            optRange -= 2;
            if (optRange < 2){
                optRange = 2;
            }
            buttonPreTime = Time.time;
        }
        if ((Axis2D_RB_Center || Axis2D_LB_Center) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activePanel = ControlPanel.empty;
            buttonPreTime = Time.time;
        }
    }

    private void colorPanelHandleOVRInput()
    {
        if ((Axis1D_LB > 0 || Axis1D_RB > 0) && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activePanel = ControlPanel.empty;
            buttonPreTime = Time.time;
        }
    }

    private void readfilePanelHandleOVRInput()
    {

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

        if (Button_A && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activeDrawPos = DrawPos.right;
            if (activePanel >= ControlPanel.readfile)
            {
                activePanel = ControlPanel.empty;
            }
            else
            {
                activePanel++;
            }
            buttonPreTime = Time.time;
        }
        if (Button_B && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            //activeDrawPos = DrawPos.right;
            //activeInfoPanel = InfoPanel.info;
            recordBehaviour.ReDo();
            buttonPreTime = Time.time;
        }

        if (Button_X && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            activeDrawPos = DrawPos.left;
            if (activePanel >= ControlPanel.readfile)
            {
                activePanel = ControlPanel.empty;
            }
            else
            {
                activePanel++;
            }
            buttonPreTime = Time.time;
        }
        if (Button_Y && (Time.time - buttonPreTime) > ButtonTimeControlSingle)
        {
            //activeDrawPos = DrawPos.left;
            //activeInfoPanel = InfoPanel.info;
            recordBehaviour.UnDo();
            buttonPreTime = Time.time;
        }

        if (Axis1D_RB > 0 || Axis1D_RT > 0)
        {
            activePanel = ControlPanel.empty;
        }

        switch (activePanel)
        {
            case ControlPanel.empty:
                emptyPanelHandleOVRInput();
                break;
            case ControlPanel.main:
                mainPanelHandleOVRInput();
                break;
            case ControlPanel.state:
                statePanelHandleOVRInput();
                break;
            case ControlPanel.shape:
                shapePanelHandleOVRInput();
                break;
            case ControlPanel.color:
                colorPanelHandleOVRInput();
                break;
            case ControlPanel.readfile:
                readfilePanelHandleOVRInput();
                break;
        }

        //Debug.Log("activePanel: " + activePanel + " activeState: " + activeState);

    }

    private void StateHandleOVRInput(DrawPos drawPos)
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
                CreateVoxels((Vector3i)tempDrawPosScaled, tempDrawRotate, colorMaterialSet, (Vector3i)tempDrawScale, activeShape);
                break;
            case OptState.delete:
                DestroyVoxels((Vector3i)tempDrawPosScaled, tempDrawRotate, (Vector3i)tempDrawScale, activeShape);
                break;
            case OptState.smooth:
                SmoothVoxels(tempDrawPosScaled, (Vector3i)tempDrawScale);
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

    private void VoxelSmoothing(Region vRegion)
    {
        //List<VoxelOpt> tempVoxelOpt;
        //for (int tempX = vRegion.lowerCorner.x; tempX <= vRegion.upperCorner.x; ++tempX)
        //{
        //    for (int tempY = vRegion.lowerCorner.y; tempY <= vRegion.upperCorner.y; ++tempY)
        //    {
        //        for (int tempZ = vRegion.lowerCorner.z; tempZ <= vRegion.upperCorner.z; ++tempZ)
        //        {
        //            VoxelOpt tempV = new VoxelOpt(new Vector3i(tempX, tempY, tempZ), terrainVolume.data.GetVoxel(tempX, tempY, tempZ), terrainVolume.data.GetVoxel(tempX, tempY, tempZ));
        //            tempVoxelOpt.Add(tempV);
        //        }
        //    }
        //}
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

                    terrainVolume.data.SetVoxel(tempX, tempY, tempZ, tempMaterialSet);
                }
            }
        }
        //for (int tempX = vRegion.lowerCorner.x; tempX <= vRegion.upperCorner.x; ++tempX)
        //{
        //    for (int tempY = vRegion.lowerCorner.y; tempY <= vRegion.upperCorner.y; ++tempY)
        //    {
        //        for (int tempZ = vRegion.lowerCorner.z; tempZ <= vRegion.upperCorner.z; ++tempZ)
        //        {
        //            terrainVolume.data.SetVoxel(tempX, tempY, tempZ, tempMaterialSet);
        //        }
        //    }
        //}

    }

    private void VoxelSetting(Vector3i Pos, Vector3 RotateEuler, MaterialSet materialSet, Vector3i range, OptShape optshape)
    {
        int xPos = Pos.x;
        int yPos = Pos.y;
        int zPos = Pos.z;

        int rangeX2 = range.x * range.x;
        int rangeY2 = range.y * range.y;
        int rangeZ2 = range.z * range.z;

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
                            }
                        }
                    }
                }
                VoxelSmoothing(new Region(xPos - range.x, yPos - range.y, zPos - range.z, xPos + range.x, yPos + range.y, zPos + range.z));
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
                            }
                        }
                    }
                }

                int upxPos = Pos.x;
                int upyPos = Pos.y + range.y;
                int upzPos = Pos.z;
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
                            }
                        }
                    }
                }

                int downxPos = Pos.x;
                int downyPos = Pos.y - range.y;
                int downzPos = Pos.z;
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
                            }
                        }
                    }
                }
                break;
        }

    }

    private void DestroyVoxels(Vector3i Pos, Vector3 RotateEular, Vector3i range, OptShape optshape)
    {
        MaterialSet emptyMaterialSet = new MaterialSet();
        recordBehaviour.Write(Pos, RotateEular, emptyMaterialSet, range, optshape, Time.time - appStartTime);
        VoxelSetting(Pos, RotateEular, emptyMaterialSet, range, optshape);
    }

    private void CreateVoxels(Vector3i Pos, Vector3 RotateEular, MaterialSet materialSet, Vector3i range, OptShape optshape)
    {
        recordBehaviour.Write(Pos, RotateEular, materialSet, range, optshape, Time.time - appStartTime);
        VoxelSetting(Pos, RotateEular, materialSet, range, optshape);
    }

    private void SmoothVoxels(Vector3 Pos, Vector3i range)
    {
        Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
        Vector3i tempPosi = (Vector3i)tempPos;
        recordBehaviour.WriteSmooth(new Region(tempPosi.x - range.x, tempPosi.y - range.y, tempPosi.z - range.z, tempPosi.x + range.x, tempPosi.y + range.y, tempPosi.z + range.z), Time.time - appStartTime);
        //TerrainVolumeEditor.BlurTerrainVolume(terrainVolume, new Region(tempPosi.x - range.x, tempPosi.y - range.y, tempPosi.z - range.z, tempPosi.x + range.x, tempPosi.y + range.y, tempPosi.z + range.z));
        VoxelSmoothing(new Region(tempPosi.x - range.x, tempPosi.y - range.y, tempPosi.z - range.z, tempPosi.x + range.x, tempPosi.y + range.y, tempPosi.z + range.z));
    }

    private void PaintVoxels(Vector3 Pos, float brushInnerRadius, float brushOuterRadius, float amount, uint materialIndex)
    {
        Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
        Vector3i tempPosi = (Vector3i)tempPos;
        TerrainVolumeEditor.PaintTerrainVolume(terrainVolume, Pos.x, Pos.y, Pos.z, brushInnerRadius, brushOuterRadius, amount, materialIndex);
    }

    private void SculptVoxels(Vector3 Pos, float brushInnerRadius, float brushOuterRadius, float amount)
    {
        Vector3 tempPos = VoxelWorldTransform.InverseTransformPoint(Pos) * VoxelWorldTransform.localScale.x;
        Vector3i tempPosi = (Vector3i)tempPos;
        TerrainVolumeEditor.SculptTerrainVolume(terrainVolume, Pos.x, Pos.y, Pos.z, brushInnerRadius, brushOuterRadius, amount);
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