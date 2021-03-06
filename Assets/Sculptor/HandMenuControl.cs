﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandMenuControl : MonoBehaviour {

    public Texture mainColorChoose;
    public Texture mainSculptorMode;
    public Texture mainMirrorMode;
    public Texture mainHighEditorMode;
    public Texture mainReplayMode;
    public Texture mainUndo;
    public Texture mainRedo;
    public Texture mainRestart;
    public Texture mainSave;
    public Texture mainLoad;
    public Texture highNetwork;

    public List<Texture> mainTextureList;
    public List<Texture> highTextureList;
    public List<Color> colorColorList;

    public AudioSource audioSource;

    private RecordBehaviour recordBehaviour;
    private HandBehaviour handBehaviour;
    private TrackAnchor trackAnchor;

    private ControlPanel activePanel;

    private int TouchID = -2;

    private int menuPoints = 0;
    private float MenuChildRadio = 0.2f;
    private float MenuLocalScale = 0.05f;
    private float MenuLocalScaleMax = 0.1f;
    private float menuAlpha = 0.3f;

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
        mainTextureList.Add(mainColorChoose);
        mainTextureList.Add(mainSculptorMode);
        mainTextureList.Add(mainMirrorMode);
        mainTextureList.Add(mainHighEditorMode);
        mainTextureList.Add(mainReplayMode);
        mainTextureList.Add(mainUndo);
        mainTextureList.Add(mainRedo);
        mainTextureList.Add(mainRestart);
        mainTextureList.Add(mainSave);
        mainTextureList.Add(mainLoad);

        highTextureList = new List<Texture>();
        highTextureList.Add(highNetwork);

        colorColorList = new List<Color>();
        for (int oi = 0; oi < 4; oi++)
        {
            for (int oj = 0; oj < 4; oj++)
            {
                for (int ok = 0; ok < 4; ok++)
                {
                    Color c = new Color(0.3f * oi, 0.3f * oj, 0.3f * ok);
                    colorColorList.Add(c);
                }
            }
        }

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
                    menuPoints = 1;
                    DrawCirclePoints(menuPoints, MenuChildRadio, new Vector3(0, 0, 0));
                    UpdateMenuCenterPos(nowPos);
                    DrawCircleMenuObj(menuPoints, highTextureList);
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
        if (nowPanel != ControlPanel.color)
        {
            foreach (GameObject tObj in MenuChildObject)
            {
                tObj.transform.Rotate(0, 0.1f, 0);
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

            // menu state animation
            float tempV = 1 - (Mathf.Clamp(dis, MenuLocalScale, MenuChildRadio) - MenuLocalScale) / (MenuChildRadio - MenuLocalScale);
            MenuChildObject[ti].transform.localScale = new Vector3(MenuLocalScale + tempV * (MenuLocalScaleMax - MenuLocalScale), MenuLocalScale + tempV * (MenuLocalScaleMax - MenuLocalScale), MenuLocalScale + tempV * (MenuLocalScaleMax - MenuLocalScale));
            Color tempC = MenuChildObject[ti].transform.GetComponent<Renderer>().material.color;
            tempC.a = menuAlpha + tempV * (1 - menuAlpha);
            MenuChildObject[ti].transform.GetComponent<Renderer>().material.color = tempC;
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

    private void DrawColorPanel()
    {
        foreach (GameObject tObj in MenuChildObject)
        {
            UnityEngine.Object.Destroy(tObj.gameObject);
        }
        MenuChildPos.Clear();
        MenuChildObject.Clear();

        for (int oi = 0; oi < 64; oi++)
        {
            GameObject tempObj;
            tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempObj.transform.parent = MenuCenterObject.transform;
            tempObj.transform.localScale = new Vector3(MenuLocalScale, MenuLocalScale, MenuLocalScale);
            tempObj.transform.localPosition = MenuChildLocalPos[oi];
            tempObj.transform.GetComponent<Renderer>().material.color = colorColorList[oi];

            Color tempC = tempObj.transform.GetComponent<Renderer>().material.color;
            tempC.a = menuAlpha;
            tempObj.transform.GetComponent<Renderer>().material.color = tempC;
            tempObj.transform.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

            MenuChildObject.Add(tempObj);
            MenuChildPos.Add(tempObj.transform.position);
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
        float newc_X = center.x - MenuLocalScaleMax * 3.5f;
        float newc_y = center.y - MenuLocalScaleMax * 3.5f;
        for (int ti=0; ti<8; ti++)
        {
            for (int tj=0; tj<8; tj++)
            {
                Vector3 p = new Vector3(newc_X + MenuLocalScaleMax * ti, newc_y + MenuLocalScaleMax * tj, 0);
                MenuChildLocalPos.Add(p);
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
