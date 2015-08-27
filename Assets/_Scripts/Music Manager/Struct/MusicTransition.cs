using UnityEngine;
using System.Collections;

namespace TheArena.Music
{

	/// <summary>
	/// Represente une transition entre deux morceaux
	/// </summary>

	public struct MusicTransition
	{

		// STATIC

		public static MusicTransition none = new MusicTransition (MusicTransitionMode.None);
		public static MusicTransition cut = new MusicTransition (MusicTransitionMode.Cut);
		public static MusicTransition fade = new MusicTransition (MusicTransitionMode.Fade, 1.0f);
		public static MusicTransition outIn = new MusicTransition (MusicTransitionMode.FadeOutIn, 1.0f);

		// ATTRIBUTS

		public MusicTransitionMode mode;

		private float _time;
		public float time {
			get { return _time; }
			set { _time = Mathf.Clamp (value, 0.1f, Mathf.Infinity); }
		}

		public float startingTime {
			get;
			private set;
		}

		public MusicTransition (MusicTransitionMode _mode = MusicTransitionMode.None, float _t = 0.1f)
		{

			mode = _mode;
			time = _t;

			startingTime = Time.unscaledTime;

		}

		// OPERATEURS

		public static MusicTransition operator +(MusicTransition mt, float t) 
		{
			return new MusicTransition(mt.mode, mt.time + t);
		}

		public static MusicTransition operator *(MusicTransition mt, float t) 
		{
			return new MusicTransition(mt.mode, mt.time * t);
		}

	}

}