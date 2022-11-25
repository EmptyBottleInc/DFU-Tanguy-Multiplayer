using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using Mirror;
using DaggerfallConnect;

public class DoorsCatcher : NetworkBehaviour
{
	Transform cam;
	public List<MonoBehaviour> doorsOpened = new List<MonoBehaviour>();
	public LayerMask layerMask;
	
	void Start()
	{
		cam = Camera.main.transform;
	}
	
	
	
	void Update()
	{
		RaycastHit hit;
		if (Physics.Raycast(cam.position, cam.forward, out hit, 3.5f, layerMask)){
			DaggerfallActionDoor door = hit.transform.gameObject.GetComponent<DaggerfallActionDoor>();
			if (door != null){
				if (door.IsMoving)
					activateDoor(door, hit.point, true);
				else
					removeDoor(door);
			}else{
				DaggerfallAction action = hit.transform.gameObject.GetComponent<DaggerfallAction>();
				
				if (action != null){
					if (action.IsMoving && action.TriggerFlag == DFBlock.RdbTriggerFlags.Direct)
						activateThing(action, hit.point, false);
					else
						removeDoor(action);
				}
			}
		}
		
	}
	
	void activateDoor(DaggerfallActionDoor door, Vector3 point, bool isDoor)
	{
		if (!doorsOpened.Contains(door)){
			doorsOpened.Add(door);
			if (doorsOpened.Count > 5)
				doorsOpened.RemoveAt(0);
			
			
			cmdToggleDoorOn(point.x, point.y, point.z, getStateBool(door.CurrentState), isDoor);
		}
	}
	
	void activateThing(DaggerfallAction door, Vector3 point, bool isDoor)
	{
		if (!doorsOpened.Contains(door)){
			doorsOpened.Add(door);
			if (doorsOpened.Count > 5)
				doorsOpened.RemoveAt(0);
			
			
			cmdToggleDoorOn(point.x, point.y, point.z, getStateBool(door.CurrentState), isDoor);
		}
			
	}
	
	void removeDoor(MonoBehaviour door)
	{
		if (doorsOpened.Contains(door))
			doorsOpened.Remove(door);
	}
	

	
	
	[Command]
	void cmdToggleDoorOn(float x, float y, float z, bool isOpen, bool isDoor)
	{
		rpcToggleDoorOn(x, y, z, isOpen, isDoor);
	}
	
	[ClientRpc]
	void rpcToggleDoorOn(float x, float y, float z, bool isOpen, bool isDoor)
	{
		if (!isLocalPlayer){
			Collider[] hitColliders = Physics.OverlapSphere(new Vector3(x, y, z) , 0.5f, layerMask);
			foreach (var hitCollider in hitColliders)
			{
				if (isDoor){
					DaggerfallActionDoor door = hitCollider.gameObject.GetComponent<DaggerfallActionDoor>();
					if (door != null){
						door.SetOpen(isOpen, false, true);
					}
				}else{
					DaggerfallAction action = hitCollider.gameObject.GetComponent<DaggerfallAction>();
					if (action != null){
						action.Receive(PlayerMultiplayer.playerObject, DaggerfallAction.TriggerTypes.Direct);
					}
				}
			}
		}
		
	}
	
	bool getStateBool(ActionState a)
	{
		return a == ActionState.PlayingForward;
	}
	
}
