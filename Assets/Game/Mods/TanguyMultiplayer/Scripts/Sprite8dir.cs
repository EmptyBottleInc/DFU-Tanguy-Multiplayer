using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprite8dir : MonoBehaviour
{
	public Transform cam;
	public float frequency = 0.25f;
	public float angle;
	public SpriteRenderer renderer;
	public PlayerAssets playerAssets;
	Vector3 lastPos;
	int walkIndex = 0;
	
	void Start()
	{
		StartCoroutine(checkAngles());
	}
	
	IEnumerator checkAngles()
	{
		yield return new WaitForSeconds(1f);
		cam = PlayerMultiplayer.localPlayer.transform;
		while (true)
		{
			if (cam != null){
				angle = getAngleToTarget();
				yield return new WaitForSeconds(0.1f);
				if (Vector3.Distance(lastPos, transform.position) < 0.05f)
					renderer.sprite = playerAssets.getIdleSprite(getIndex(angle));
				else{
					renderer.sprite = playerAssets.getWalkSprite(getIndex(angle), walkIndex);
					walkIndex++;
					if (walkIndex >= playerAssets.getWalkCount())
						walkIndex = 0;
				}
				lastPos = transform.position;
			}
			yield return new WaitForSeconds(frequency);
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
	
	
	

}
