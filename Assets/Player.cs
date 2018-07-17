using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState {
	Entering,
	Active,
	Exiting
}

public class Player : MonoBehaviour {

	public PlayerState state;
	private int index;
	public float speed = 1.0f;

	public GameObject targetControlPrefab;
	public TargetController targetControl;
	private GameObject target;

	//Probably should just grab dome object and get these values so they only have to be changed in one place
	public float neutralRadius = 1f;
	public float outsideRadius = 2.5f;
	public float insideRadius;

	public float distanceFromEdge;

	// Use this for initialization
	void Start () {
		targetControl = Instantiate (targetControlPrefab, Vector3.zero, Quaternion.identity).GetComponent<TargetController> ();
	}
	
	// Update is called once per frame
	void Update () {

		Navigate ();
		distanceFromEdge = UpdateDistance();
		RotateTarget ();
		UpdateTargetDistance ();
	}

	//SUPER HACKY JUST FOR TESTING
	public void Navigate()
	{
		switch (index) {
		case(0):
			if (Input.GetKey (KeyCode.W))
				transform.Translate (Vector3.forward * Time.deltaTime * speed);
			if (Input.GetKey (KeyCode.A))
				transform.Rotate(Vector3.up * Time.deltaTime * 100);
			if (Input.GetKey (KeyCode.D))
				transform.Rotate (Vector3.up * Time.deltaTime * -100);
			if (Input.GetKey (KeyCode.S))
				transform.Translate (Vector3.back * Time.deltaTime * speed);
			break;
		case(1):
			if (Input.GetKey (KeyCode.UpArrow))
				transform.Translate (Vector3.forward * Time.deltaTime * speed);
			if (Input.GetKey (KeyCode.LeftArrow))
				transform.Translate (Vector3.left * Time.deltaTime * speed);
			if (Input.GetKey (KeyCode.RightArrow))
				transform.Translate (Vector3.right * Time.deltaTime * speed);
			if (Input.GetKey (KeyCode.DownArrow))
				transform.Translate (Vector3.back * Time.deltaTime * speed);
			break;
		default:
			break;
		}
	}

	public float UpdateDistance()
	{
		return outsideRadius - Vector3.Distance (Vector3.zero, this.transform.position);
	}

	public void RotateTarget()
	{
		targetControl.UpdateRotation (this.transform);
	}

	public void UpdateTargetDistance()
	{
		targetControl.MoveTarget (distanceFromEdge);
	}
			

	public int GetIndex()
	{
		return index;
	}

	public void SetIndex(int newInd)
	{
		index = newInd;
	}

	public void SetState(PlayerState newState)
	{
		state = newState;
	}

	public PlayerState GetState()
	{
		return state;
	}

	public void OnDestroy()
	{
        if(targetControl != null)
		    Destroy (targetControl.gameObject);
	}



}
