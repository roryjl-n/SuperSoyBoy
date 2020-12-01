using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour 
{
	public AudioClip goalClip;

	// Here, we check for player collisions with the Goal GameObject. If this happens, the
	// player has reached the goal so you play the goalClip audio clip and restart the level after a 0.5 sec delay.
	void OnCollisionEnter2D(Collision2D coll)
    {
		if (coll.gameObject.tag == "Player")
		{
			var audioSource = GetComponent<AudioSource>();
			if (audioSource != null && goalClip != null)
			{
				audioSource.PlayOneShot(goalClip);
			}
			GameManager.instance.RestartLevel(0.5f);
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
