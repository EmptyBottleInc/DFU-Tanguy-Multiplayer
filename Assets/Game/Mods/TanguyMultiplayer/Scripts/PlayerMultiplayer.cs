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
	public static int state = 0;
	
	List<GameObject> refered = new List<GameObject>();
	
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
		state = isServer ? 1 : 2;
		if (!isServer)
			importOptions();
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
	
	
	public void importOptions()
	{
		cmdImportOptions();
	}
	
	[Command]
	public void cmdImportOptions()
	{
		rpcImportOptions(OptionsMultiplayer.Export());
	}
	
	[ClientRpc]
	public void rpcImportOptions(string s)
	{
		if (!isServer && localPlayer){
			OptionsMultiplayer.Import(s);
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
	
	void OnDestroy()
	{
		for (int i = 0; i < refered.Count; i++){
			Destroy(refered[i]);
		}
		
		if (isLocalPlayer){
			state = 0;
		}
			
	}
}
