using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandMenuControl : MonoBehaviour {

    public Texture mainColorChooseMode;
    public Texture mainSculptorMode;
    public Texture mainColorPaintMode;
    public Texture mainUndo;
    public Texture mainRedo;
    public Texture mainRestart;
    public Texture mainReplay;
    public Texture mainHighEditorMode;
    public Texture mainSave;
    public Texture mainLoad;

    public List<Texture> mainTextureList;
    public List<Color> colorColorList;

    public AudioSource audioSource;

    private RecordBehaviour recordBehaviour;
    private HandBehaviour handBehaviour;
    private TrackAnchor trackAnchor;

    private ControlPanel activePanel;

    private int TouchID = -2;

    private int menuPoints = 0;
    private float MenuChildRadio = 0.15f;
    private float MenuLocalScale = 0.08f;

    private GameObject MenuCenterObject;
    private List<Vector3> MenuChildLocalPos;
    private List<GameObject> MenuChildObject;
    private List<Vector3> MenuChildPos; // the menu position use to judge touch or not

    private Color colorChose = Color.gray;

    private bool hasPlayed = false;

    // Use this for initialization
    void Start () {

        activePanel = ControlPanel.empty;

        MenuCenterObject = new GameObject();
        MenuChildLocalPos = new List<Vector3>();
        MenuChildObject = new List<GameObject>();
        MenuChildPos = new List<Vector3>();

        mainTextureList = new List<Texture>();
        mainTextureList.Add(mainColorChooseMode);
        mainTextureList.Add(mainSculptorMode);
        mainTextureList.Add(mainColorPaintMode);
        mainTextureList.Add(mainUndo);
        mainTextureList.Add(mainRedo);
        mainTextureList.Add(mainRestart);
        mainTextureList.Add(mainReplay);
        mainTextureList.Add(mainHighEditorMode);
        mainTextureList.Add(mainSave);
        mainTextureList.Add(mainLoad);

        colorColorList = new List<Color>();
        colorColorList.Add(Color.black);
        colorColorList.Add(Color.blue);
        colorColorList.Add(Color.cyan);
        colorColorList.Add(Color.green);
        colorColorList.Add(Color.grey);
        colorColorList.Add(Color.magenta);
        colorColorList.Add(Color.red);
        colorColorList.Add(Color.white);
        colorColorList.Add(Color.yellow);

        handBehaviour = GetComponent<HandBehaviour>();
        recordBehaviour = GetComponent<RecordBehaviour>();
        trackAnchor = GetComponent<TrackAnchor>();

        audioSource.loop = false;
        audioSource.Stop();
    }
	
	// Update is called once per frame
	void Update () {

        ControlPanel nowPanel = handBehaviour.GetActivePanel();
        DrawPos nowPos = handBehaviour.GetActiveDrawPos();

        // here only use to update the canvas contents. all the hand behavior handle in handBehivor.cs

        if (nowPanel != activePanel)
        {
            switch (nowPanel)
            {
                case ControlPanel.empty:
                    menuPoints = 0;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(menuPoints, mainTextureList);
                    break;
                case ControlPanel.main:
                    menuPoints = 10;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(menuPoints, mainTextureList);
                    break;
                case ControlPanel.color:
                    menuPoints = 9;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(menuPoints, colorColorList);
                    break;
                case ControlPanel.replay:
                    // use recordBehaviour fileNames size to create menu objects.
                    menuPoints = Mathf.Clamp(recordBehaviour.recordFileNames.Count, 0, 30);
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(menuPoints, recordBehaviour.recordFileNames);
                    break;
                case ControlPanel.load:
                    menuPoints = Mathf.Clamp(recordBehaviour.loadFileNames.Count, 0, 30);
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(menuPoints, recordBehaviour.loadFileNames);
                    break;
                case ControlPanel.high:
                    menuPoints = 1;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(menuPoints, colorColorList);
                    break;
            }
            activePanel = nowPanel;
        }

        TouchID = -1;
        if (activePanel != ControlPanel.empty)
        {
            TouchID = CheckMenuTouch(nowPos, MenuChildPos);
            if (TouchID >= 0)
            {
                Debug.Log("TouchID:" + TouchID);
            }
        }

        // MenuChildObject rotate
        if (nowPanel != ControlPanel.replay)
        {
            foreach (GameObject tObj in MenuChildObject)
            {
                tObj.transform.Rotate(0, 1, 0);
            }
        }

    }

    private int CheckMenuTouch(DrawPos nowPos, List<Vector3> nowPosList)
    {
        // check touch or not
        Vector3 handPos = trackAnchor.GetLeftChildPosition();
        if (nowPos == DrawPos.right)
        {
            handPos = trackAnchor.GetRightChildPosition();
        }

        for (int ti = 0; ti < nowPosList.Count; ti++)
        {
            float dis = Vector3.Distance(nowPosList[ti], handPos);
            if (dis < MenuLocalScale / 2)
            {
                audioSource.transform.position = nowPosList[ti];
                if (hasPlayed == false)
                {
                    hasPlayed = true;
                    audioSource.Play();
                }
                return ti;
            }
            else
            {
                hasPlayed = false;
            }
        }

        return -1;
    }

    private void UpdateMenuCenterPos(DrawPos nowPos)
    {
        if (nowPos == DrawPos.left)
        {
            MenuCenterObject.transform.position = trackAnchor.GetLeftChildPosition();
            MenuCenterObject.transform.LookAt(Camera.main.transform.position);
        }
        else
        {
            MenuCenterObject.transform.position = trackAnchor.GetRightChildPosition();
            MenuCenterObject.transform.LookAt(Camera.main.transform.position);
        }
    }

    private void DrawCircleMenuObj(int MenuPoints, List<string> textlist)
    {
        foreach (GameObject tObj in MenuChildObject)
        {
            UnityEngine.Object.Destroy(tObj.gameObject);
        }
        MenuChildPos.Clear();
        MenuChildObject.Clear();
        for (int oi = 0; oi < MenuPoints; oi++)
        {
            GameObject tempObj = new GameObject();
            tempObj.AddComponent<TextMesh>();
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuLocalScale, MenuLocalScale, MenuLocalScale);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
            tempObj.GetComponent<TextMesh>().text = textlist[oi].Substring(textlist[oi].LastIndexOf('\\') + 1);
//            tempObj.GetComponent<TextMesh>().text = textlist[oi];
            tempObj.GetComponent<TextMesh>().characterSize = 0.1f;
            tempObj.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }
    }

    private void DrawCircleMenuObj(int MenuPoints, List<Texture> texturelist)
    {
        foreach (GameObject tObj in MenuChildObject)
        {
            UnityEngine.Object.Destroy(tObj.gameObject);
        }
        MenuChildPos.Clear();
        MenuChildObject.Clear();
        for (int oi = 0; oi < MenuPoints; oi++)
        {
            GameObject tempObj;
            tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuLocalScale, MenuLocalScale, MenuLocalScale);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.mainTexture = texturelist[oi];
            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }
    }

    private void DrawCircleMenuObj(int MenuPoints, List<Color> colorlist)
    {
        foreach (GameObject tObj in MenuChildObject)
        {
            UnityEngine.Object.Destroy(tObj.gameObject);
        }
        MenuChildPos.Clear();
        MenuChildObject.Clear();
        for (int oi = 0; oi < MenuPoints; oi++)
        {
            GameObject tempObj;
            tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuLocalScale, MenuLocalScale, MenuLocalScale);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.color = colorlist[oi];
            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }
    }

    private void DrawCircleMenuObj(int MenuPoints)
    {
        foreach (GameObject tObj in MenuChildObject)
        {
            UnityEngine.Object.Destroy(tObj.gameObject);
        }
        MenuChildPos.Clear();
        MenuChildObject.Clear();
        for (int oi = 0; oi < MenuPoints; oi++)
        {
            GameObject tempObj;
            tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuLocalScale, MenuLocalScale, MenuLocalScale);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }
    }

    private void DrawCirclePoints(int points, float radius, Vector3 center)
    {
        MenuChildLocalPos.Clear();
        float slice = 2 * Mathf.PI / points;
        for (float i = 0; i < points; i++)
        {
            float angle = slice * i;
            float newX = (center.x + radius * Mathf.Cos(angle));
            float newY = (center.z + radius * Mathf.Sin(angle));
            Vector3 p = new Vector3(newX, newY, 0);
            MenuChildLocalPos.Add(p);
            //Debug.Log("LocalPos: " + p);
        }
    }

    public int GetTouchID()
    {
        return TouchID;
    }

    public int GetMenuPoints()
    {
        return menuPoints;
    }

}
