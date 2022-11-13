using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using Mirror;

public class DoorsCatcher : NetworkBehaviour
{
	
	Transform cam;
	
	void Start()
	{
		cam = Camera.main.transform;
	}
	
	void Update()
	{
		if (Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			if (Physics.Raycast(cam.position, cam.forward, out hit, 3.3f)){
				DaggerfallActionDoor door = hit.transform.gameObject.GetComponent<DaggerfallActionDoor>();
				if (door != null)
					cmdToggleDoorOn(hit.point.x, hit.point.y, hit.point.z, !door.IsOpen);
			}
		}
	}
	
	
	[Command]
	void cmdToggleDoorOn(float x, float y, float z, bool isOpen)
	{
		
		rpcToggleDoorOn(x, y, z, isOpen);
	}
	
	[ClientRpc]
	void rpcToggleDoorOn(float x, float y, float z, bool isOpen)
	{
		if (!isLocalPlayer){
			Collider[] hitColliders = Physics.OverlapSphere(new Vector3(x, y, z) , 0.1f);
			foreach (var hitCollider in hitColliders)
			{
				
				DaggerfallActionDoor door = hitCollider.gameObject.GetComponent<DaggerfallActionDoor>();
				if (door != null){
					print("OPEN DOOR " + isOpen );
					door.SetOpen(isOpen);
				}
			}
		}
		
	}
	
	
	/*public Transform[] locations;
	public static List<DaggerfallActionDoor> doors;
	
	void Start()
	{
		if (isLocalPlayer)
			init();
	}
	
	void init()
	{
		PlayerEnterExit pEnterExit = PlayerMultiplayer.playerObject.GetComponent<PlayerEnterExit>();
		locations = new Transform[3];
		locations[0] = pEnterExit.ExteriorParent.transform;
		locations[1] = pEnterExit.InteriorParent.transform;
		locations[2] = pEnterExit.DungeonParent.transform;
		
		StartCoroutine(Check());
	}
	
	int getLocationEnabled()
	{
		for (int i = 0; i < locations.Length; i++){
			if (locations[i].gameObject.activeSelf)
				return i;
		}
		return 0;
	}
	
	
	IEnumerator Check()
	{
		int locationEnabled = getLocationEnabled();
		while (true)
		{
			int i = getLocationEnabled();
			if (locationEnabled != i)
				setDoors();
			
			
			CheckDoorsStates();
			yield return new WaitForSeconds(5.6f);
		}
	}
	
	
	void checkDoorsStates()
	{
		foreach(GameObject door in GameObject.FindGameObjectsWithTag("Untagged")){
			if (door.name !== "Action Doors"){
				foreach (Transform t in door.transform){
					
				}
			}
		}
	}
	
	void setDoors()
	{
		doors = new List<DaggerfallActionDoor>();
		foreach (GameObject door in GameObject.Finds
	}*/
}
