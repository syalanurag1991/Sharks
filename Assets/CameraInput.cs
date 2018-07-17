using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInput : MonoBehaviour {

    private bool personPresent = false;

    public List<SharkManager> sharks = new List<SharkManager>();
	public DebugColorChange colorChange;
	public NetworkAssist networkAssist;
    void Update()
    {
		if(networkAssist.IsAnyoneDetected() != personPresent)
			ChangePersonPresent(networkAssist.IsAnyoneDetected ());
        //For testing without camera input
        if (Input.GetKeyDown(KeyCode.P))
            ChangePersonPresent(true);
        if (Input.GetKeyUp(KeyCode.P))
            ChangePersonPresent(false);
    }


    public void ChangePersonPresent(bool newVal)
    {
        personPresent = newVal;
        foreach (SharkManager shark in sharks)
            shark.ChangePersonPresent(personPresent);
		colorChange.ChangeColor (newVal);
    }
}
