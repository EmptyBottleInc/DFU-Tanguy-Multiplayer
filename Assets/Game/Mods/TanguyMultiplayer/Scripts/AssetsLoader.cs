using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class AssetsLoader : MonoBehaviour
{
	#if UNITY_EDITOR
	public PlayerAssets playerAssets;
	
	[Header("EDITOR")]
	public string character = "TEXTURE.475 (Fighter Mage)";
	public bool search = false;
	
	
	void Update()
	{
		if (search){
			search = false;
			ImportAsset();
		}
	}
	
	void ImportAsset()
    {
		PlayerAssets.Asset asset = new PlayerAssets.Asset();
		asset.idleSprites = new List<Sprite>();
		asset.walkSprites = new List<Sprite>();
		asset.attackSprites = new List<Sprite>();
		asset.walkCount = 4;
		asset.attackCount = 6;
		
		//Looping over idle sprites
		for (int i = 0; i < 5; i++){
			int num = 15 + i;
			asset.idleSprites.Add(getSprite(num + "-0"));
		}
		
		//Looping over walk sprites
		for (int i = 0; i < 5; i++){
			for (int j = 0; j < 4; j++){
				asset.walkSprites.Add(getSprite(i + "-" + j));
			}
			
		}
		
		//Looping over attack sprites
		for (int i = 5; i < 10; i++){
			for (int j = 0; j < 6; j++){
				asset.attackSprites.Add(getSprite(i + "-" + j));
			}
			
		}
		
		playerAssets.assets.Add(asset);
    }
	
	Sprite getSprite(string s){
		return (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Game/Mods/TanguyMultiplayer/Textures/Sprites/" + character + "/" + s + ".PNG", typeof(Sprite));
	}
	#endif
}
