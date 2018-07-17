using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimAIFollow : MonoBehaviour {

	public Transform target;
	public float moveSpeed = 0.1f;
	public float rotationSpeed = 0.2f;
	float activationDistance = 0.15f;
	float range = 500f;
	float stop = 0f;

	public Animator animator;

	// Use this for initialization
	void Start () {
		animator = this.GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

		//rotate to look at the player --------------------
		this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
			Quaternion.LookRotation(target.position - this.transform.position), rotationSpeed*Time.deltaTime);

		//move towards the player -----------------
		float distance = Vector3.Distance(this.transform.position, target.position);

		if(distance<=range && distance>stop){
			this.transform.position += this.transform.forward * moveSpeed * Time.deltaTime;
		}
		else if (distance<=stop) {
			//TODO: do arrive slow down here
		}

		//Target Activation ---------------------------

		if (distance <= activationDistance) {
			print ("swimmer activated target");
			//activate something here
		}

		//change animation speed according to follow speed ----------------

		animator.speed = moveSpeed*0.5f; //adjust magic number

		
	}

	public void RetargetSwimmer(Transform newTarget, float newMoveSpeed, float newRotationSpeed) {
		//TODO: lerp to target speed
		target = newTarget;
		moveSpeed = newMoveSpeed;
		rotationSpeed = newRotationSpeed;
	}

	void OnTriggerEnter(Collider other) {
		//TODO: if player do sthg else
	}

}
