using UnityEngine;
using System.Collections;

namespace TheArena
{

	/// <summary>
	/// Outils applicable sur de la musique. Classe indispensable pour MusicManager.
	/// </summary>

	public class MusicTools
	{

		// METHODES STATICS

		/// <summary>
		/// Retourne un clip au hasard parmis le tableau de clips.
		/// </summary>
		/// <returns>The AudioClip selectionné. Retourne null si aucun clip n'a été sélectionné.</returns>
		/// <param name="clips">Les clips parmis lesquels la méthode va piocher dedans.</param>

		public static AudioClip RandomClip (AudioClip[] clips)
		{

			if (clips.Length == 0)
				return null;

			int i = Random.Range (0, clips.Length - 1);

			return clips [i];

		}

	}

}