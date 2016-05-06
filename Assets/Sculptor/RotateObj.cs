using UnityEngine;
using System.Collections;

public class RotateObj : MonoBehaviour {

    public Vector3 rotateSpeed = new Vector3(0, 5, 0);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        this.transform.Rotate(rotateSpeed);

	}
}
