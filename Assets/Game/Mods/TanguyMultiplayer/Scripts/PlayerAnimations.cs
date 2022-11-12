using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using Mirror;

public class PlayerAnimations : NetworkBehaviour
{
	public Sprite8dir sprite8dir;
	
    void Start()
	{
		StartCoroutine(Check());
	}
	
	IEnumerator Check()
	{
		FPSWeapon weapon = PlayerMultiplayer.playerObject.GetComponent<WeaponManager>().ScreenWeapon;
		
		while (true){
			if (weapon.IsAttacking())
				cmdPlayAttack();
			
			yield return new WaitForSeconds(1.45f);
		}
	}
	
	[Command]
	void cmdPlayAttack()
	{
		rpcPlayAttack();
	}
	
	[ClientRpc]
	void rpcPlayAttack()
	{
		if (!isLocalPlayer)
			sprite8dir.playAttack();
	}
}
