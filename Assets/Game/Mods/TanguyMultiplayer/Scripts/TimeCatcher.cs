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
		lastTime = worldTime.Now.ToClassicDaggerfallTime();
		
		while (true)
		{
			
			if (worldTime.Now.ToClassicDaggerfallTime() - lastTime > 40){
				
				if (OptionsMultiplayer.timeHost && !isServer){
					cmdReceiveTime();
				}else{
					DaggerfallDateTime now = worldTime.Now;
					cmdSendTime(now.ToSeconds());//(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
				}
				
				
			}
			lastTime = worldTime.Now.ToClassicDaggerfallTime();
			yield return new WaitForSeconds(2.25f);
		}
	}
	
	[Command]
	public void cmdSendTime(ulong i/*int year, int month, int day, int hour, int minute, float second*/){
		rpcSendTime(i/*year, month, day, hour, minute, second*/);
	}
	
	[Command]
	public void cmdReceiveTime()
	{
		DaggerfallDateTime now = worldTime.Now;
		receiveTime(now.ToSeconds()/*now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second*/);
	}
	
	[ClientRpc]
	void receiveTime(ulong i/*int year, int month, int day, int hour, int minute, float second*/)
	{
		if (isLocalPlayer){
			worldTime.Now.FromSeconds(i);//new DaggerfallDateTime(/*year, month, day, hour, minute, Mathf.Clamp(second, 0, 590917)*/);
			lastTime = worldTime.Now.ToSeconds();
		}
	}
	
	[ClientRpc]
	public void rpcSendTime(ulong i/*int year, int month, int day, int hour, int minute, float second*/){
		if (!isLocalPlayer){
			worldTime.Now.FromSeconds(i);//(/*year, month, day, hour, minute, Mathf.Clamp(second, 0, 590917)*/);
			lastTime = worldTime.Now.ToSeconds();
		}
	}
	
	
	
}
