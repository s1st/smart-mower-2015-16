using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Main QLearning implementation
/// </summary>
/// <description>
/// Since all variations of learning in this project
/// are derived from this learner,
/// this class holds the main RL logic
/// </description>

class QLearner : Learner, EpisodeManager.IEpisodeChangesReceiver
{
	/// <summary>
	/// Simple interface to receive the parameters of a <see cref="T:QLearner"/> object when they are set.
	/// </summary>
    public interface IQLearnerParamReceiver{

		/// <summary>
		/// Notification about the initial parameters of the <see cref="T:QLearner"/> object.
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
        void InitialValues(float greed, float discount, float learnrate, 
            float initVals, bool etraces, float gamma, 
            bool model, bool refined, int n);
    }

    /// <summary>
    /// <see cref="T:string"/> identifying this learner class.
    /// Used to reference this class in the <see cref="T:ParamSet"/>.
    /// </summary>
    public static readonly string LearnerType = "QLearner";

    QTable _qTable;
    EligibilityTable _eTable;
    ModelTable _mTable;
    ModelTableSimple _simpleMTable;

    float _greediness = 0f;
    float _discountValue = 1.0f;
    float _learnRate = 0.7f;
	float _initialQValue = 0f;
	string _qTableSerializationFile;

    bool _run_with_etraces = false;
    float _gamma = 0.9f;
	string _eTableSerializationFile;

    bool _modelPlanning = true;
    bool _refined = true;
	string _modelTableSerializationFile;
    int _N = 10;

    bool _showParamLabel = true;

    MovementAction _lastAction;
	State _lastState;
    static List<IQLearnerParamReceiver> _receivers = new List<IQLearnerParamReceiver>();

    /// <summary>
    /// Constructor of the QLearner
    /// </summary>
    /// <param name="stateExtractor"></param>
    /// The mower, that is learning
    /// <param name="mower"></param>
    /// 
    /// The following are all classic QLearning parameters,
    /// with values in between 0 and 1:
    /// 
    /// Greediness of the Q learning algorithm
    /// <param name="greediness"></param>
    /// Discountvalue: How much are old experiences weighted
    /// <param name="discountValue"></param>
    /// How fast or slow shall the mower learn?
    /// Fast: High value, slow: low value
    /// <param name="learnRate"></param>
    /// 
    /// "Special" parameters:
    /// 
    /// QLearning with optimitstic initial values
    /// <param name="initialQValue"></param>
	/// File to load the qTable from. Can be null.
	/// <param name="qTableDeserializationFile">
	/// File to write the qTable to after the episode limit is reached.
	/// <param name="qTableSerializationFile">
    /// Running with ETraces, yes or no?
    /// <param name="eligTraces"></param>
    /// If so, with what gamma?
    /// <param name="gamma"></param>
	/// File to load the EligibilityTable from. Can be null.
	/// <param name="eTableDeserializationFile">
	/// File to write the EligibilityTable to after the episode limit is reached.
	/// <param name="eTableSerializationFile">
    /// Running with Model?
    /// <param name="modelPlanning"></param>
    /// Using refined (true) or DynaQ (false) model
    /// <param name="refined"></param>
	/// File to load the Model from. Can be null.
	/// <param name="modelTableDeserializationFile">
	/// File to write the Model to after the episode limit is reached.
	/// <param name="modelTableSerializationFile">
    /// How many N virtual steps are executed in the model?
    /// <param name="n"></param>
    /// Shall the parameter label be shown?
    /// <param name="showParamLabel"></param>

    public QLearner(StateExtractor stateExtractor,
					Mower mower,
					float greediness,
					float discountValue,
					float learnRate,
					float initialQValue,
					string qTableDeserializationFile,
					string qTableSerializationFile,
                    bool eligTraces,
                    float gamma,
					string eTableDeserializationFile,
					string eTableSerializationFile,
                    bool modelPlanning,
                    bool refined,
					string modelTableDeserializationFile,
					string modelTableSerializationFile,
                    int n,
                    bool showParamLabel) 
		: base (stateExtractor)
	{
		Greediness = greediness;
		DiscountValue = discountValue;
		LearnRate = learnRate;
		InitialQValue = initialQValue;
		_qTableSerializationFile = qTableSerializationFile;
		_qTable = new QTable(InitialQValue, qTableDeserializationFile);
        Run_with_etraces = eligTraces;
        Gamma = gamma;
		_eTableSerializationFile = eTableSerializationFile;
        ModelPlanning = modelPlanning;
        Refined = refined;
		_modelTableSerializationFile = modelTableSerializationFile;
        if(Run_with_etraces)
			_eTable = new EligibilityTable(0f, eTableDeserializationFile);
        if (ModelPlanning)
        {
            if(refined)
				_mTable = new ModelTable(mower, modelTableDeserializationFile);
            else
				_simpleMTable = new ModelTableSimple(0f, modelTableDeserializationFile);
        }
            
        N = n;
        _showParamLabel = showParamLabel;
        
        Debug.Log(string.Format(
            "QLearner instantiated with greediness:{0}, discountValue:{1}, learnRate:{2}, initialQValue:{3}, N for Model:{4}",
                        Greediness,
                        DiscountValue,
                        LearnRate,
                        InitialQValue,
                        N));

		EpisodeManager.AddEpisodeChangesReceiver(this);
    }

    /// <summary>
    /// Constructor for XML
    /// </summary>
    /// <param name="stateExtractor"></param>
    /// <param name="mower"></param>
    /// <param name="paramSet"></param>

	public QLearner(StateExtractor stateExtractor, Mower mower, ParamSet paramSet)
		: this (stateExtractor,
				mower,
				paramSet.FloatParam(ParamSet.ParamGroupQLearner, ParamSet.ParamGreediness),
				paramSet.FloatParam(ParamSet.ParamGroupQLearner, ParamSet.ParamDiscountValue),
				paramSet.FloatParam(ParamSet.ParamGroupQLearner, ParamSet.ParamLearnRate),
				paramSet.FloatParam(ParamSet.ParamGroupQLearner, ParamSet.ParamInitialQValue),
				paramSet.StringParam(ParamSet.ParamGroupQLearner, ParamSet.ParamQTableDeserialiaztionFile),
				paramSet.StringParam(ParamSet.ParamGroupQLearner, ParamSet.ParamQTableSerialiaztionFile),
                paramSet.BoolParam(ParamSet.ParamGroupMower, ParamSet.ParamEligTraces),
                paramSet.FloatParam(ParamSet.ParamGroupMower, ParamSet.ParamGamma),
				paramSet.StringParam(ParamSet.ParamGroupMower, ParamSet.ParamEligibilityTableDeserialiaztionFile),
				paramSet.StringParam(ParamSet.ParamGroupMower, ParamSet.ParamEligibilityTableSerialiaztionFile),
                paramSet.BoolParam(ParamSet.ParamGroupMower, ParamSet.ParamModelPlanning),
                paramSet.BoolParam(ParamSet.ParamGroupMower, ParamSet.ParamModelPlanningRefined),
				paramSet.StringParam(ParamSet.ParamGroupMower, ParamSet.ParamModelTableDeserialiaztionFile),
				paramSet.StringParam(ParamSet.ParamGroupMower, ParamSet.ParamModelTableSerialiaztionFile),
                paramSet.IntParam(ParamSet.ParamGroupMower, ParamSet.ParamN),
                paramSet.BoolParam(ParamSet.ParamGroupSetupManager, ParamSet.ParamShow)
                )
    {

    }

    /// <summary>
    /// Adds a reveiver for parameter notifications.
    /// </summary>
    /// <param name="rec"></param>

    public static void SetParamReceiver(IQLearnerParamReceiver rec)
    {
        _receivers.Add(rec);
    }

	/// <summary>
	/// Learn using the observed reward.
	/// </summary>
	/// <param name="reward">The observed reward.</param>
    public override void Learn(float reward)
    {
        if (_showParamLabel)
        {
            _receivers[0].InitialValues(Greediness, DiscountValue, LearnRate, InitialQValue,
            Run_with_etraces, Gamma, ModelPlanning, Refined, N);
        }
        // Old state
        StateActionPair oldSAP = new StateActionPair(_lastState, _lastAction);
        float oldQvalue = _qTable.getQValue(oldSAP);

        // Current state
		State currentState = base.CurrentState;
		MovementAction bestCurrentAction = _qTable.getBestActionForState(currentState);
        StateActionPair currentSAP = new StateActionPair(currentState, bestCurrentAction);
        float bestCurrentQValue = _qTable.getQValue(currentSAP);

        if(Run_with_etraces)
        {
            float delta = reward + DiscountValue * bestCurrentQValue - oldQvalue;
			_eTable.SetEligibilityValue(oldSAP, 1f);
            _qTable.AddScaledValues(LearnRate * delta, _eTable);
            _eTable.ScaleAllEligibilityValues(DiscountValue * Gamma);
		} else {
            // Standard QLearning
			float newQValue = Mathf.Lerp(oldQvalue, reward + (DiscountValue * bestCurrentQValue), LearnRate);
			_qTable.setQValue(oldSAP, newQValue);
		}
        // Refined Model
        if(ModelPlanning && Refined)
        {
			// Update the model according to the observed state
			_mTable.IncorporateObservedState(currentState);

			State virtualFromState, virtualToState;
			MovementAction virtualPerformedAction;
			float virtualReward;
			// Perfrom N virtual steps
            for(int i = 0; i < N; i++)
            {
				// Generated the virtual step
				bool virtualStepGenerated = _mTable.GenerateRandomModelStep(out virtualFromState, out virtualPerformedAction, out virtualToState, out virtualReward);
				if (virtualStepGenerated) {
					StateActionPair virtualFromSAP = new StateActionPair(virtualFromState, virtualPerformedAction);
					// Standard QLearning
					float fromStateQVal = _qTable.getQValue(virtualFromSAP);
					// Get the best action after the virtual step
					MovementAction bestAction = _qTable.getBestActionForState(virtualToState);
					StateActionPair virtualToSAP = new StateActionPair(virtualToState, bestAction);
					// Q value update for the virtual step
					float toStateQVal = _qTable.getQValue(virtualToSAP);
					float newQVal = Mathf.Lerp(fromStateQVal, virtualReward + (DiscountValue * toStateQVal), LearnRate);
					_qTable.setQValue(virtualFromSAP, newQVal);
				}
            }
        }
        // DynaQ Model
        if(ModelPlanning && !Refined)
        {
            _simpleMTable.setStateRewardPairAtStateActionPair(oldSAP, new StateRewardPair(currentState, reward));
            for (int i = 0; i < N; i++)
            {
                StateActionPair randSAP = _qTable.getRandomVisitedStateAndAction();
                StateRewardPair srp = _simpleMTable.getStateRewardPair(randSAP);
                // Standard QLearning
                float qVal = _qTable.getQValue(randSAP);

                //MovementAction bAct = _qTable.getBestActionForState(currentState);
                MovementAction bAct = _qTable.getBestActionForState(srp.State);

                // new (current) parameters
                //StateActionPair cSAP = new StateActionPair(currentState, bestCurrentAction);
                StateActionPair cSAP = new StateActionPair(srp.State, bAct);
                float bQVal = _qTable.getQValue(cSAP);
                float newQVal = Mathf.Lerp(qVal, srp.Reward + (DiscountValue * bQVal), LearnRate);
                _qTable.setQValue(randSAP, newQVal);
            }
        }
    }

    /// <summary>
    /// Predicts the optimal action for the current state by looking up the QTable
    /// </summary>
    /// <returns>The optimal action</returns>
    public override MovementAction PredictOptimalAction()
    {
        _qTable.CurActions = base.AvailableActions;
        float randNum = UnityEngine.Random.Range(0f, 1f);
        MovementAction action = null;
        if (randNum > Greediness)
        {
            int randIndex = UnityEngine.Random.Range(0, base.AvailableActions.Count);
            action = base.AvailableActions[randIndex];
        } else
        {
            action = _qTable.getBestActionForState(base.CurrentState);
        } 
		_lastState = CurrentState;
        _lastAction = action;
        return action;
    }

    // --- Getters and Setters from here ---

    public float Greediness
    {
        get
        {
            return _greediness;
        }

        set
        {
            _greediness = value;
        }
    }

    public float DiscountValue
    {
        get
        {
            return _discountValue;
        }

        set
        {
            _discountValue = value;
        }
    }

    public float LearnRate
    {
        get
        {
            return _learnRate;
        }

        set
        {
            _learnRate = value;
        }
    }

    public float InitialQValue
    {
        get
        {
            return _initialQValue;
        }

        set
        {
            _initialQValue = value;
        }
    }

    public bool Run_with_etraces
    {
        get
        {
            return _run_with_etraces;
        }

        set
        {
            _run_with_etraces = value;
        }
    }

    public float Gamma
    {
        get
        {
            return _gamma;
        }

        set
        {
            _gamma = value;
        }
    }

    public bool ModelPlanning
    {
        get
        {
            return _modelPlanning;
        }

        set
        {
            _modelPlanning = value;
        }
    }

    public bool Refined
    {
        get
        {
            return _refined;
        }

        set
        {
            _refined = value;
        }
    }

    public int N
    {
        get
        {
            return _N;
        }

        set
        {
            _N = value;
        }
    }

	// --- EpisodeManager.IEpisodeChangesReceiver ---

	/// <summary>
	/// Notification that the current episode will end.
	/// </summary>
	/// <param name="episode">The current episode to end.</param>
	public void EpisodeWillEnd(uint episode) {
		// In case of the last episode, save the learning progress if the project parameters tell to do so.
		if (EpisodeManager.Episode >= EpisodeManager.EpisodeLimit) {
			if (_qTableSerializationFile != null && _qTableSerializationFile.Length > 0) {
				_qTable.SerializeTableToFile(_qTableSerializationFile);
			}
			if (_run_with_etraces) {
				_eTable.SerializeTableToFile(_eTableSerializationFile);
			}
			if (ModelPlanning) {
				if (_refined) {
					if (_modelTableSerializationFile != null && _modelTableSerializationFile.Length > 0) {
						_mTable.SerializeTableToFile(_modelTableSerializationFile);
					}
				} else {
					if (_modelTableSerializationFile != null && _modelTableSerializationFile.Length > 0) {
						_simpleMTable.SerializeTableToFile(_modelTableSerializationFile);
					}
				}
			}
		}
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
