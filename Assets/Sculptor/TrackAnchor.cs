using UnityEngine;
using System.Collections;

using Cubiquity;

public class TrackAnchor : MonoBehaviour {

    public GameObject BasicProceduralVolume = null;

    public GameObject leftHandAnchor = null;
    public GameObject rightHandAnchor = null;

    private GameObject leftHand = null;
    private GameObject leftHandChild = null;

    private GameObject rightHand = null;
    private GameObject rightHandChild = null;

    private GameObject terrainWorld = null;

    private GameObject colorCube = null;
    private Vector3 colorCubeSize = new Vector3(0.2f, 0.2f, 0.2f);
    private float colorAlpha = 0.05f;
    private float colorChildAlpha = 0.3f;

    private HandBehaviour handBehaviour;
    private TerrainVolume terrainVolume;
    private Transform VoxelWorldTransform;

    private int optRange;
    private OptShape activeShape, nowShape;

    private Vector3 rotateEuler = new Vector3(0, 0, 0);

    private Color materialColor;
    private Color materialChildColor;
    private Vector3 leftChildPosition = new Vector3(0, 0, 0); // change z
    private Vector3 rightChildPosition = new Vector3(0, 0, 0); // change z

    private GameObject twiceHand;
    private HandOpt activeHandOpt = HandOpt.singleOpt;
    private ControlPanel showColorCube = ControlPanel.empty;

    private Vector3 ColorBlackPoint = new Vector3(0, 0, 0);
    private Color ColorChose = Color.white;

    // Use this for initialization
    void Start () {

        terrainVolume = BasicProceduralVolume.GetComponent<TerrainVolume>();
        VoxelWorldTransform = terrainVolume.transform;

        handBehaviour = GetComponent<HandBehaviour>();

        terrainWorld = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //materialColor = terrainWorld.transform.GetComponent<Renderer>().material.color;
        //materialColor.a = colorAlpha;
        //terrainWorld.transform.GetComponent<Renderer>().material.color = materialColor;
        //terrainWorld.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        rightHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        materialColor = rightHand.transform.GetComponent<Renderer>().material.color;
        materialColor.a = colorAlpha;
        rightHand.transform.GetComponent<Renderer>().material.color = materialColor;
        rightHand.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        rightHandChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightHandChild.transform.position = rightChildPosition;
        rightHandChild.transform.parent = rightHand.transform;
        materialChildColor = rightHandChild.transform.GetComponent<Renderer>().material.color;
        materialChildColor.a = colorChildAlpha;
        rightHandChild.transform.GetComponent<Renderer>().material.color = materialChildColor;
        rightHandChild.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        leftHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        materialColor = leftHand.transform.GetComponent<Renderer>().material.color;
        materialColor.a = colorAlpha;
        leftHand.transform.GetComponent<Renderer>().material.color = materialColor;
        leftHand.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        leftHandChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftHandChild.transform.position = leftChildPosition;
        leftHandChild.transform.parent = leftHand.transform;
        materialChildColor = leftHandChild.transform.GetComponent<Renderer>().material.color;
        materialChildColor.a = colorChildAlpha;
        leftHandChild.transform.GetComponent<Renderer>().material.color = materialChildColor;
        leftHandChild.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        twiceHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        materialColor = twiceHand.transform.GetComponent<Renderer>().material.color;
        materialColor.a = colorChildAlpha;
        twiceHand.transform.GetComponent<Renderer>().material.color = materialColor;
        twiceHand.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        colorCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        materialColor = colorCube.transform.GetComponent<Renderer>().material.color;
        materialColor.a = colorChildAlpha;
        colorCube.transform.GetComponent<Renderer>().material.color = materialColor;
        colorCube.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
        colorCube.transform.localScale = colorCubeSize;

        leftHandChild.SetActive(true);
        rightHandChild.SetActive(true);
        twiceHand.SetActive(false);

        colorCube.SetActive(false);

        var transparentLayer = LayerMask.NameToLayer("TransparentFX");
        terrainWorld.layer = transparentLayer;
        rightHand.layer = transparentLayer;
        rightHandChild.layer = transparentLayer;
        leftHand.layer = transparentLayer;
        leftHandChild.layer = transparentLayer;
        twiceHand.layer = transparentLayer;
        colorCube.layer = transparentLayer;
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
                    UnityEngine.Object.Destroy(rightHand.gameObject);
                    UnityEngine.Object.Destroy(rightHandChild.gameObject);
                    UnityEngine.Object.Destroy(leftHand.gameObject);
                    UnityEngine.Object.Destroy(leftHandChild.gameObject);
                    UnityEngine.Object.Destroy(twiceHand.gameObject);
                    leftHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leftHandChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rightHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rightHandChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    twiceHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case OptShape.sphere:
                    UnityEngine.Object.Destroy(rightHand.gameObject);
                    UnityEngine.Object.Destroy(rightHandChild.gameObject);
                    UnityEngine.Object.Destroy(leftHand.gameObject);
                    UnityEngine.Object.Destroy(leftHandChild.gameObject);
                    UnityEngine.Object.Destroy(twiceHand.gameObject);
                    leftHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    leftHandChild = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    rightHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    rightHandChild = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    twiceHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case OptShape.cylinder:
                    UnityEngine.Object.Destroy(rightHand.gameObject);
                    UnityEngine.Object.Destroy(rightHandChild.gameObject);
                    UnityEngine.Object.Destroy(leftHand.gameObject);
                    UnityEngine.Object.Destroy(leftHandChild.gameObject);
                    UnityEngine.Object.Destroy(twiceHand.gameObject);
                    leftHand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leftHandChild = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    rightHand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    rightHandChild = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    twiceHand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                case OptShape.capsule:
                    UnityEngine.Object.Destroy(rightHand.gameObject);
                    UnityEngine.Object.Destroy(rightHandChild.gameObject);
                    UnityEngine.Object.Destroy(leftHand.gameObject);
                    UnityEngine.Object.Destroy(leftHandChild.gameObject);
                    UnityEngine.Object.Destroy(twiceHand.gameObject);
                    leftHand = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    leftHandChild = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    rightHand = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    rightHandChild = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    twiceHand = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
            }
            materialColor = rightHand.transform.GetComponent<Renderer>().material.color;
            materialColor.a = 0.05f;
            rightHand.transform.GetComponent<Renderer>().material.color = materialColor;
            rightHand.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            rightHandChild.transform.position = rightChildPosition;
            rightHandChild.transform.parent = rightHand.transform;
            materialChildColor = rightHandChild.transform.GetComponent<Renderer>().material.color;
            materialChildColor.a = 0.3f;
            rightHandChild.transform.GetComponent<Renderer>().material.color = materialChildColor;
            rightHandChild.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            materialColor = leftHand.transform.GetComponent<Renderer>().material.color;
            materialColor.a = 0.05f;
            leftHand.transform.GetComponent<Renderer>().material.color = materialColor;
            leftHand.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            leftHandChild.transform.position = leftChildPosition;
            leftHandChild.transform.parent = leftHand.transform;
            materialChildColor = leftHandChild.transform.GetComponent<Renderer>().material.color;
            materialChildColor.a = 0.3f;
            leftHandChild.transform.GetComponent<Renderer>().material.color = materialChildColor;
            leftHandChild.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            activeShape = nowShape;

            if (activeHandOpt == HandOpt.pairOpt)
            {
                leftHandChild.SetActive(false);
                rightHandChild.SetActive(false);
                twiceHand.SetActive(true);
            }
            else
            {
                leftHandChild.SetActive(true);
                rightHandChild.SetActive(true);
                twiceHand.SetActive(false);
            }
            var transparentLayer = LayerMask.NameToLayer("TransparentFX");
            terrainWorld.layer = transparentLayer;
            rightHand.layer = transparentLayer;
            rightHandChild.layer = transparentLayer;
            leftHand.layer = transparentLayer;
            leftHandChild.layer = transparentLayer;
            twiceHand.layer = transparentLayer;
            colorCube.layer = transparentLayer;
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
        leftHand.transform.localScale = VoxelWorldTransform.localScale * optRange;

        rightHand.transform.position = rightHandAnchor.transform.position;
        rightHand.transform.rotation = rightHandAnchor.transform.rotation;
        rightHand.transform.localScale = VoxelWorldTransform.localScale * optRange;

        Vector3 temp = rightHandAnchor.transform.position - leftHandAnchor.transform.position;
        //twiceHand.transform.rotation = Quaternion.Euler(0, 0, 90);
        twiceHand.transform.position = leftHandAnchor.transform.position + temp / 2;
        twiceHand.transform.localScale = new Vector3(System.Math.Abs(temp.x), System.Math.Abs(temp.y), System.Math.Abs(temp.z));

        // twice hand
        HandOpt tempActiveHandOpt = handBehaviour.GetActiveHandOpt();
        if (tempActiveHandOpt != activeHandOpt)
        {
            if (tempActiveHandOpt == HandOpt.pairOpt)
            {
                leftHandChild.SetActive(false);
                rightHandChild.SetActive(false);
                twiceHand.SetActive(true);
            }
            else
            {
                leftHandChild.SetActive(true);
                rightHandChild.SetActive(true);
                twiceHand.SetActive(false);
            }
        }
        activeHandOpt = tempActiveHandOpt;

        // color 
        ControlPanel tempShowColorCube = handBehaviour.GetActivePanel();
        // update state
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
        // update data
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

            //Debug.Log("Color: " + ColorChose.r + ", " + ColorChose.g + ", " + ColorChose.b);
        }



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

    public Color GetColorChose()
    {
        return ColorChose;
    }
}
