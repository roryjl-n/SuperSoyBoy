using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour 
{
	/*  This code will grab and cache a reference to the Text component on the same
	GameObject the script exists on. In the Update() loop, the text will be changed to
	display the time since the level last reloaded, rounded to two decimal places.*/

	public decimal time;

	private Text timerText;

	void Awake()
	{
		timerText = GetComponent<Text>();
	}

	// By storing the game time in the new time variable before updating the timerText value, you’re giving other scripts the ability to access the time value.
	void Update()
	{
		time = System.Math.Round((decimal)Time.timeSinceLevelLoad, 2);
		timerText.text = time.ToString();

	}
}
