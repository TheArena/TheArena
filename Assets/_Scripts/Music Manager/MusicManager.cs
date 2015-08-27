using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace TheArena.Music
{

	/// <summary>
	///  Cette classe permet de gérer la musique durant le jeu.
	///  Elle réalise automatiquement les transitions entre les différents thèmes en synchronisant les musiques sur la mesure.
	/// </summary>

	[DisallowMultipleComponent]
	[AddComponentMenu ("Audio/Music Manager")]
	public class MusicManager : MonoBehaviour {

		// ATTRIBUTS STATICS

		/// <summary>
		/// Indique si la musique est actuellement en cours de transition.
		/// Impossible de changer la musique pendant cette transition pour éviter les sautes. (Read Only)
		/// </summary>
		/// <value><c>true</c> si c'est en cours de transition; sinon, <c>false</c>.</value>
		
		public static bool isFading {
			get
			{
				return _instance.fading;
			}
		}
		
		/// <summary>
		/// Retourne le style de musique actuellement joué
		/// </summary>
		
		private static MusicStyle _style;
		public static MusicStyle style {
			get { return _style; }
			private set { _style = value; }
		}

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

		internal bool fading {
			get
			{
				if (_transition.mode == MusicTransitionMode.None)
					return false;
				else
					return true;
			}
		}

		// PROPRIETES

		private MusicTransition _transition;
		private AudioSource _sourcePlaying, _sourceWaiting;

		// METHODES STATICS

		/// <summary>
		/// Permet de jouer un style de musique avec la transition souhaité
		/// </summary>
		/// <param name="style">Le style de la musique.</param>

		public static void Play (MusicStyle s, MusicTransition t)
		{
			_instance.StartTransition (s, t);
		}

		/// <summary>
		/// Arrête la musique.
		/// </summary>

		public static void Stop (MusicTransition t)
		{
			_instance.StartTransition (MusicStyle.Empty, t);
		}

		// METHODES INTERNE

		internal void StartTransition (MusicStyle s, MusicTransition t)
		{

			// Le style de musique en lecture est celui demandé

			if (style == s) {
				Debug.LogWarning ("Musique déjà en lecture");
				return;
			}

			// Si le style demandé est "Empty". Empty est prioritaire

			if (s == MusicStyle.Empty)
			{
				_GiveToAudioSource (null);
				_transition = _CheckTransition (t);
				style = s;
				return;
			}

			AudioClip nextclip = _GetClip (s);

			// Aucun clip n'a été trouvé

			if (nextclip == null) {
				Debug.LogError ("Aucun clip à jouer. Ajoutez des clips à la liste (" + style + ")");
				return;
			}

			// Si le Clip a bien été chargé dans un AudioSource

			if (_GiveToAudioSource (nextclip))
			{
				_transition = _CheckTransition (t);
				style = s;
				return;
			}
			else
			{
				Debug.LogWarning ("Impossible de changer de musique pour le moment");
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

		private AudioClip _GetClip (MusicStyle s)
		{

			AudioClip [] clips = clips_intro;

			switch (s) {
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

				// Si c'est la première lecture, ces transitions sont interdites et remplacés par Fade ou Cut
				switch (check.mode) {
				case MusicTransitionMode.Fade:
				case MusicTransitionMode.FadeOutIn:
					mode = MusicTransitionMode.Fade;
					break;
				default :
					mode = MusicTransitionMode.Cut;
					break;
				}

			} else if (_sourceWaiting.clip == null) {

				// Si il n'y a pas de prochain clip, ces transitions sont interdites et remplacés par Fade ou Cut
				switch (check.mode) {
				case MusicTransitionMode.Fade:
				case MusicTransitionMode.FadeOutIn:
					mode = MusicTransitionMode.Fade;
					break;
				default :
					mode = MusicTransitionMode.Cut;
					break;
				}

			} else {

				mode = check.mode;

			}

			return new MusicTransition (mode, check.time);
		}

		/// <summary>
		/// Si une transition est programmé, la méthode appel la méthode correspondante.
		/// </summary>

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
			case MusicTransitionMode.FadeOutIn:
				_UpdateFadeOutIn ();
				break;
			}
			
		}

		/// <summary>
		/// Methode appelé lors de la transition CUT
		/// </summary>

		private void _Cut ()
		{
			_sourceWaiting.volume = 1.0f;
			_sourceWaiting.time = 0.0f;
			_sourceWaiting.Play ();
			_sourcePlaying.volume = 0.0f;
			_sourcePlaying.Stop ();

			EndTransition ();
		}

		/// <summary>
		/// Methode appelé lors de la transition FADE
		/// </summary>

		private void _UpdateFade ()
		{
			
			if (!_sourceWaiting.isPlaying && _sourceWaiting.clip != null) {
				
				// On démarre la lecture
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
					_sourcePlaying.volume = 0.0f;
					_sourcePlaying.Stop ();
					EndTransition ();
				}
				
			}
		}

		/*private void _UpdateFadeIn ()
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
			
		}*/

		/// <summary>
		/// Méthode appelé lors de la transition FadeOutIn
		/// </summary>

		private void _UpdateFadeOutIn ()
		{

			float time = (Time.unscaledTime - _transition.startingTime) / (_transition.time);

			if (Mathf.Approximately (_sourcePlaying.volume, 0.0f))
			{
				// On arrête le son
				_sourcePlaying.volume = 0.0f;
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

		/// <summary>
		/// Méthode appelé à la fin de chaque transition. Elle inverse les sources Waiting et Playing
		/// </summary>

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
