using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Inital starting point,
/// where all the initialisation happens.
/// </summary>
public class SetupManager : MonoBehaviour {

	/// <summary>
	/// The project parameter set.
	/// </summary>
	private ParamSet projectParamSet;

	/// <summary>
	/// Whether UI is displayed or not.
	/// </summary>
	private bool showUI;

	/// <summary>
	/// Whether the simulation starts right away or not.
	/// </summary>
	private bool autoRun;

	/// <summary>
	/// The garden world.
	/// </summary>
	private Garden garden;

	/// <summary>
	/// The statistics writer which writes the learning progress to a file.
	/// </summary>
	private StatisticsWriter statisticsWriter;

	/// <summary>
	/// The UI representation of the garden world.
	/// </summary>
	public GuiGarden guiGarden;
    
	/// <summary>
	/// Used to setup all major components in the project.
	/// </summary>
    void Awake() {

		UnityEngine.Random.seed = 42;

		string gardenWorldXML;
		string projectParamsXML;
		string [] argv = Environment.GetCommandLineArgs();
		if (argv.Length < 3) {
			// Files used during testing inside the Unity Editor
			gardenWorldXML = Path.Combine("..", "experiment_garden.xml");
			projectParamsXML = Path.Combine("..", "smartmower_params.xml");
		} else {
			// Use filenames from command line
			gardenWorldXML = argv[1];
			projectParamsXML = argv[2];
		}
		// Load parameters
		projectParamSet = new ParamSet(projectParamsXML);
		showUI = projectParamSet.BoolParam(ParamSet.ParamGroupSetupManager, ParamSet.ParamShowUI);
		autoRun = projectParamSet.BoolParam(ParamSet.ParamGroupSetupManager, ParamSet.ParamAutoRun);
		// Create garden
		garden = GardenFactory.CreateGardenFromFile(gardenWorldXML, projectParamSet);
        
		// Handle UI
		if (showUI) {
			guiGarden.Init(garden);
		} else {
			guiGarden.gameObject.SetActive(false);
			GameObject.Find("EpisodeLabel").SetActive(false);
			GameObject.Find("StepLabel").SetActive(false);
		}

		// Set episode limit
		EpisodeManager.EpisodeLimit = projectParamSet.IntParam(ParamSet.ParamGroupEpisodeManager, ParamSet.ParamEpisodeLimit);

		// Create statistics writer
		this.statisticsWriter = new StatisticsWriter(garden, projectParamSet);
    }

    /// <summary>
    /// Starts the project, if the parameters command it to.
    /// </summary>
    void Start () {

		// Handle the auto run param.
		bool autoRun = projectParamSet.BoolParam(ParamSet.ParamGroupSetupManager, ParamSet.ParamAutoRun);
		if (autoRun) {
			GameObject.FindObjectOfType<RunManager>().StartStopButtonFunctionality = RunManager.StartStopFunctionality.StopFuntionality;
			GameObject stepGenGameObject = GameObject.FindGameObjectWithTag("StepGenerator");
			StepGenerator stepGen = stepGenGameObject.GetComponent<StepGenerator>();
			stepGen.StartSteps();
		} else {
			GameObject.FindObjectOfType<RunManager>().StartStopButtonFunctionality = RunManager.StartStopFunctionality.StartFunctionality;
		}
			
	}
	

}
