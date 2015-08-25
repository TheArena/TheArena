
using UnityEngine;

[RequireComponent(typeof(PlayerMotor2D))]
public class PlayerController2D : MonoBehaviour
{
	private PlayerMotor2D _motor;
	
	// Use this for initialization
	void Start ()
	{
		_motor = GetComponent<PlayerMotor2D> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Mouvements selon X
		if (Mathf.Abs (Input.GetAxis (TheArena.Input.HORIZONTAL)) > TheArena.Globals.INPUT_THRESHOLD) {
			_motor.normalizedXMovement = Input.GetAxis (TheArena.Input.HORIZONTAL); //on va demander au moteur d'effectuer un mouvement
		} else {
			_motor.normalizedXMovement = 0;//n va demander au moteur de s'arreter
		}
		// Mouvements selon Y, idem
		if (Mathf.Abs (Input.GetAxis (TheArena.Input.VERTICAL)) > TheArena.Globals.INPUT_THRESHOLD) {
			_motor.normalizedYMovement = Input.GetAxis (TheArena.Input.VERTICAL); //on va demander au moteur d'effectuer un mouvement
		} else {
			_motor.normalizedYMovement = 0;///n va demander au moteur de s'arreter
		}
			
		// Jump?
		if (Input.GetButtonDown (TheArena.Input.JUMP)) {
			//_motor.Jump ();a demander au moteur d'effectuer un mouvement
		}
			
		if (Input.GetButtonDown (TheArena.Input.HIT)) {
			// On lance une attaque (Il faut un script pour ça à part je pense)
		}
			
		if (Input.GetButtonDown (TheArena.Input.BLOCK)) {
			// On Utilise le bouclier (Classe idem qu'au dessus je pense)
		}

	}
}
