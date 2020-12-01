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

	public float airAccel = 3f;

	public float jump = 14f;

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
		// 1 This code sets acceleration to 0 to start with (a default, initial value). It then looks to
		// see if the player is on the ground or not, setting acceleration to accel or airAccel respectively.
		var acceleration = 0f;
		if (PlayerIsOnGround())
		{
			acceleration = accel;
		}
		else
		{
			acceleration = airAccel;
		}
		var xVelocity = 0f;
		// 2 If horizontal axis controls are neutral, then xVelocity is set to 0, otherwise  
		// xVelocity is set to the current x velocity of the rb (aka Rigidbody2D) component.
		if (PlayerIsOnGround() && input.x == 0)
		{
			xVelocity = 0f;
		}
		else
		{
			xVelocity = rb.velocity.x;
		}
		// This ensures that the yVelocity value is set to the jump value of 14 when the character
		// is jumping from the ground, or from a wall. Otherwise, it’s set to the current velocity of the rigidbody.
		var yVelocity = 0f;
		if (PlayerIsTouchingGroundOrWall() && input.y == 1)
		{
			yVelocity = jump;
		}
		else
		{
			yVelocity = rb.velocity.y;
		}
		// 3 Force is added to rb by calculating the current value of the horizontal axis controls
		// multiplied by speed, which is in turn multiplied by acceleration.
		// 0 is used for the Y component, as jumping is not yet being taken into account.
		rb.AddForce(new Vector2(((input.x * speed) - rb.velocity.x)
		* acceleration, 0));
		// 4 The yVelocity is now used to modify the velocity of the character’s Rigidbody
		rb.velocity = new Vector2(xVelocity, yVelocity);
		/* This checks to see if there is a wall to the left or right of the player, that they are not on the ground, and that they are currently jumping.
		 If this is the case, the character’s Rigidbody velocity is set to a new velocity, using the current Y velocity, but with a change to the X (horizontal) velocity.
		 The change in horizontal velocity is equal to the opposite direction of the wall, multiplied by the speed factor, and then multiplied down by factor of 0.75 (to slightly
		 dampen the horizontal movement a bit). The opposite direction of the wall is calculated by calling the new GetWallDirection() 
		 method and negating its returned value (-1 becomes 1, and 1 becomes -1). */
		if (IsWallToLeftOrRight() && !PlayerIsOnGround() && input.y == 1)
		{
			rb.velocity = new Vector2(-GetWallDirection()
			* speed * 0.75f, rb.velocity.y);
		}
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

	public bool IsWallToLeftOrRight()
	{
		// 1 Again we’re using the implicit bool conversion check of the Physics2D.Raycast()
		// method to see if either of two raycasts sent out to the left (-Vector2.right) and to 
		// he right (Vector2.right) of the character sprite hit anything.
		bool wallOnleft = Physics2D.Raycast(new Vector2(
		transform.position.x - width, transform.position.y),
		-Vector2.right, rayCastLengthCheck);
		bool wallOnRight = Physics2D.Raycast(new Vector2(
		transform.position.x + width, transform.position.y),
		Vector2.right, rayCastLengthCheck);
		// 2 If either of these raycasts hit anything, the method returns true — otherwise, false.
		if (wallOnleft || wallOnRight)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	// This method will return true if the character is either on the ground, or has a wall to the left or right of them.
	// If they are against a wall, or on the ground, then you’ll know to apply the jump value to the character rigidbody’s Y velocity. 
	// Otherwise, you’ll just leave the Y velocity as it currently is.
	public bool PlayerIsTouchingGroundOrWall()
	{
		if (PlayerIsOnGround() || IsWallToLeftOrRight())
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	// This returns an integer based on whether the wall is left (-1), right (1), or neither (0). 
	// We’ll use the returned value to multiply against the character’s X velocity to either 
	// make him go left or right (when wall jumping) based on which side the wall is on.
	public int GetWallDirection()
	{
		bool isWallLeft = Physics2D.Raycast(new Vector2(
		transform.position.x - width, transform.position.y),
		-Vector2.right, rayCastLengthCheck);
		bool isWallRight = Physics2D.Raycast(new Vector2(
		transform.position.x + width, transform.position.y),
		Vector2.right, rayCastLengthCheck);
		if (isWallLeft)
		{
			return -1;
		}
		else if (isWallRight)
		{
			return 1;
		}
		else
		{
			return 0;
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
