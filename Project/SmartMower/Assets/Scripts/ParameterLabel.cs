using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Simple script attached to a label for (debug) output of parameters
/// </summary>
public class ParameterLabel : MonoBehaviour, QLearner.IQLearnerParamReceiver {

	/// <summary>
	/// The label to display the text.
	/// </summary>
    private Text label;

	/// <summary>
	/// Sets the initial parameter values to display.
	/// </summary>
	/// <param name="greed">Greediness value.</param>
	/// <param name="discount">Discount value.</param>
	/// <param name="learnrate">Learnrate.</param>
	/// <param name="initVals">QTable initial value.</param>
	/// <param name="etraces">Whether eligibility traces are used or not.</param>
	/// <param name="gamma">Gamma value</param>
	/// <param name="model">Whether model base learning is used or not.</param>
	/// <param name="refined">Whether the refinde, true garden model is used.</param>
	/// <param name="n">The number of virtual steps performd using the model.</param>
    public void InitialValues(float greed, float discount, float learnrate, 
        float initVals, bool etraces, float gamma, 
        bool model, bool refined, int n)
    {
        label.text = string.Format(
@"Greed: {0}
Discount: {1}
Learnrate: {2}
InitialQValues: {3}
ETraces: {4}
Gamma: {5}
Modelplanning: {6}
Refined Model: {7}
N: {8}", 
greed, discount, learnrate, initVals, etraces, gamma, model, refined, n);
    }

    /// <summary>
    /// Basic initialisation.
    /// </summary>
    void Start () {
        QLearner.SetParamReceiver(this);
        label = this.GetComponent<Text>();
    }

}
