using UnityEngine;
using System.Collections;
using TheArena;

public class Exemple : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
		MusicManager.Play (MusicStyle.Introduction, MusicTransition.fadeIn * 5.0f);

	}
	
	public void ChangeMusic ()
	{

		MusicManager.Play (MusicStyle.Beginning, MusicTransition.outIn * 10.0f);

	}
}
