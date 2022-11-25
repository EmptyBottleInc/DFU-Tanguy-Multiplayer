using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;

public class Sprite8dir : MonoBehaviour
{
	public Transform cam;
	public float frequency = 0.25f;
	public float angle;
	public SpriteRenderer renderer;
	public PlayerAssets playerAssets;
	Vector3 lastPos;
	int walkIndex = 0, attackIndex = 0;
	bool isAttacking = false;
	public LayerMask groundLayer;
	DaggerfallAudioSource aud;
	public SoundClips attackSound;
	public SoundClips[] walkSounds;
	
	void OnEnable()
	{
		StopAllCoroutines();
		StartCoroutine(checkAngles());
	}
	
	IEnumerator checkAngles()
	{
		yield return new WaitForSeconds(1f);
		cam = PlayerMultiplayer.localPlayer.transform;
		aud = GetComponent<DaggerfallAudioSource>();
		while (true)
		{
			if (cam != null){
				angle = getAngleToTarget();
				placeBase();
				yield return new WaitForSeconds(0.1f);
				if (!isAttacking){
					if (Vector3.Distance(lastPos, transform.position) < 0.05f)
						renderer.sprite = playerAssets.getIdleSprite(getIndex(angle));
					else{
						renderer.sprite = playerAssets.getWalkSprite(getIndex(angle), walkIndex);
						walkIndex++;
						if (walkIndex >= playerAssets.getWalkCount())
							walkIndex = 0;
						if (walkIndex % 2 == 0)
							aud.PlayOneShot(walkSounds[Random.Range(0, walkSounds.Length)]);
					}
					lastPos = transform.position;
					yield return new WaitForSeconds(frequency);
				}else{
					
					renderer.sprite = playerAssets.getAttackSprite(getIndex(angle), attackIndex);
					attackIndex++;
					if (attackIndex >= playerAssets.getAttackCount()){
						attackIndex = 0;
						isAttacking = false;
					}
					if (attackIndex == 2)
						aud.PlayOneShot(attackSound);
					
					yield return new WaitForSeconds(0.07f);
				}
				
				
			}
			
		}
	}
	
	float getAngleToTarget()
	{
		Vector3 pos = transform.position;
		Vector3 pathPos = cam.position;
		Vector3 targetDir = pathPos - pos;
		Vector3 forward = transform.forward;
		
		
        return Vector3.SignedAngle(targetDir, forward, cam.up);
	}
	
	int getIndex(float angle)
	{
		renderer.flipX = (angle > 0f);
		angle = Mathf.Abs(angle);
		
		if (angle < 22.5f)
			return 0;
		if (angle < 67.5f)
			return 1;
		if (angle < 112.5f)
			return 2;
		if (angle < 157.5f)
			return 3;
		if (angle < 180f)
			return 4;
		return 0;
		
	}
	
	public void playAttack()
	{
		isAttacking = true;

	}
	
	void placeBase()
	{
		RaycastHit hit;
		Vector3 size = renderer.size * 2.1f;
		if (Physics.Raycast(transform.position, -transform.up, out hit, size.y/2f, groundLayer)){
			transform.position = hit.point +(transform.up* size.y * 0.45f);
		}else
			transform.localPosition = Vector3.zero;
	}

}
