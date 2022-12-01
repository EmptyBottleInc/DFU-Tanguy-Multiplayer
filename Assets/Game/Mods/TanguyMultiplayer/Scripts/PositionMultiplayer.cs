using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;

public class PositionMultiplayer : NetworkBehaviour
{
	public float hideDistance = 500;
	public float sendDistance = 50;
	public GameObject visual;
	StreamingWorld world;
	Transform playerTransform;
	PlayerGPS gps;
	
	[SyncVar]
	public int x = 0, z = 0;
	
    void Start()
    {
        if (isLocalPlayer){
			world = GameObject.Find("StreamingWorld").GetComponent<StreamingWorld>();
			playerTransform = GameManager.Instance.PlayerObject.transform;
			StartCoroutine(SetCoordinates());
			
			StartCoroutine(SendLocation());
		}else
			StartCoroutine(Check());
    }
	
	void FixedUpdate()
	{
		if (playerTransform != null){
			transform.position = playerTransform.position;
			transform.rotation = playerTransform.rotation;
		}
	}
	
	
	
	IEnumerator Check()
	{
		yield return new WaitForSeconds(2f);
		gps = GameManager.Instance.PlayerGPS;
		while (true){
			visual.SetActive(Vector2.Distance(new Vector2(x, z), new Vector2(gps.WorldX, gps.WorldZ)) < hideDistance);
			yield return new WaitForSeconds(3.15f);
		}
	}
	
	
	IEnumerator SetCoordinates()
	{
		yield return new WaitForSeconds(1f);
		gps = GameManager.Instance.PlayerGPS;
		while (true){
			int gpsX = gps.WorldX, gpsZ = gps.WorldZ;
			if (Vector2.Distance(new Vector2(x, z), new Vector2(gpsX, gpsZ)) > sendDistance){
				cmdSendCoordinates(gpsX, gpsZ);
			}
			
			yield return new WaitForSeconds(3.12f);
		}
	}
	
	IEnumerator SendLocation()
	{
		yield return new WaitForSeconds(1.5f);
		DFLocation location = gps.CurrentLocation;
		while (true){
			if (OptionsMultiplayer.sendLocation && location.Name != gps.CurrentLocation.Name){
				location = gps.CurrentLocation;
				cmdSendLocation(location.RegionName, location.Name);
			}
			yield return new WaitForSeconds(5.56f);
		}
	}
	
	[Command]
	void cmdSendLocation(string region, string location){
		rpcSendLocation(region, location);
	}
	
	[ClientRpc]
	void rpcSendLocation(string region, string location){
		if (!isLocalPlayer){
			gps.DiscoverLocation(region, location);
		}
	}
	
	
	[Command]
	public void cmdSendCoordinates(int _x, int _z)
	{
		x = _x;
		z = _z;
	}

}
