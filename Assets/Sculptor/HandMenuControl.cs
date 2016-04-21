﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandMenuControl : MonoBehaviour {

    public GameObject leftHandAnchor = null;
    public GameObject rightHandAnchor = null;

    public Texture mainColorChooseMode;
    public Texture mainSculptorMode;
    public Texture mainColorPaintMode;
    public Texture mainUndo;
    public Texture mainRedo;
    public Texture mainReset;
    public Texture mainReplay;
    public Texture mainHighEditorMode;

    private HandBehaviour handBehaviour;

    private ControlPanel activePanel;

    private int TouchID = -2;

    private int menuPoints = 0;
    private float MenuChildRadio = 0.2f;
    private float MenuLocalScale = 0.1f;

    private GameObject MenuCenterObject;
    private List<Vector3> MenuChildLocalPos;
    private List<GameObject> MenuChildObject;
    private List<Vector3> MenuChildPos; // the menu position use to judge touch or not

    private List<Texture> mainTextureList;
    private List<Color> colorColorList;

    private Color colorChose = Color.gray;

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
        mainTextureList.Add(mainReset);
        mainTextureList.Add(mainReplay);
        mainTextureList.Add(mainHighEditorMode);

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
    }
	
	// Update is called once per frame
	void Update () {

        handBehaviour = GetComponent<HandBehaviour>();
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
                    menuPoints = 8;
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
            }
            activePanel = nowPanel;
        }

        if (activePanel != ControlPanel.empty)
        {
            TouchID = CheckMenuTouch(nowPos);
            if (TouchID >= 0)
            {
                Debug.Log("TouchID:" + TouchID);
            }

            // chose color
            if (activePanel == ControlPanel.color && TouchID >= 0)
            {
                colorChose = colorColorList[TouchID];
            }

        }

        // MenuChildObject rotate
        foreach (GameObject tObj in MenuChildObject)
        {
            tObj.transform.Rotate(0, 1, 0);
        }

    }

    private int CheckMenuTouch(DrawPos nowPos)
    {
        // check touch or not
        if (nowPos == DrawPos.left)
        {
            for (int ti = 0; ti < MenuChildPos.Count; ti++)
            {
                float dis = Vector3.Distance(MenuChildPos[ti], leftHandAnchor.transform.position);
                if (dis < MenuLocalScale)
                {
                    return ti;
                }
            }
        }
        else
        {
            for (int ti = 0; ti < MenuChildPos.Count; ti++)
            {
                float dis = Vector3.Distance(MenuChildPos[ti], rightHandAnchor.transform.position);
                if (dis < MenuLocalScale)
                {
                    return ti;
                }
            }
        }
        return -1;
    }

    private void UpdateMenuCenterPos(DrawPos nowPos)
    {
        if (nowPos == DrawPos.left)
        {
            MenuCenterObject.transform.position = leftHandAnchor.transform.position;
            MenuCenterObject.transform.LookAt(Camera.main.transform.position);
        }
        else
        {
            MenuCenterObject.transform.position = rightHandAnchor.transform.position;
            MenuCenterObject.transform.LookAt(Camera.main.transform.position);
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

    public Color GetColorChose()
    {
        return colorChose;
    }
}