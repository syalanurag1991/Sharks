using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkManager : MonoBehaviour {

    public bool personPresent = false;

	public SharkState state;
	public Transform testTarget;

    public CameraShake cameraShake;

	public bool inAttackArea = false;

	public AudioSource audioz;

	ParticleSystem ps;

	void Start() {
		ps = GetComponentInChildren<ParticleSystem> ();
	}

	void Update ()
    {

		if(inAttackArea && personPresent)
		{
		    ChangeState(SharkState.Curious);
            inAttackArea = false;
		}
	}

    //Managed from MainController/CameraInput
    public void ChangePersonPresent(bool val)
    {
        personPresent = val;
    }

    void Charge()
	{
		StartCoroutine(ChargeForward());
	}

	IEnumerator ChargeForward()
	{

		//transform.LookAt(testTarget);
		//StartCoroutine(RotateTowards());

		audioz.PlayOneShot(audioz.clip);

		float speed = Vector3.Distance(transform.position, testTarget.position)/1.5f;
        bool unshaken = true;
        print("this" + speed);
        speed *= 1.5f;
		float time = 0f;
		while(time < 1.5f)
		{
			//emmit buubles here
			ps.Emit(10);

			time += Time.deltaTime;
            //float tempSpeed = Mathf.Abs(speed*(2.5f - (time*2)));
            //BEGIN SUPER DUPER MAGICAL NUMBERS :P
            if (time > 0.8f && unshaken)
            {
                cameraShake.ShakeCamera(Random.Range(0.1f, 0.3f), Random.Range(0.1f, 0.5f));
                unshaken = false;
            }
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
			yield return null;
		}
		ChangeState(SharkState.Idle);
		yield break;
	}

	IEnumerator RotateTowards()
	{
		Vector3 direction = testTarget.position - transform.position;
 		Quaternion toRotation = Quaternion.LookRotation(direction);
 		float lerp = 0;
 		while(lerp < 1f)
 		{
 			lerp += 0.05f;
 			transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, lerp);
 			yield return null;
 		}
 		GetComponent<SwimAIFollow>().enabled = false;
 		GetComponent<Animator>().speed = 0.5f;
        StartCoroutine(ChangeStateDelayed(SharkState.Attacking, 1.5f));
 		yield break;
	}

	public void ChangeState(SharkState newState)
	{
		state = newState;
		StateStart();
	}

    public IEnumerator ChangeStateDelayed(SharkState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeState(newState);
    }

	void StateStart()
	{
		switch(state)
		{
			case(SharkState.Idle):
				GetComponent<SwimAIFollow>().enabled = true;
				break;
			case(SharkState.Attacking):
				GetComponent<Animator>().speed = 1f;
				GetComponent<Animator>().SetTrigger("attack");
				GetComponent<SwimAIFollow>().enabled = false;
				Charge();
				break;
			case(SharkState.Curious):
				StartCoroutine(RotateTowards());
				break;
			default:
			break;
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.name == "SharkAttackArea")
		{
			print("shark attack area");
			inAttackArea = true;
		}
	}

	void OnTriggerExit(Collider col)
	{
		if(col.gameObject.name == "SharkAttackArea")
		{
			inAttackArea = false;
			print("shark attack area");
		}
	}

}
