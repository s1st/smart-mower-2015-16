using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridPosition = Garden.GridPosition;

/// <summary>
/// The Mower robot inside the garden.
/// </summary>
public class Mower : MovingGardenObject, EpisodeManager.IEpisodeChangesReceiver
{
    /// <summary>
	/// Interface for observers of the mower robot.
	/// TODO: Extend, when needed (e.g. notification when the mower "scores").
	/// </summary>
	public interface IMowerObserver {

		/// <summary>
		/// Notification that the mower moved from one position to another.
		/// </summary>
		/// <param name="mower">The mower.</param>
		/// <param name="oldPosition">Its old position.</param>
		/// <param name="newPosition">Its new position.</param>
		void MowerMoved(Mower mower, GridPosition oldPosition, GridPosition newPosition);
	}

	/// <summary>
	/// The garden the mower is operating in.
	/// </summary>
    private Garden garden;

	/// <summary>
	/// The number of remaining tiles to mow.
	/// </summary>
	private uint remainingTilesToMow;

	/// <summary>
	/// The learner used to control the mower.
	/// </summary>
	private Learner learner;

	/// <summary>
	/// The reward given for successfully mowing one tile of the garden.
	/// </summary>
	private float mowReward = 10.0f;

	/// <summary>
	/// The rewatd given for moving onto a tile already mown.
	/// </summary>
	private float notMownReward = -1.0f;

	/// <summary>
	/// The list of mower observers.
	/// </summary>
	private List<IMowerObserver> observers = new List<IMowerObserver>();

	/// <summary>
	/// Initializes a new instance of the <see cref="Mower"/> class.
	/// </summary>
	/// <param name="x">The x coordinate of the start position.</param>
	/// <param name="y">The y coordinate of the start position.</param>
	/// <param name="garden">The <see cref="T:Garden"/> this mower operates in.</param>
	/// <param name="paramSet">The <see cref="T:ParamSet"/> of the project which contains the neccessary parameters for the mower.</param>
	public Mower(uint x, uint y, Garden garden, ParamSet paramSet)
		: base (x,y) {
		this.Garden = garden;
		this.PossibleMovements = MovementAction.VonNeumannNeighbourhoodMovements();
		this.MowReward = paramSet.FloatParam(ParamSet.ParamGroupMower, ParamSet.ParamMowReward);
		this.NotMownReward = paramSet.FloatParam(ParamSet.ParamGroupMower, ParamSet.ParamNotMownReward);
		string learnerParam = paramSet.StringParam(ParamSet.ParamGroupMower, 
            ParamSet.ParamLearnerType);
		if (learnerParam.Equals(QLearner.LearnerType)) {
			this.learner = new QLearner(new StateExtractor(this), this, paramSet);
		} else if (learnerParam.Equals(RandomLearner.LearnerType)) {
			this.learner = new RandomLearner(new StateExtractor(this));
		} else {
			throw new Exception("Unknown LearnerType detected.");
		}
		EpisodeManager.AddEpisodeChangesReceiver(this);

    }

	/// <summary>
	/// Initializes a new instance of the <see cref="Mower"/> class.
	/// </summary>
	/// <param name="position">The start position.</param>
	/// <param name="garden">The <see cref="T:Garden"/> this mower operates in.</param>
	/// <param name="paramSet">The <see cref="T:ParamSet"/> of the project which contains the neccessary parameters for the mower.</param>
	public Mower(GridPosition position, Garden garden, ParamSet paramSet)
		: this((uint)position.X, (uint)position.Y, garden, paramSet) {

	}



	public Garden Garden
	{
		get
		{
			return garden;
		}
		private set {
			garden = value;
			remainingTilesToMow = garden.NumGrassTiles;
		}
	}

	public uint RemainingTilesToMow {
		get {
			return remainingTilesToMow;
		}
		private set {
			remainingTilesToMow = value;
		}
	}

	public float MowReward {
		get {
			return mowReward;
		}
		private set {
			mowReward = value;
		}
	}

	public float NotMownReward {
		get {
			return notMownReward;
		}
		private set {
			notMownReward = value;
		}
	}

	public bool MowingFinished {
		get {
			return remainingTilesToMow == 0;
		}
	}

	/// <summary>
	/// Performs one action from the available actions.
	/// </summary>
	/// <param name="availableActions">All available actions. Must not be null or empty.</param>
	public override void PerformMovementActionFromAvailableActions (List<MovementAction> availableActions) {
		// Ask the magic oracle what action to perform.
		MovementAction chosenMovement = learner.OptimalAction(availableActions);
		PerformMovement(chosenMovement);
	}
		

	/// <summary>
	/// Registers a new observer of the mower.
	/// </summary>
	/// <returns><c>true</c>, if the observer was added, <c>false</c> otherwise.</returns>
	/// <param name="observer">The new observer to register.</param>
	public bool AddObserver (IMowerObserver observer) {
		if (! observers.Contains(observer)) {
			observers.Add(observer);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Removes the given observer from the list of ovservers.
	/// </summary>
	/// <returns><c>true</c>, if the observer was removed, <c>false</c> otherwise.</returns>
	/// <param name="observer">The observer to remove.</param>
	public bool RemoveObserver(IMowerObserver observer) {
		if (observers.Contains(observer)) {
			observers.Remove(observer);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Performs the given movement action.
	/// </summary>
	/// <param name="movementAction">The Movement action to perform.</param>
	private void PerformMovement(MovementAction movementAction) {
		GridPosition oldPosition = this.Position;
		this.Position = this.Position + movementAction.GridMovement;
		float reward = Mow();
        learner.Learn(reward);
		foreach (var observer in observers) {
			observer.MowerMoved(this, oldPosition, this.Position);
		}
    }

	/// <summary>
	/// Resets the state of the object to reflect the beginning of a new episode.
	/// </summary>
	protected override void ResetToInitialState() {
		GridPosition oldPosition = Position;
		base.ResetToInitialState();
		foreach (var observer in observers) {
			observer.MowerMoved(this, oldPosition, Position);
		}
		RemainingTilesToMow = Garden.NumGrassTiles;
	}

	/// <summary>
	/// Notification that the current episode will end.
	/// </summary>
	/// <param name="episode">The current episode to end.</param>
	public void EpisodeWillEnd(uint episode) {
		// Do nothing.
	}

	/// <summary>
	/// Notification that the current episode did end.
	/// </summary>
	/// <param name="episode">The episode that ended.</param>
	public void EpisodeDidEnd(uint episode) {
		ResetToInitialState();
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

	/// <summary>
	/// Mow at the current position of the mower.
	/// </summary>
	/// <returns>The reward based on whether the mowing was successful or not.</returns>
    private float Mow()
    {
        float reward = 0f;
        if (Garden.GetTileStatus(this.Position) == Tile.MowStatus.LongGrass) {
			Garden.SetTileStatus(this.Position, Tile.MowStatus.ShortGrass);
			RemainingTilesToMow --;
            reward = MowReward;
		}
        else
        {
            reward = NotMownReward;   
        }
		return reward;
    }
}

