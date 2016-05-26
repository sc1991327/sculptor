using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeadCanvasControl : MonoBehaviour {

    public bool isUse = false;

    public float HMDDistanceToEye = 0.8f;

    public GameObject HandObject = null;
    public GameObject CameraManagerObject = null;

    public GameObject steamStep0;
    public GameObject steamStep1;
    public GameObject steamStep2;
    public GameObject steamStep3;
    public GameObject steamStep4;
    public GameObject steamStep5;
    public GameObject steamStep6;
    private List<GameObject> steamSteps;

    public GameObject oculusStep0;
    public GameObject oculusStep1;
    public GameObject oculusStep2;
    public GameObject oculusStep3;
    public GameObject oculusStep4;
    public GameObject oculusStep5;
    public GameObject oculusStep6;
    private List<GameObject> oculusSteps;

    private HandBehaviour handBehaviour;
    private CameraManager cameraManager;

    private OptModePanel activeMode;
    private VRMode vrMode;

    private int activeInfoPanelTimes;
    private int menusize;

    void Start()
    {
        handBehaviour = HandObject.GetComponent<HandBehaviour>();
        cameraManager = CameraManagerObject.GetComponent<CameraManager>();

        vrMode = cameraManager.GetVRMode();

        GameObject tempGObject = new GameObject();

        steamSteps = new List<GameObject>();
        steamSteps.Add(tempGObject);
        steamSteps.Add(steamStep0);
        steamSteps.Add(steamStep1);
        steamSteps.Add(steamStep2);
        steamSteps.Add(steamStep3);
        steamSteps.Add(steamStep4);
        steamSteps.Add(steamStep5);
        steamSteps.Add(steamStep6);

        oculusSteps = new List<GameObject>();
        oculusSteps.Add(tempGObject);
        oculusSteps.Add(oculusStep0);
        oculusSteps.Add(oculusStep1);
        oculusSteps.Add(oculusStep2);
        oculusSteps.Add(oculusStep3);
        oculusSteps.Add(oculusStep4);
        oculusSteps.Add(oculusStep5);
        oculusSteps.Add(oculusStep6);

        for (int tempi = 0; tempi < steamSteps.Count; tempi++)
        {
            steamSteps[tempi].SetActive(false);
        }
        for (int tempi = 0; tempi < oculusSteps.Count; tempi++)
        {
            oculusSteps[tempi].SetActive(false);
        }

        if (vrMode == VRMode.SteamVR)
        {
            menusize = steamSteps.Count;
        }
        else if (vrMode == VRMode.OculusVR)
        {
            menusize = oculusSteps.Count;
        }else
        {
            menusize = 0;
        }

    }

    void Update()
    {

        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, HMDDistanceToEye));
        transform.rotation = Camera.main.transform.rotation;

        if (isUse)
        {
            int temptimes = handBehaviour.GetActiveInfoPanelTimes() % menusize;
            if (temptimes != activeInfoPanelTimes)
            {
                startPanelHandle(temptimes);

                activeInfoPanelTimes = temptimes;
            }
        }
    }

    void infoPanelHandle()
    {
        activeMode = handBehaviour.GetActiveOptModePanel();

        // show info
        switch (activeMode)
        {
            case OptModePanel.sculptor:
                break;

            case OptModePanel.mirror:
                break;

            case OptModePanel.network:
                break;

            case OptModePanel.replay:
                break;
        }
    }

    void startPanelHandle(int activeTimes)
    {
        switch (vrMode)
        {
            case VRMode.None:
                break;

            case VRMode.OculusVR:
                for (int tempi = 0; tempi < menusize; tempi++)
                {
                    if (tempi == activeTimes)
                        oculusSteps[tempi].SetActive(true);
                    else
                        oculusSteps[tempi].SetActive(false);
                }
                break;

            case VRMode.SteamVR:
                for (int tempi = 0; tempi < menusize; tempi++)
                {
                    if (tempi == activeTimes)
                        steamSteps[tempi].SetActive(true);
                    else
                        steamSteps[tempi].SetActive(false);
                }
                break;
        }

    }

}
