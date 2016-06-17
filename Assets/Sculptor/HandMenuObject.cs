using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public enum MenuPanel { empty, main, color, replay, load, edit };
public enum MenuPos { left, right };

public class HandMenuObject : MonoBehaviour {

    // Use to recognize VR device
    // Object have different local object coordinate in Oculus/Vive Camera Use, So we need to recognize.
    public GameObject cameraManagerObj;
    private CameraManager cameraManager;
    private VRMode vrMode;

    // Make sure the 3D menu potion objects face to the use always
    public GameObject headAnchor;
    public GameObject leftHandAnchor;
    public GameObject rightHandAnchor;

    // Audio
    public AudioSource audioSource;

    // Show the menu state
    public MenuPanel menuPanel;
    MenuPanel nowPanel;
    public MenuPos menuPos;

    // EXAMPLE to use GameObject menu options
    // You can instead of this part to use in your own menu.

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

    private List<GameObject> mainMenuObjectList;
    private List<GameObject> editMenuObjectList;

    public List<Color> colorColorList;

    // System Use variables

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

    // System Use Variables

    private bool ismenushow = false;
    private bool hasPlayed = false;
    private bool isreuseobj = false;
    private bool iscolorobj = false;

    Tweener menuTweener;

    // Use this for initialization
    void Start()
    {
        // System variables initial (1)

        MenuCenterObject = new GameObject();
        MenuChildLocalPos = new List<Vector3>();
        MenuChildObject = new List<GameObject>();

        // EXAMPLE - Create menu potion objects list

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

        foreach (GameObject temp in mainMenuObjectList)
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

        // System objects initial (2)

        menuPanel = MenuPanel.empty;
        nowPanel = menuPanel;
        menuPos = MenuPos.left;

        cameraManager = cameraManagerObj.GetComponent<CameraManager>();
        vrMode = cameraManager.GetVRMode();

        audioSource.loop = false;
        audioSource.Stop();

        SetupTween();
    }

    // Tween use method

    private void SetupTween()
    {
        if (menuTweener == null)
        {
            menuTweener = MenuCenterObject.transform.DOScale(MenuCenterObject.transform.localScale, 0.5f).SetAutoKill(false).Pause();
            MenuCenterObject.transform.localScale *= 0.3f;
            menuTweener.SetEase(Ease.OutElastic);
        }

    }

    private void PlayHideMenuAnim()
    {
        menuTweener.PlayBackwards();
    }

    private void PlayShowMenuAnim()
    {
        SetupTween();
        menuTweener.Rewind();
        menuTweener.PlayForward();
    }

    // Update is called once per frame
    void Update()
    {

        // Button Check

        VirtualOpt vOpt = new VirtualOpt();
        vOpt = cameraManager.GetVirtualOpt();

        bool Button_A = vOpt.Button_A;
        bool Button_B = vOpt.Button_B;
        bool Button_X = vOpt.Button_X;
        bool Button_Y = vOpt.Button_Y;

        if (Button_X)
        {
            if (ismenushow == false)
                nowPanel = MenuPanel.main;
            ismenushow = true;
            menuPos = MenuPos.left;
        }
        else if (Button_A)
        {
            if (ismenushow == false)
                nowPanel = MenuPanel.main;
            ismenushow = true;
            menuPos = MenuPos.right;
        }
        else
        {
            ismenushow = false;
            nowPanel = MenuPanel.empty;
        }

        // Show Menu - Only excuse in panel change
        if (nowPanel != menuPanel)
        {
            menuPanel = nowPanel;
            switch (menuPanel)
            {
                case MenuPanel.empty:
                    menuPoints = 0;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(menuPos);
                    DrawCircleMenuObj(menuPoints);
                    break;
                case MenuPanel.main:
                    menuPoints = mainMenuObjectList.Count;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(menuPos);
                    DrawCircleMenuObj(mainMenuObjectList);
                    break;
                case MenuPanel.color:
                    DrawColorPanelPoints(new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(menuPos);
                    DrawColorPanel();
                    break;
                case MenuPanel.replay:
                    menuPoints = 0;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(menuPos);
                    DrawCircleMenuObj(menuPoints);
                    break;
                case MenuPanel.load:
                    menuPoints = 0;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(menuPos);
                    DrawCircleMenuObj(menuPoints);
                    break;
                case MenuPanel.edit:
                    menuPoints = editMenuObjectList.Count;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(menuPos);
                    DrawCircleMenuObj(editMenuObjectList);
                    break;
            }
            if (menuPanel == MenuPanel.empty)
            {
                PlayHideMenuAnim();
            }
            else
            {
                PlayShowMenuAnim();
            }
        }

        // Handle Menu Touch
        TouchID = -1;
        if (menuPanel != MenuPanel.empty)
        {
            TouchID = CheckMenuTouch(menuPos);

            if (menuPanel == MenuPanel.main)
            {
                switch (TouchID)
                {
                    case 0:
                        nowPanel = MenuPanel.color;
                        break;
                    case 1:
                        nowPanel = MenuPanel.edit;
                        break;
                    case 2:
                        nowPanel = MenuPanel.empty;
                        break;
                    case 3:
                        nowPanel = MenuPanel.empty;
                        break;
                    case 4:
                        nowPanel = MenuPanel.empty;
                        break;
                    case 5:
                        nowPanel = MenuPanel.empty;
                        break;
                    case 6:
                        nowPanel = MenuPanel.empty;
                        break;
                    case 7:
                        nowPanel = MenuPanel.empty;
                        break;
                }
            }

        }

    }

    // Functional Functions

    private int CheckMenuTouch(MenuPos menuPos)
    {
        // check touch or not
        Vector3 handPos = leftHandAnchor.transform.position;
        if (menuPos == MenuPos.right)
        {
            handPos = rightHandAnchor.transform.position;
        }

        if (iscolorobj)
        {
            int minPos = 0;
            float minPosDist = Vector3.Distance(MenuChildObject[0].transform.position, handPos);
            for (int ti = 0; ti < MenuChildObject.Count; ti++)
            {
                MenuChildObject[ti].transform.localScale = new Vector3(MenuColorStartLocalScale, MenuColorStartLocalScale, MenuColorStartLocalScale);

                float tempdis = Vector3.Distance(MenuChildObject[ti].transform.position, handPos);
                if (tempdis < minPosDist)
                {
                    minPosDist = tempdis;
                    minPos = ti;
                }
            }

            for (int ti = 0; ti < MenuChildObject.Count; ti++)
            {
                int temp = (minPos % 10);
                if ((ti % 10) != temp)
                {
                    MenuChildObject[ti].transform.localScale = new Vector3(0, 0, 0);
                }
                else
                {
                    MenuChildObject[ti].transform.localScale = new Vector3(MenuColorStartLocalScale, MenuColorStartLocalScale, MenuColorStartLocalScale);
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
            for (int ti = 0; ti < MenuChildObject.Count; ti++)
            {
                float dis = Vector3.Distance(MenuChildObject[ti].transform.position, handPos);

                if (dis < MenuEndLocalScale / 2)
                {
                    audioSource.transform.position = MenuChildObject[ti].transform.position;
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
                float tempV = 1 - (Mathf.Clamp(dis, MenuEndLocalScale, MenuChildRadio) - MenuEndLocalScale) / (MenuChildRadio - MenuEndLocalScale);
                MenuChildObject[ti].transform.localScale = new Vector3(MenuEndLocalScale + tempV * (MenuLocalScaleMax - MenuEndLocalScale), MenuEndLocalScale + tempV * (MenuLocalScaleMax - MenuEndLocalScale), MenuEndLocalScale + tempV * (MenuLocalScaleMax - MenuEndLocalScale));
            }
        }

        return -1;
    }

    private void UpdateMenuCenterPos(MenuPos menuPos)
    {
        if (menuPos == MenuPos.left)
        {
            MenuCenterObject.transform.position = leftHandAnchor.transform.position;
            MenuCenterObject.transform.LookAt(headAnchor.transform.position);
        }
        else
        {
            MenuCenterObject.transform.position = rightHandAnchor.transform.position;
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
            MenuChildObject.Clear();
        }
        else
        {
            foreach (GameObject tObj in MenuChildObject)
            {
                UnityEngine.Object.Destroy(tObj.gameObject);
            }
            MenuChildObject.Clear();
        }
    }

    // Draw 3D Color Panel
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
            //tempObj.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            MenuChildObject.Add(tempObj);
        }

    }

    // Draw GameObject Component Menu
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
        }
    }

    // Draw TextObject Component Menu
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
        }
    }

    // Draw TextureObject Component Menu
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
        }
    }

    // Draw ColorObject Component Menu
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
        }
    }

    // Draw RandomColorObject Component Menu
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
        }
    }

    // Calculate Menu Option Position
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

    // Calculate 3D ColorPanel Position
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
