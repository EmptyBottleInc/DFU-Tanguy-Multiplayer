using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootMultiplayer : MonoBehaviour
{
	public LootCatcher lootCatcher;
	
	
	void OnDisable()
	{
		print("LOOT REMOVED");
		lootCatcher.DisableLootOn(transform.position);
	}
	
	void OnDestroy()
	{
		print("LOOT REMOVED");
		lootCatcher.DisableLootOn(transform.position);
	}
}
