using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
	NetworkManager manager;
	protected Callback<LobbyCreated_t> lobbyCreated;
	protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
	protected Callback<LobbyEnter_t> lobbyEntered;
	const string hostAdressKey = "HostAdress";
	
	void Start()
	{
		manager = GetComponent<NetworkManager>();
		
		if (!SteamManager.Initialized) { return;}
		
		lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
		gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
		lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
	}
	
	
	public void HostLobby()
	{
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, GetComponent<NetworkManager>().maxConnections);
	}
	
	
	void OnLobbyCreated(LobbyCreated_t callback)
	{
		if (callback.m_eResult != EResult.k_EResultOK){
			return;
		}
		GetComponent<NetworkManager>().StartHost();
		SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAdressKey, SteamUser.GetSteamID().ToString());
		//GameManager.spawnLobby();
		//GameObject map = Instantiate(Resources.Load("Maps/Flower hotel/Flower hotel") as GameObject);
		//NetworkServer.Spawn(map);
	}
	
	void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
	{
		SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
	}
	
	void OnLobbyEntered(LobbyEnter_t callback)
	{
		if (NetworkServer.active)
			return;
		string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAdressKey);
		GetComponent<NetworkManager>().networkAddress = hostAddress;
		GetComponent<NetworkManager>().StartClient();
		//GameManager.spawnClientMap();
	}
	
	public void StopNetwork()
	{
		NetworkManager manager = NetworkManager.singleton;
		manager.StopClient();
		manager.StopServer();
		
	}
	
	
	
	
}
