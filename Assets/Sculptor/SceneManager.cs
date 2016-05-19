using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour {

    public GameObject cameraManagerObj;

    private CameraManager cameraManager;

	// Use this for initialization
	void Start () {

        cameraManager = cameraManagerObj.GetComponent<CameraManager>();

        if (cameraManager.GetVRMode() == VRMode.SteamVR)
        {
            transform.position = new Vector3(0, 1.28f, 0);
        }

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
