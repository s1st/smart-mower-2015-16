using System;
using System.Collections.Generic;
using UnityEngine;
using GridPosition = Garden.GridPosition;

/// <summary>
/// The elementary part a garden consits of. Each Tile rerpesents a unique, quadratic patch of a garden.
/// </summary>
public class Tile
{
	/// <summary>
	/// The different states the ground of the tile can be in.
	/// </summary>
    public enum MowStatus {
        ShortGrass,
        LongGrass,
		ChargingStation,
		Obstacle
    }

	/// <summary>
	/// The position of the tile in the 2D-grid of the garden
	/// </summary>
	private GridPosition position;

	/// <summary>
	/// The state of the tile regarding its ground.
	/// </summary>
    private MowStatus mowStatus;

	/// <summary>
	/// The initial mow state that is restored on reset
	/// </summary>
	private MowStatus initialMowStatus;

	/// <summary>
	/// Whether the tile is occupied by an object or not.
	/// </summary>
	private bool occupied;

	/// <summary>
	/// The initial occupied state that is restored on reset.
	/// </summary>
	private bool initialOccupiedState;

	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="x">The x-position.</param>
	/// <param name="y">The y-position.</param>
	/// <param name="mowStatus">The mow status of the tile.</param>
	/// <param name="occupied">Whether the tile is occupied or not.</param>
    public Tile(uint x, uint y, MowStatus mowStatus, bool occupied) {
		this.Position = new GridPosition((int)x, (int)y);
		this.Occupied = occupied;
		this.SetMowStatus(mowStatus);
		SetCurrentStateAsInitialState();
    }

	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="position">The position of the tile.</param>
	/// <param name="mowStatus">The mow status of the tile.</param>
	/// <param name="occupied">Whether the tile is occupied or not.</param>
	public Tile(GridPosition position, MowStatus mowStatus, bool occupied) 
		: this((uint)position.X, (uint)position.Y, mowStatus, occupied) {

	}

	public GridPosition Position {
		get {
			return position;
		}
		set {
			position = value;
		}
	}

    public MowStatus GetMowStatus()
    {
        return mowStatus;
    }

    public void SetMowStatus(MowStatus value)
    { 
        mowStatus = value;
    }
		
	public MowStatus InitialMowStatus {
		get {
			return initialMowStatus;
		}
	}

	public bool Occupied {
		get {
			return occupied;
		}
		set {
			occupied = value;
		}
	}	

	public bool InitialOccupiedState {
		get {
			return initialOccupiedState;
		}
	}

	/// <summary>
	/// Sets the current state of the tile as the initial one.
	/// </summary>
	public void SetCurrentStateAsInitialState () {
		initialOccupiedState = Occupied;
		initialMowStatus = this.GetMowStatus();
	}

    public bool Equals(Tile other)
    {
        if( this.Position.Equals(other.Position) &&
		   	this.Occupied == other.Occupied && 
            this.GetMowStatus() == other.GetMowStatus())
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}