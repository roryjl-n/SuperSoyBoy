using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
	private InputField input;

	void Start()
	{
		/* 1 Locating and caching the InputField component on the GameObject to which the
		script is attached. You then add a new listener method to the onValueChanged event
		that all InputFields have by default.Whenever the input value changes, the
		SavePlayerName() method executes. */
		input = GetComponent<InputField>();
		input.onValueChanged.AddListener(SavePlayerName);
		/* 2 You then use PlayerPrefs to look for and retrieve the value for a key named
		PlayerName. If it exists, then the input field’s text is changed to display the player
		name stored in PlayerPrefs. */
		var savedName = PlayerPrefs.GetString("PlayerName");
		if (!string.IsNullOrEmpty(savedName))
		{
			input.text = savedName;
			GameManager.instance.playerName = savedName;
		}
	}
	private void SavePlayerName(string playerName)
	{
		/* 3 SavePlayerName() takes the supplied playerName and sets the key named
		PlayerName to this value. PlayerPrefs.Save() is then called to persist the state of
		PlayerPrefs to storage. */
		PlayerPrefs.SetString("PlayerName", playerName);
		PlayerPrefs.Save();
		GameManager.instance.playerName = playerName;
	}
}
