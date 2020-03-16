using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for Learner implementations.
/// </summary>
public abstract class Learner {


	/// <summary>
	/// The state extractor used to determine the state of the mower robot inside the garden.
	/// </summary>
	private StateExtractor _stateExtractor;

	/// <summary>
	/// The currently availabel actions of the mower.
	/// </summary>
    private List<MovementAction> _availableActions;


    /// <summary>
    /// Initializes a new instance of the <see cref="Learner"/> class.
    /// </summary>
    /// <param name="stateExtractor">The StateExtractor providing state information.</param>
    public Learner (StateExtractor stateExtractor) {
		this._stateExtractor = stateExtractor;
	}

	public StateExtractor StateExtractor {
		get {
			return _stateExtractor;
		}
	}

    public State CurrentState
    {
        get
        {
			return _stateExtractor.ExtractState();
        }
    }

    public List<MovementAction> AvailableActions
    {
        get
        {
            return _availableActions;
        }

        set
        {
            _availableActions = value;
        }
    }


    /// <summary>
    /// Predicts the optimal action to take.
    /// </summary>
    /// <returns>The optimal action.</returns>
    /// <param name="availableActions">All currently available actions. Must not be null or empty.</param>
    public MovementAction OptimalAction (List<MovementAction> availableActions)
    {
        AvailableActions = availableActions;
        return PredictOptimalAction();
    }

	/// <summary>
	/// Wrapper function to predict the optimal action to take.
	/// </summary>
	/// <returns>The optimal action.</returns>
    public abstract MovementAction PredictOptimalAction();

	/// <summary>
	/// Learn using the observed reward.
	/// </summary>
	/// <param name="reward">The observed reward.</param>
	public abstract void Learn (float reward);
		
}
