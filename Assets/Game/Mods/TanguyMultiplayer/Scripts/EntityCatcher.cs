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
		public DaggerfallEntityBehaviour ed;
		public Vector3 pos;
		public string title;
		public string referedId;
		public int state;//0 idle, 1 controlled by local, 2 controlled by network
		
		public enemy(DaggerfallEntityBehaviour e, Vector3 p, string s, string id, int b)
		{
			ed = e;
			pos = p;
			title = s;
			referedId = id;
			state = b;
		}
		
		
	}
	
	public float syncRange = 1.5f;
	public float radius = 500f;
	public LayerMask layer;
	public static List<enemy> enemies = new List<enemy>();
	public List<enemy> enemiesDebug;
	GameObject[] locations;
	GameObject[] locationsChilds = new GameObject[3];
	public int[] childIndex = new int[] {0, 0, 2};
	
	
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
		yield return new WaitForSeconds(1f);
		CheckEntities();
		
		PlayerEnterExit pEnterExit = PlayerMultiplayer.playerObject.GetComponent<PlayerEnterExit>();
		locations = new GameObject[3];
		locations[0] = pEnterExit.ExteriorParent;
		locations[1] = pEnterExit.InteriorParent;
		locations[2] = pEnterExit.DungeonParent;
		
		int locationEnabled = getLocationEnabled();
		
		
		while (true)
		{
			if (hasLocationChanged(locationEnabled)){
				locationEnabled = getLocationEnabled();
				yield return new WaitForSecondsRealtime(1.1f);
				CheckEntities();
				
				if (enemies.Count == 0){//Relaunch the checking later, in certain areas such as castles, it sometimes needs
					yield return new WaitForSecondsRealtime(1f);
					CheckEntities();
				}
			}else
				CheckEncounters();
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
	
	bool hasLocationChanged(int last)
	{
		bool res = false;
		int actual = getLocationEnabled();
		if (actual != last || actual == 1)
			res = true;
		if (locations[last].transform.childCount >= childIndex[last]+1){
			if (locationsChilds[last] != locations[last].transform.GetChild(childIndex[last]).gameObject){
				res = true;
				locationsChilds[0] = (locations[0].transform.childCount >= childIndex[0]+1 ? locations[0].transform.GetChild(childIndex[0]).gameObject : null);
				locationsChilds[1] = (locations[1].transform.childCount >= childIndex[1]+1 ? locations[1].transform.GetChild(childIndex[1]).gameObject : null);
				locationsChilds[2] = (locations[2].transform.childCount >= childIndex[2]+1 ? locations[2].transform.GetChild(childIndex[2]).gameObject : null);
			}
		}
		
		return res;
	}
	
	
	void CheckEncounters()
	{
		print("CHECK RANDOM ENCOUNTERS");
		string request = "";
		Collider[] hitColliders = Physics.OverlapSphere(transform.position , 50, layer);
        foreach (var hitCollider in hitColliders)
        {
			GameObject g = hitCollider.gameObject;
            DaggerfallEntityBehaviour ed = g.GetComponent<DaggerfallEntityBehaviour>();
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
				enemies.Add(isRegistered(g.GetComponent<DaggerfallEntityBehaviour>(), g.transform.localPosition));
				enemiesDebug = enemies;
				
			}
        }
		print("REQUEST SEND " + request);
		if (request != ""){
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
		print("REQUEST RECEIVED " + s);
		if (!isLocalPlayer){
			this.enabled = true;
			StartCoroutine(tryEncounters(s, 8));
		}
	}
	
	//If the other player isn't in same location, retry later multiple times
	IEnumerator tryEncounters(string s, int tries)
	{
		List<string> lines = new List<string>(s.Split('#'));
		for (int i = 0; i < tries; i++){
			List<string> toRemove = new List<string>();
			for (int j = 0; j < lines.Count; j++){
				if (lines[j].Length > 1){
					
					string[] values = lines[j].Split('@');//0: name, 1: type, 2: gender, 3: allied, 4: posX, 5: posY, 6: posZ, 7: parent
					GameObject parent = GameObject.Find(values[7]);
						
					if (parent != null){
						toRemove.Add(lines[j]);
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
						enemies.Add(isRegistered(g.GetComponent<DaggerfallEntityBehaviour>(), pos, 2));
						enemiesDebug = enemies;
					}
				}
				yield return new WaitForSecondsRealtime(0.13f);
			}
			yield return new WaitForSecondsRealtime(0.1f);
			
			foreach (string r in toRemove){
				lines.Remove(r);
			}
			yield return new WaitForSeconds(4.25f);
		}
		this.enabled = false;
	}
	
	
	
	
	void CheckEntities()
	{
		print("CHECK ENTITIES");
		List<enemy> enemiesFounded = new List<enemy>();
		Collider[] hitColliders = Physics.OverlapSphere(transform.position , radius, layer);
        foreach (var hitCollider in hitColliders)
        {
			GameObject g = hitCollider.gameObject;
			if (g.GetComponent<DaggerfallEnemy>() != null)
				enemiesFounded.Add(isRegistered(g.GetComponent<DaggerfallEntityBehaviour>()));
			
        }
		enemies = enemiesFounded;
		enemiesDebug = enemiesFounded;
	}
	
	enemy isRegistered(DaggerfallEntityBehaviour ed)
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
		return new enemy(ed, ed.transform.position, ed.gameObject.name, PlayerMultiplayer.id, 0);
	}
	
	enemy isRegistered(DaggerfallEntityBehaviour ed, Vector3 pos, int state = 0)
	{
		if (ed.gameObject.name.Contains("*synced*")){
			foreach (enemy e in enemies)
			{
				if (e.ed == ed)
					return e;
			}
		}else
			ed.gameObject.name += "*synced*";
		return new enemy(ed, pos, ed.gameObject.name, PlayerMultiplayer.id, state);
	}
	
	
	void checkForDeaths()
	{
		int j = 0;
		int i = -1;
		foreach (enemy e in enemies)
		{
			if (e.ed != null && e.ed.Entity.CurrentHealth <= 0f){
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
							e.ed.Entity.SetHealth(-1);
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
		yield return new WaitForSeconds(1.5f);
		while (true)
		{
			for (int i = 0; i< enemies.Count; i++){
				if (enemies[i].ed != null){
					Vector3 newPos = enemies[i].ed.transform.position;
					if (enemies[i].state == 0 || enemies[i].state == 1){
						if (Vector3.Distance(enemies[i].pos, newPos) > 0.5f){ //check if the enemy moved from a player side
							enemies[i] = new enemy(enemies[i].ed, enemies[i].pos, enemies[i].title, PlayerMultiplayer.id, 1);
							cmdMoveEntityOn(enemies[i].pos.x, enemies[i].pos.y, enemies[i].pos.z, newPos.x, newPos.y, newPos.z, enemies[i].title, PlayerMultiplayer.id);
							
						}
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
				if (enemies[i].title == s && enemies[i].ed != null && (enemies[i].state == 0 || enemies[i].state == 2)){
					if (Vector3.Distance(enemies[i].pos, new Vector3(x, y, z)) < syncRange){
						enemies[i].ed.transform.position = new Vector3(newX, newY, newZ);
						//enemies[i].ed.GetComponent<EnemyMotor>().enabled = false;
						enemies[i] = new enemy(enemies[i].ed, enemies[i].pos, enemies[i].title, id, 2);
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
