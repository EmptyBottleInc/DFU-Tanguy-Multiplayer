using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop.Game;
using System;

public class EntityCatcher : NetworkBehaviour
{
	[Serializable]
	public struct enemy{
		public EnemyDeath ed;
		public Vector3 pos;
		public string title;
		
		public enemy(EnemyDeath e, Vector3 p, string s)
		{
			ed = e;
			pos = p;
			title = s;
		}
	}
	
	
	public float radius = 500f;
	public LayerMask layer;
	public static List<enemy> enemies;
	GameObject[] locations;
	
	void Start()
	{
		if (isLocalPlayer){
			StartCoroutine(Check());
			StartCoroutine(Search());
		}
	}
	
	
	IEnumerator Check()
	{
		yield return new WaitForSeconds(2f);
		while (true)
		{
			checkForDeaths();
			yield return new WaitForSeconds(.8f);
		}
	}
	
	IEnumerator Search()
	{
		CheckEntities();
		
		PlayerEnterExit pEnterExit = PlayerMultiplayer.playerObject.GetComponent<PlayerEnterExit>();
		locations = new GameObject[3];
		locations[0] = pEnterExit.ExteriorParent;
		locations[1] = pEnterExit.InteriorParent;
		locations[2] = pEnterExit.DungeonParent;
		
		int locationEnabled = getLocationEnabled();
		
		
		
		
		while (true)
		{
			int i = getLocationEnabled();
			if (locationEnabled != i){
				locationEnabled = i;
				CheckEntities();
			}
				
			yield return new WaitForSeconds(1.2f);
		}
	}
	
	int getLocationEnabled()
	{
		for (int i = 0; i < locations.Length; i++){
			if (locations[i].activeSelf)
				return i;
		}
		return -1;
	}
	
	void CheckEntities()
	{
		enemies = new List<enemy>();
		Collider[] hitColliders = Physics.OverlapSphere(transform.position , radius, layer);
        foreach (var hitCollider in hitColliders)
        {
            EnemyDeath ed = hitCollider.gameObject.GetComponent<EnemyDeath>();
			if (ed != null){
				enemies.Add(new enemy(ed, ed.transform.position, ed.gameObject.name));
			}
        }
	}
	
	void checkForDeaths()
	{
		int j = 0;
		int i = -1;
		foreach (enemy e in enemies)
		{
			if (e.ed.performDeath){
				Vector3 pos = e.pos;
				if (isServer)
					rpcDeathOn(pos.x, pos.y, pos.z, e.title);
				else
					cmdDeathOn(pos.x, pos.y, pos.z, e.title);
				i = j;
			}
			j++;
			
		}
		if (i != -1)
			enemies.RemoveAt(i);
	}
	
	[Command]
	public void cmdDeathOn(float x, float y, float z, string s)
	{
		rpcDeathOn(x, y, z, s);
	}
	
	
	[ClientRpc]
	public void rpcDeathOn(float x, float y, float z, string s)
	{
		if (!isLocalPlayer){
			int j = -1;
			foreach (enemy e in enemies){
				if (e.title == s){
					
					if (Vector3.Distance(e.pos, new Vector3(x, y, z)) < 8f){
						if (e.ed != null)
							e.ed.performDeath = true;
						j = enemies.IndexOf(e);
					}
				}
			}
			if (j != -1)
				enemies.RemoveAt(j);
			
			
			/*enemies[x].ed.CompleteDeath();
			enemies.RemoveAt(x);*/
		}
	}
	
	
}
