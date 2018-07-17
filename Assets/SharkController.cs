using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySteer.Behaviors;

public enum SharkState
{
    Idle,
    Pursuing,
	Curious,
    Attacking
}

public class SharkController : MonoBehaviour {

    public List<DetectableObject> targets = new List<DetectableObject>();

    public AutonomousVehicle sharkVehicle;
    public SteerForPursuit pursuit;
    public SwimAIFollow circle;
    public float moveSpeed;

    private Animator anim;
    private int currentTargetIndex = 0;

    private bool chanceToBite = true;

    public SharkState state = SharkState.Idle;


	// Use this for initialization
	void Start () {
        sharkVehicle = GetComponent<AutonomousVehicle>();
        pursuit = GetComponent<SteerForPursuit>();
        circle = GetComponent<SwimAIFollow>();
        anim = GetComponent<Animator>();
        InvokeRepeating("ChangePursuit", 1.0f, 10.0f);
        ChangeState(SharkState.Idle);
    }
	
	// Update is called once per frame
	void Update () {

        UpdateAnimationSpeed(GetCurrentSpeed() / 1.5f);

        if (chanceToBite)
            TryBite();
	}

    public void AddTarget(GameObject newTarget)
    {
       // if(!pursuit.isActiveAndEnabled)
       //     pursuit.enabled=true;

        DetectableObject temp = newTarget.GetComponent<DetectableObject>();
        if (temp != null)
            targets.Add(temp);
        ChangePursuit();
    }

    void ChangePursuit()
    {
        if (targets.Count <= 0)
            return;

        if (!pursuit.isActiveAndEnabled)
            pursuit.enabled = true;

        if (currentTargetIndex < targets.Count - 1)
            currentTargetIndex++;
        else
            currentTargetIndex = 0;

        pursuit.Quarry = targets[currentTargetIndex];
    }

    public void ChangePursuitSpecific(DetectableObject newTarget)
    {
        pursuit.Quarry = newTarget;
    }

    public void ChangePursuitWeight(float edgeProximity)
    {
        //20 as DEFAULT
        if(pursuit.isActiveAndEnabled)
            pursuit.Weight = 40 - (edgeProximity * 20);
        if (edgeProximity < 1.0f)
            ChangeState(SharkState.Pursuing);
        else
            ChangeState(SharkState.Idle);

    }

    public void ChangeState(SharkState newState)
    {
        state = newState;
        switch(state)
        {
            case (SharkState.Pursuing):
                //pursuit.Weight = true;
                chanceToBite = true;
                circle.enabled = false;
                break;
            case (SharkState.Idle):
                // pursuit.enabled = false;
                chanceToBite = false;
                circle.enabled = true;
                break;
            default:
                break;
        }
    }

    float GetCurrentSpeed()
    {
        if (sharkVehicle)
            return sharkVehicle.Speed;
        else
            return 1;
    }

    void UpdateAnimationSpeed(float newSpeed)
    {
        if(anim)
            anim.speed = newSpeed;
    }

    void TryBite()
    {
        //magic #'s ooo la la
        if (Random.Range(0, 800) > 1)
            return;

        anim.SetTrigger("eat");
    }
}
