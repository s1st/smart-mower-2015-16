using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

/// <summary>
/// This Class holds the table the refined model
/// </summary>

class ModelTable : EpisodeManager.IEpisodeChangesReceiver
{
	/// <summary>
	/// Simple subclass of <see cref="T:StateExtractor"/> to extract states from a garden model inside a <see cref="T:ModelTable"/>.
	/// </summary>
	protected class ModelStateExtractor : StateExtractor {

		/// <summary>
		/// Private placeholder constructor.
		/// </summary>
		/// <param name="mower">Mower.</param>
		private ModelStateExtractor(Mower mower)
			: base(mower)
		{
			
		}

		/// <summary>
		/// Extracts the state from the garden model at the given position.
		/// </summary>
		/// <param name="gardenModel">The garden model.</param>
		/// <param name="position">The position.</param>
		/// <returns>The extracted state.</returns>
		public static State ExtractStateFromGardenModelAtPosition(ModelTable gardenModel, Garden.GridPosition position) {
			return new State (
				gardenModel.GardenModelAtPosition(position + MovementAction.North().GridMovement),
				gardenModel.GardenModelAtPosition(position + MovementAction.NorthEast().GridMovement),
				gardenModel.GardenModelAtPosition(position + MovementAction.East().GridMovement),
				gardenModel.GardenModelAtPosition(position + MovementAction.SouthEast().GridMovement),
				gardenModel.GardenModelAtPosition(position + MovementAction.South().GridMovement),
				gardenModel.GardenModelAtPosition(position + MovementAction.SouthWest().GridMovement),
				gardenModel.GardenModelAtPosition(position + MovementAction.West().GridMovement),
				gardenModel.GardenModelAtPosition(position + MovementAction.NorthWest().GridMovement),
				position
			);
		}
	}

	/// <summary>
	/// The width of the garden model.
	/// </summary>
	private uint gardenWidth;

	/// <summary>
	/// The height of the garden model.
	/// </summary>
	private uint gardenHeight;

	/// <summary>
	/// The garden model itself.
	/// </summary>
	private List<Tile.MowStatus> gardenModel;

	/// <summary>
	/// The movements available to the mower.
	/// </summary>
	private List<MovementAction> mowerMovements;

	/// <summary>
	/// The reward received for successfully mowing.
	/// </summary>
	private float mowReward;

	/// <summary>
	/// The reward for moving on an empty garden tile.
	/// </summary>
	private float notMownReward;

	/// <summary>
	/// Initializes a new instance of the <see cref="ModelTable"/> class as an internal model of the garden.
	/// </summary>
	/// <param name="gardenWidth">The garden width.</param>
	/// <param name="gardenHeight">The garden height.</param>
	/// <param name="mowerMovements">All movements the mower can perform.</param>
	/// <param name="mowReward">The reward granted for successfully mowing one tile of the garden.</param>
	/// <param name="notMownReward">The reward granted for moving onto a garden tile without grass to mow.</param>
	/// <param name="deserializeFromFile">The file where this <see cref="T:ModelTable"/> should be deserialized from. Can be null.</param>
	public ModelTable(uint gardenWidth, uint gardenHeight, List<MovementAction> mowerMovements, float mowReward, float notMownReward, string deserializeFromFile=null)
    {
		InitGardenModel(gardenWidth, gardenHeight);
		this.mowerMovements = new List<MovementAction>(mowerMovements);
		this.mowReward = mowReward;
		this.notMownReward = notMownReward;
		if (deserializeFromFile != null && deserializeFromFile.Length > 0) {
			DeserializeTableFromFile(deserializeFromFile);
		}
		EpisodeManager.AddEpisodeChangesReceiver(this);
    }

	/// <summary>
	/// Initializes a new instance of the <see cref="ModelTable"/> class.
	/// </summary>
	/// <description>
	/// Wrapper constructor. The given mower contains all information neccessary to initialize the model.
	/// </description>
	/// <param name="mower">The <see cref="T:Mower"/> mower whose observations will refine this model.</param>
	/// <param name="deserializeFromFile">The file where this <see cref="T:ModelTable"/> should be deserialized from. Can be null.</param>
	public ModelTable(Mower mower, string deserializeFromFile=null) 
		: this(mower.Garden.GardenWidth, mower.Garden.GardenHeigth, mower.PossibleMovements, mower.MowReward, mower.NotMownReward, deserializeFromFile)
	{
		
	}

	/// <summary>
	/// Incorporates the given state that was observed by the mower into the garden model.
	/// </summary>
	/// <param name="observedState">The state observed by the mower.</param>
	public void IncorporateObservedState(State observedState) {
		Garden.GridPosition mowerPosition = observedState.Position;
		SetGardenModelAtPosition(mowerPosition + MovementAction.North().GridMovement, observedState.NorthTileStatus);
		SetGardenModelAtPosition(mowerPosition + MovementAction.East().GridMovement, observedState.EastTileStatus);
		SetGardenModelAtPosition(mowerPosition + MovementAction.South().GridMovement, observedState.SouthTileStatus);
		SetGardenModelAtPosition(mowerPosition + MovementAction.West().GridMovement, observedState.WestTileStatus);
		SetGardenModelAtPosition(mowerPosition + MovementAction.NorthEast().GridMovement, observedState.NorthEastTileStatus);
		SetGardenModelAtPosition(mowerPosition + MovementAction.SouthEast().GridMovement, observedState.SouthEastTileStatus);
		SetGardenModelAtPosition(mowerPosition + MovementAction.SouthWest().GridMovement, observedState.SouthWestTileStatus);
		SetGardenModelAtPosition(mowerPosition + MovementAction.NorthWest().GridMovement, observedState.NorthWestTileStatus);
	}

	/// <summary>
	/// Generates a virtual step of a mower at a random position in the garden model.
	/// </summary>
	/// <description>
	/// The step is created by randomly choosing a position inside the garden as well as a random movement.
	/// The outcome of this action is determined by the current belief the garden model holds.
	/// It might happen, that the garden model does not have enough information to generate such a virtual step.
	/// Therefore, the return value of this method always has to be checked!
	/// </description>
	/// <returns><c>true</c>, if a random model step could be generated, <c>false</c> otherwise.</returns>
	/// <param name="fromState">The state the step originated.</param>
	/// <param name="performedAction">The action performed at the fromState.</param>
	/// <param name="toState">The state that is observed after the movement.</param>
	/// <param name="receivedReward">The received reward for this step.</param>
	/// <param name="numRetries">The number of retries to generate a virtual step, before this method aborts.</param>
	public bool GenerateRandomModelStep(out State fromState, out MovementAction performedAction, out State toState, out float receivedReward, uint numRetries=1)
	{
		fromState = null;
		toState = null;
		performedAction = null;
		receivedReward = 0f;
		uint retries = 0;
		bool stepGenerated = false;
		while (retries <= numRetries) {
			// Select random position inside garden model
			int randX = UnityEngine.Random.Range(0, (int)gardenWidth);
			int randY = UnityEngine.Random.Range(0, (int)gardenHeight);
			Garden.GridPosition virtualPosition = new Garden.GridPosition(randX, randY);
			fromState = ModelTable.ModelStateExtractor.ExtractStateFromGardenModelAtPosition(this, virtualPosition);
			// Check which positions can be reached from here
			List<MovementAction> possibleMovements = new List<MovementAction>(mowerMovements.Count);
			foreach (var movement in mowerMovements) {
				Garden.GridPosition resultingPosition = virtualPosition + movement.GridMovement;
				if (GardenModelAtPosition(resultingPosition) != Tile.MowStatus.Obstacle) {
					possibleMovements.Add(movement);
				}
			}
			if (possibleMovements.Count == 0) {
				retries ++;
				continue;
			} else {
				// Select random movement from the possible ones
				performedAction = possibleMovements[UnityEngine.Random.Range(0, possibleMovements.Count)];
				Garden.GridPosition resultingPosition = virtualPosition + performedAction.GridMovement;
				receivedReward = (GardenModelAtPosition(resultingPosition) == Tile.MowStatus.LongGrass) ? mowReward : notMownReward;
				// Predict new state after the movement
				toState = ModelTable.ModelStateExtractor.ExtractStateFromGardenModelAtPosition(this, resultingPosition);
				stepGenerated = true;
				break;
			}
		}
		return stepGenerated;
	}

	/// <summary>
	/// Initialises the garden model.
	/// </summary>
	/// <description>
	/// The initial belief for the garden model states that the complete garden consitst of long grass.
	/// </description>
	/// <param name="gardenWidth">The garden width.</param>
	/// <param name="gardenHeight">The garden height.</param>
	private void InitGardenModel(uint gardenWidth, uint gardenHeight)
	{
		this.gardenWidth = gardenWidth;
		this.gardenHeight = gardenHeight;
		gardenModel = new List<Tile.MowStatus>((int)(this.gardenWidth * this.gardenHeight));
		for (int i = 0; i < (int)(this.gardenWidth * this.gardenHeight); i++) {
			gardenModel.Add(Tile.MowStatus.LongGrass);	
		}
	}

	/// <summary>
	/// Returns the current belief of the <see cref="T:Tile.MowStatus"/> at the given position in the garden model.
	/// </summary>
	/// <returns>The position in the garden model.</returns>
	/// <param name="position">Position.</param>
	public Tile.MowStatus GardenModelAtPosition(Garden.GridPosition position) 
	{
		int index = position.Y*(int)gardenWidth + position.X;
		if (IsValidPositionInModel(position) && index < gardenModel.Count) {
			return gardenModel[index];
		}
		return Tile.MowStatus.Obstacle;
	}

	/// <summary>
	/// Sets the garden model at the given position.
	/// </summary>
	/// <returns><c>true</c>, if the garden model at position could be set, <c>false</c> otherwise.</returns>
	/// <param name="position">The position.</param>
	/// <param name="tileStatus">The tile status to set at the given position in the garden model.</param>
	private bool SetGardenModelAtPosition(Garden.GridPosition position, Tile.MowStatus tileStatus)
	{
		if (IsValidPositionInModel(position)) {
			int index = position.Y*(int)gardenWidth + position.X;
			gardenModel[index] = tileStatus;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Determines whether the given position is a valid position in the garden model.
	/// </summary>
	/// <returns><c>true</c> if this position is valid; otherwise, <c>false</c>.</returns>
	/// <param name="position">The position to check.</param>
	private bool IsValidPositionInModel(Garden.GridPosition position) 
	{
		if (position.X >= gardenWidth || 
			position.X < 0 ||
			position.Y >= gardenHeight ||
			position.Y < 0) {
			return false;
		} else {
			return true;
		}
	}

	/// <summary>
	/// Serializes the main content of the table to the given file.
	/// </summary>
	/// <param name="fileName">The name of the file to serialive to.</param>
	public void SerializeTableToFile(string fileName) {
		FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write); 
		XmlSerializer serializer = new XmlSerializer(typeof(List<Tile.MowStatus>));
		serializer.Serialize(fs, gardenModel); 
	}

	/// <summary>
	/// Deserializes the main content of the table from the given file.
	/// </summary>
	/// <param name="fileName">The name of the file to deserialize from.</param>
	public void DeserializeTableFromFile(string fileName) {
		FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read); 
		XmlSerializer serializer = new XmlSerializer(typeof(List<Tile.MowStatus>));
		gardenModel.Clear();
		gardenModel = (List<Tile.MowStatus>)serializer.Deserialize(fs);
	}

	/// <summary>
	/// Notification that the current episode will end.
	/// </summary>
	/// <param name="episode">The current episode to end.</param>
	public void EpisodeWillEnd(uint episode) {
		/* This is very important:
		** The knowledge that after each episode all grass tiles are reset to long grass,
		** is crucial knowledge for the garden model!
		*/
		for (int i = 0; i < (int)(this.gardenWidth * this.gardenHeight); i++) {
			if (gardenModel[i] == Tile.MowStatus.ShortGrass) {
				gardenModel[i] = Tile.MowStatus.LongGrass;
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

