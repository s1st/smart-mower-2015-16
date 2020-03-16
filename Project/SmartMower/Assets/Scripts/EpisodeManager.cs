using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <see cref="T:EpisodeManager"/> class handles begin and end of episodes.
/// It informs all interested objects about the changes regarding the learning episode.
/// <description>
/// Note: So far, this script is not attached to an Unity GameObject. This might change, if neccessary.
/// </description>
/// </summary>
public class EpisodeManager {

	/// <summary>
	/// Interface for all notification receivers of an <see cref="T:EpisodeManager"/> object.
	/// </summary>
	public interface IEpisodeChangesReceiver {

		/// <summary>
		/// Notification that the current episode will end.
		/// </summary>
		/// <param name="episode">The current episode to end.</param>
		void EpisodeWillEnd(uint episode);

		/// <summary>
		/// Notification that the current episode did end.
		/// </summary>
		/// <param name="episode">The episode that ended.</param>
		void EpisodeDidEnd(uint episode);

		/// <summary>
		/// Notification that a new episode will start.
		/// </summary>
		/// <param name="episode">The episode that will start.</param>
		void EpisodeWillStart(uint newEpisode);

		/// <summary>
		/// Notification that a new episode did start.
		/// </summary>
		/// <param name="episode">The episode that did start.</param>
		void EpisodeDidStart(uint newEpisode);

	}

	/// <summary>
	/// The number of the current learning episode.
	/// </summary>
	private static uint episode = 1;

	/// <summary>
	/// The number of episodes that are run before the program exits.
	/// Use a negative number to specify no limit.
	/// </summary>
	private static int episodeLimit = -1;

	/// <summary>
	/// The list of notification receivers.
	/// </summary>
	private static List<IEpisodeChangesReceiver> episodeChangesRecievers = new List<IEpisodeChangesReceiver>();

	public static uint Episode {
		get {
			return episode;
		}
		private set {
			episode = value;
		}
	}

	public static int EpisodeLimit {
		get {
			return episodeLimit;
		}
		set {
			episodeLimit = value;
		}
	}

	/// <summary>
	/// Ends the current episode and start a new one.
	/// </summary>
	public static void NextEpisode () {

		foreach (var receiver in episodeChangesRecievers) {
			receiver.EpisodeWillEnd(Episode);
		}
		Debug.Log(string.Format("Episode {0} ended.", Episode));
		foreach (var receiver in episodeChangesRecievers) {
			receiver.EpisodeDidEnd(Episode);
		}

		// Check the episode limit
		if (Episode >= EpisodeLimit) {
			// NOTE: This only closes the program, if ran as a standalone application.
			// If the program is run inside Unity, nothing happens here.
			Application.Quit();
		}

		foreach (var receiver in episodeChangesRecievers) {
			receiver.EpisodeWillStart(Episode +1);
		}
		Episode ++;
		Debug.Log(string.Format("Episode {0} began.", Episode));
		foreach (var receiver in episodeChangesRecievers) {
			receiver.EpisodeDidStart(Episode);
		}
	} 


	/// <summary>
	/// Registers a new object to receive notifications about episode changes.
	/// </summary>
	/// <returns><c>true</c>, if the receiver was added, <c>false</c> otherwise.</returns>
	/// <param name="observer">The new receiver to register.</param>
	public static bool AddEpisodeChangesReceiver (IEpisodeChangesReceiver receiver) {
		if (! episodeChangesRecievers.Contains(receiver)) {
			episodeChangesRecievers.Add(receiver);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Removes the given object from the list of episode changes receivers.
	/// </summary>
	/// <returns><c>true</c>, if the receiver was removed, <c>false</c> otherwise.</returns>
	/// <param name="observer">The receiver to remove.</param>
	public static bool RemoveEpisodeChangesReceiver(IEpisodeChangesReceiver receiver) {
		if (episodeChangesRecievers.Contains(receiver)) {
			episodeChangesRecievers.Remove(receiver);
			return true;
		} else {
			return false;
		}
	}
}
