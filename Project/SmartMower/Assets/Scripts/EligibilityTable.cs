using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

/// <summary>
/// Table representing eligibility traces as a mapping from <see cref="T:StateActionPair"/> objects to eligibility values.
/// </summary>
public class EligibilityTable
{
	/// <summary>
	/// The initial eligibility value of each <see cref="T:StateActionPair"/>.
	/// </summary>
    private float initialValue;

	/// <summary>
	/// The eligibility values itself. Mapping from <see cref="T:StateActionPair"/> to eligibility value.
	/// </summary>
    private Dictionary<StateActionPair, float> eligibilityTable;

	/// <summary>
	/// Initializes a new instance of the <see cref="EligibilityTable"/> class.
	/// </summary>
	/// <param name="initValue">The initial eligibility value of each <see cref="T:StateActionPair"/>. </param>
	/// <param name="deserializeFromFile">The file where this <see cref="T:QTable"/> should be deserialized from. Can be null.</param>
	public EligibilityTable(float initValue=0f, string deserializeFromFile=null)
    {
        initialValue = initValue;
        eligibilityTable = new Dictionary<StateActionPair, float>();
		if (deserializeFromFile != null && deserializeFromFile.Length > 0) {
			DeserializeTableFromFile(deserializeFromFile);
		}
    }

	/// <summary>
	/// Sets the eligibility value of the given <see cref="T:StateActionPair"/>.
	/// </summary>
	/// <param name="saPair">The <see cref="T:StateActionPair"/> whose eligibility value is set.</param>
	/// <param name="eligibility">The eligibility value to set.</param>
    public void SetEligibilityValue(StateActionPair saPair, float eligibility)
    {
		eligibilityTable[saPair] = eligibility;
    }

	/// <summary>
	/// Returns the eligibility value of the given <see cref="T:StateActionPair"/>.
	/// </summary>
	/// <returns>The eligibility value.</returns>
	/// <param name="saPair">The <see cref="T:StateActionPair"/> whose eligibility value is returned.</param>
    public float GetEligibilityValue(StateActionPair saPair)
    {
        if (!eligibilityTable.ContainsKey(saPair))
        {
            SetEligibilityValue(saPair, initialValue);
        }
        return eligibilityTable[saPair];
    }

	/// <summary>
	/// Scales the eligibility values of all <see cref="T:StateActionPair"/> objects in the table.
	/// </summary>
	/// <param name="scale">The scale to apply (multiplicative).</param>
    public void ScaleAllEligibilityValues(float scale)
    {
        var eligibilityTableCopy = new Dictionary<StateActionPair, float>(eligibilityTable);
        foreach (var eValPair in eligibilityTable)
        {
            eligibilityTableCopy[eValPair.Key] = eValPair.Value * scale;
        }
        eligibilityTable = eligibilityTableCopy;
    }

	/// <summary>
	/// Serializes the main content of the table to the given file.
	/// </summary>
	/// <param name="fileName">The name of the file to serialive to.</param>
	public void SerializeTableToFile(string fileName) 
	{
		List<EligibilityTableKeyValuePair> entries = new List<EligibilityTableKeyValuePair>(eligibilityTable.Count);
		foreach (var key in eligibilityTable.Keys) {
			entries.Add(new EligibilityTableKeyValuePair(key, eligibilityTable[key]));
		}

		XmlSerializer serializer = new XmlSerializer(typeof(List<EligibilityTableKeyValuePair>));
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
		XmlSerializer serializer = new XmlSerializer(typeof(List<EligibilityTableKeyValuePair>));
		List<EligibilityTableKeyValuePair> list = (List<EligibilityTableKeyValuePair>)serializer.Deserialize(fs);
		eligibilityTable.Clear();
		foreach (EligibilityTableKeyValuePair entry in list)
		{
			eligibilityTable[entry.StateActionPairKey] = entry.FloatValue;
		}
	}

	/// <summary>
	/// Wrapper class describing a key-value pair of the <see cref="T:EligibilityTable"/>.
	/// Necessary for serialization, since dictionaries are not supported by XMLSerialization.
	/// </summary>
	public class EligibilityTableKeyValuePair : QTable.QTableKeyValuePair 
	{
		/// <summary>
		/// Default empty constructor.
		/// Initializes a new instance of the <see cref="T:EligibilityTable+EligibilityTableKeyValuePair"/> class.
		/// Necessary for serialization.
		/// </summary>
		public EligibilityTableKeyValuePair() 
			: base()
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:EligibilityTable+EligibilityTableKeyValuePair"/> class.
		/// </summary>
		/// <param name="saPair">The <see cref="T:StateExtractor+StateActionPair"/> key.</param>
		/// <param name="floatValue">The float value.</param>
		public EligibilityTableKeyValuePair(StateActionPair stateActionPair, float floatValue) 
			:base(stateActionPair, floatValue)
		{

		}
	}
		

}

