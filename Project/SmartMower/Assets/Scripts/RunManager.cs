using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple script attached to the run controls (Start/Stop, Step)
/// Handles their functionality.
/// </summary>
public class RunManager : MonoBehaviour {

	/// <summary>
	/// Enumeration for the functionality of the Start/Stop button.
	/// </summary>
	public enum StartStopFunctionality
	{
		StartFunctionality,
		StopFuntionality
	}

	/// <summary>
	/// The Start/Stop Button.
	/// </summary>
	public Button StartStopButton;

	/// <summary>
	/// The Step Button.
	/// </summary>
	public Button StepButton;

	/// <summary>
	/// The functionality of the Start/Stop button.
	/// </summary>
	private StartStopFunctionality startStopButtonFunctionality = StartStopFunctionality.StartFunctionality;

	/// <summary>
	/// The project-wide <see cref="T:StepGenerator"/>.
	/// </summary>
	private StepGenerator stepGenerator;

	public StartStopFunctionality StartStopButtonFunctionality {
		get {
			return startStopButtonFunctionality;
		}
		set {
			startStopButtonFunctionality = value;
			if (startStopButtonFunctionality == StartStopFunctionality.StartFunctionality) {
				StartStopButton.GetComponentInChildren<Text>().text = "Start";
				StepButton.interactable = true;
			} else {
				StartStopButton.GetComponentInChildren<Text>().text = "Stop";
				StepButton.interactable = false;
			}
		}
	}

	/// <summary>
	/// Used to initialize the RunManager.
	/// </summary>
	void Awake () {
		GameObject stepGenGameObject = GameObject.FindGameObjectWithTag("StepGenerator");
		stepGenerator = stepGenGameObject.GetComponent<StepGenerator>();
	}

	/// <summary>
	/// Called when the Start/Stop button is clicked.
	/// </summary>
	public void StartStopButtonClicked () {
		if (StartStopButtonFunctionality == StartStopFunctionality.StartFunctionality) {
			stepGenerator.StartSteps();
			StartStopButtonFunctionality = StartStopFunctionality.StopFuntionality;
		} else {
			stepGenerator.HaltSteps();
			StartStopButtonFunctionality = StartStopFunctionality.StartFunctionality;
		}
	}

	/// <summary>
	/// Called when the Step button is clicked.
	/// </summary>
	public void StepButtonClicked () {
		stepGenerator.ForceStep();
	}
}
