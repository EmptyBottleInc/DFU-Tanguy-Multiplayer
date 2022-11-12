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
		public string referedId;
		
		public enemy(EnemyDeath e, Vector3 p, string s, string id)
		{
			ed = e;
			pos = p;
			title = s;
			referedId = id;
		}
		
		
	}
	
	
	public float radius = 500f;
	public LayerMask layer;
	public static List<enemy> enemies = new List<enemy>();
	public List<enemy> enemiesDebug;
	GameObject[] locations;
	
	void Start()
	{
		if (isLocalPlayer){
			StartCoroutine(Check());
			StartCoroutine(Search());
			StartCoroutine(checkIfMove());
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
				
			yield return new WaitForSecondsRealtime(1.2f);
		}
	}
	
	int getLocationEnabled()
	{
		for (int i = 0; i < locations.Length; i++){
			if (locations[i].activeSelf)
				return i;
		}
		return 0;
	}
	
	void CheckEntities()
	{
		print("CHECK ENTITIES");
		List<enemy> enemiesFounded = new List<enemy>();
		Collider[] hitColliders = Physics.OverlapSphere(transform.position , radius, layer);
        foreach (var hitCollider in hitColliders)
        {
            EnemyDeath ed = hitCollider.gameObject.GetComponent<EnemyDeath>();
			if (ed != null)
				enemiesFounded.Add(isRegistered(ed));
			
        }
		enemies = enemiesFounded;
		enemiesDebug = enemiesFounded;
	}
	
	enemy isRegistered(EnemyDeath ed)
	{
		foreach (enemy e in enemies)
		{
			if (e.ed == ed)
				return e;
		}
		return new enemy(ed, ed.transform.position, ed.gameObject.name, PlayerMultiplayer.id);
	}
	
	
	void checkForDeaths()
	{
		int j = 0;
		int i = -1;
		foreach (enemy e in enemies)
		{
			if (e.ed != null && e.ed.performDeath){
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
	
	IEnumerator checkIfMove()
	{
		while (true)
		{
			for (int i = 0; i< enemies.Count; i++){
				if (enemies[i].ed != null){
					Vector3 newPos = enemies[i].ed.transform.position;
					if (enemies[i].referedId == PlayerMultiplayer.id && Vector3.Distance(enemies[i].pos, newPos) > 0.5f){ //check if the enemy moved from a player side
						enemies[i] = new enemy(enemies[i].ed, enemies[i].pos, enemies[i].title, PlayerMultiplayer.id);//Turning canMove off so it is controlled by this player
						cmdMoveEntityOn(enemies[i].pos.x, enemies[i].pos.y, enemies[i].pos.z, newPos.x, newPos.y, newPos.z, enemies[i].title, PlayerMultiplayer.id);
						
					}
				}
			}
			
			yield return new WaitForSeconds(0.12f);
		}
		
	}
	
	[Command]
	public void cmdMoveEntityOn(float x, float y, float z, float newX, float newY, float newZ, string s, string id){
		moveEntityOn(x, y, z, newX, newY, newZ, s, id);
	}
	
	[ClientRpc]
	public void moveEntityOn(float x, float y, float z, float newX, float newY, float newZ, string s, string id){
		if (!isLocalPlayer){
			
			for (int i = 0; i < enemies.Count; i++){
				if (enemies[i].title == s && enemies[i].ed != null){
					if (Vector3.Distance(enemies[i].pos, new Vector3(x, y, z)) < 8f){
						enemies[i].ed.transform.position = new Vector3(newX, newY, newZ);
						//enemies[i].ed.GetComponent<EnemyMotor>().enabled = false;
						enemies[i] = new enemy(enemies[i].ed, enemies[i].pos, enemies[i].title, id);
					}
				}
				
			}/*
			foreach (enemy e in enemies){
				if (e.title == s){
					if (Vector3.Distance(e.pos, new Vector3(x, y, z)) < 8f){
						e.ed.transform.position = new Vector3(newX, newY, newZ);
						e.ed.GetComponent<EnemyMotor>().enabled = false;
					}
				}
			}*/
		}
	}
	
	
}
