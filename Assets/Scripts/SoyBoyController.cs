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

	public bool isJumping;
	public float jumpSpeed = 8f;
	private float rayCastLengthCheck = 0.005f;
	private float width;
	private float height;

	public float jumpDurationThreshold = 0.25f;
	private float jumpDuration;

	// In short, this ensures component references are cached when the game starts.
	void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();

		/* Here, we grab the width and height of SoyBoy’s sprite. In addition to these dimensions, 
		we also add 0.1 and 0.2 to the width and height of the sprite respectively
		This just gives a little bit of a buffer, to allow the raycast checks later on to start outside of the
		sprite, rather than inside the sprite bounds. */
		width = GetComponent<Collider2D>().bounds.extents.x + 0.1f;
		height = GetComponent<Collider2D>().bounds.extents.y + 0.2f;
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

		// This gives Super Soy Boy’s Rigidbody a new velocity if the user has pressed the jump button for less than the jumpDurationThreshold. 
		//  The velocity given in the X direction is the same as his current sideways movement speed but his velocity given in the Y
		// direction is set to jumpSpeed (which is 8). This equates to upward movement, forming a satisfying input-duration controlled jump!
		if (isJumping && jumpDuration < jumpDurationThreshold)
		{
			rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
		}
	}

		// Use this for initialization
	void Start () 
	{
		
	}

	public bool PlayerIsOnGround()
	{
		/* 1 The first ground check performs a Raycast directly below the center of the 
		character (Transform.position.x), using a length equal to the value of 
		rayCastLengthCheck which is defaulted to 0.005f — a very short raycast is therefore 
		sent down from the bottom of the SoyBoy sprite.
		The other two ground checks do exactly the same thing, but slightly to the left and right of center.
		These three raycast checks allow for accurate ground detection. */
		bool groundCheck1 = Physics2D.Raycast(new Vector2(
		transform.position.x, transform.position.y - height),
		-Vector2.up, rayCastLengthCheck);
		bool groundCheck2 = Physics2D.Raycast(new Vector2(
		transform.position.x + (width - 0.2f),
		transform.position.y - height), -Vector2.up,
		rayCastLengthCheck);
		bool groundCheck3 = Physics2D.Raycast(new Vector2(
		transform.position.x - (width - 0.2f),
		transform.position.y - height), -Vector2.up,
		rayCastLengthCheck);
		// 2 If any of the ground checks come back as true, then this method returns true to the caller. Otherwise, it will return false.
		if (groundCheck1 || groundCheck2 || groundCheck3)
		{
			return true;
		}
		else
		{
			return false;
		}
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

		// As long as the jump button is held down, jumpDuration is counted up using the time the 
		// previous frame took to complete (Time.deltaTime).  If jump is released, jumpDuration is 
		// set back to 0 for the next time it needs to be timed, and the isJumping boolean flag is set back to false.
		if (input.y >= 1f)
		{
			jumpDuration += Time.deltaTime;
		}
		else
		{
			isJumping = false;
			jumpDuration = 0f;
		}

		// This code calls the PlayerIsOnGround() method to determine if the player is touching the ground or not (true or false). 
		// It also checks if the player is not already jumping.
		if (PlayerIsOnGround() && isJumping == false)
		{
			if (input.y > 0f)
			{
				isJumping = true;
			}
		}

		// This checks for jumpDuration being longer than the jumpDurationThreshold (0.25 seconds).
		//  If the jump button is held in longer than 0.25 seconds, then the jump is effectively cancelled, meaning the player can only jump up to a certain height.
		if (jumpDuration > jumpDurationThreshold) input.y = 0f;
	}
}
