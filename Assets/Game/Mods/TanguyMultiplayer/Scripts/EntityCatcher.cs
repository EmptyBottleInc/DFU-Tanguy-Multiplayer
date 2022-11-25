using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using System;
using DaggerfallWorkshop.Game.Entity;

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
	
	public float syncRange = 1.5f;
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
				yield return new WaitForSecondsRealtime(1.1f);
				CheckEntities();
				
				if (enemies.Count == 0){//Relaunch the checking later, in certain areas such as castles, it sometimes needs
					yield return new WaitForSecondsRealtime(1f);
					CheckEntities();
				}
			}
			yield return new WaitForSecondsRealtime(1.2f);
			CheckEncounters();
			yield return new WaitForSecondsRealtime(0.8f);
		}
	}
	
	
	void CheckEncounters()
	{
		print("CHECK RANDOM ENCOUNTERS");
		string request = "";
		Collider[] hitColliders = Physics.OverlapSphere(transform.position , 50, layer);
        foreach (var hitCollider in hitColliders)
        {
			GameObject g = hitCollider.gameObject;
            EnemyDeath ed = g.GetComponent<EnemyDeath>();
			if (ed != null && !g.name.Contains("*synced*")){
				
				SetupDemoEnemy setupEnemy = g.GetComponent<SetupDemoEnemy>();
				Vector3 pos = g.transform.localPosition;
				request += g.name + "@"
					+ (int)setupEnemy.EnemyType + "@"
					+ (int)setupEnemy.EnemyGender + "@"
					+ setupEnemy.AlliedToPlayer + "@"
					+ pos.x + "@"
					+ pos.y + "@"
					+ pos.z + "@"
					+ g.transform.parent.name
					+ '#';
				enemies.Add(isRegistered(g.GetComponent<EnemyDeath>(), g.transform.localPosition));
				enemiesDebug = enemies;
				
			}
        }
		
		if (request != ""){
			if (isServer)
				rpcSendEncounters(request);
			else
				cmdSendEncounters(request);
			
		}
	}
	
	[Command]
	void cmdSendEncounters(string s)
	{
		rpcSendEncounters(s);
	}
	
	[ClientRpc]
	void rpcSendEncounters(string s)
	{
		print("REQUEST " + s);
		if (!isLocalPlayer){
			string[] lines = s.Split('#');
			foreach (string line in lines){
				if (line.Length > 1){
					
					string[] values = line.Split('@');//0: name, 1: type, 2: gender, 3: allied, 4: posX, 5: posY, 6: posZ, 7: parent
					GameObject parent = GameObject.Find(values[7]);
					
					if (parent != null){
						int type = int.Parse(values[1]);
						int gender = int.Parse(values[2]);
						
						//If player hasn't commit any crime, the guards will be destroyed, turning them into Knight so
						if ((MobileTypes) type == MobileTypes.Knight_CityWatch && GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.None){
							type = (int)MobileTypes.Knight;
							gender = (int)MobileGender.Male;
						}
						
						Vector3 pos = new Vector3(float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]));
						GameObject g = GameObjectHelper.CreateEnemy(values[0], (MobileTypes) type, pos, (MobileGender)gender, parent.transform, (values[3] == "True" ? MobileReactions.Passive : MobileReactions.Hostile));
						g.transform.localPosition = pos;
						g.name = values[0];
						enemies.Add(isRegistered(g.GetComponent<EnemyDeath>(), pos));
						enemiesDebug = enemies;
					}
				}
			}
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
		/*foreach (enemy e in enemies)
		{
			if (e.ed == ed)
				return e;
		}*/
		if (ed.gameObject.name.Contains("*synced*")){
			foreach (enemy e in enemies)
			{
				if (e.ed == ed)
					return e;
			}
		}else
			ed.gameObject.name += "*synced*";
		return new enemy(ed, ed.transform.position, ed.gameObject.name, PlayerMultiplayer.id);
	}
	
	enemy isRegistered(EnemyDeath ed, Vector3 pos)
	{
		if (ed.gameObject.name.Contains("*synced*")){
			foreach (enemy e in enemies)
			{
				if (e.ed == ed)
					return e;
			}
		}else
			ed.gameObject.name += "*synced*";
		return new enemy(ed, pos, ed.gameObject.name, PlayerMultiplayer.id);
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
					
					if (Vector3.Distance(e.pos, new Vector3(x, y, z)) < syncRange){
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
					if (Vector3.Distance(enemies[i].pos, new Vector3(x, y, z)) < syncRange*2f){
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
