using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeadCanvasControl : MonoBehaviour {

    public GameObject HandObject = null;

    public GameObject startPanel;
    public GameObject infoPanel;

    public GameObject textStep1;
    public GameObject textStep2;
    public GameObject textStep3;
    public GameObject textStep4;
    public GameObject textStep5;
    public GameObject textStep6;
    private List<GameObject> textSteps;

    public GameObject textSculptor;
    public GameObject textMirror;
    public GameObject textNetwork;
    public GameObject textReplay;

    private HandBehaviour handBehaviour;

    private OptModePanel activeMode;

    private int activeInfoPanelTimes;

    void Start()
    {
        handBehaviour = HandObject.GetComponent<HandBehaviour>();

        startPanel.SetActive(false);
        infoPanel.SetActive(false);

        textSteps = new List<GameObject>();
        textSteps.Add(textStep1);
        textSteps.Add(textStep2);
        textSteps.Add(textStep3);
        textSteps.Add(textStep4);
        textSteps.Add(textStep5);
        textSteps.Add(textStep6);

        startPanel.SetActive(true);
        infoPanel.SetActive(false);
        activeInfoPanelTimes = 0;
        startPanelHandle(activeInfoPanelTimes);
    }

    void Update()
    {

        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0.6f));
        transform.rotation = Camera.main.transform.rotation;

        int temptimes = handBehaviour.GetActiveInfoPanelTimes();
        if (temptimes != activeInfoPanelTimes)
        {
            if (temptimes < textSteps.Count)
            {
                // start panel
                startPanel.SetActive(true);
                infoPanel.SetActive(false);
                startPanelHandle(temptimes);
            }
            else if ((temptimes - textSteps.Count) % 2 == 1)
            {
                // info panel
                startPanel.SetActive(false);
                infoPanel.SetActive(true);
                infoPanelHandle();
            }
            else
            {
                // empty panel
                startPanel.SetActive(false);
                infoPanel.SetActive(false);
            }
            activeInfoPanelTimes = temptimes;
        }

    }

    void infoPanelHandle()
    {
        activeMode = handBehaviour.GetActiveOptModePanel();

        // show info
        switch (activeMode)
        {
            case OptModePanel.sculptor:
                textSculptor.SetActive(true);
                textMirror.SetActive(false);
                textNetwork.SetActive(false);
                textReplay.SetActive(false);
                break;

            case OptModePanel.mirror:
                textSculptor.SetActive(false);
                textMirror.SetActive(true);
                textNetwork.SetActive(false);
                textReplay.SetActive(false);
                break;

            case OptModePanel.network:
                textSculptor.SetActive(false);
                textMirror.SetActive(false);
                textNetwork.SetActive(true);
                textReplay.SetActive(false);
                break;

            case OptModePanel.replay:
                textSculptor.SetActive(false);
                textMirror.SetActive(false);
                textNetwork.SetActive(false);
                textReplay.SetActive(true);
                break;
        }
        infoPanel.GetComponentInChildren<Text>().color = Color.green;
    }

    void startPanelHandle(int activeText)
    {
        for (int tempi = 0; tempi < textSteps.Count; tempi++)
        {
            if (tempi == activeText)
                textSteps[tempi].SetActive(true);
            else
                textSteps[tempi].SetActive(false);
        }
        startPanel.GetComponentInChildren<Text>().color = Color.red;
    }

}
