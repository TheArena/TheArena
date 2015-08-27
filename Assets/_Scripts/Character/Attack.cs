using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour
{

	public float cooldownAttack = 0.3f;
	public float durationAttack = 0.5f;
	public float baseDamage = 20;
	public float pushDistance = 2;
	public LayerMask triggerMaskAttack;

	private float currentCooldown;
	private float currentDuration;
	private bool hitLastTurn = false; // Does it hit this turn ?

	// Use this for initialization
	void Start ()
	{
		currentCooldown = 0;
		currentDuration = 0;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (hitLastTurn) { // to avoid hitting several times with one hit
			currentDuration = 0;
			hitLastTurn = false;
		}
		currentCooldown -= Time.deltaTime;
		currentDuration -= Time.deltaTime;
	}

	public void hit ()
	{
		if (currentCooldown < 0) {
			currentDuration = durationAttack;
			currentCooldown = cooldownAttack;
		}
	}

	public void OnTriggerStay2D (Collider2D other)
	{
		if (currentDuration > 0) {
			if (((1 << other.gameObject.layer) & triggerMaskAttack) != 0) { // if the thing we collided with is an enemy
				//coll.collider.Enemy.getHit (baseDamage, pushDistance);
				hitLastTurn = true;
			}
		}
	}
}
