using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayAnimator : MonoBehaviour {

	float rotationSpeed = 3;

	// Use this for initialization
	void Start () {
		rotationSpeed *= Random.Range (0.8f, 1.2f);
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.Rotate(new Vector3(0,rotationSpeed*Time.deltaTime,0),Space.Self);
	}
}
