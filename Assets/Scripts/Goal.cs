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

			/* 1 FindObjectOfType finds the timer script component instance in the level scene. The
			generic type Timer is indicated here. This Unity method finds the first instance of a
			script of type Timer. As there is only one Timer script per level, which is used to
			track level time, this instance is returned. */
			var timer = FindObjectOfType<Timer>();
			// 2 The new SaveTime method you created is called on the GameManager singleton, passing in the current level runtime from the Timer script.
			GameManager.instance.SaveTime(timer.time);
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
