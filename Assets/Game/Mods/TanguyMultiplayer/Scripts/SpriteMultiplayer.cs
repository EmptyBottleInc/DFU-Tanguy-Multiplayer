using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect;
using DaggerfallWorkshop.Utility;

public class SpriteMultiplayer : MonoBehaviour
{
	[Header("Parameters")]
	public float minDistance = 0.05f;
	public float walkInterval = 0.5f;
	public SoundClips walkSound;
	public SoundClips attackSound;
	public SoundClips bowSound;
	public LayerMask groundLayer;
	
	[Header("Runtime")]
	public string job;
	public int gender;
	
	[Header("References")]
	public SetupDemoEnemy demoEnemy;
	public DaggerfallMobileUnit sprite;
	public MobileTypes type;
	public EnemySounds sound;
	public DaggerfallAudioSource aud;
	
	CharacterController controller;
	
	
	
	bool isAttacking = false;
	float timeWalk = 0f;
	
	void Start()
	{
		demoEnemy.ApplyEnemySettings(getCorrectType(job), MobileReactions.Passive, (gender == 0 ? MobileGender.Male : MobileGender.Female), 0, true);
		sprite.ChangeEnemyState(MobileStates.Idle);
		controller = GetComponent<CharacterController>();
		GameObjectHelper.AlignControllerToGround(controller);
		
	}
	
	void OnEnable()
	{
		StartCoroutine(Check());
	}
	
	IEnumerator Check()
	{
		yield return new WaitForSeconds(0.1f);
		Vector3 lastPos = transform.position;lastPos.y = 0;
		while(true)
		{
			if (!sprite.IsPlayingOneShot()){
				Vector3 actualPos = transform.position; actualPos.y = 0;
				if (Vector3.Distance(lastPos, actualPos) > minDistance){
					sprite.ChangeEnemyState(MobileStates.Move);
					lastPos = actualPos;
					if (Time.time > timeWalk+ walkInterval){
						timeWalk = Time.time;
						aud.PlayOneShot(walkSound);
					}
				}else
					sprite.ChangeEnemyState(MobileStates.Idle);
			}
			placeBase();
			
			
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	void placeBase()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.parent.position, -transform.up, out hit, 1.2f, groundLayer))
			GameObjectHelper.AlignControllerToGround(controller);
	}
	
	public void playAttack()
	{
		aud.PlayOneShot(attackSound);
		sprite.ChangeEnemyState(MobileStates.PrimaryAttack);
		
	}
	
	public void playBow()
	{
		aud.PlayOneShot(bowSound);
		sprite.ChangeEnemyState(MobileStates.RangedAttack1);
	}
	
	MobileTypes getCorrectType(string s)
	{
		switch (s){
			case "Spellsword":
				return MobileTypes.Spellsword;
				break;
			case "Warrior":
				return MobileTypes.Warrior;
				break;
			case "Battlemage":
				return MobileTypes.Battlemage;
				break;
			case "Sorcerer":
				return MobileTypes.Sorcerer;
				break;
			case "Bard":
				return MobileTypes.Bard;
				break;
			case "Mage":
				return MobileTypes.Mage;
				break;
			case "Healer":
				return MobileTypes.Healer;
				break;
			case "Nightblade":
				return MobileTypes.Nightblade;
				break;
			case "Burglar":
				return MobileTypes.Burglar;
				break;
			case "Acrobat":
				return MobileTypes.Acrobat;
				break;
			case "Rogue":
				return MobileTypes.Rogue;
				break;
			case "Assassin":
				return MobileTypes.Assassin;
				break;
			case "Archer":
				return MobileTypes.Archer;
				break;
			case "Ranger":
				return MobileTypes.Ranger;
				break;
			case "Monk":
				return MobileTypes.Monk;
				break;
			case "Barbarian":
				return MobileTypes.Barbarian;
				break;
			case "Knight":
				return MobileTypes.Knight;
				break;
			
		}
		return MobileTypes.Thief;
	}
	
}
