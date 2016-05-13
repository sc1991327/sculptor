using UnityEngine;
using System.Collections;

public class handAnchor : MonoBehaviour {

    public GameObject cameraManagerObj;

    public GameObject oculusAnchor;
    public GameObject steamAnchor;

    private CameraManager cameraManager;
    private VRMode vrMode;

	// Use this for initialization
	void Start () {

        cameraManager = cameraManagerObj.GetComponent<CameraManager>();

    }
	
	// Update is called once per frame
	void Update () {

        switch (cameraManager.GetVRMode())
        {
            case VRMode.None:
                break;

            case VRMode.SteamVR:
                transform.position = steamAnchor.transform.position;
                transform.rotation = steamAnchor.transform.rotation;
                transform.localScale = steamAnchor.transform.localScale;
                break;

            case VRMode.OculusVR:
                transform.position = oculusAnchor.transform.position;
                transform.rotation = oculusAnchor.transform.rotation;
                transform.localScale = oculusAnchor.transform.localScale;
                break;
        }

	}
}
