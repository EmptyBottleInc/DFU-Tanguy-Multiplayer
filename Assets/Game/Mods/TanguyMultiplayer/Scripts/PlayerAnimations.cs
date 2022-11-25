using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop;
using Mirror;

public class PlayerAnimations : NetworkBehaviour
{
	//public Sprite8dir sprite8dir;
	public SpriteMultiplayer sprite;
	
    void Start()
	{
		StartCoroutine(Check());
	}
	
	IEnumerator Check()
	{
		FPSWeapon weapon = PlayerMultiplayer.playerObject.GetComponent<WeaponManager>().ScreenWeapon;
		
		while (true){
			if (weapon.IsAttacking()){
				cmdPlayAttack(weapon.WeaponType != WeaponTypes.Bow);
				yield return new WaitForSeconds(1f);
			}
			
			yield return new WaitForSeconds(0.12f);
		}
	}
	
	[Command]
	void cmdPlayAttack(bool melee)
	{
		rpcPlayAttack(melee);
	}
	
	[ClientRpc]
	void rpcPlayAttack(bool melee)
	{
		if (!isLocalPlayer){
			if (melee)
				sprite.playAttack();
			else
				sprite.playBow();
			
		}
	}
}
