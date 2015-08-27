using UnityEngine;
using System.Collections;
using TheArena.Music;

public class Exemple : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
		MusicManager.Play (MusicStyle.Introduction, MusicTransition.fade * 5.0f);

	}
	
	public void StopMusic ()
	{

		MusicManager.Stop (MusicTransition.fade * 3.0f);

	}
}
