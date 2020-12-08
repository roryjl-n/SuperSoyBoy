using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour 
{
	/*  This code will grab and cache a reference to the Text component on the same
	GameObject the script exists on. In the Update() loop, the text will be changed to
	display the time since the level last reloaded, rounded to two decimal places.*/

    private Text timerText;

	void Awake()
	{
		timerText = GetComponent<Text>();
	}

	void Update()
	{
		timerText.text =
		System.Math.Round((decimal)Time.timeSinceLevelLoad,
		2).ToString();
	}
}
