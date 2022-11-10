using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextBillboard : MonoBehaviour
{
	Transform cam;
	
	void Start()
	{
		cam = Camera.main.transform;
	}
	
	void Update() {
        transform.rotation = cam.rotation;
    }
}
