using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
	public GameObject hud;
	
	void Start()
	{
		Instantiate(hud);
		HudMultiplayer.steamLobby = GetComponent<SteamLobby>();
	}
}
