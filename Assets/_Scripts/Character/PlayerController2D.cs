
using UnityEngine;

[RequireComponent(typeof(PlayerMotor2D))]
public class PlayerController2D : MonoBehaviour
{
	private PlayerMotor2D _motor;
	private Attack _attack;
	
	// Use this for initialization
	void Start ()
	{
		_motor = GetComponent<PlayerMotor2D> ();
		_attack = GetComponentInChildren<Attack> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Mouvements selon X
		if (Mathf.Abs (Input.GetAxis (TheArena.Ctes.Input.HORIZONTAL)) > TheArena.Ctes.Globals.INPUT_THRESHOLD) {
			_motor.normalizedXMovement = Input.GetAxis (TheArena.Ctes.Input.HORIZONTAL); //on va demander au moteur d'effectuer un mouvement
		} else {
			_motor.normalizedXMovement = 0;//n va demander au moteur de s'arreter
		}
		// Mouvements selon Y, idem
		if (Mathf.Abs (Input.GetAxis (TheArena.Ctes.Input.VERTICAL)) > TheArena.Ctes.Globals.INPUT_THRESHOLD) {
			_motor.normalizedYMovement = Input.GetAxis (TheArena.Ctes.Input.VERTICAL); //on va demander au moteur d'effectuer un mouvement
		} else {
			_motor.normalizedYMovement = 0;///n va demander au moteur de s'arreter
		}
			
		// Jump?
		if (Input.GetButtonDown (TheArena.Ctes.Input.JUMP)) {
			//_motor.Jump ();a demander au moteur d'effectuer un mouvement
		}
			
		if (Input.GetButtonDown (TheArena.Ctes.Input.HIT)) {
			// On lance une attaque
			_attack.hit ();
		}
			
		if (Input.GetButtonDown (TheArena.Ctes.Input.BLOCK)) {
			// On Utilise le bouclier (Classe idem qu'au dessus je pense)
		}

		// Make the player look toward the mouse
		Vector3 mousepos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		transform.LookAt (transform.position + Vector3.forward, new Vector3 (mousepos.x - transform.position.x, mousepos.y - transform.position.y, 0));
	}
}
