using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A placeholder learner implementation for testing purposes.
/// It does not learn in any way and simply chooses the optimal action randomly.
/// The probability of all actions is assumed to be uniform.
/// </summary>
public class RandomLearner : Learner {

	/// <summary>
	/// <see cref="T:string"/> identifying this learner class.
	/// Used to reference this class in the <see cref="T:ParamSet"/>.
	/// </summary>
	public static readonly string LearnerType = "RandomLearner";

	/// <summary>
	/// Initializes a new instance of the <see cref="RandomLearner"/> class.
	/// </summary>
	/// <param name="stateExtractor">The State extractor available to the <see cref="T:Learner/> subclass. Not really needed by the <see cref="T:RandomLearner"/>.</param>
	public RandomLearner (StateExtractor stateExtractor) 
		: base (stateExtractor) {
		
	}

	/// <summary>
	/// Wrapper function to predict the optimal action to take.
	/// </summary>
	/// <returns>The optimal action. In this case, it is simply a randomly chosen one.</returns>
	public override MovementAction PredictOptimalAction () {
		int randIndex = UnityEngine.Random.Range(0, base.AvailableActions.Count);
		return base.AvailableActions[randIndex];
	}

	/// <summary>
	/// Learn using the observed reward.
	/// Does nothing indeed, since the <see cref="T:RandomLearner"/> does not learn.
	/// </summary>
	/// <param name="reward">The observed reward.</param>
	public override void Learn (float reward) {
		// Do nothing!
	}
}
