using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

/// <summary>
/// Table containing the state-action values needed for QLearning.
/// </summary>
public class QTable
{
    /// <summary>
    /// Inital opimistic values.
    /// </summary>
    private float initialValue;

	/// <summary>
	/// The currently possible actions.
	/// Needed for DynaQ.
	/// </summary>
    private List<MovementAction> curActions;

    /// <summary>
    /// The QTable holds all actions in each state of the mower.
    /// The float value is the expected accumulated reward of a action in a
    /// certain state and is updated permanently.
    /// </summary>
    private Dictionary<StateActionPair, float> _qTable;

    public List<MovementAction> CurActions
    {
        get
        {
            return curActions;
        }

        set
        {
            curActions = value;
        }
    }

	/// <summary>
	/// Initializes a new instance of the <see cref="QTable"/> class.
	/// </summary>
	/// <param name="initValue">The inital value for all table entries.</param>
	/// <param name="deserializeFromFile">The file where this <see cref="T:QTable"/> should be deserialized from. Can be null.</param>
	public QTable(float initValue=0f, string deserializeFromFile=null)
    {
        initialValue = initValue;
		_qTable = new Dictionary<StateActionPair, float>();
		if (deserializeFromFile != null && deserializeFromFile.Length > 0) {
			DeserializeTableFromFile(deserializeFromFile);
		}
    }

	/// <summary>
	/// Sets the Q-value for the given <see cref="T:StateActionPair"/>.
	/// </summary>
	/// <param name="saPair">The <see cref="T:StateActionPair"/>.</param>
	/// <param name="reward">The Q-value (expected accumulated reward) to set.</param>
    public void setQValue(StateActionPair saPair, float reward)
    {
        _qTable[saPair] = reward;
    }

	/// <summary>
	/// Returns the Q-value.
	/// </summary>
	/// <returns>The Q-value.</returns>
	/// <param name="saPair">The <see cref="T:StateActionPair"/> whose Q-value is returned.</param>
    public float getQValue(StateActionPair saPair)
    {
        if (!_qTable.ContainsKey(saPair))
        {
            setQValue(saPair, initialValue);
        }
        return _qTable[saPair];
    }

    /// <summary>
    /// Helper function to get all movements for one state.
    /// TODO maybe optimize: iterating through all keys might not be very fast
	/// Needed for DynaQ.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public List<MovementAction> getAllMovementsForState(State state)
    {
        // This list may not be null,
        // otherwise initially, the mower does not know where to move
        List<MovementAction> movements = new List<MovementAction>();
        Dictionary<StateActionPair, float>.KeyCollection keys = _qTable.Keys;
        foreach (StateActionPair sap in keys)
        {
            if(sap.state == state)
            {
                movements.Add(sap.action);
            }
        }
        return movements;
    }

	/// <summary>
	/// Returns the best action (with highest expected reward) for the given state.
	/// </summary>
	/// <returns>The best action to take in the given state.</returns>
	/// <param name="state">The given state.</param>
    public MovementAction getBestActionForState(State state)
    {
        float bestReward = float.MinValue;
        MovementAction bestAction = null;

        foreach (MovementAction action in CurActions)
        {
            StateActionPair sap = new StateActionPair(state, action);
            float reward = this.getQValue(sap);
            if (bestAction == null || reward > bestReward)
            {
                bestAction = action;
                bestReward = reward;
            }
        }
        return bestAction;
    }

	/// <summary>
	/// Adding all scaled eligibility values to the QTable.
	/// Needed for QLearning with eligibility traces.
	/// </summary>
	/// <param name="scale">The scale to apply to all eligibility traces before adding them.</param>
	/// <param name="eTable">The <see cref="T:EligibilityTable"/> containing all eligibility values.</param>
    public void AddScaledValues(float scale, EligibilityTable eTable)
    {
        var qTableCopy = new Dictionary<StateActionPair, float>(_qTable);
        foreach (var qValPair in _qTable)
        {
            qTableCopy[qValPair.Key] = qValPair.Value + eTable.GetEligibilityValue(qValPair.Key) * scale;
        }
        _qTable = qTableCopy;
    }

    /// <summary>
    /// Returns a random state, that has already been visited.
	/// Needed for DynaQ.
    /// </summary>
    public StateActionPair getRandomVisitedStateAndAction()
    {
        while (true)
        {
            int randIndex = UnityEngine.Random.Range(0, _qTable.Count);
            KeyValuePair<StateActionPair, float> k = _qTable.ElementAt(randIndex);
            // If the reward is the initial Value, the State has
            // never been visited
            if (k.Value != initialValue)
            {
                return k.Key;
            }
        }
    } 

	/// <summary>
	/// Serializes the main content of the table to the given file.
	/// </summary>
	/// <param name="fileName">The name of the file to serialive to.</param>
	public void SerializeTableToFile(string fileName) 
	{
		List<QTableKeyValuePair> entries = new List<QTableKeyValuePair>(_qTable.Count);
		foreach (var key in _qTable.Keys) {
			entries.Add(new QTableKeyValuePair(key, _qTable[key]));
		}
			
		XmlSerializer serializer = new XmlSerializer(typeof(List<QTableKeyValuePair>));
		FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write); 
		serializer.Serialize(fs, entries); 
		fs.Close(); 
	}

	/// <summary>
	/// Deserializes the main content of the table from the given file.
	/// </summary>
	/// <param name="fileName">The name of the file to deserialize from.</param>
	private void DeserializeTableFromFile(string fileName)
	{
		FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read); 
		XmlSerializer serializer = new XmlSerializer(typeof(List<QTableKeyValuePair>));
		List<QTableKeyValuePair> list = (List<QTableKeyValuePair>)serializer.Deserialize(fs);
		_qTable.Clear();
		foreach (QTableKeyValuePair entry in list)
		{
			_qTable[entry.StateActionPairKey] = entry.FloatValue;
		}
	}

	/// <summary>
	/// Wrapper class describing a key-value pair of the <see cref="QTable"/>.
	/// Necessary for serialization, since dictionaries are not supported by XMLSerialization.
	/// </summary>
	public class QTableKeyValuePair 
	{
		/// <summary>
		/// Gets or sets the state action pair key.
		/// </summary>
		/// <value>The state action pair key.</value>
		public StateActionPair StateActionPairKey {get;set;}

		/// <summary>
		/// Gets or sets the float value.
		/// </summary>
		/// <value>The float value.</value>
		public float FloatValue {get;set;}

		/// <summary>
		/// Default empty constructor.
		/// Initializes a new instance of the <see cref="QTable+QTableKeyValuePair"/> class.
		/// Necessary for serialization.
		/// </summary>
		public QTableKeyValuePair() {}

		/// <summary>
		/// Initializes a new instance of the <see cref="QTable+QTableKeyValuePair"/> class.
		/// </summary>
		/// <param name="saPair">The <see cref="T:StateExtractor+StateActionPair"/> key.</param>
		/// <param name="floatValue">The float value.</param>
		public QTableKeyValuePair(StateActionPair stateActionPair, float floatValue) 
		{
			StateActionPairKey = stateActionPair;
			FloatValue = floatValue;
		}
	}

}
