using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
	public string playerName;

	//static field that will hold the instance reference for the GameManager singleton
	public static GameManager instance;

	/* The public method is simply a proxy method — a way of calling the private coroutine
	method that actually reloads the Game scene. This method is made public so it is easy to
	call from the singleton GameManager instance anywhere else in the game code. */
	public void RestartLevel(float delay)
	{
		StartCoroutine(RestartLevelDelay(delay));
	}
	private IEnumerator RestartLevelDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene("Game");
	}

	/*This is the core of the singleton pattern. The first time this script executes Awake(),
	instance will be null, and so the method will assign the one instance of GameManager
	to instance. Any other time the method runs, instance will no longer be null.
	If another GameManager instance is created elsewhere, then that instance is destroyed,
	ensuring that there is only ever one instance (the first one that was initialized). */
	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
