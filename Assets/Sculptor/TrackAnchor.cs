using UnityEngine;
using System.Collections;

using Cubiquity;

public class TrackAnchor : MonoBehaviour {

    public GameObject BasicProceduralVolume = null;

    public GameObject leftHandAnchor = null;
    public GameObject rightHandAnchor = null;

    public Texture boundTexture = null;
    public Texture planeTexture = null;

    public GameObject leftHand = null;
    public GameObject leftHandMesh = null;
    private GameObject leftHandChild = null;

    public GameObject rightHand = null;
    public GameObject rightHandMesh = null;
    private GameObject rightHandChild = null;

    private GameObject terrainWorld = null;

    private BoundIndicator boundIndicator;
    private ProceduralTerrainVolume proceduralTerrainVolume;

    //private GameObject colorCube = null;
    //private Vector3 colorCubeSize = new Vector3(0.2f, 0.2f, 0.2f);
    private float colorAlpha = 0.05f;
    private float colorChildAlpha = 0.3f;

    private HandBehaviour handBehaviour;
    private TerrainVolume terrainVolume;

    private int optRange;
    private OptShape activeShape, nowShape;

    private Vector3 rotateEuler = new Vector3(0, 0, 0);

    private Color materialColor;
    private Color materialChildColor;
    private Vector3 leftChildPosition = new Vector3(0, 0, 0); // change z
    private Vector3 rightChildPosition = new Vector3(0, 0, 0); // change z

    private GameObject twiceHand;
    private HandOpt activeHandOpt = HandOpt.singleOpt;

    private GameObject mirrorPlane;
    private GameObject mirrorChildPlane1;
    private GameObject mirrorChildPlane2;
    private GameObject mirrorAnchorPoint0;
    private GameObject mirrorAnchorPoint1;
    private GameObject mirrorAnchorPoint2;
    private Vector3 mirrorScale = new Vector3(30, 30, 30);

    private OptModePanel activeMode, nowMode;
    private OptState activeState;

    private float twoHandObjSizeMax = 60;

    //private ControlPanel showColorCube = ControlPanel.empty;
    //private Vector3 ColorBlackPoint = new Vector3(0, 0, 0);
    //private Color ColorChose = Color.white;

    // Use this for initialization
    void Start () {

        terrainVolume = BasicProceduralVolume.GetComponent<TerrainVolume>();

        handBehaviour = GetComponent<HandBehaviour>();

        terrainWorld = GameObject.CreatePrimitive(PrimitiveType.Cube);
        terrainWorld.transform.GetComponent<Renderer>().material.mainTexture = planeTexture;
        materialColor = terrainWorld.transform.GetComponent<Renderer>().material.color;
        materialColor.a = 0.6f;
        terrainWorld.transform.GetComponent<Renderer>().material.color = materialColor;
        terrainWorld.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        rightHandChild = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightHandChild.transform.position = rightChildPosition;
        rightHandChild.transform.parent = rightHand.transform;
        materialChildColor = rightHandChild.transform.GetComponent<Renderer>().material.color;
        materialChildColor.a = colorChildAlpha;
        rightHandChild.transform.GetComponent<Renderer>().material.color = materialChildColor;
        rightHandChild.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        leftHandChild = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftHandChild.transform.position = leftChildPosition;
        leftHandChild.transform.parent = leftHand.transform;
        materialChildColor = leftHandChild.transform.GetComponent<Renderer>().material.color;
        materialChildColor.a = colorChildAlpha;
        leftHandChild.transform.GetComponent<Renderer>().material.color = materialChildColor;
        leftHandChild.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        twiceHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        materialColor = twiceHand.transform.GetComponent<Renderer>().material.color;
        materialColor.a = colorChildAlpha;
        twiceHand.transform.GetComponent<Renderer>().material.color = materialColor;
        twiceHand.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        //colorCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //materialColor = colorCube.transform.GetComponent<Renderer>().material.color;
        //materialColor.a = colorChildAlpha;
        //colorCube.transform.GetComponent<Renderer>().material.color = materialColor;
        //colorCube.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
        //colorCube.transform.localScale = colorCubeSize;

        mirrorPlane = new GameObject();
        mirrorPlane.name = "MirrorPlane";

        mirrorChildPlane1 = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mirrorChildPlane1.transform.Rotate(0, 0, 0);
        mirrorChildPlane1.transform.localScale = mirrorScale;
        mirrorChildPlane1.transform.parent = mirrorPlane.transform;
        mirrorChildPlane1.transform.GetComponent<Renderer>().material.mainTexture = planeTexture;
        materialColor = mirrorChildPlane1.transform.GetComponent<Renderer>().material.color;
        materialColor.a = colorChildAlpha;
        mirrorChildPlane1.transform.GetComponent<Renderer>().material.color = materialColor;
        mirrorChildPlane1.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        mirrorChildPlane2 = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mirrorChildPlane2.transform.Rotate(0, 0, 180);
        mirrorChildPlane2.transform.localScale = mirrorScale;
        mirrorChildPlane2.transform.parent = mirrorPlane.transform;
        mirrorChildPlane2.transform.GetComponent<Renderer>().material.mainTexture = planeTexture;
        materialColor = mirrorChildPlane2.transform.GetComponent<Renderer>().material.color;
        materialColor.a = colorChildAlpha;
        mirrorChildPlane2.transform.GetComponent<Renderer>().material.color = materialColor;
        mirrorChildPlane2.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        mirrorAnchorPoint0 = new GameObject();
        mirrorAnchorPoint1 = new GameObject();
        mirrorAnchorPoint2 = new GameObject();
        //mirrorAnchorPoint0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //mirrorAnchorPoint1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //mirrorAnchorPoint2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mirrorAnchorPoint0.transform.position = new Vector3(0, 0, 0);
        mirrorAnchorPoint1.transform.position = new Vector3(1, 0, 0);
        mirrorAnchorPoint2.transform.position = new Vector3(0, 0, 1);
        mirrorAnchorPoint0.transform.parent = mirrorPlane.transform;
        mirrorAnchorPoint1.transform.parent = mirrorPlane.transform;
        mirrorAnchorPoint2.transform.parent = mirrorPlane.transform;

        mirrorPlane.transform.parent = terrainWorld.transform;

        proceduralTerrainVolume = BasicProceduralVolume.GetComponent<ProceduralTerrainVolume>();
        boundIndicator = proceduralTerrainVolume.gameObject.GetComponent<BoundIndicator>();
        boundIndicator.transform.GetComponent<Renderer>().material.mainTexture = boundTexture;
        Color tempBoundColor = boundIndicator.transform.GetComponent<Renderer>().material.color;
        tempBoundColor.a = colorChildAlpha;
        boundIndicator.transform.GetComponent<Renderer>().material.color = tempBoundColor;
        boundIndicator.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        leftHandChild.SetActive(true);
        rightHandChild.SetActive(true);
        twiceHand.SetActive(false);

        mirrorPlane.SetActive(false);
        activeMode = OptModePanel.sculptor;

        //colorCube.SetActive(false);

        var transparentLayer = LayerMask.NameToLayer("TransparentFX");
        terrainWorld.layer = transparentLayer;
        rightHandChild.layer = transparentLayer;
        leftHandChild.layer = transparentLayer;
        twiceHand.layer = transparentLayer;
        //colorCube.layer = transparentLayer;
    }

    // Update is called once per frame
    void Update () {

        optRange = handBehaviour.GetOptRange();

        nowShape = handBehaviour.GetActiveShape();
        if (nowShape != activeShape)
        {
            switch (nowShape)
            {
                case OptShape.cube:
                    UnityEngine.Object.Destroy(twiceHand.gameObject);
                    twiceHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case OptShape.sphere:
                    UnityEngine.Object.Destroy(twiceHand.gameObject);
                    twiceHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case OptShape.cylinder:
                    UnityEngine.Object.Destroy(twiceHand.gameObject);
                    twiceHand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                case OptShape.capsule:
                    UnityEngine.Object.Destroy(twiceHand.gameObject);
                    twiceHand = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
            }

            rightHandChild.transform.position = rightChildPosition;
            rightHandChild.transform.parent = rightHand.transform;
            materialChildColor = rightHandChild.transform.GetComponent<Renderer>().material.color;
            materialChildColor.a = 0.3f;
            rightHandChild.transform.GetComponent<Renderer>().material.color = materialChildColor;
            rightHandChild.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            leftHandChild.transform.position = leftChildPosition;
            leftHandChild.transform.parent = leftHand.transform;
            materialChildColor = leftHandChild.transform.GetComponent<Renderer>().material.color;
            materialChildColor.a = 0.3f;
            leftHandChild.transform.GetComponent<Renderer>().material.color = materialChildColor;
            leftHandChild.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            activeShape = nowShape;

            if (activeHandOpt == HandOpt.pairOpt)
            {
                twiceHand.SetActive(true);
            }
            else
            {
                twiceHand.SetActive(false);
            }

            var transparentLayer = LayerMask.NameToLayer("TransparentFX");
            terrainWorld.layer = transparentLayer;
            rightHandChild.layer = transparentLayer;
            leftHandChild.layer = transparentLayer;
            twiceHand.layer = transparentLayer;
        }

        activeState = handBehaviour.GetActiveState();
        switch (activeState)
        {
            case OptState.create:
                leftHandMesh.transform.localEulerAngles = (new Vector3(-90, 90, 90));
                rightHandMesh.transform.localEulerAngles = (new Vector3(-90, 90, 90));
                break;
            case OptState.delete:
                leftHandMesh.transform.localEulerAngles = (new Vector3(180, 90, 90));
                rightHandMesh.transform.localEulerAngles = (new Vector3(180, 90, 90));
                break;
            case OptState.smooth:
                leftHandMesh.transform.localEulerAngles = (new Vector3(90, 90, 90));
                rightHandMesh.transform.localEulerAngles = (new Vector3(90, 90, 90));
                break;
            case OptState.paint:
                leftHandMesh.transform.localEulerAngles = (new Vector3(0, 90, 90));
                rightHandMesh.transform.localEulerAngles = (new Vector3(0, 90, 90));
                break;
        }

        // mirror
        nowMode = handBehaviour.GetActiveOptModePanel();
        if (nowMode != activeMode)
        {
            if (nowMode == OptModePanel.mirror)
            {
                mirrorPlane.SetActive(true);
            }
            else
            {
                mirrorPlane.SetActive(false);
            }

            activeMode = nowMode;
        }

        terrainWorld.transform.position = terrainVolume.transform.position;
        terrainWorld.transform.rotation = terrainVolume.transform.rotation;
        terrainWorld.transform.localScale = terrainVolume.transform.localScale;

        // child first
        leftChildPosition.z = handBehaviour.GetLeftChildPosZ();
        rightChildPosition.z = handBehaviour.GetRightChildPosZ();

        leftHandChild.transform.localPosition = leftChildPosition;
        rightHandChild.transform.localPosition = rightChildPosition;

        leftHand.transform.position = leftHandAnchor.transform.position;
        leftHand.transform.rotation = leftHandAnchor.transform.rotation;
        leftHand.transform.localScale = terrainVolume.transform.localScale * optRange;

        rightHand.transform.position = rightHandAnchor.transform.position;
        rightHand.transform.rotation = rightHandAnchor.transform.rotation;
        rightHand.transform.localScale = terrainVolume.transform.localScale * optRange;

        Vector3 temp = rightHandChild.transform.position - leftHandChild.transform.position;
        float tempvx = Mathf.Abs(Mathf.Min(temp.x, twoHandObjSizeMax * terrainVolume.transform.localScale.x));
        float tempvy = Mathf.Abs(Mathf.Min(temp.y, twoHandObjSizeMax * terrainVolume.transform.localScale.y));
        float tempvz = Mathf.Abs(Mathf.Min(temp.z, twoHandObjSizeMax * terrainVolume.transform.localScale.z));
        //twiceHand.transform.rotation = Quaternion.Euler(0, 0, 90);
        twiceHand.transform.position = leftHandChild.transform.position + temp / 2;
        twiceHand.transform.localScale = new Vector3(tempvx, tempvy, tempvz);

        // twice hand
        HandOpt tempActiveHandOpt = handBehaviour.GetActiveHandOpt();
        if (tempActiveHandOpt != activeHandOpt)
        {
            if (tempActiveHandOpt == HandOpt.pairOpt)
            {
                twiceHand.SetActive(true);
            }
            else
            {
                twiceHand.SetActive(false);
            }
        }
        activeHandOpt = tempActiveHandOpt;

        // bound

        //if (IsHandInVolume(true) && IsHandInVolume(false))
        //{
        //    boundIndicator.Hide();
        //}
        //else
        //{
        //    boundIndicator.Show();
        //}
        boundIndicator.Show();

        // color 
        /*ControlPanel tempShowColorCube = handBehaviour.GetActivePanel();
        if (tempShowColorCube != showColorCube)
        {
            if (tempShowColorCube == ControlPanel.color)
            {
                colorCube.SetActive(true);
                DrawPos tempDrawPos = handBehaviour.GetActiveDrawPos();
                if (tempDrawPos == DrawPos.left)
                {
                    colorCube.transform.position = leftHandAnchor.transform.position;
                }
                else
                {
                    colorCube.transform.position = rightHandAnchor.transform.position;
                }
                ColorBlackPoint = colorCube.transform.position - colorCubeSize / 2;
            }
            else
            {
                colorCube.SetActive(false);
            }
            showColorCube = tempShowColorCube;
        }
        if (tempShowColorCube == ControlPanel.color)
        {
            DrawPos tempDrawPos = handBehaviour.GetActiveDrawPos();
            Vector3 tempPosV;
            float tempPosLX, tempPosLY, tempPosLZ;
            if (tempDrawPos == DrawPos.left)
            {
                tempPosV = leftHandAnchor.transform.position;
                tempPosLX = Mathf.Clamp(tempPosV.x, ColorBlackPoint.x, ColorBlackPoint.x + colorCubeSize.x);
                tempPosLY = Mathf.Clamp(tempPosV.y, ColorBlackPoint.y, ColorBlackPoint.y + colorCubeSize.y);
                tempPosLZ = Mathf.Clamp(tempPosV.z, ColorBlackPoint.z, ColorBlackPoint.z + colorCubeSize.z);

                leftHandChild.transform.position = new Vector3(tempPosLX, tempPosLY, tempPosLZ);
            }
            else
            {
                tempPosV = rightHandAnchor.transform.position;
                tempPosLX = Mathf.Clamp(tempPosV.x, ColorBlackPoint.x, ColorBlackPoint.x + colorCubeSize.x);
                tempPosLY = Mathf.Clamp(tempPosV.y, ColorBlackPoint.y, ColorBlackPoint.y + colorCubeSize.y);
                tempPosLZ = Mathf.Clamp(tempPosV.z, ColorBlackPoint.z, ColorBlackPoint.z + colorCubeSize.z);

                rightHandChild.transform.position = new Vector3(tempPosLX, tempPosLY, tempPosLZ);
            }
            ColorChose = new Color((tempPosLX - ColorBlackPoint.x) / colorCubeSize.x, (tempPosLY - ColorBlackPoint.y) / colorCubeSize.y, (tempPosLZ - ColorBlackPoint.z) / colorCubeSize.z);
            ColorChose.a = colorChildAlpha;

            leftHandChild.GetComponent<Renderer>().material.color = ColorChose;
            rightHandChild.GetComponent<Renderer>().material.color = ColorChose;
        }*/
    }

    public Vector3 GetRightChildPosition()
    {
        return rightHandChild.transform.position;
    }
    
    public Vector3 GetLeftChildPosition()
    {
        return leftHandChild.transform.position;
    }

    public Vector3 GetTwiceChildPosition()
    {
        return twiceHand.transform.position;
    }

    public Vector3 GetTwiceChildLocalScale()
    {
        return twiceHand.transform.localScale;
    }

    public void SetMirrorPlaneTransform(Vector3 pos, Quaternion rot)
    {
        //mirrorPlane.transform.position = pos;
        mirrorPlane.transform.rotation = rot;
    }

    public Transform GetMirrorPlaneTransform()
    {
        return mirrorPlane.transform;
    }

    public Vector3 GetMirrorAnchorPoint0()
    {
        return mirrorAnchorPoint0.transform.position;
    }

    public Vector3 GetMirrorAnchorPoint1()
    {
        return mirrorAnchorPoint1.transform.position;
    }

    public Vector3 GetMirrorAnchorPoint2()
    {
        return mirrorAnchorPoint2.transform.position;
    }
}
