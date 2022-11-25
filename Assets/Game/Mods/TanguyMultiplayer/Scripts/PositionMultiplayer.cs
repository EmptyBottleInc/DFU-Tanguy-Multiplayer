using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop;

public class PositionMultiplayer : NetworkBehaviour
{
	public float hideDistance = 500;
	public float sendDistance = 50;
	public GameObject visual;
	StreamingWorld world;
	Transform playerTransform;
	
	[SyncVar]
	public int x, z;
	
    void Start()
    {
		
        if (isLocalPlayer){
			world = GameObject.Find("StreamingWorld").GetComponent<StreamingWorld>();
			playerTransform = PlayerMultiplayer.playerObject.transform;
			StartCoroutine(SetCoordinates());
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
		PlayerGPS gps = PlayerMultiplayer.playerObject.GetComponent<PlayerGPS>();
		while (true){
			visual.SetActive(Vector2.Distance(new Vector2(x, z), new Vector2(gps.WorldX, gps.WorldZ)) < hideDistance);
			yield return new WaitForSeconds(3.15f);
		}
	}
	
	
	IEnumerator SetCoordinates()
	{
		PlayerGPS gps = PlayerMultiplayer.playerObject.GetComponent<PlayerGPS>();
		while (true){
			int gpsX = gps.WorldX, gpsZ = gps.WorldZ;
			if (Vector2.Distance(new Vector2(x, z), new Vector2(gpsX, gpsZ)) > sendDistance){
				cmdSendCoordinates(gpsX, gpsZ);
			}
			yield return new WaitForSeconds(3.12f);
		}
	}
	
	
	[Command]
	public void cmdSendCoordinates(int _x, int _z)
	{
		x = _x;
		z = _z;
	}

}
