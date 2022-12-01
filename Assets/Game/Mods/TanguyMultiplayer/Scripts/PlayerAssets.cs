using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using System;
using TMPro;


public class PlayerAssets : NetworkBehaviour
{
	[Serializable]
	public struct Asset{
		public string Name;
		public List<string> Jobs;
		public int Gender;
		public List<Sprite> idleSprites;
		public int walkCount;
		public List<Sprite> walkSprites;
		public int attackCount;
		public List<Sprite> attackSprites;
	}
	
	
	public List<Asset> assets;
	
	[SyncVar]
	public int gender;
	[SyncVar]
	public string job;
	[SyncVar]
	public string playerName;
	
	
	public static int hostLevel = 0;
	
	Asset actualAsset;
	
	public TextMeshPro nameText;
	public GameObject torch;
	public SpriteMultiplayer spriteMultiplayer;
	DaggerfallEntity pEntity;
	
	
	
    void Start()
    {
        if (isLocalPlayer)
			init();
		else
			Invoke("initProperAsset", 0.5f); //Delaying assets finding because sometimes the SyncVar aren't already set on start
	}
	
	void init()
	{
		pEntity = GameManager.Instance.PlayerEntity as DaggerfallEntity;
		Genders genders = pEntity.Gender;
		DFCareer career = pEntity.Career;
		
		cmdSendInfos(
			(genders == Genders.Male ? 0 : 1),
			career.Name, 
			pEntity.Name
			
		);
		
		if (isServer){
			rpcSendHostLevel(pEntity.Level);
			StartCoroutine(TrackLevel());
		}else
			cmdApplyHostLevel();
		
		if (isLocalPlayer)
			StartCoroutine(trackTorch());
		/*if (!OptionsMultiplayer.useHighestLevel){
			
		}else{
			if (isLocalPlayer)
				rpcSendHostLevel(pEntity.Level);
		}*/
	}
	
	[Command]
	public void cmdSendInfos(int g, string j, string n)
	{
		gender = g;
		job = j;
		playerName = n;
	}
	
	[Command]
	public void cmdApplyHostLevel()
	{
		rpcSendHostLevel(hostLevel);
	}
	
	[ClientRpc]
	public void rpcSendHostLevel(int l)
	{
		hostLevel = l;
	}
	
	[Command]
	public void cmdSendHighLevel(int l)
	{
		rpcSendHighLevel(l);
	}
	
	[ClientRpc]
	public void rpcSendHighLevel(int l)
	{
		if (hostLevel < l)
			hostLevel = l;
	}
	
	IEnumerator TrackLevel()
	{
		while (true){
			yield return new WaitForSeconds(1.1f);
			if (hostLevel != pEntity.Level)
				rpcSendHostLevel(pEntity.Level);
		}
	}
	
	IEnumerator trackTorch()
	{
		yield return new WaitForSeconds(1.653f);
		GameObject playerTorch = PlayerMultiplayer.playerObject.GetComponent<EnablePlayerTorch>().PlayerTorch;
		bool lastState = false;
		while (true)
		{
			bool torchEnable = playerTorch.activeSelf;
			if (torchEnable != lastState){
				cmdEnableTorch(torchEnable);
				lastState = torchEnable;
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	
	[Command]
	void cmdEnableTorch(bool b)
	{
		rpcEnableTorch(b);
	}
	
	[ClientRpc]
	void rpcEnableTorch(bool b)
	{
		if (!isLocalPlayer)
			torch.SetActive(b);
	}
	
	
	void initProperAsset()
	{
		if (OptionsMultiplayer.displayName)
			nameText.SetText(playerName);
		else
			nameText.gameObject.SetActive(false);
		
		/*foreach (Asset a in assets){
			if (a.Jobs.Contains(job) && gender == a.Gender){
				actualAsset = a;
				return;
			}
		}
		foreach (Asset a in assets){
			if (gender == a.Gender){
				actualAsset = a;
				return;
			}
		}*/
		actualAsset = assets[0];
		spriteMultiplayer.gender = gender;
		spriteMultiplayer.job = job;
		spriteMultiplayer.gameObject.SetActive(true);
	}
	
	
	
	
	
	
	
	
	public Sprite getIdleSprite(int i){
		return actualAsset.idleSprites[i];
	}
	
	public Sprite getWalkSprite(int i, int j){
		return actualAsset.walkSprites[i*4+j];
	}
	
	public Sprite getAttackSprite(int i, int j){
		return actualAsset.attackSprites[j + i*4];
	}
	
	public int getWalkCount()
	{
		return actualAsset.walkCount;
	}
	public int getAttackCount()
	{
		return actualAsset.attackCount;
	}
	
	void OnDestroy()
	{
		if (isServer)
			hostLevel = 0;
	}

}
