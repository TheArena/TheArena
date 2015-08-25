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

		//private bool _isFading;
		public bool isFading {
			get
			{
				if (_transition.mode == MusicTransitionMode.None)
					return false;
				else
					return true;
			}
			//private set { _isFading = value; }
		}

		// PROPRIETES

		private MusicTransition _transition;
		private AudioSource _sourcePlaying, _sourceWaiting;

		// METHODES STATICS

		/// <summary>
		/// Permet de jouer un style de musique sur la prochaine mesure
		/// </summary>
		/// <param name="style">Le style de la musique.</param>

		public static void Play (MusicStyle style, MusicTransition transition)
		{
			_instance.ManagerPlayOnMeasure (style, transition);
		}

		// METHODES PUBLICS

		public void ManagerPlayOnMeasure (MusicStyle style, MusicTransition transition)
		{
			AudioClip nextclip = _GetClip (style);

			if (nextclip == null) {
				Debug.LogError ("Aucun clip à jouer. Ajoutez des clips à la liste (" + style + ")");
				return;
			}

			// Si le Clip a bien été chargé dans un AudioSource
			if (_GiveToAudioSource (nextclip))
			{
				_transition = _CheckTransition (transition);
			}
			else
			{
				Debug.LogError ("Impossible de changer de musique pour le moment");
				return;
			}

		}

		// CALLS DU BEHAVIOUR
		
		private void Update ()
		{

			_UpdateTransition ();

		}

		// METHODES PRIVEES

		private void _Init ()
		{

			//isFading = false;

			_transition = MusicTransition.none;
			//_waitingForFading = false;

			// Mixer
			if (output == null)
				Debug.LogError ("Il manque une sortie AudioMixerGroup");

			// AudioSource
			_sourcePlaying = gameObject.AddComponent <AudioSource>() as AudioSource;
			_sourceWaiting = gameObject.AddComponent <AudioSource>() as AudioSource;
			_InitAudioSource (_sourcePlaying);
			_InitAudioSource (_sourceWaiting);
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

			_sourceWaiting.clip = clip;

			return true;

		}

		/// <summary>
		/// La méthode s'assure que le choix de la transition n'est pas mauvaise.
		/// </summary>
		/// <returns>La transition correcte</returns>
		/// <param name="check">La transition a testé</param>

		private MusicTransition _CheckTransition (MusicTransition check)
		{

			MusicTransitionMode mode = MusicTransitionMode.Cut;

			if (!_sourcePlaying.isPlaying) {

				// Si c'est la première lecture, ces transitions sont interdites et remplacés par FadeIn ou Cut
				switch (check.mode) {
				case MusicTransitionMode.Fade:
				case MusicTransitionMode.FadeIn:
				case MusicTransitionMode.FadeOut:
				case MusicTransitionMode.FadeOutFadeIn:
				case MusicTransitionMode.OnMeasure:
				case MusicTransitionMode.OnBeat:
					mode = MusicTransitionMode.FadeIn;
					break;
				default :
					mode = MusicTransitionMode.Cut;
					break;
				}

			} else if (_sourceWaiting.clip == null) {

				// Si il n'y a pas de prochain clip, ces transitions sont interdites et remplacés par FadeOut ou Cut
				switch (check.mode) {
				case MusicTransitionMode.Fade:
				case MusicTransitionMode.FadeIn:
				case MusicTransitionMode.FadeOutFadeIn:
				case MusicTransitionMode.OnMeasure:
				case MusicTransitionMode.OnBeat:
					mode = MusicTransitionMode.FadeOut;
					break;
				default :
					mode = MusicTransitionMode.Cut;
					break;
				}

			} else {

				// Si la lecture peut basculer sur un autre style, on empeche le FadeIn et on le remplace par FadeOutFadeIn
				switch (check.mode) {
				case MusicTransitionMode.FadeIn:
					mode = MusicTransitionMode.FadeOutFadeIn;
					break;
				default :
					mode = check.mode;
					break;
				}

			}

			return new MusicTransition (mode, check.time);
		}

		private void _UpdateTransition ()
		{

			//Debug.Log (_transition.mode);

			// TRANSITION EN ATTENTE
			switch (_transition.mode) {
			case MusicTransitionMode.Cut:
				_Cut ();
				break;
			case MusicTransitionMode.Fade:
				_UpdateFade ();
				break;
			case MusicTransitionMode.FadeIn:
				_UpdateFadeIn ();
				break;
			case MusicTransitionMode.FadeOut:
				_UpdateFadeOut ();
				break;
			case MusicTransitionMode.FadeOutFadeIn:
				_UpdateFadeOutFadeIn ();
				break;
			}
			
		}

		private void _Cut ()
		{
			_sourceWaiting.volume = 1.0f;
			_sourceWaiting.time = 0.0f;
			_sourceWaiting.Play ();
			_sourcePlaying.Stop ();

			EndTransition ();
		}

		private void _UpdateFade ()
		{
			
			if (!_sourceWaiting.isPlaying) {
				
				// On démarre la lecture
				_sourceWaiting.volume = 0.0f;
				_sourceWaiting.time = 0.0f;
				_sourceWaiting.Play ();
				
			} else {
				
				// On Augmente le son progressivement du Waiting, et on descend celui du Playing
				float time = (Time.unscaledTime - _transition.startingTime) / _transition.time;

				_sourceWaiting.volume = Mathf.Lerp (0, 1, time);
				_sourcePlaying.volume = Mathf.Lerp (1, 0, time);
				
				// On arrête la transition si volume == 1
				if (Mathf.Approximately (_sourceWaiting.volume, 1.0f))
				{
					_sourcePlaying.Stop ();
					EndTransition ();
				}
				
			}
		}

		private void _UpdateFadeIn ()
		{
			
			if (!_sourceWaiting.isPlaying) {
				
				// On démarre la lecture
				_sourceWaiting.volume = 0.0f;
				_sourceWaiting.time = 0.0f;
				_sourceWaiting.Play ();
				
			} else {
				
				// On Augmente le son progressivement
				float time = (Time.unscaledTime - _transition.startingTime) / _transition.time;
				_sourceWaiting.volume = Mathf.Lerp (0, 1, time);

				// On arrête la transition si volume == 1
				if (Mathf.Approximately (_sourceWaiting.volume, 1.0f))
					EndTransition ();
				
			}
		}

		private void _UpdateFadeOut ()
		{
			
			if (Mathf.Approximately (_sourceWaiting.volume, 0.0f))
			{
				// On arrête le son
				_sourcePlaying.Stop ();
				EndTransition ();
				
			}
			else
			{
				// On Diminue le son progressivement
				float time = _transition.time / (Time.unscaledTime - _transition.startingTime);
				_sourcePlaying.volume = Mathf.Lerp (1, 0, time);
				
			}
			
		}

		private void _UpdateFadeOutFadeIn ()
		{

			float time = (Time.unscaledTime - _transition.startingTime) / (_transition.time);

			if (Mathf.Approximately (_sourcePlaying.volume, 0.0f))
			{
				// On arrête le son
				_sourcePlaying.Stop ();

				if (!_sourceWaiting.isPlaying) {
					
					// On démarre la lecture
					_sourceWaiting.volume = 0.0f;
					_sourceWaiting.time = 0.0f;
					_sourceWaiting.Play ();
					
				} else {

					// On augmente le son progressivement
					_sourceWaiting.volume = Mathf.Lerp (0, 1, (time * 2) - 1);

					// On arrête la transition si volume == 1
					if (Mathf.Approximately (_sourceWaiting.volume, 1.0f))
						EndTransition ();

				}

			}
			else
			{
				// On Diminue le son progressivement
				_sourcePlaying.volume = Mathf.Lerp (1, 0, time * 2);
				
			}
			
		}

		private void EndTransition ()
		{
			// On arrête la transition
			_transition = MusicTransition.none;

			// On inverse les references. Waiting devient Playing et Playing devient Waiting
			AudioSource wait = _sourceWaiting;
			_sourceWaiting = _sourcePlaying;
			_sourcePlaying = wait;
		}

	}

}
