using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkAssist : NetworkBehaviour {

	public TextMesh IPAddressText;
	private bool foundIPAddress = false;
	public bool isClientReady = false;
	public bool showIPAddress = false;

	//192.168.0.25

	private NetworkManager manager;
	void Awake()
	{
		manager = GetComponent<NetworkManager>();

		if(IPAddressText != null)
			IPAddressText.text = "";
	}


	//private bool isNetworkActive = false;
	void Update () {

		//isNetworkActive = NetworkManager.singleton.isNetworkActive;

		if (!foundIPAddress && showIPAddress && IPAddressText != null) {
			IPAddressText.text = Network.player.ipAddress;
		}

		if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null) {
			if (isClientReady) {
				manager.StartClient();
			} else {
				manager.StartHost();
			}	
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isClientReady) {
				manager.StopClient ();
			} else {
				manager.StopHost();
			}
		}

		GameObject networkPlayer = GameObject.Find("NetworkPlayer(Clone)");
		if (networkPlayer != null) {
			someoneDetected = true;
			Debug.Log("Someone Dectected");
		} else {
			someoneDetected = false;
			Debug.Log("No-one Dectected");
		}

	}

	private bool someoneDetected = false;
	public bool IsAnyoneDetected() {
		return someoneDetected;
	}

	void OnApplicationQuit(){
		if (isClientReady) manager.StopClient ();
		else manager.StopHost ();
	}
}
