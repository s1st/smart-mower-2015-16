using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;

/// <summary>
/// This class is used to collect various statistics from the garden and its associated objects in order to measure the learning success.
/// These statistics are the written to a csv file.
/// NOTE: So far, only the number of steps needed to mow the complete garden is saved for each episode.
/// </summary>
public class StatisticsWriter : EpisodeManager.IEpisodeChangesReceiver {

	/// <summary>
	/// csv heading for the episodes.
	/// </summary>
	private static readonly string statisticsNameEpisode = "Episode";

	/// <summary>
	/// csv heading for the steps needed.
	/// </summary>
	private static readonly string statisticsNameStepsNeeded = "StepsNeeded";

	/// <summary>
	/// The csv delimiter used when writing to csv file.
	/// </summary>
	private readonly string csvDelimiter = ";";

	/// <summary>
	/// The name of the csv file where the statistics are written to.
	/// </summary>
	private string statisticsFileName;

	/// <summary>
	/// The garden whose statistics are collected and saved.
	/// </summary>
	private Garden garden;

	/// <summary>
	/// The global step generator used to determine, how many steps were needed until the complete garden was mown.
	/// </summary>
	private StepGenerator stepGenerator;

	/// <summary>
	/// Initializes a new instance of the <see cref="StatisticsWriter"/> class.
	/// </summary>
	/// <param name="paramSet">The set of project parameters.</param>
	public StatisticsWriter (Garden garden, ParamSet paramSet) {
		this.garden = garden;
		this.statisticsFileName = paramSet.StringParam(ParamSet.ParamGroupStatisticsWriter, ParamSet.ParamStatisticsFileName);
		GameObject stepGenGameObject = GameObject.FindGameObjectWithTag("StepGenerator");
		this.stepGenerator = stepGenGameObject.GetComponent<StepGenerator>();
		EpisodeManager.AddEpisodeChangesReceiver(this);
		// Write the statistics headline into the csv file
		WriteValuesIntoCSVLine(new string[] {statisticsNameEpisode,statisticsNameStepsNeeded});
	}

	/// <summary>
	/// Writes the given list of values into a single line in the csv statistics file.
	/// </summary>
	/// <param name="csvValues">The value to write.</param>
	private void WriteValuesIntoCSVLine(string [] values) {
		StringBuilder builder = new StringBuilder();
		builder.AppendLine(string.Join(csvDelimiter, values));
		File.AppendAllText(statisticsFileName, builder.ToString());
	}

	/// <summary>
	/// Notification that the current episode will end.
	/// </summary>
	/// <param name="episode">The current episode to end.</param>
	public void EpisodeWillEnd(uint episode) {
		uint stepsNeeded = (uint)stepGenerator.StepCount;
		string [] valuesToWrite = new string [] {episode.ToString(), stepsNeeded.ToString()};
		WriteValuesIntoCSVLine(valuesToWrite);
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
		// Do nothing.
	}

}
