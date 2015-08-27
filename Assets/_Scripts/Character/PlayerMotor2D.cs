using UnityEngine;
using System.Collections;

// Cette classe va effectuer réellement les déplacement phisiques de l'objet auquel elle est raccrochée
// Dans l'idéal on évite d'utiliser la physique pour le Player, c'est souvent désagréable à jouer? Il faut donc refaire le moteur de déplacement à la main
public class PlayerMotor2D : MonoBehaviour
{

	#region variablesPublic
	public float groundSpeed = 8f; // self explained
	public float timeToGroundSpeed = 0.1f; //Afin d'avoir une sensation d'accélération
	public float groundStopDistance = 0.333f; //Afin d'avoir une sensation de décélération


	public float normalizedXMovement { get; set; } // Set the x movement direction. This is multiplied by the max speed. -1 is full left, 
	//1 is full right. Higher numbers will result in faster speed.

	public float normalizedYMovement { get; set; } // Set the y movement direction. This is multiplied by the max speed. -1 is going down, 
	//1 is going UP. Higher numbers will result in faster speed.


	public enum MotorState
	{
		OnGround,
		Jumping
	}
	public MotorState motorState { get; private set; }

	public enum Facing
	{
		up,
		right,
		down,
		left
	}
	public Facing motorFacing { get; set; }
	#endregion variablesPublic	

	private Rigidbody2D _rigidbody2D;

	#region Fonctionss

	private void Awake ()
	{
		_rigidbody2D = GetComponent<Rigidbody2D> ();

	}

	private void Start ()
	{
		motorState = MotorState.OnGround;
	}

	public void Jump ()
	{
	}


	private void FixedUpdate ()
	{
		SetFacing ();	// Set the state of the direction of the player to the good value
		ApplyMovement ();
	}


	private void SetFacing ()
	{
		if (Mathf.Abs (normalizedXMovement) < Mathf.Abs (normalizedYMovement)) {
			if (normalizedYMovement < 0) {
				motorFacing = Facing.down;
			} else if (normalizedYMovement > 0) {
				motorFacing = Facing.up;
			}
		} else if (Mathf.Abs (normalizedXMovement) > Mathf.Abs (normalizedYMovement)) {
			if (normalizedXMovement < 0) {
				motorFacing = Facing.left;
			} else if (normalizedXMovement > 0) {
				motorFacing = Facing.right;
			}
		}
	}

	private void ApplyMovement ()
	{
		float speedX = _rigidbody2D.velocity.x;
		float speedY = _rigidbody2D.velocity.y;
		float maxSpeed = groundSpeed;
		
		// PERF: Optimal math out the window in favor of ease of figuring out, can resolve later if a problem.
		if (Mathf.Abs (normalizedXMovement) > 0) {
			if (timeToGroundSpeed > 0) {
				// If we're moving faster than our normalizedXMovement * groundSpeed then decelerate rather than 
				// accelerate.
				//
				// Or if we are trying to move in the direction opposite of where we are facing.
					
				if (speedX > 0 && normalizedXMovement > 0 && speedX > normalizedXMovement * maxSpeed ||
					speedX < 0 && normalizedXMovement < 0 && speedX < normalizedXMovement * maxSpeed ||
					speedX < 0 && normalizedXMovement > 0 || speedX > 0 && normalizedXMovement < 0) {

					float deceleration = (maxSpeed * maxSpeed) / (2 * groundStopDistance);
					speedX = Accelerate (speedX, deceleration, normalizedXMovement * maxSpeed);
				} else {
					float acceleration = normalizedXMovement * (maxSpeed / timeToGroundSpeed);		
					speedX = Accelerate (speedX, acceleration, normalizedXMovement * maxSpeed);
				}
			} else {
				speedX = normalizedXMovement * maxSpeed;
			}
		}

		// This is the deceleration part
		else if (_rigidbody2D.velocity.x != 0) {

			if (groundStopDistance > 0) {
				float deceleration = (groundSpeed * groundSpeed) / (2 * groundStopDistance);
						
				speedX = Accelerate (speedX, deceleration, 0);
			} else {
				speedX = 0;
			}	
		}

		//idem for y
		if (Mathf.Abs (normalizedYMovement) > 0) {
			if (timeToGroundSpeed > 0) {
				// If we're moving faster than our normalizedXMovement * groundSpeed then decelerate rather than 
				// accelerate.
				//
				// Or if we are trying to move in the direction opposite of where we are facing.
				
				if (speedY > 0 && normalizedYMovement > 0 && speedY > normalizedYMovement * maxSpeed ||
					speedY < 0 && normalizedYMovement < 0 && speedY < normalizedYMovement * maxSpeed ||
					speedY < 0 && normalizedYMovement > 0 || speedY > 0 && normalizedYMovement < 0) {
					
					float deceleration = (maxSpeed * maxSpeed) / (2 * groundStopDistance);
					speedY = Accelerate (speedY, deceleration, normalizedYMovement * maxSpeed);
				} else {
					float acceleration = normalizedYMovement * (maxSpeed / timeToGroundSpeed);		
					speedY = Accelerate (speedY, acceleration, normalizedYMovement * maxSpeed);
				}
			} else {
				speedY = normalizedYMovement * maxSpeed;
			}
		} else if (_rigidbody2D.velocity.y != 0) {
			
			if (groundStopDistance > 0) {
				float deceleration = (groundSpeed * groundSpeed) / (2 * groundStopDistance);
				
				speedY = Accelerate (speedY, deceleration, 0);
			} else {
				speedY = 0;
			}	
		}
		
		_rigidbody2D.velocity = new Vector2 (speedX, speedY);

	}

	private float Accelerate (float speed, float acceleration, float limit)
	{
		// acceleration can be negative or positive to note acceleration in that direction.
		speed += acceleration * Time.fixedDeltaTime;
		
		if (acceleration > 0) {
			if (speed > limit) {
				speed = limit;
			}
		} else {
			if (speed < limit) {
				speed = limit;
			}
		}
		
		return speed;
	}

	#endregion

}
