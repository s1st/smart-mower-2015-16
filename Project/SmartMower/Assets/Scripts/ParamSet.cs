using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

/// <summary>
/// Class providing various parameters for the project.
/// </summary>
/// <description>
/// The parameters are loaded from a given xml file.
/// </description>
public class ParamSet {

	/// <summary>
	/// Invalid ParamSet XML file exception.
	/// </summary>
	[Serializable]
	public class InvalidParamSetXMLException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="T:InvalidParamSetXMLException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public InvalidParamSetXMLException (string message) : base (message) {

		}

	}

	//--The names of all available parameters in the parameter set --//

	public static readonly string ParamGroupSetupManager = "SetupManager";
	public static readonly string ParamShowUI = "ShowUI";
	public static readonly string ParamAutoRun = "AutoRun";
    public static readonly string ParamShow = "ShowParamLabel";


    public static readonly string ParamGroupEpisodeManager = "EpisodeManager";
	public static readonly string ParamEpisodeLimit = "EpisodeLimit";

	public static readonly string ParamGroupStatisticsWriter = "StaticsticsWriter";
	public static readonly string ParamStatisticsFileName = "StatisticsFileName";

	public static readonly string ParamGroupMower = "Mower";
	public static readonly string ParamLearnerType = "LearnerType";
	public static readonly string ParamMowReward = "MowReward";
	public static readonly string ParamNotMownReward = "NotMownReward";
    public static readonly string ParamEligTraces = "EligibityTraces";
    public static readonly string ParamGamma = "Gamma";
	public static readonly string ParamEligibilityTableDeserialiaztionFile = "EligibilityTableDeserialiaztionFile";
	public static readonly string ParamEligibilityTableSerialiaztionFile = "EligibilityTableSerialiaztionFile";
    public static readonly string ParamModelPlanning = "ModelPlanning";
    public static readonly string ParamModelPlanningRefined = "Refined";
	public static readonly string ParamModelTableDeserialiaztionFile = "ModelTableDeserialiaztionFile";
	public static readonly string ParamModelTableSerialiaztionFile = "ModelTableSerialiaztionFile";
    public static readonly string ParamN = "N";


    public static readonly string ParamGroupQLearner = "QLearner";
	public static readonly string ParamGreediness = "Greediness";
	public static readonly string ParamLearnRate = "LearnRate";
	public static readonly string ParamInitialQValue = "InitialQValue";
	public static readonly string ParamDiscountValue = "DiscountValue";
	public static readonly string ParamQTableDeserialiaztionFile = "QTableDeserialiaztionFile";
	public static readonly string ParamQTableSerialiaztionFile = "QTableSerialiaztionFile";


	private static readonly string topLevelNode = "Params";

	/// <summary>
	/// The in-memory version of the parameter xml file.
	/// </summary>
	private XmlDocument paramDocument;


	/// <summary>
	/// Initializes a new instance of the <see cref="ParamSet"/> class using the given xml file.
	/// </summary>
	/// <description>
	/// The referenced xml file is expected to have the following strucutre:
	/// <Params>
	/// 	<#GroupName1#>
	/// 		<#ParamName1#> param value </#ParamName1#>
	/// 		<#ParamName2#> param value </#ParamName2#>
	/// 		...
	/// 	</#GroupName1#>
	/// 	...
	/// </Params>
	/// </description>
	/// <param name="xmlFilename">Filename of the xml file containing the parameters.</param>
	public ParamSet (string xmlFilename) {
		// Init the xml document
		paramDocument = new XmlDocument();
		paramDocument.Load(xmlFilename);
	}

	/// <summary>
	/// Fetches a boolean parameter.
	/// </summary>
	/// <returns>The boolean parameter.</returns>
	/// <param name="paramGroup">The name of the parameter group the wanted parameter belongs to.</param>
	/// <param name="paramName">The name of the wanted parameter itself.</param>
	public bool BoolParam(string paramGroup, string paramName) {
		bool param;
		string paramAsString = Param(paramGroup, paramName);
		bool parseResult = bool.TryParse(paramAsString, out param);
		if (!parseResult) {
			throw ExceptionForWrongDatatype(paramGroup, paramName, "bool");
		} else {
			return param;
		}
	}

	/// <summary>
	/// Fetches an integer parameter.
	/// </summary>
	/// <returns>The integer parameter.</returns>
	/// <param name="paramGroup">The name of the parameter group the wanted parameter belongs to.</param>
	/// <param name="paramName">The name of the wanted parameter itself.</param>
	public int IntParam(string paramGroup, string paramName) {
		int param;
		string paramAsString = Param(paramGroup, paramName);
		bool parseResult = int.TryParse(paramAsString, out param);
		if (!parseResult) {
			throw ExceptionForWrongDatatype(paramGroup, paramName, "int");
		} else {
			return param;
		}
	}

	/// <summary>
	/// Fetches a float parameter.
	/// </summary>
	/// <returns>The float parameter.</returns>
	/// <param name="paramGroup">The name of the parameter group the wanted parameter belongs to.</param>
	/// <param name="paramName">The name of the wanted parameter itself.</param>
	public float FloatParam(string paramGroup, string paramName) {
		float param;
		string paramAsString = Param(paramGroup, paramName);
		bool parseResult = float.TryParse(paramAsString,NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat,  out param);
		if (!parseResult) {
			throw ExceptionForWrongDatatype(paramGroup, paramName, "float");
		} else {
			return param;
		}
	}

	/// <summary>
	/// Fetches a string parameter.
	/// </summary>
	/// <returns>The string parameter.</returns>
	/// <param name="paramGroup">The name of the parameter group the wanted parameter belongs to.</param>
	/// <param name="paramName">The name of the wanted parameter itself.</param>
	public string StringParam(string paramGroup, string paramName) {
		return Param(paramGroup, paramName);
	}


	private string Param (string paramGroup, string paramName) {
		XmlNode node = paramDocument.SelectSingleNode("/" + topLevelNode + "/" + paramGroup + "/" + paramName);
		if (node == null) {
			throw ExceptionForMissingParam(paramGroup, paramName);
		} else {
			return node.InnerText;
		}
	}

	/// <summary>
	/// Convenience method to create an exception due to a missing parameter in the XML file.
	/// </summary>
	/// <returns>The missing-parameter exception.</returns>
	/// <param name="paramGroup">The parameter group that was expected to contain the desired parameter.</param>
	/// <param name="paramName">The desired parameter.</param>
	private InvalidParamSetXMLException ExceptionForMissingParam(string paramGroup, string paramName) {
		return new InvalidParamSetXMLException("Param \"" + paramGroup + "/" + paramName + "\" not found.");
	}

	/// <summary>
	/// Convenience method to create an exception due to the wrong datatype of a parameter in the XML file.
	/// </summary>
	/// <returns>The wrong-datatype exception.</returns>
	/// <param name="paramGroup">The parameter group that contained the parameter with a wrong datatype.</param>
	/// <param name="paramName">The parameter with the wrong datatype.</param>
	/// <param name="expectedDataType">The data type that was expected.</param>
	private InvalidParamSetXMLException ExceptionForWrongDatatype(string paramGroup, string paramName, string expectedDataType) {
		return new InvalidParamSetXMLException("Param \"" + paramGroup + "/" + paramName + "\" is of wrong datatype. (Expected: " + expectedDataType + ").");
	}

}
