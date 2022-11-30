using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Weather;
using Mirror;

public class WeatherCatcher : NetworkBehaviour
{
	WeatherManager weatherManager;
	PlayerWeather playerWeather;
	WeatherType lastWeather;
	string weatherName;
	
	void Start()
	{
		init();
	}
	
	void init()
	{
		weatherManager = GameManager.Instance.WeatherManager;
		playerWeather = weatherManager.PlayerWeather;
		if (isLocalPlayer)
			StartCoroutine(Check());
	
	}
	
	
	IEnumerator Check()
	{
		lastWeather = playerWeather.WeatherType;
		
		while (true)
		{
			
			if (lastWeather != playerWeather.WeatherType){
				if (isServer){//Weather is controlled by the host to avoid apocalyptic weather change from every clients
					print("WEATHER SEND " + playerWeather.WeatherType.ToString());
					rpcSendWeather(playerWeather.WeatherType.ToString());
					lastWeather = playerWeather.WeatherType;
				}else{
					cmdReceiveWeather();
				}
			}
			
			yield return new WaitForSeconds(2.56f);
		}
	}
	
	[Command]
	void cmdReceiveWeather()
	{
		rpcSendWeather(playerWeather.WeatherType.ToString());
	}
	
	
	
	[ClientRpc]
	public void rpcSendWeather(string weather)
	{
		if (!isServer){
			weatherName = weather;
			setWeather();
		}
	}
	
	void setWeather()
	{
		if (GameObject.Find("Exterior") != null)
			weatherManager.SetWeather(getWeatherType(weatherName));
		else
			Invoke("setWeather", 0.5f); //retry setting weather later if player still isn't outside
	}
	
	
	
	WeatherType getWeatherType(string s)
	{
		switch (s) {
			case "Sunny":
				return WeatherType.Sunny;
				break;
			case "Cloudy":
				return WeatherType.Cloudy;
				break;
			case "Overcast":
				return WeatherType.Overcast;
				break;
			case "Fog":
				return WeatherType.Fog;
				break;
			case "Rain":
				return WeatherType.Rain;
				break;
			case "Rain_Normal":
				return WeatherType.Rain;
				break;
			case "Snow":
				return WeatherType.Snow;
				break;
			case "Snow_Normal":
				return WeatherType.Snow;
				break;
			case "Thunder":
				return WeatherType.Thunder;
				break;
			
		}
		return WeatherType.Sunny;
	}
}
