using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCaustic : MonoBehaviour {

	public Texture[] causticSprites;
	public float frameDuration = 0.1f;
	float timer;
	int curSprite = 0;

	// Use this for initialization
	void Start () {
		
	}
	

	// Update is called once per frame
	void Update () {
		if (timer >= frameDuration) {
			this.GetComponent<Projector> ().material.SetTexture ("_ShadowTex", causticSprites [curSprite]);
			curSprite++;
			if (curSprite >= causticSprites.Length) {
				curSprite = 0;
			}
			timer = 0;
		}

		timer += Time.deltaTime;
	}
}
