using UnityEngine;
using System.Collections;

public enum HandObj { left, right };

public class HandManager : MonoBehaviour {

    public GameObject HandObject;

    public GameObject ModelCreateObject;
    public GameObject ModelDeleteObject;
    public GameObject ModelSmoothObject;
    public GameObject ModelPaintObject;

    private HandBehaviour handBehaviour;
    public HandObj activeHand;
    public OptState activeState = OptState.create;

    // Use this for initialization
    void Start () {

        handBehaviour = HandObject.GetComponent<HandBehaviour>();

        ModelCreateObject.SetActive(true);
        ModelDeleteObject.SetActive(false);
        ModelSmoothObject.SetActive(false);
        ModelPaintObject.SetActive(false);

    }
	
	// Update is called once per frame
	void Update () {

        OptState tempState = handBehaviour.GetActiveStateRight();
        if (activeHand == HandObj.left)
        {
            tempState = handBehaviour.GetActiveStateLeft();
        }

        if (tempState != activeState)
        {
            switch (tempState)
            {
                case OptState.create:
                    ModelCreateObject.SetActive(true);
                    ModelDeleteObject.SetActive(false);
                    ModelSmoothObject.SetActive(false);
                    ModelPaintObject.SetActive(false);
                    break;

                case OptState.delete:
                    ModelCreateObject.SetActive(false);
                    ModelDeleteObject.SetActive(true);
                    ModelSmoothObject.SetActive(false);
                    ModelPaintObject.SetActive(false);
                    break;

                case OptState.smooth:
                    ModelCreateObject.SetActive(false);
                    ModelDeleteObject.SetActive(false);
                    ModelSmoothObject.SetActive(true);
                    ModelPaintObject.SetActive(false);
                    break;

                case OptState.paint:
                    ModelCreateObject.SetActive(false);
                    ModelDeleteObject.SetActive(false);
                    ModelSmoothObject.SetActive(false);
                    ModelPaintObject.SetActive(true);
                    break;
            }
            activeState = tempState;
        }

	}
}
