using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop;

public class PlayerMultiplayer : NetworkBehaviour
{
	[Header("References")]
	public GameObject[] toEnable;
	public NetworkBehaviour[] toDisable;
	public Sprite8dir sprite8dir;
	public string[] messages;
	
	public static GameObject playerObject;
	
	public static PlayerMultiplayer localPlayer;
	
	public static string id;
	public static int state = 0;
	public static int random = 0;
	static float randomTime = 0;
	
	List<GameObject> refered = new List<GameObject>();
	
	void Start()
	{
		setupLocal();
	}
	
	void init()
	{
		playerObject = GameManager.Instance.PlayerObject;
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
			Invoke("sendMessage", 1.5f);
		}
	}
	
	void sendMessage()
	{
		if (OptionsMultiplayer.sendMessage){
			PositionMultiplayer pos = GetComponent<PositionMultiplayer>();
			PlayerGPS gps = GameManager.Instance.PlayerGPS;
			PlayerAssets assets = GetComponent<PlayerAssets>();
			
			float distance = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(gps.WorldX, gps.WorldZ));
			string message = string.Format(messages[UnityEngine.Random.Range(3, 5)], assets.playerName);
			if (distance < 7500){
				message = string.Format(messages[0], assets.playerName);
			}else if (distance < 25000){
				message = string.Format(messages[1], assets.playerName);
			}else if (distance < 80000){
				message = string.Format(messages[2], assets.playerName);
			}
			DaggerfallUI.MessageBox(message);
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
	
	
	void OnDestroy()
	{
		for (int i = 0; i < refered.Count; i++){
			Destroy(refered[i]);
		}
		
		if (isLocalPlayer){
			state = 0;
		}
			
	}
	
	
	public static int getRandom(float min, float max)
	{
		if (randomTime + 1 < Time.time){
			randomTime = Time.time;
			random = 0;
		}
		random++;
		return (int)(min + (max-min) * Mathf.Pow(Mathf.Sin(random*4.132f), 2)-1);
	}
}
