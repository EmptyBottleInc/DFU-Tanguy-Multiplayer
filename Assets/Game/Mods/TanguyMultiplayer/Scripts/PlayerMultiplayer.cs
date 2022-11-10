using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMultiplayer : NetworkBehaviour
{
	[Header("References")]
	public GameObject[] toEnable;
	public NetworkBehaviour[] toDisable;
	public Sprite8dir sprite8dir;
	
	
	[Header("Runtime")]
	public Transform playerTransform;
	
	
	public static GameObject playerObject;
	
	public static PlayerMultiplayer localPlayer;
	
	public static string id;
	
	void Start()
	{
		setupLocal();
	}
	
	void init()
	{
		playerObject = GameObject.FindGameObjectsWithTag("Player")[0];
		playerTransform = playerObject.transform;
		transform.position = playerTransform.position;
		localPlayer = this;
		id = "" + GetComponent<NetworkIdentity>().netId;
	}
	
	void setupLocal()
	{
		if (isLocalPlayer){
			enableAll(false);
			init();
		}else{
			enableAll(true);
			this.enabled = false;
			
		}
	}
	
	void enableAll(bool b)
	{
		foreach (GameObject g in toEnable)
		{
			g.SetActive(b);
		}
		
		foreach (NetworkBehaviour n in toDisable)
		{
			n.enabled = !b;
		}
	}
	
	void FixedUpdate()
	{
		if (isLocalPlayer){
			transform.position = playerTransform.position;
			transform.rotation = playerTransform.rotation;
		}
	}
}
