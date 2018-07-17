using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour 
{
	public List<Player> players = new List<Player>();
	public GameObject playerPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //TESTING
		if(Input.GetKeyDown(KeyCode.Space))
		{
            Spawn(new Vector3(0, 0.5f, 0) , Quaternion.identity);
		}

	}

	public Player Spawn(Vector3 loc, Quaternion rot) {
		Player newPlayer = Instantiate (playerPrefab, loc, rot).GetComponent<Player>();
		players.Add (newPlayer);
		newPlayer.SetIndex (players.Count-1);
		return newPlayer;
	}

	public void Destroy(int ind) {
		Destroy (players [ind].gameObject);
	}

	public PlayerState GetState(int ind) {
		return players [ind].GetState ();
	}

}
