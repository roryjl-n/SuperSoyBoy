using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

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
	void Start () 
	{
		/* That line hooks up OnSceneLoaded() to a Unity SceneManager event named sceneLoaded
		that fires when a scene loads. OnSceneLoaded() checks the name of the scene. It calls
		DisplayPreviousTimes() when the result is the Game scene. */
		SceneManager.sceneLoaded += OnSceneLoaded;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public List<PlayerTimeEntry> LoadPreviousTimes()
	{
		/* 1 Here, you use a try...catch statement to attempt to load saved time entries for the
		player. You construct the path to the file using a combination of the player’s name
		(stored it in PlayerPrefs) and the Application.persistentDataPath. 
		The using statement opens a file stream to the file path to read in any existing time
		entries using a binary formatter object. You use the Deserialize() method to read
		— aka unpack — the data in the file by passing in the opened file stream. If the read
		is successful, it returns a list of player time entries.*/
		try
		{
			var scoresFile = Application.persistentDataPath +
			"/" + playerName + "_times.dat";
			using (var stream = File.Open(scoresFile, FileMode.Open))
			{
				var bin = new BinaryFormatter();
				var times = (List<PlayerTimeEntry>)bin.Deserialize(stream);
				return times;
			}
		}
		/* 2 If deserialization is unsuccessful, the catch statement finds the errors. In here, you
		log an info message and return an empty list back to the caller. This is important
		because the player might not have logged or saved times, e.g., when playing the first
		time. */
		catch (IOException ex)
		{
			Debug.LogWarning("Couldn’t load previous times for: " +
			playerName + ". Exception: " + ex.Message);
			return new List<PlayerTimeEntry>();
		}
	}

	public void SaveTime(decimal time)
	{
		/* 3 When saving a time, you fetch existing times first with the LoadPreviousTimes() method. */
		var times = LoadPreviousTimes();
		/* 4 You create an instance of the new PlayerTimeEntry object. Then you store the
		current date and runtime, as passed to the method, in the entryDate and time
		property fields. */
		var newTime = new PlayerTimeEntry();
		newTime.entryDate = DateTime.Now;
		newTime.time = time;
		/* 5 You create a binary formatter object to do the magic serialization — a.k.a. packing –
		of the list of player time entries to a file. You create the file path using a
		combination of the player’s name and the Application.persistentDataPath. Again,
		the using statement opens the file.
		However, this time it uses the FileMode.Create option, which lets you create a new
		file or overwrite existing files in the same path. Finally, you add the new time entry
		to the list of pre-existing times before saving them all back into the file. */
		var bFormatter = new BinaryFormatter();
		var filePath = Application.persistentDataPath +
		"/" + playerName + "_times.dat";
		using (var file = File.Open(filePath, FileMode.Create))
		{
			times.Add(newTime);
			bFormatter.Serialize(file, times);
		}
	}

	public void DisplayPreviousTimes()
	{
		/* 1 Collects existing times using the LoadPreviousTimes() method.
		times.OrderBy(time => time.time).Take(3) is a LINQ query that sorts times from
		Sfastest to slowest, and then takes the first three of each. */
		var times = LoadPreviousTimes();
		var topThree = times.OrderBy(time => time.time).Take(3);
		// 2 Finds the PreviousTimes text component.
		var timesLabel = GameObject.Find("PreviousTimes")
		.GetComponent<Text>();
		// 3 Changes it to show each time found, separating entries with a line break using the "\n" string.
		timesLabel.text = "BEST TIMES \n";
		foreach (var time in topThree)
		{
			timesLabel.text += time.entryDate.ToShortDateString() +
			": " + time.time + "\n";
		}
	}

	// The code checks to see if the scene is the Game scene, and if so, displays the previous times.
	private void OnSceneLoaded(Scene scene, LoadSceneMode loadsceneMode)
	{
		if (scene.name == "Game")
		{
			DisplayPreviousTimes();
		}
	}

}
