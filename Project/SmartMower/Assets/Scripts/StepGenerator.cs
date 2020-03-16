
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The StepGenerator is used to provide a global clock.
/// Arbitrary objects can register to receive a notification after each step.
/// In this project, the StepGenerator is the only object that interacts with the built-in Unity update cycle.
/// </summary>
public class StepGenerator : MonoBehaviour, EpisodeManager.IEpisodeChangesReceiver {

	/// <summary>
	/// The interface each object has to implement, if it wants to receive the step signal.
	/// </summary>
	public interface IStepReceiver {
		void DoStep();
	}

	/// <summary>
	/// All registered step receivers
	/// </summary>
	private List<IStepReceiver> stepReceivers = new List<IStepReceiver>();

	/// <summary>
	/// Whether steps are generated or not.
	/// </summary>
	private bool generateSteps = false;

	/// <summary>
	/// The amount of time between each step.
	/// </summary>
	private float timeBetweenSteps = 0.0f;

	/// <summary>
	/// The time since the last step occured.
	/// </summary>
	private float timeSinceLastStep = 0.0f;

	/// <summary>
	/// The number of steps that occured since the last reset.
	/// </summary>
	private int stepCount = 0;



	public float TimeBetweenSteps {
		get {
			return timeBetweenSteps;
		}
		set {
			timeBetweenSteps = value;
		}
	}

	public int StepCount {
		get {
			return stepCount;
		}
	}

	/// <summary>
	/// Makes the StepGenerator generate steps.
	/// </summary>
	public void StartSteps () {
		generateSteps = true;
	}

	/// <summary>
	/// Makes the StepGenerator pause.
	/// </summary>
	public void HaltSteps () {
		generateSteps = false;
		timeSinceLastStep = 0.0f;
	}

	/// <summary>
	/// Adds a new step receiver.
	/// </summary>
	/// <returns><c>true</c>, if the step receiver was added, <c>false</c> otherwise.</returns>
	/// <param name="stepReceiver">The new step receiver.</param>
	public bool AddStepReceiver (IStepReceiver stepReceiver) {
		if (! stepReceivers.Contains(stepReceiver)) {
			stepReceivers.Add(stepReceiver);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Removes a step receiver.
	/// </summary>
	/// <returns><c>true</c>, if the step receiver was removed, <c>false</c> otherwise.</returns>
	/// <param name="stepReceiver">The step receiver to remove.</param>
	public bool RemoveStepReceiver (IStepReceiver stepReceiver) {
		if (stepReceivers.Contains(stepReceiver)) {
			stepReceivers.Remove(stepReceiver);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Forces the StepGenerator to perform one step.
	/// </summary>
	public void ForceStep () {
		GenerateStep();
	}

	/// <summary>
	/// Sets the step counter to <c>0</c>.
	/// </summary>
	public void ResetStepCounter () {
		stepCount = 0;
	}
		

	/// <summary>
	/// Basic initialisation.
	/// </summary>
	void Start () {
		EpisodeManager.AddEpisodeChangesReceiver(this);
	}

	/// <summary>
	/// Checks after each frame, if the next time step has to occur.
	/// </summary>
	void Update () {
		if (generateSteps) {
			timeSinceLastStep += Time.deltaTime;
			if (timeSinceLastStep >= TimeBetweenSteps) {
				GenerateStep();
			}
		}
	}


	/// <summary>
	/// Generates one time step.
	/// </summary>
	private void GenerateStep() {
		timeSinceLastStep = 0.0f;
		stepCount ++;
		foreach (var stepReceiver in stepReceivers) {
			stepReceiver.DoStep();
		}
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
		ResetStepCounter();
	}

	/// <summary>
	/// Notification that a new episode did start.
	/// </summary>
	/// <param name="episode">The episode that did start.</param>
	public void EpisodeDidStart(uint newEpisode) {
		// Do nothing.
	}
}
