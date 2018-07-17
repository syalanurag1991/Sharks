using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySteer.Behaviors;

public class TargetController : MonoBehaviour {

	private GameObject target;

    private SharkController shark;
	//private SwimAIFollow sharkAI;

	void Start()
	{
		shark = GameObject.Find ("MainShark").GetComponent<SharkController> ();

		target = transform.GetChild (0).gameObject;
        shark.AddTarget(target);
		//sharkAI.RetargetSwimmer (target.transform, sharkAI.moveSpeed, sharkAI.rotationSpeed);
	}
		

	public void UpdateRotation(Transform player)
	{
		Vector3 targetPos = new Vector3 (player.position.x, transform.position.y, player.position.z);
		transform.LookAt (targetPos);
	}

	public void MoveTarget(float playerEdgeDist)
	{
		float tempDist = MathFunctions.Map (playerEdgeDist, 0f, 2.5f, 0.5f, 5.0f);
		Vector3 newLoc = new Vector3 (0, 0, 2.5f + tempDist);
		target.transform.localPosition = newLoc;
        shark.ChangePursuitWeight(playerEdgeDist);
	}

	
}

public class MathFunctions
{
    public static float Map(float x, float x1, float x2, float y1, float y2)
    {
        var m = (y2 - y1) / (x2 - x1);
        var c = y1 - m * x1;
        return m * x + c;
    }
}
