using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// This Class holds the table for building a Dyna-Q-based model.
/// The table will be updated inside the normal QLearning algoritm.
/// Slows down the process: N < 500 for 50 episodes takes more than 3 hours.
/// With smaller N, the results are not very promising.
/// For more theoretical information see Lecture 11, p. 13 ff.
/// </summary>

public class ModelTableSimple
{
    float _initVal;
    private Dictionary<StateActionPair, StateRewardPair> _modelTable;

	public ModelTableSimple(float initVal=0f, string deserializeFromFile=null)
    {
        _initVal = initVal;
        _modelTable = new Dictionary<StateActionPair, StateRewardPair>();
		if (deserializeFromFile != null && deserializeFromFile.Length > 0) {
			DeserializeTableFromFile(deserializeFromFile);
		}
    }
		

    /// <summary>
    /// Use this method to insert the next state plus the reward
    /// at the current StateActionPair position
    /// </summary>
    /// <param name="saPair"></param>
    /// <param name="stateReward"></param>

    public void setStateRewardPairAtStateActionPair(StateActionPair saPair, StateRewardPair stateReward)
    {
        _modelTable[saPair] = stateReward;
    }

    public StateRewardPair getStateRewardPair(StateActionPair saPair)
    {
        if (!_modelTable.ContainsKey(saPair))
        {
            // If the StateRewardPair is not yet in the table, 
            // it has to be saved.
            // Therefore the possible next following state has to be saevd in a new
            // StateRewardPair.
            // To do so, all possible Movements for the next state have to extracted
            // -> new method in StateExtractor, that retrieves the possible Movements
            // for any State or Position.
            // Independently of the mower!
            _modelTable[saPair] = new StateRewardPair(saPair.state, _initVal);
        }
        return _modelTable[saPair];
    }


	/// <summary>
	/// Serializes the main content of the table to the given file.
	/// </summary>
	/// <param name="fileName">The name of the file to serialive to.</param>
	public void SerializeTableToFile(string fileName) 
	{
		List<ModelTableSimpleKeyValuePair> entries = new List<ModelTableSimpleKeyValuePair>(_modelTable.Count);
		foreach (var key in _modelTable.Keys) {
			entries.Add(new ModelTableSimpleKeyValuePair(key, _modelTable[key]));
		}

		FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write); 
		XmlSerializer serializer = new XmlSerializer(typeof(List<ModelTableSimpleKeyValuePair>));
		serializer.Serialize(fs, entries); 
	}

	/// <summary>
	/// Deserializes the main content of the table from the given file.
	/// </summary>
	/// <param name="fileName">The name of the file to deserialize from.</param>
	private void DeserializeTableFromFile(string fileName)
	{
		FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read); 
		XmlSerializer serializer = new XmlSerializer(typeof(List<ModelTableSimpleKeyValuePair>));
		List<ModelTableSimpleKeyValuePair> list = (List<ModelTableSimpleKeyValuePair>)serializer.Deserialize(fs);
		_modelTable.Clear();
		foreach (ModelTableSimpleKeyValuePair entry in list)
		{
			_modelTable[entry.StateActionPairKey] = entry.StateRewardPairValue;
		}
	}

	/// <summary>
	/// Wrapper class describing a key-value pair of the <see cref="ModelTableSimple"/>.
	/// Necessary for serialization, since dictionaries are not supported by XMLSerialization.
	/// </summary>
	public class ModelTableSimpleKeyValuePair 
	{
		/// <summary>
		/// Gets or sets the state action pair key.
		/// </summary>
		/// <value>The state action pair key.</value>
		public StateActionPair StateActionPairKey {get;set;}

		/// <summary>
		/// Gets or sets the state reward pair value.
		/// </summary>
		/// <value>The state reward pair value.</value>
		public StateRewardPair StateRewardPairValue {get;set;}

		/// <summary>
		/// Default empty constructur.
		/// Initializes a new instance of the <see cref="ModelTableSimple+ModelTableSimpleKeyValuePair"/> class.
		/// Necessary for serialization.
		/// </summary>
		public ModelTableSimpleKeyValuePair() 
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModelTableSimple+ModelTableSimpleKeyValuePair"/> class.
		/// </summary>
		/// <param name="stateActionPairKey">The state action pair key.</param>
		/// <param name="stateRewardPairValue">The state reward pair value.</param>
		public ModelTableSimpleKeyValuePair(StateActionPair stateActionPairKey, StateRewardPair stateRewardPairValue) 
		{
			StateActionPairKey = stateActionPairKey;
			StateRewardPairValue = stateRewardPairValue;
		}
	}
}

