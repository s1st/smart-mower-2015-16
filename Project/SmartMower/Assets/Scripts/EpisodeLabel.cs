using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// Script for a simple label showing the current episode number.
/// </summary>
public class EpisodeLabel : MonoBehaviour, EpisodeManager.IEpisodeChangesReceiver {

	/// <summary>
	/// The text label to control
	/// </summary>
	private Text textLabel;


	/// <summary>
	/// Initialisation
	/// </summary>
	void Start () {
		textLabel = this.GetComponent<Text>();
		EpisodeManager.AddEpisodeChangesReceiver(this);
	}

	/// <summary>
	/// Notification that the current episode will end.
	/// </summary>
	/// <param name="episode">The current episode to end.</param>
	public void EpisodeWillEnd(uint episode) {
		// Do nothing.
	}

	/// <summary>
	/// Notification that the current episode did end.
	/// </summary>
	/// <param name="episode">The episode that ended.</param>
	public void EpisodeDidEnd(uint episode) {
		// Do nothing.
	}

	/// <summary>
	/// Notification that a new episode will start.
	/// </summary>
	/// <param name="episode">The episode that will start.</param>
	public void EpisodeWillStart(uint newEpisode) {
		// Do nothing.
	}

	/// <summary>
	/// Notification that a new episode did start.
	/// </summary>
	/// <param name="episode">The episode that did start.</param>
	public void EpisodeDidStart(uint newEpisode) {
		textLabel.text = string.Format("Episode: {0}", newEpisode);
	}
}
