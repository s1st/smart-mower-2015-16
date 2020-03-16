using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the state, how the mower perceive its inner state and the environment.
/// </summary>
public class State {
	/// <summary>
	/// Gets or sets the north tile status.
	/// Public for serialization.
	/// </summary>
	/// <value>The north tile status.</value>
	public Tile.MowStatus NorthTileStatus {get;set;}
	/// <summary>
	/// Gets or sets the north east tile status.
	/// Public for serialization.
	/// </summary>
	/// <value>The north east tile status.</value>
	public Tile.MowStatus NorthEastTileStatus {get;set;}
	/// <summary>
	/// Gets or sets the east tile status.
	/// Public for serialization.
	/// </summary>
	/// <value>The east tile status.</value>
	public Tile.MowStatus EastTileStatus {get;set;}
	/// <summary>
	/// Gets or sets the south east tile status.
	/// Public for serialization.
	/// </summary>
	/// <value>The south east tile status.</value>
	public Tile.MowStatus SouthEastTileStatus {get;set;}
	/// <summary>
	/// Gets or sets the south tile status.
	/// Public for serialization.
	/// </summary>
	/// <value>The south tile status.</value>
	public Tile.MowStatus SouthTileStatus {get;set;}
	/// <summary>
	/// Gets or sets the south west tile status.
	/// Public for serialization.
	/// </summary>
	/// <value>The south west tile status.</value>
	public Tile.MowStatus SouthWestTileStatus {get;set;}
	/// <summary>
	/// Gets or sets the west tile status.
	/// Public for serialization.
	/// </summary>
	/// <value>The west tile status.</value>
	public Tile.MowStatus WestTileStatus {get;set;}
	/// <summary>
	/// Gets or sets the north west tile status.
	/// Public for serialization.
	/// </summary>
	/// <value>The north west tile status.</value>
	public Tile.MowStatus NorthWestTileStatus {get;set;}
	/// <summary>
	/// Gets or sets the current position on the 2D grid.
	/// Public for serialization.
	/// </summary>
	/// <value>The position on the 2D grid.</value>
	public Garden.GridPosition Position {get;set;}


	/// <summary>
	/// Default empty contructor.
	/// Initializes a new instance of the <see cref="State"/> class.
	/// Necessary for serialization.
	/// </summary>
	public State() 
		: this (Tile.MowStatus.LongGrass,
				Tile.MowStatus.LongGrass,
				Tile.MowStatus.LongGrass,
				Tile.MowStatus.LongGrass,
				Tile.MowStatus.LongGrass,
				Tile.MowStatus.LongGrass,
				Tile.MowStatus.LongGrass,
				Tile.MowStatus.LongGrass,
				new Garden.GridPosition()) {
		
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="State"/> class.
	/// </summary>
	/// <param name="northTileStatus">North tile status.</param>
	/// <param name="northEastTileStatus">North east tile status.</param>
	/// <param name="eastTileStatus">East tile status.</param>
	/// <param name="southEastStatus">South east status.</param>
	/// <param name="southTileStatus">South tile status.</param>
	/// <param name="southWestTileStatus">South west tile status.</param>
	/// <param name="westTileStatus">West tile status.</param>
	/// <param name="northWestTileStatus">North west tile status.</param>
	/// <param name="position">Current position.</param>
	public State (Tile.MowStatus northTileStatus,
				Tile.MowStatus northEastTileStatus, 
				Tile.MowStatus eastTileStatus, 
				Tile.MowStatus southEastStatus,
				Tile.MowStatus southTileStatus, 
				Tile.MowStatus southWestTileStatus,
				Tile.MowStatus westTileStatus, 
				Tile.MowStatus northWestTileStatus,
				Garden.GridPosition position
		) {
		this.NorthTileStatus = northTileStatus;
		this.NorthEastTileStatus = northEastTileStatus;
		this.EastTileStatus = eastTileStatus;
		this.SouthEastTileStatus = southEastStatus;
		this.SouthTileStatus = southTileStatus;
		this.SouthWestTileStatus = southWestTileStatus;
		this.WestTileStatus = westTileStatus;
		this.NorthWestTileStatus = northWestTileStatus;
		this.Position = position;
	}



	public override bool Equals	(object other) {
		if (other == null) { 
			return false; 
		}

		if (other.GetType() == typeof(State)) {
			var otherState = (State) other;
			return (this.NorthTileStatus == otherState.NorthTileStatus &&
				this.EastTileStatus == otherState.EastTileStatus &&
				this.SouthTileStatus == otherState.SouthTileStatus &&
				this.WestTileStatus == otherState.WestTileStatus &&
				this.NorthEastTileStatus == otherState.NorthEastTileStatus &&
				this.NorthWestTileStatus == otherState.NorthWestTileStatus &&
				this.SouthEastTileStatus == otherState.SouthEastTileStatus &&
				this.SouthWestTileStatus == otherState.SouthWestTileStatus &&
				this.Position.Equals(otherState.Position)
				);
		}

		return false;
	}

	public override int GetHashCode () {
		var hash = 13;
		hash = hash * 7 + (int)NorthTileStatus;
		hash = hash * 7 + (int)EastTileStatus;
		hash = hash * 7 + (int)SouthTileStatus;
		hash = hash * 7 + (int)WestTileStatus;
		
		hash = hash * 7 + (int)NorthEastTileStatus;
		hash = hash * 7 + (int)NorthWestTileStatus;
		hash = hash * 7 + (int)SouthEastTileStatus;
		hash = hash * 7 + (int)SouthWestTileStatus;

		hash = hash * 7 + Position.GetHashCode();
		return hash;
	}
}

/// <summary>
/// Extracts the state of the mower robot inside the garden at its current position.
/// </summary>
public class StateExtractor {

	/// <summary>
	/// The mower, whose state is extracted by this <see cref="T:StateExtractor"/>.
	/// </summary>
	private Mower  mower;

	/// <summary>
	/// Initializes a new instance of the <see cref="StateExtractor"/> class.
	/// </summary>
	/// <param name="mower">The <see cref="T:Mower"/>, whose state is extracted.</param>
	public StateExtractor (Mower mower) {
		this.mower = mower;
	}

	/// <summary>
	/// Extracts the current state of the mower.
	/// </summary>
	/// <returns>The current <see cref="T:State"/>.</returns>
	public State ExtractState () {
		return new State (
			mower.Garden.GetTileStatus(mower.Position + MovementAction.North().GridMovement),
			mower.Garden.GetTileStatus(mower.Position + MovementAction.NorthEast().GridMovement),
			mower.Garden.GetTileStatus(mower.Position + MovementAction.East().GridMovement),
			mower.Garden.GetTileStatus(mower.Position + MovementAction.SouthEast().GridMovement),
			mower.Garden.GetTileStatus(mower.Position + MovementAction.South().GridMovement),
			mower.Garden.GetTileStatus(mower.Position + MovementAction.SouthWest().GridMovement),
			mower.Garden.GetTileStatus(mower.Position + MovementAction.West().GridMovement),
			mower.Garden.GetTileStatus(mower.Position + MovementAction.NorthWest().GridMovement),
			mower.Position
		);
	}

	/// <summary>
	/// Extracts the possible movements of the mower.
	/// Needed for DnyaQ.
	/// </summary>
	/// <returns>The possible movements.</returns>
    public List<MovementAction> ExtractPossibleMovements()
    {
        return mower.PossibleMovements;
    }
}

/// <summary>
/// This struct holds a state and an action performed in this state.
/// </summary>

public class StateActionPair
{
	/// <summary>
	/// Gets or sets the state.
	/// Settable for serialization.
	/// </summary>
	/// <value>The state.</value>
	public State state {get; set;}

	/// <summary>
	/// Gets or sets the action.
	/// Settable for serialization.
	/// </summary>
	/// <value>The action.</value>
	public MovementAction action {get; set;}

	/// <summary>
	/// Default empty contructor.
	/// Initializes a new instance of the <see cref="StateActionPair"/> class.
	/// Necessary for serialization.
	/// </summary>
	public StateActionPair() 
		: this (new State(), new MovementAction()) {

	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StateActionPair"/> class.
	/// </summary>
	/// <param name="state">The state.</param>
	/// <param name="action">The action.</param>
    public StateActionPair(State state, MovementAction action)
    {
        this.state = state;
        this.action = action;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) { return false; }

        if (obj.GetType() == typeof(StateActionPair))
        {
            var otherStateAction = (StateActionPair)obj;
            return otherStateAction.state.Equals(this.state)
                && otherStateAction.action == this.action;
        }

        return false;
    }

    public override int GetHashCode()
    {
        var hash = this.state.GetHashCode();
        hash = hash * 7 + this.action.GetHashCode();
        return hash;
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}", this.state.ToString(), this.action.ToString());
    }

}

/// <summary>
/// This struct holds a state and reward perceived when observing this state.
/// Needed for DynaQ
/// </summary>
public class StateRewardPair
{
	/// <summary>
	/// Gets or sets the state.
	/// Settable for serialization.
	/// </summary>
	/// <value>The state.</value>
	public State State {get;set;}

	/// <summary>
	/// Gets or sets the reward.
	/// Settable for serialization.
	/// </summary>
	/// <value>The reward.</value>
	public float Reward {get;set;}

	/// <summary>
	/// Default empty contructor.
	/// Initializes a new instance of the <see cref="StateRewardPair"/> class.
	/// Necessary for serialization.
	/// </summary>
	public StateRewardPair() 
		: this (new State(), 0f)
	{
		
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StateRewardPair"/> class.
	/// </summary>
	/// <param name="state">The state.</param>
	/// <param name="reward">The reward.</param>
    public StateRewardPair(State state, float reward)
    {
        this.State = state;
        this.Reward = reward;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) { return false; }

        if (obj.GetType() == typeof(StateRewardPair))
        {
            var otherStateRewardPair = (StateRewardPair)obj;
            return otherStateRewardPair.State.Equals(this.State)
                && otherStateRewardPair.Reward == this.Reward;
            //TODO !! better comparision for floats
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = this.State.GetHashCode();
        hash = hash * 7 + this.Reward.GetHashCode();
        return hash;
    }
}
