using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Label showing number of steps
/// </summary>

public class StepLabel : MonoBehaviour, StepGenerator.IStepReceiver {

	/// <summary>
	/// The text label to control
	/// </summary>
	private Text textLabel;

	/// <summary>
	/// The <see cref="T:StepGenerator"/> that provides the current step number.
	/// </summary>
	private StepGenerator stepGenerator;

	/// <summary>
	/// Basic initialisation.
	/// </summary>
	void Start () {
		textLabel = this.GetComponent<Text>();
		GameObject stepGenGameObject = GameObject.FindGameObjectWithTag("StepGenerator");
		stepGenerator = stepGenGameObject.GetComponent<StepGenerator>();
		stepGenerator.AddStepReceiver(this);
	}

	/// <summary>
	/// Implementation of the <see cref="T:StepGenerator+IStepReceiver"/> interface.
	/// </summary>
	public void DoStep() {
		textLabel.text = string.Format("Steps: {0}", stepGenerator.StepCount);
	}

}
