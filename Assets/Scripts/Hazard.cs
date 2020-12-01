using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour 
{
	// These will hold some references to assets that will be used for when the player touches the saw blade and triggers the hazard action.
	public GameObject playerDeathPrefab;
	public AudioClip deathClip;
	public Sprite hitSprite;
	private SpriteRenderer spriteRenderer;

	// This caches a reference to the SpriteRenderer when the script starts.
	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		// 1 First we check to ensure the colliding GameObject has the "Player" tag.
		if (coll.transform.tag == "Player")
		{
			// 2 Next, determine if an AudioClip has been assigned to the script and that a valid Audio Source component exists. If so, play the deathClip audio effect.
			var audioSource = GetComponent<AudioSource>();
			if (audioSource != null && deathClip != null)
			{
				audioSource.PlayOneShot(deathClip);
			}
			// 3 Instantiate the playerDeathPrefab at the collision point and swap the sprite of the saw blade with the hitSprite version.
			Instantiate(playerDeathPrefab, coll.contacts[0].point,
			Quaternion.identity);
			spriteRenderer.sprite = hitSprite;
			// 4 Lastly, destroy the colliding object (the player).
			Destroy(coll.gameObject);
			//This will call the GameManager instance’s RestartLevel() method, passing in a delay of 1.25 seconds. 
			// This will run the coroutine and restart the level after a 1.25 second delay when called, giving just enough time for the player to see the death particle effect.
			GameManager.instance.RestartLevel(1.25f);
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
