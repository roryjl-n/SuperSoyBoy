using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationScript : MonoBehaviour
{
	public float rotationsPerMinute = 640f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		//This will rotate the GameObject the script is attached to around the Z-axis based on the value of rotationsPerMinute.
		transform.Rotate(0, 0, rotationsPerMinute * Time.deltaTime, Space.Self);
	}
}
