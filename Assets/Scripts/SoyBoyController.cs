using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This line validates that any GameObject attached to this script has the minimum necessary components required to work — it basically pre-qualifies the components. 
//This is a handy script-writing trick that ensures the script only applies to GameObjects that meet the requirements.
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D),typeof(Animator))]

public class SoyBoyController : MonoBehaviour 
{
	// pre-defined values to use when calculating how much force to apply to Super Soy Boy’s Rigidbody.
	public float speed = 14f;
	public float accel = 6f;
	// The Vector2 input field stores the controller’s current input values for x and y at any point in time.
	private Vector2 input;
	// The last three private fields cache references to the SpriteRenderer, Rigidbody2D and Animator components.
	private SpriteRenderer sr;
	private Rigidbody2D rb;
	private Animator animator;

	// In short, this ensures component references are cached when the game starts.
	void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{
		// 1 Assign the value of accel — the public float field — to a private variable name acceleration.
		var acceleration = accel;
		var xVelocity = 0f;
		// 2 If horizontal axis controls are neutral, then xVelocity is set to 0, otherwise  
		// xVelocity is set to the current x velocity of the rb (aka Rigidbody2D) component.
		if (input.x == 0)
		{
			xVelocity = 0f;
		}
		else
		{
			xVelocity = rb.velocity.x;
		}
		// 3 Force is added to rb by calculating the current value of the horizontal axis controls
		// multiplied by speed, which is in turn multiplied by acceleration.
		// 0 is used for the Y component, as jumping is not yet being taken into account.
		rb.AddForce(new Vector2(((input.x * speed) - rb.velocity.x)
		* acceleration, 0));
		// 4 Velocity is reset on rb so it can stop Super Soy Boy from moving left or right when controls are in a neutral state.
		// Otherwise, velocity is set to exactly what it’s currently at.
		rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

		// Use this for initialization
		void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		// 1 . Input.GetAxis() gets X and Y values from the built-in Unity control axes named Horizontal and Jump.
		input.x = Input.GetAxis("Horizontal");
		input.y = Input.GetAxis("Jump");
		// 2 If input.x is greater than 0, then the player is facing right, so the sprite gets flipped on the X-axis.
		//  Otherwise, the player must be facing left, so the sprite is set back to “not flipped”.
		if (input.x > 0f)
		{
			sr.flipX = false;
		}
		else if (input.x < 0f)
		{
			sr.flipX = true;
		}


	}
}
