using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using UnityEngine.UI;
using Mirror;

public class HudMultiplayer : MonoBehaviour
{
	public Canvas canvas;
	public GraphicRaycaster raycaster;
	public GameObject[] checks;
	public static SteamLobby steamLobby;
	public Text status;
	public GameObject options, stop, host;
	
	DaggerfallUI gameUI;
	
	
    void Start()
    {
		StartCoroutine(Check());
    }
	
	
	IEnumerator Check()
	{
		InputManager inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
		gameUI = GameObject.Find("DaggerfallUI").GetComponent<DaggerfallUI>();
		UserInterfaceManager uiManager = gameUI.UserInterfaceManager;
		while (true)
		{
			
			canvas.enabled = isPauseMenu(uiManager);
			raycaster.enabled = canvas.enabled;
			if (canvas.enabled){
				setStatus();
				options.SetActive(PlayerMultiplayer.state == 0);
				host.SetActive(PlayerMultiplayer.state == 0);
				stop.SetActive(!(PlayerMultiplayer.state == 0));
			}
			
			yield return new WaitForSecondsRealtime(0.32f);
		}
	}
	
	bool isPauseMenu(UserInterfaceManager uiManager)
	{
		return (uiManager.TopWindow != null ? uiManager.TopWindow.ToString() == "DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallPauseOptionsWindow" : false);
	}
	
	void setStatus()
	{
		
		switch (PlayerMultiplayer.state){
			case 0:
				status.text = "Not connected";
				status.color = Color.red;
				break;
			case 1:
				status.text = "Connected as Host";
				status.color = Color.green;
				break;
			case 2:
				status.text = "Connected as Client";
				status.color = Color.green;
				break;
			default:
				status.text = "Unknown state";
				status.color = Color.blue;
				break;
			
		}
	}
	
	public void enableGameUI(bool b)
	{
		gameUI.enabled = b;
	}
	
	public void toggleTimeHost()
	{
		OptionsMultiplayer.timeHost = !OptionsMultiplayer.timeHost;
		checks[0].SetActive(OptionsMultiplayer.timeHost);
	}
	
	public void toggleName()
	{
		OptionsMultiplayer.displayName = !OptionsMultiplayer.displayName;
		checks[1].SetActive(OptionsMultiplayer.displayName);
	}
	
	public void toggleHighestLevel()
	{
		OptionsMultiplayer.useHighestLevel = !OptionsMultiplayer.useHighestLevel;
		checks[2].SetActive(OptionsMultiplayer.useHighestLevel);
	}
	
	public void toggleSendLocation()
	{
		OptionsMultiplayer.sendLocation = !OptionsMultiplayer.sendLocation;
		checks[3].SetActive(OptionsMultiplayer.sendLocation);
	}
	public void toggleSendMessage()
	{
		OptionsMultiplayer.sendMessage = !OptionsMultiplayer.sendMessage;
		checks[4].SetActive(OptionsMultiplayer.sendMessage);
	}
	
	public void refreshAllChecks()
	{
		checks[0].SetActive(OptionsMultiplayer.timeHost);
		checks[1].SetActive(OptionsMultiplayer.displayName);
		checks[2].SetActive(OptionsMultiplayer.useHighestLevel);
		checks[3].SetActive(OptionsMultiplayer.sendLocation);
		checks[4].SetActive(OptionsMultiplayer.sendMessage);
	}
	
	public void hostButton()
	{
		steamLobby.HostLobby();
	}
	
	public void stopButton()
	{
		steamLobby.StopNetwork();
	}
}
