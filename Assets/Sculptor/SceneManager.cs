using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour {

    public GameObject HandObject = null;
    public GameObject cameraManagerObj;
    public GameObject proceduralTerrainVolumeObj;

    private CameraManager cameraManager;
    private ProceduralTerrainVolume proceduralTerrainVolume;

    public GameObject steamStep0;
    public GameObject steamStep1;
    public GameObject steamStep2;
    public GameObject steamStep3;
    public GameObject steamStep4;
    public GameObject steamStep5;
    public GameObject steamStep6;

    public GameObject oculusStep0;
    public GameObject oculusStep1;
    public GameObject oculusStep2;
    public GameObject oculusStep3;
    public GameObject oculusStep4;
    public GameObject oculusStep5;
    public GameObject oculusStep6;

    public GameObject steamSculptorHelp;
    public GameObject steamMirrorHelp;
    public GameObject steamRotateHelp;
    public GameObject steamNetworkHelp;

    public GameObject oculusSculptorHelp;
    public GameObject oculusMirrorHelp;
    public GameObject oculusRotateHelp;
    public GameObject oculusNetworkHelp;

    private List<GameObject> startSteps;
    private List<GameObject> helpSteps;

    private HandBehaviour handBehaviour;

    private OptModePanel activeMode;
    private VRMode vrMode;

    private int activeInfoPanelTimes;

    // Use this for initialization
    void Start () {

        steamStep0.SetActive(false);
        steamStep1.SetActive(false);
        steamStep2.SetActive(false);
        steamStep3.SetActive(false);
        steamStep4.SetActive(false);
        steamStep5.SetActive(false);
        steamStep6.SetActive(false);

        oculusStep0.SetActive(false);
        oculusStep1.SetActive(false);
        oculusStep2.SetActive(false);
        oculusStep3.SetActive(false);
        oculusStep4.SetActive(false);
        oculusStep5.SetActive(false);
        oculusStep6.SetActive(false);

        steamSculptorHelp.SetActive(false);
        steamMirrorHelp.SetActive(false);
        steamRotateHelp.SetActive(false);
        steamNetworkHelp.SetActive(false);

        oculusSculptorHelp.SetActive(false);
        oculusMirrorHelp.SetActive(false);
        oculusRotateHelp.SetActive(false);
        oculusNetworkHelp.SetActive(false);

        handBehaviour = HandObject.GetComponent<HandBehaviour>();
        cameraManager = cameraManagerObj.GetComponent<CameraManager>();
        proceduralTerrainVolume = proceduralTerrainVolumeObj.GetComponent<ProceduralTerrainVolume>();

        vrMode = cameraManager.GetVRMode();

        if (vrMode == VRMode.SteamVR)
        {
            float temp = proceduralTerrainVolume.GetVoxelRadiusDistance();
            transform.position = new Vector3(0, temp, 0);
        }

        GameObject tempGObject = new GameObject();

        startSteps = new List<GameObject>();
        helpSteps = new List<GameObject>();

        if (vrMode == VRMode.SteamVR)
        {
            startSteps.Add(tempGObject);
            startSteps.Add(steamStep0);
            startSteps.Add(steamStep1);
            startSteps.Add(steamStep2);
            startSteps.Add(steamStep3);
            startSteps.Add(steamStep4);
            startSteps.Add(steamStep5);
            startSteps.Add(steamStep6);
            helpSteps.Add(steamSculptorHelp);
            helpSteps.Add(steamMirrorHelp);
            helpSteps.Add(steamRotateHelp);
            helpSteps.Add(steamNetworkHelp);
        }
        else if (vrMode == VRMode.OculusVR)
        {
            startSteps.Add(tempGObject);
            startSteps.Add(oculusStep0);
            startSteps.Add(oculusStep1);
            startSteps.Add(oculusStep2);
            startSteps.Add(oculusStep3);
            startSteps.Add(oculusStep4);
            startSteps.Add(oculusStep5);
            startSteps.Add(oculusStep6);
            helpSteps.Add(oculusSculptorHelp);
            helpSteps.Add(oculusMirrorHelp);
            helpSteps.Add(oculusRotateHelp);
            helpSteps.Add(oculusNetworkHelp);
        }

    }
	
	// Update is called once per frame
	void Update () {


        int temptimes = handBehaviour.GetActiveInfoPanelTimes();
        if (temptimes != activeInfoPanelTimes)
        {
            if (temptimes < startSteps.Count)
            {
                startPanelHandle(temptimes);
            }
            else if((temptimes - startSteps.Count) % 2 == 1)
            {
                activeMode = handBehaviour.GetActiveOptModePanel();

                // show info
                switch (activeMode)
                {
                    case OptModePanel.sculptor:
                        helpPanelHandle(0);
                        break;

                    case OptModePanel.mirror:
                        helpPanelHandle(1);
                        break;

                    case OptModePanel.rotate:
                        helpPanelHandle(2);
                        break;

                    case OptModePanel.network:
                        helpPanelHandle(3);
                        break;

                }

            }
            else
            {
                helpPanelHandle(-1);
                startPanelHandle(-1);
            }

            activeInfoPanelTimes = temptimes;
        }

    }

    void helpPanelHandle(int activeTimes)
    {
        if (helpSteps.Count == 0)
        {
            return;
        }
        
        for (int tempi = 0; tempi < helpSteps.Count; tempi++)
        {
            if (tempi == activeTimes)
                helpSteps[tempi].SetActive(true);
            else
                helpSteps[tempi].SetActive(false);
        }
    }

    void startPanelHandle(int activeTimes)
    {
        if (startSteps.Count == 0)
        {
            return;
        }

        for (int tempi = 0; tempi < startSteps.Count; tempi++)
        {
            if (tempi == activeTimes)
                startSteps[tempi].SetActive(true);
            else
                startSteps[tempi].SetActive(false);
        }
    }
}
