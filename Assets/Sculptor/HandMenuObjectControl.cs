using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class HandMenuObjectControl : MonoBehaviour
{
    public GameObject cameraManagerObj;
    public GameObject headAnchor;

    public GameObject mainColorChoose;
    public GameObject mainEditorMode;
    public GameObject mainReplayMode;
    public GameObject mainUndo;
    public GameObject mainRedo;
    public GameObject mainRestart;
    public GameObject mainSave;
    public GameObject mainLoad;
    public GameObject editSculptorMode;
    public GameObject editMirrorMode;
    public GameObject editRotateMode;
    public GameObject editNetworkMode;

    public AudioSource audioSource;

    private List<GameObject> mainMenuObjectList;
    private List<GameObject> editMenuObjectList;
    public List<Color> colorColorList;

    private RecordBehaviour recordBehaviour;
    private HandBehaviour handBehaviour;
    private TrackAnchor trackAnchor;

    private ControlPanel activePanel;

    private int TouchID = -2;

    private int menuPoints = 0;
    private float MenuChildRadio = 0.2f;
    private float MenuStartLocalScale = 0.001f;
    private float MenuEndLocalScale = 0.08f;
    private float MenuLocalScaleMax = 0.12f;
    private float menuAlpha = 0.3f;
    private float MenuColorStartLocalScale = 0.007f;
    private float MenuColorEndLocalScale = 0.05f;
    private float MenuColorObjRange = 0.03f;

    private GameObject MenuCenterObject;
    private List<Vector3> MenuChildLocalPos;
    private List<GameObject> MenuChildObject;
    private List<Vector3> MenuChildPos; // the menu position use to judge touch or not

    private bool hasPlayed = false;

    private bool isreuseobj = false;
    private bool iscolorobj = false;

    private CameraManager cameraManager;
    private VRMode vrMode;

    // Use this for initialization
    void Start()
    {

        activePanel = ControlPanel.empty;

        MenuCenterObject = new GameObject();
        MenuChildLocalPos = new List<Vector3>();
        MenuChildObject = new List<GameObject>();
        MenuChildPos = new List<Vector3>();

        mainMenuObjectList = new List<GameObject>();
        mainMenuObjectList.Add(mainColorChoose);
        mainMenuObjectList.Add(mainEditorMode);
        mainMenuObjectList.Add(mainReplayMode);
        mainMenuObjectList.Add(mainUndo);
        mainMenuObjectList.Add(mainRedo);
        mainMenuObjectList.Add(mainRestart);
        mainMenuObjectList.Add(mainSave);
        mainMenuObjectList.Add(mainLoad);

        editMenuObjectList = new List<GameObject>();
        editMenuObjectList.Add(editSculptorMode);
        editMenuObjectList.Add(editMirrorMode);
        editMenuObjectList.Add(editRotateMode);
        editMenuObjectList.Add(editNetworkMode);

        foreach(GameObject temp in mainMenuObjectList)
        {
            temp.SetActive(false);
        }

        foreach (GameObject temp in editMenuObjectList)
        {
            temp.SetActive(false);
        }

        colorColorList = new List<Color>();
        for (int oi = 0; oi < 10; oi++)
        {
            for (int oj = 0; oj < 10; oj++)
            {
                for (int ok = 0; ok < 10; ok++)
                {
                    Color c = new Color(0.1f * oi, 0.1f * oj, 0.1f * ok);
                    colorColorList.Add(c);
                }
            }
        }

        cameraManager = cameraManagerObj.GetComponent<CameraManager>();
        vrMode = cameraManager.GetVRMode();

        handBehaviour = GetComponent<HandBehaviour>();
        recordBehaviour = GetComponent<RecordBehaviour>();
        trackAnchor = GetComponent<TrackAnchor>();

        audioSource.loop = false;
        audioSource.Stop();
    }

    // Update is called once per frame
    void Update()
    {

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
                    DrawCircleMenuObj(menuPoints);
                    break;
                case ControlPanel.main:
                    menuPoints = mainMenuObjectList.Count;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(mainMenuObjectList);
                    break;
                case ControlPanel.color:
                    DrawColorPanelPoints(new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawColorPanel();
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
                    menuPoints = editMenuObjectList.Count;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(editMenuObjectList);
                    break;
            }
            activePanel = nowPanel;
        }

        TouchID = -1;
        if (activePanel != ControlPanel.empty)
        {
            TouchID = CheckMenuTouch(nowPos, MenuChildPos);
            //if (TouchID >= 0)
            //{
            //    Debug.Log("TouchID:" + TouchID);
            //}
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

        if (iscolorobj)
        {
            int minPos = 0;
            float minPosDist = Vector3.Distance(nowPosList[0], handPos);
            for (int ti = 0; ti < nowPosList.Count; ti++)
            {
                MenuChildObject[ti].transform.localScale = new Vector3(MenuColorStartLocalScale, MenuColorStartLocalScale, MenuColorStartLocalScale);

                float tempdis = Vector3.Distance(nowPosList[ti], handPos);
                if (tempdis < minPosDist)
                {
                    minPosDist = tempdis;
                    minPos = ti;
                }
            }

            for (int ti = 0; ti < nowPosList.Count; ti++)
            {
                int temp = (minPos % 10);
                if ((ti % 10) != temp )
                {
                    //MenuChildObject[ti].transform.localScale = new Vector3(0, 0, 0);
                    Color tempp = MenuChildObject[ti].transform.GetComponent<Renderer>().material.color;
                    tempp.a = 0.06f;
                    MenuChildObject[ti].transform.GetComponent<Renderer>().material.color = tempp;
                }
                else
                {
                    //MenuChildObject[ti].transform.localScale = new Vector3(MenuColorStartLocalScale, MenuColorStartLocalScale, MenuColorStartLocalScale);
                    Color tempp = MenuChildObject[ti].transform.GetComponent<Renderer>().material.color;
                    tempp.a = 1.0f;
                    MenuChildObject[ti].transform.GetComponent<Renderer>().material.color = tempp;
                }
            }

            MenuChildObject[minPos].transform.localScale = new Vector3(MenuColorEndLocalScale, MenuColorEndLocalScale, MenuColorEndLocalScale);

            Color tempC = MenuChildObject[minPos].transform.GetComponent<Renderer>().material.color;
            tempC.a = 1.0f;
            MenuChildObject[minPos].transform.GetComponent<Renderer>().material.color = tempC;
            MenuChildObject[minPos].transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            return minPos;
        }
        else
        {
            for (int ti = 0; ti < nowPosList.Count; ti++)
            {
                float dis = Vector3.Distance(nowPosList[ti], handPos);

                if (dis < MenuEndLocalScale / 2)
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

                // menu state animation
                if (MenuChildObject[ti].transform.localScale.x < MenuEndLocalScale)
                {
                    MenuChildObject[ti].transform.localScale += new Vector3(0.003f, 0.003f, 0.003f);
                }
                else
                {
                    float tempV = 1 - (Mathf.Clamp(dis, MenuEndLocalScale, MenuChildRadio) - MenuEndLocalScale) / (MenuChildRadio - MenuEndLocalScale);
                    MenuChildObject[ti].transform.localScale = new Vector3(MenuEndLocalScale + tempV * (MenuLocalScaleMax - MenuEndLocalScale), MenuEndLocalScale + tempV * (MenuLocalScaleMax - MenuEndLocalScale), MenuEndLocalScale + tempV * (MenuLocalScaleMax - MenuEndLocalScale));
                }
            }
        }

        return -1;
    }

    private void UpdateMenuCenterPos(DrawPos nowPos)
    {
        if (nowPos == DrawPos.left)
        {
            MenuCenterObject.transform.position = trackAnchor.GetLeftChildPosition();
            MenuCenterObject.transform.LookAt(headAnchor.transform.position);
        }
        else
        {
            MenuCenterObject.transform.position = trackAnchor.GetRightChildPosition();
            MenuCenterObject.transform.LookAt(headAnchor.transform.position);
        }
    }

    private void clearCircleMenuObj(bool ReuseObj)
    {
        if (ReuseObj)
        {
            foreach (GameObject tObj in MenuChildObject)
            {
                tObj.SetActive(false);
            }
            MenuChildPos.Clear();
            MenuChildObject.Clear();
        }
        else
        {
            foreach (GameObject tObj in MenuChildObject)
            {
                UnityEngine.Object.Destroy(tObj.gameObject);
            }
            MenuChildPos.Clear();
            MenuChildObject.Clear();
        }
    }

    private void DrawColorPanel()
    {
        iscolorobj = true;
        clearCircleMenuObj(isreuseobj);
        isreuseobj = false;

        for (int oi = 0; oi < colorColorList.Count; oi++)
        {
            GameObject tempObj;
            tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.color = colorColorList[oi];

            Color tempC = tempObj.transform.GetComponent<Renderer>().material.color;
            tempC.a = 1.0f;
            tempObj.transform.GetComponent<Renderer>().material.color = tempC;
            tempObj.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }

    }

    private void DrawCircleMenuObj(List<GameObject> menuobjects)
    {
        iscolorobj = false;
        clearCircleMenuObj(isreuseobj);
        isreuseobj = true;

        for (int oi = 0; oi < menuobjects.Count; oi++)
        {
            GameObject tempObj = menuobjects[oi];
            tempObj.SetActive(true);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            if (vrMode == VRMode.SteamVR)
            {
                tempObj.transform.localEulerAngles = new Vector3(0, 180, 0);
            }
            tempObj.transform.localScale = new Vector3(MenuStartLocalScale, MenuStartLocalScale, MenuStartLocalScale);

            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }
    }

    private void DrawCircleMenuObj(int MenuPoints, List<string> textlist)
    {
        iscolorobj = false;
        clearCircleMenuObj(isreuseobj);
        isreuseobj = false;

        for (int oi = 0; oi < MenuPoints; oi++)
        {
            GameObject tempObj = new GameObject();
            tempObj.AddComponent<TextMesh>();
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuStartLocalScale, MenuStartLocalScale, MenuStartLocalScale);
            if (vrMode == VRMode.SteamVR)
            {
                tempObj.transform.localEulerAngles = new Vector3(0, 180, 0);
            }
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
            tempObj.GetComponent<TextMesh>().text = textlist[oi].Substring(textlist[oi].LastIndexOf('\\') + 1);
            tempObj.GetComponent<TextMesh>().characterSize = 0.2f;
            tempObj.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }
    }

    private void DrawCircleMenuObj(int MenuPoints, List<Texture> texturelist)
    {
        iscolorobj = false;
        clearCircleMenuObj(isreuseobj);
        isreuseobj = false;

        for (int oi = 0; oi < MenuPoints; oi++)
        {
            GameObject tempObj;
            tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuStartLocalScale, MenuStartLocalScale, MenuStartLocalScale);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.mainTexture = texturelist[oi];

            Color tempC = tempObj.transform.GetComponent<Renderer>().material.color;
            tempC.a = menuAlpha;
            tempObj.transform.GetComponent<Renderer>().material.color = tempC;
            tempObj.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }
    }

    private void DrawCircleMenuObj(int MenuPoints, List<Color> colorlist)
    {
        iscolorobj = false;
        clearCircleMenuObj(isreuseobj);
        isreuseobj = false;

        for (int oi = 0; oi < MenuPoints; oi++)
        {
            GameObject tempObj;
            tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuStartLocalScale, MenuStartLocalScale, MenuStartLocalScale);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.color = colorlist[oi];

            Color tempC = tempObj.transform.GetComponent<Renderer>().material.color;
            tempC.a = menuAlpha;
            tempObj.transform.GetComponent<Renderer>().material.color = tempC;
            tempObj.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
        }
    }

    private void DrawCircleMenuObj(int MenuPoints)
    {
        iscolorobj = false;
        clearCircleMenuObj(isreuseobj);
        isreuseobj = false;

        for (int oi = 0; oi < MenuPoints; oi++)
        {
            GameObject tempObj;
            tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuStartLocalScale, MenuStartLocalScale, MenuStartLocalScale);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);

            Color tempC = tempObj.transform.GetComponent<Renderer>().material.color;
            tempC.a = menuAlpha;
            tempObj.transform.GetComponent<Renderer>().material.color = tempC;
            tempObj.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

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

    private void DrawColorPanelPoints(Vector3 center)
    {
        MenuChildLocalPos.Clear();
        float newc_X = center.x - MenuColorObjRange * 5;
        float newc_y = center.y - MenuColorObjRange * 5;
        float newc_z = center.z - MenuColorObjRange * 5;
        for (int ti = 0; ti < 10; ti++)
        {
            for (int tj = 0; tj < 10; tj++)
            {
                for (int tk = 0; tk < 10; tk++)
                {
                    Vector3 p = new Vector3(newc_X + MenuColorObjRange * ti, newc_y + MenuColorObjRange * tj, newc_z + MenuColorObjRange * tk);
                    MenuChildLocalPos.Add(p);
                }
            }
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
