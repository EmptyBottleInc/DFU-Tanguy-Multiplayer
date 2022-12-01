using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Entity;

public class LootCatcher : NetworkBehaviour
{
	public GameObject lootPrefab;
	public Transform[] locations;
	
	void Start()
	{
		if (isLocalPlayer)
			init();
	}
	
	void init()
	{
		PlayerEnterExit pEnterExit = GameManager.Instance.PlayerEnterExit;
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
		
		while (true)
		{
			DaggerfallLoot loot = getPlayerLoot(getLocationEnabled());
			if (loot != null){
				prepareLootSending(loot);
				
			}
			
			
			yield return new WaitForSeconds(3.6f);
		}
	}
	
	Transform getLocationLoot(int loc)
	{
		if (loc==1)
			return locations[loc].GetChild(1);
		return locations[loc].GetChild(2);
	}
	
	DaggerfallLoot getPlayerLoot(int loc)
	{
		foreach (Transform t in getLocationLoot(loc)){
			print("LOOT FOUNDED");
			if (t.name == "DroppedLoot"){
				
				DaggerfallLoot loot = t.gameObject.GetComponent<DaggerfallLoot>();
				if (loot != null){
					if (loot.playerOwned){
						t.name +="#";
						t.gameObject.layer = 0;
						t.gameObject.AddComponent<LootMultiplayer>();
						t.gameObject.GetComponent<LootMultiplayer>().lootCatcher = this;
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
			items+=(int)item.ItemGroup + "@" + item.TemplateIndex + '@' + item.NativeMaterialValue + '@' + item.stackCount + "#";
		}
		print("LOOT ITEMS " + items);
		Vector3 pos = loot.transform.position;
		cmdSpawnLoot(pos.x, pos.y, pos.z, items, loot.transform.parent.name);
	}
	
	[Command]
	void cmdSpawnLoot(float x, float y, float z, string s, string p)
	{
		rpcSpawnLoot(x, y, z, s, p);
	}
	
	[ClientRpc]
	void rpcSpawnLoot(float x, float y, float z, string items, string parentName)
	{
		if (!isLocalPlayer){
			GameObject parent = GameObject.Find(parentName);
			if (parent == null)
				return;
			GameObject lootHandler = Instantiate(lootPrefab, new Vector3(x, y, z), Quaternion.identity);
			lootHandler.GetComponent<DaggerfallBillboard>().SetMaterial(205, 0);
			lootHandler.GetComponent<DaggerfallBillboard>().AlignToBase();
			lootHandler.GetComponent<LootMultiplayer>().lootCatcher = PlayerMultiplayer.localPlayer.GetComponent<LootCatcher>();
			
			lootHandler.transform.parent = parent.transform;
			lootHandler.name = "MultiplayerLoot";
			DaggerfallLoot loot = lootHandler.GetComponent<DaggerfallLoot>();
			ItemCollection itemsList = new ItemCollection();
			foreach (string s in items.Split('#')){
				string[] infos = s.Split('@');
				if (infos.Length > 1){
					int templateIndex = int.Parse(infos[1]);
					int material = int.Parse(infos[2]);
					int stackCount = int.Parse(infos[3]);
					ItemGroups itemGroup = getItemGroup(int.Parse(infos[0]));
					if (itemGroup != ItemGroups.None){
						DaggerfallUnityItem item = ItemBuilder.CreateItem(itemGroup, templateIndex);
						item.stackCount = stackCount;
						switch (itemGroup){
							case ItemGroups.Weapons:
								ItemBuilder.ApplyWeaponMaterial(item, (WeaponMaterialTypes)material);
								break;
							case ItemGroups.Armor:
								PlayerEntity pEntity = GameManager.Instance.PlayerEntity;
								ItemBuilder.ApplyArmorSettings(item, pEntity.Gender, pEntity.Race, (ArmorMaterialTypes)material);
								break;
						}
						itemsList.AddItem(item);
					}
				}
			}
			loot.Items = itemsList;
		}
	}
	
	ItemGroups getItemGroup(int i)
	{
		switch ((ItemGroups)i){
			case ItemGroups.QuestItems:
				return ItemGroups.None; 
			
			
		}
		/*
		switch (s){
			case "Armor":
				return ItemGroups.Armor;
				break;
			case "Weapons":
				return ItemGroups.Weapons;
				break;
			case "MensClothing":
				return ItemGroups.MensClothing;
				break;
			case "WomensClothing":
				return ItemGroups.WomensClothing;
				break;
			case "MagicItems":
				return ItemGroups.MagicItems;
				break;
			case "Books":
				return ItemGroups.Books;
				break;
			case "PlantIngredients1":
				return ItemGroups.PlantIngredients1;
				break;
			case "PlantIngredients2":
				return ItemGroups.PlantIngredients2;
				break;
			case "CreatureIngredients1":
				return ItemGroups.CreatureIngredients1;
				break;
			case "CreatureIngredients2":
				return ItemGroups.CreatureIngredients2;
				break;
			case "CreatureIngredients3":
				return ItemGroups.CreatureIngredients3;
				break;
			case "MiscellaneousIngredients1":
				return ItemGroups.MiscellaneousIngredients1;
				break;
			case "MiscellaneousIngredients2":
				return ItemGroups.MiscellaneousIngredients2;
				break;
			case "MetalIngredients":
				return ItemGroups.MetalIngredients;
				break;
			case "ReligiousItems":
				return ItemGroups.ReligiousItems;
				break;
			case "Gems":
				return ItemGroups.Gems;
				break;
			case "Jewellery":
				return ItemGroups.Jewellery;
				break;
			case "Currency":
				return ItemGroups.Currency;
				break;
		}*/
		return (ItemGroups)i;
	}
	
	public void DisableLootOn(Vector3 pos)
	{
		clientDisableLootOn(pos.x, pos.y, pos.z);
	}
	
	[Client]
	void clientDisableLootOn(float x, float y, float z)
	{
		cmdDisableLootOn(x, y, z);
	}
	
	[Command]
	void cmdDisableLootOn(float x, float y, float z)
	{
		rpcDisableLootOn(x, y, z);
	}
	
	[ClientRpc]
	void rpcDisableLootOn(float x, float y, float z)
	{
		print("DISABLE LOOT ON");
		Collider[] hitColliders = Physics.OverlapSphere(new Vector3(x, y, z) , 0.1f);
		foreach (var hitCollider in hitColliders)
		{
			DaggerfallLoot loot = hitCollider.gameObject.GetComponent<DaggerfallLoot>();
			if (loot != null){
				print("LOOT DESTROYED");
				//Destroy(hitCollider.gameObject);
				hitCollider.gameObject.SetActive(false);
			}
		}
	}
	
	
	
}
