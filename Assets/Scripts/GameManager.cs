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
	// buttonPrefab will hold a reference to the button template each level button will use.
	public GameObject buttonPrefab;
	// selectedLevel will temporarily hold the path to a level file based on which level button the player clicks.
	private string selectedLevel;

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
		// DiscoverLevels() is called when the Game Manager initializes to retrieve any level files from JSON, and populate the menu with buttons for each.
		DiscoverLevels();
	}

	// This checks for an Escape keypress and loads the Menu scene if detected.
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene("Menu");
		}
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
			var levelName = Path.GetFileName(selectedLevel);
			var scoresFile = Application.persistentDataPath +
			 "/" + playerName + "_" + levelName + "_times.dat";
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
		var levelName = Path.GetFileName(selectedLevel);
		var filePath = Application.persistentDataPath +
		 "/" + playerName + "_" + levelName + "_times.dat";
		using (var file = File.Open(filePath, FileMode.Create))
		{
			times.Add(newTime);
			bFormatter.Serialize(file, times);
		}
	}

	public void DisplayPreviousTimes()
	{
		var times = LoadPreviousTimes();
		var levelName = Path.GetFileName(selectedLevel);
		if (levelName != null)
		{
			levelName = levelName.Replace(".json", "");
		}
		var topThree = times.OrderBy(time => time.time).Take(3);
		var timesLabel = GameObject.Find("PreviousTimes")
		.GetComponent<Text>();
		timesLabel.text = levelName + "\n";
		timesLabel.text += "BEST TIMES \n";
		foreach (var time in topThree)
		{
			timesLabel.text += time.entryDate.ToShortDateString()
			+ ": " + time.time + "\n";
		}
	}

	/* This runs whenever a scene loads that also has the GameManager.cs singleton in it. By
	checking that the scene name is Game, as well as ensuring selectedLevel is set to
	something, you ensure that the next bit of code will only run when the Game scene
	loads and the selectedLevel value has been set. */
	private void OnSceneLoaded(Scene scene, LoadSceneMode loadsceneMode)
	{
		if (!string.IsNullOrEmpty(selectedLevel)
		&& scene.name == "Game")
		{
			Debug.Log("Loading level content for: " + selectedLevel);
			LoadLevelContent();
			DisplayPreviousTimes();
		}
		if (scene.name == "Menu")
		{
			DiscoverLevels();
		}
	}

	// This sets selectedLevel to the path string passed into the method and then loads the Game Scene.
	private void SetLevelName(string levelFilePath)
	{
		selectedLevel = levelFilePath;
		SceneManager.LoadScene("Game");
	}

	/* this method will seek out and discover JSON level files! First, it locates
	the UI panel GameObject in the Menu scene and grabs a reference to the RectTransform
	component. It then searches for JSON files — by looking for JSON extensions — in the
	game’s Application.dataPath. */
	private void DiscoverLevels()
	{
		var levelPanelRectTransform =
		GameObject.Find("LevelItemsPanel")
		.GetComponent<RectTransform>();
		var levelFiles = Directory.GetFiles(Application.dataPath,
		"*.json");
		/* Here you’re looping through the
		levelFiles array. Each iteration calculates the yOffset, starting at -30f and reducing
		by 65f decrements. This calculation determines the vertical position of each button on
		the panel. */
		var yOffset = 0f;
		for (var i = 0; i < levelFiles.Length; i++)
		{
			if (i == 0)
			{
				yOffset = -30f;
			}
			else
			{
				yOffset -= 65f;
			}
			// levelFile stores the current file from the levelFiles array. Then it determines the file name via Path.GetFileName().
			var levelFile = levelFiles[i];
			var levelName = Path.GetFileName(levelFile);
			// 1 Instantiates a copy of the button prefab.
			var levelButtonObj = (GameObject)Instantiate(buttonPrefab,
			 Vector2.zero, Quaternion.identity);
			// 2 Gets its Transform and makes it a child of LevelItemsPanel
			var levelButtonRectTransform = levelButtonObj
			 .GetComponent<RectTransform>();
			levelButtonRectTransform.SetParent(levelPanelRectTransform,
			 true);
			// 3 Positions it based on a fixed X-position and a variable Y-position, aka yOffset.
			levelButtonRectTransform.anchoredPosition =
			 new Vector2(212.5f, yOffset);
			/* 4 Sets the button text to the level’s name. The GetChild() method finds the Text
			component. It also passes in an index that refers to the index number of the child
			Transform component. This is a 0 based index, so the first child under the button
			will be 0. */
			var levelButtonText = levelButtonObj.transform.GetChild(0)
			 .GetComponent<Text>();
			levelButtonText.text = levelName;

			var levelButton = levelButtonObj.GetComponent<Button>();
			levelButton.onClick.AddListener(
			 delegate { SetLevelName(levelFile); });
			levelPanelRectTransform.sizeDelta =
			 new Vector2(levelPanelRectTransform.sizeDelta.x, 60f * i);
		}
		levelPanelRectTransform.offsetMax =
			new Vector2(levelPanelRectTransform.offsetMax.x, 0f);
	}

	// This code runs when the Game scene loads. It finds the existing Level GameObject, destroys it and then creates a new, “clean” Level GameObject
	private void LoadLevelContent()
	{
		var existingLevelRoot = GameObject.Find("Level");
		Destroy(existingLevelRoot);
		var levelRoot = new GameObject("Level");

		/* 1 Reads the JSON file content of the selected level — selectedLevel is the path where
		the level resides after the player clicks the corresponding button. JsonUtility is a
		class that deserializes the JSON content to an instance of LevelDataRepresentation.
		It also sets all the nested field values, so this object contains all the level hierarchy
		deserialized in this one object. */
		var levelFileJsonContent = File.ReadAllText(selectedLevel);
		var levelData = JsonUtility.FromJson<LevelDataRepresentation>(
		 levelFileJsonContent);
		// 2 Makes levelData.levelItems into a fully populated array of LevelItemRepresentation instances.
		foreach (var li in levelData.levelItems)
		{
			// 3 For every item that is looped through in the array, the script locates correct prefab and loads it from the Resources/Prefabs folder based on the prefabName value.
			var pieceResource =
			Resources.Load("Prefabs/" + li.prefabName);
			if (pieceResource == null)
			{
				Debug.LogError("Cannot find resource: " + li.prefabName);
			}
			// 4 Instantiates a clone of this prefab. When the prefab has a sprite, it’s then configured based on sprite data from the JSON file for that item.
			var piece = (GameObject)Instantiate(pieceResource,
			li.position, Quaternion.identity);
			var pieceSprite = piece.GetComponent<SpriteRenderer>();
			if (pieceSprite != null)
			{
				pieceSprite.sortingOrder = li.spriteOrder;
				pieceSprite.sortingLayerName = li.spriteLayer;
				pieceSprite.color = li.spriteColor;
			}
			// 5 Makes the object a child of the Level GameObject then sets and its position, rotation and scale based on the data for that item in the level file.
			piece.transform.parent = levelRoot.transform;
			piece.transform.position = li.position;
			piece.transform.rotation = Quaternion.Euler(
			li.rotation.x, li.rotation.y, li.rotation.z);
			piece.transform.localScale = li.scale;
		}
		/* This code locates SoyBoy and places him at the playerStartPosition location saved in
		the JSON file. Next, it positions the camera in line with SoyBoy, so the level loads with
		the view squarely on that little chunk of tofu.*/
		var SoyBoy = GameObject.Find("SoyBoy");
		SoyBoy.transform.position = levelData.playerStartPosition;
		Camera.main.transform.position = new Vector3(
		 SoyBoy.transform.position.x, SoyBoy.transform.position.y,
		 Camera.main.transform.position.z);
		// 1 Locates the smooth follow script CameraLerpToTransform with the FindObjectOfType() method.
		var camSettings = FindObjectOfType<CameraLerpToTransform>();
		// 2 Checks that the Smooth Follow script was found, and if so, it populates settings for speed, bounds and tracking target with the values found in the JSON file.
		if (camSettings != null)
		{
			camSettings.cameraZDepth =
			levelData.cameraSettings.cameraZDepth;
			camSettings.camTarget = GameObject.Find(
			levelData.cameraSettings.cameraTrackTarget).transform;
			camSettings.maxX = levelData.cameraSettings.maxX;
			camSettings.maxY = levelData.cameraSettings.maxY;
			camSettings.minX = levelData.cameraSettings.minX;
			camSettings.minY = levelData.cameraSettings.minY;
			camSettings.trackingSpeed =
			levelData.cameraSettings.trackingSpeed;
		}
	}
}
