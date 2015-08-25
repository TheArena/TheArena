using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace TheArena
{

/// <summary>
///  Cette classe permet de gérer la musique durant le jeu.
///  Elle réalise automatiquement les transitions entre les différents thèmes en synchronisant les musiques sur la mesure.
/// </summary>

[DisallowMultipleComponent]
[AddComponentMenu ("Audio/Music Manager")]
public class MusicManager : MonoBehaviour {

	// PROPRIETES STATICS

	private static MusicManager _manager;
	private static MusicManager _instance {
		get
		{
			if (!_manager) {
				_manager = FindObjectOfType (typeof(MusicManager)) as MusicManager;
				
				if (!_manager) {
					Debug.LogError ("Il faut un MusicManager sur la scene");
				} else {
					_manager._Init (); 
				}
			}
			
			return _manager;
		}
	}

	// ATTRIBUTS

	/// <summary>
	/// Output est le groupe vers lequel le son est redirigé <see cref="UnityEngine.Audio.AudioMixerGroup"/>
	/// </summary>
	/// 
	public AudioMixerGroup output;

	/// <summary>
	/// BPM est le nombre de battement par minute.
	/// Il doit être le même sur l'ensemble des musiques utilisés de façon à garantir des transitions propres.
	/// </summary>

	[Tooltip ("Beat Per Minutes / Tempo")]
	[Range (60, 240)]

	public int bpm = 120;

	[Header ("Liste des AudioClip")]

	public AudioClip[] clips_intro;
	public AudioClip[] clips_begin,
	 					clips_lowlife,
	 					clips_countdown,
						clips_victory,
						clips_defeat;

	[HideInInspector]

	/// <summary>
	/// Indique si la musique est actuellement en cours de transition.
	/// Impossible de changer la musique pendant cette transition pour éviter les sautes. (Read Only)
	/// </summary>
	/// <value><c>true</c> si c'est en cours de transition; sinon, <c>false</c>.</value>

	public bool isFading {
		get { return isFading; }
		private set { isFading = value; }
	}

	// PROPRIETES

	private AudioSource _source1, _source2;
	private bool _waitingForFading;

	// METHODES STATICS

	/// <summary>
	/// Permet de jouer un style de musique sur la prochaine mesure
	/// </summary>
	/// <param name="style">Le style de la musique.</param>

	public static void PlayOnMeasure (MusicStyle style)
	{
		_instance.ManagerPlayOnMeasure (style);
	}

	// METHODES PUBLICS

	public void ManagerPlayOnMeasure (MusicStyle style)
	{
		AudioClip nextclip = _GetClip (style);

		if (nextclip == null) {
			Debug.LogError ("Aucun clip à jouer. Ajoutez des clips à la liste (" + style + ")");
			return;
		}

		if (_GiveToAudioSource (nextclip)) {
			_waitingForFading = true;
		} else {
			Debug.LogError ("Impossible de changer de musique pour le moment");
			return;
		}

	}

	// METHODES PRIVEES

	private void _Init ()
	{

		// Mixer
		if (output == null)
			Debug.LogError ("Il manque une sortie AudioMixerGroup");

		// AudioSource
		_source1 = gameObject.AddComponent <AudioSource>() as AudioSource;
		_source2 = gameObject.AddComponent <AudioSource>() as AudioSource;
		_InitAudioSource (_source1);
		_InitAudioSource (_source2);
	}


	/// <summary>
	/// Initialise un AudioSource
	/// </summary>
	/// <param name="source">L'AudioSource a initialisé</param>

	private void _InitAudioSource (AudioSource source)
	{

		source.playOnAwake = false;
		source.priority = 20;
		source.loop = true;
		source.mute = false;
		source.outputAudioMixerGroup = output;
		source.spatialBlend = 0.0f;
		source.volume = 1.0f;
		source.time = 0.0f;

	}

	/// <summary>
	/// Permet d'obtenir un AudioClip aléatoire du style correspondant.
	/// </summary>
	/// <returns>Le clip selectionné</returns>
	/// <param name="style">Le style de musique choisi</param>

	private AudioClip _GetClip (MusicStyle style)
	{

		AudioClip [] clips = clips_intro;

		switch (style) {
		case MusicStyle.Beginning:
			clips = clips_begin;
			break;
		case MusicStyle.Countdown:
			clips = clips_countdown;
			break;
		case MusicStyle.Defeat:
			clips = clips_defeat;
			break;
		case MusicStyle.Lowlife:
			clips = clips_lowlife;
			break;
		case MusicStyle.Victory:
			clips = clips_victory;
			break;
		default :
			clips = clips_intro;
			break;
		}

		return MusicTools.RandomClip (clips);
			
	}


	/// <summary>
	/// Donne le AudioClip à l'un des deux AudioSource (Celui qui n'est pas en lecture)
	/// </summary>
	/// <returns><c>true</c>, si un AudioSource était disponible pour récupérer l'AudioClip,
	/// <c>false</c> si aucun AudioSource n'a pu récupérer l'AudioClip.</returns>
	/// <param name="clip">Le clip a donné</param>

	private bool _GiveToAudioSource (AudioClip clip)
	{

		if (isFading)
			return false;

		AudioSource source;

		if (!_source1.isPlaying)
			source = _source1;
		else if (!_source2.isPlaying)
			source = _source2;
		else
			return false;

		source.clip = clip;

		return true;

	}

}

}
