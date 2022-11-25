using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using Mirror;

public class TimeCatcher : NetworkBehaviour
{
	
	WorldTime worldTime;
	static ulong lastTime;
	
	void Start()
	{
		init();
	}
	
	void init()
	{
		worldTime = GameObject.Find("DaggerfallUnity").GetComponent<WorldTime>();

		if (isLocalPlayer)		
			StartCoroutine(Check());
		
	}
	
	
	IEnumerator Check()
	{
		lastTime = worldTime.Now.ToSeconds();
		
		while (true)
		{
			
			if (worldTime.Now.ToSeconds() - lastTime > 40){
				
				if (OptionsMultiplayer.timeHost && !isServer){
					cmdReceiveTime();
				}else{
					DaggerfallDateTime now = worldTime.Now;
					cmdSendTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
				}
				
				
			}
			lastTime = worldTime.Now.ToSeconds();
			yield return new WaitForSeconds(2.25f);
		}
	}
	
	[Command]
	public void cmdSendTime(int year, int month, int day, int hour, int minute, float second){
		rpcSendTime(year, month, day, hour, minute, second);
	}
	
	[Command]
	public void cmdReceiveTime()
	{
		DaggerfallDateTime now = worldTime.Now;
		receiveTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
	}
	
	[ClientRpc]
	void receiveTime(int year, int month, int day, int hour, int minute, float second)
	{
		if (isLocalPlayer){
			worldTime.DaggerfallDateTime = new DaggerfallDateTime(year, month, day, hour, minute, Mathf.Clamp(second, 0, 590917));
			lastTime = worldTime.Now.ToSeconds();
		}
	}
	
	[ClientRpc]
	public void rpcSendTime(int year, int month, int day, int hour, int minute, float second){
		if (!isLocalPlayer){
			worldTime.DaggerfallDateTime = new DaggerfallDateTime(year, month, day, hour, minute, Mathf.Clamp(second, 0, 590917));
			lastTime = worldTime.Now.ToSeconds();
		}
	}
	
	
	
}
