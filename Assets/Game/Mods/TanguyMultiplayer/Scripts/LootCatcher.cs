using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Items;

public class LootCatcher : NetworkBehaviour
{
	
	public Transform[] locations;
	
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
		locations[2] = pEnterExit.DungeonParent.transform.GetChild(2);
		
		StartCoroutine(Check());
	}
	
	int getLocationEnabled()
	{
		for (int i = 0; i < locations.Length; i++){
			if (locations[i].gameObject.activeSelf)
				return i;
		}
		return -1;
	}
	
	
	IEnumerator Check()
	{
		
		while (true)
		{
			DaggerfallLoot loot = getPlayerLoot(getLocationEnabled());
			if (loot != null)
				prepareLootSending(loot);
			
			
			yield return new WaitForSeconds(5.6f);
		}
	}
	
	DaggerfallLoot getPlayerLoot(int loc)
	{
		foreach (Transform t in locations[loc]){
			print("LOOT FOUNDED");
			if (t.name == "DroppedLoot"){
				
				DaggerfallLoot loot = t.gameObject.GetComponent<DaggerfallLoot>();
				if (loot != null){
					if (loot.playerOwned){
						t.name +="#";
						return loot;
					}
				}
				
			}
		}
		return null;
	}
	
	void prepareLootSending(DaggerfallLoot loot)
	{
		string items = "";
		foreach (DaggerfallUnityItem item in loot.Items.CloneAll()){
			items+=item.shortName + "#";
		}
		
		print("LOOT ITEMS " + items);
		
	}
}
