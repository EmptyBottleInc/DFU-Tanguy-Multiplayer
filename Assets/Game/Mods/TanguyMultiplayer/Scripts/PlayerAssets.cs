using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect;
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
	
	Asset actualAsset;
	
	public TextMeshPro nameText;
	
	
	
	
    void Start()
    {
        if (isLocalPlayer)
			init();
		else
			Invoke("initProperAsset", 0.5f); //Delaying assets finding because sometimes the SyncVar aren't already set on start
	}
	
	void init()
	{
		DaggerfallEntity pEntity = PlayerMultiplayer.playerObject.GetComponent<DaggerfallEntityBehaviour>().Entity;
		Genders genders = pEntity.Gender;
		DFCareer career = pEntity.Career;
		
		cmdSendInfos(
			(genders == Genders.Male ? 0 : 1),
			career.Name, 
			pEntity.Name
		);
	}
	
	[Command]
	public void cmdSendInfos(int g, string j, string n)
	{
		gender = g;
		job = j;
		playerName = n;
	}
	
	
	
	
	
	void initProperAsset()
	{
		nameText.SetText(playerName);
		
		foreach (Asset a in assets){
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
		}
		actualAsset = assets[0];
		
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
	

}
