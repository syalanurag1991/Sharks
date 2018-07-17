using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugColorChange : MonoBehaviour {

	public Color initialColor;
	public Color activeColor;

	// Use this for initialization
	void Start () {
		GetComponent<Renderer> ().material.color = initialColor;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ChangeColor(bool active)
	{
		if (active)
			GetComponent<Renderer> ().material.color = activeColor;
		else
			GetComponent<Renderer> ().material.color = initialColor;
	}
		
}
