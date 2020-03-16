using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridPosition = Garden.GridPosition;

/// <summary>
/// Wrapper class to describe various movement actions on the 2D grid.
/// </summary>
public class MovementAction {

	/// <summary>
	/// Enumeration for all geographic directions.
	/// </summary>
	public enum Direction
	{
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest
	}

	/// <summary>
	/// Convenience method to construct a <see cref="T:MovementAction"/> representing one step to the north.
	/// </summary>
	/// <returns>The desired movement action.</returns>
	public static MovementAction North() {
		return new MovementAction(Direction.North, new GridPosition(0, -1));
	}
	/// <summary>
	/// Convenience method to construct a <see cref="T:MovementAction"/> representing one step to the north-east.
	/// </summary>
	/// <returns>The desired movement action.</returns>
	public static MovementAction NorthEast() {
		return new MovementAction(Direction.NorthEast, new GridPosition(+1, -1));
	}
	/// <summary>
	/// Convenience method to construct a <see cref="T:MovementAction"/> representing one step to the east.
	/// </summary>
	/// <returns>The desired movement action.</returns>
	public static MovementAction East() {
		return new MovementAction(Direction.East, new GridPosition(+1, 0));
	}
	/// <summary>
	/// Convenience method to construct a <see cref="T:MovementAction"/> representing one step to the south-east.
	/// </summary>
	/// <returns>The desired movement action.</returns>
	public static MovementAction SouthEast() {
		return new MovementAction(Direction.SouthEast, new GridPosition(+1, +1));
	}
	/// <summary>
	/// Convenience method to construct a <see cref="T:MovementAction"/> representing one step to the south.
	/// </summary>
	/// <returns>The desired movement action.</returns>
	public static MovementAction South() {
		return new MovementAction(Direction.South, new GridPosition(0, +1));
	}
	/// <summary>
	/// Convenience method to construct a <see cref="T:MovementAction"/> representing one step to the south-west.
	/// </summary>
	/// <returns>The desired movement action.</returns>
	public static MovementAction SouthWest() {
		return new MovementAction(Direction.SouthWest, new GridPosition(-1, +1));
	}
	/// <summary>
	/// Convenience method to construct a <see cref="T:MovementAction"/> representing one step to the west.
	/// </summary>
	/// <returns>The desired movement action.</returns>
	public static MovementAction West() {
		return new MovementAction(Direction.West, new GridPosition(-1, 0));
	}
	/// <summary>
	/// Convenience method to construct a <see cref="T:MovementAction"/> representing one step to the norht-west.
	/// </summary>
	/// <returns>The desired movement action.</returns>
	public static MovementAction NorthWest() {
		return new MovementAction(Direction.NorthWest, new GridPosition(-1, -1));
	}


	/// <summary>
	/// Convenience method to contruct a list of <see cref="T:MovementAction"/> objects describing the possible movements regarding the von-Neumann neighbourhood.
	/// </summary>
	/// <returns>The list of possible <see cref="T:MovementAction"/> objects in the von-Neumann neighbourhood.</returns>
	public static List<MovementAction> VonNeumannNeighbourhoodMovements() {
		List<MovementAction> movements = new List<MovementAction>(4);
		movements.Add(North());
		movements.Add(South());
		movements.Add(West());
		movements.Add(East());
		return movements;
	}

	public MovementAction() {
		
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MovementAction"/> class.
	/// </summary>
	/// <param name="direction">The direction of the movement.</param>
	/// <param name="gridMovement">The mathematical movement on the 2D grid.</param>
	public MovementAction(MovementAction.Direction direction, GridPosition gridMovement) {
		this.MovementDirection = direction;
		this.GridMovement = gridMovement;
	}

	/// <summary>
	/// The direction of the movement.
	/// Public for serialization.
	/// </summary>
	public Direction MovementDirection  {get;set;}

	/// <summary>
	/// The mathematical movement on the 2D grid.
	/// Public for serialization.
	/// </summary>
	public  GridPosition GridMovement  {get;set;}


	public override bool Equals(object obj)
	{
		if (obj == null) { return false; }

		if (obj.GetType() == typeof(MovementAction))
		{
			var otherMovement = (MovementAction)obj;
			return otherMovement.GridMovement.Equals(this.GridMovement)
				&& otherMovement.MovementDirection == this.MovementDirection;
		}

		return false;
	}

	public override int GetHashCode()
	{
		var hash = this.MovementDirection.GetHashCode();
		return hash;
	}
}

/// <summary>
/// A moving object inside the garden.
/// It provides a list of all possible movement actions it can perform.
/// </summary>
public abstract class MovingGardenObject : GardenObject {

	/// <summary>
	/// The movements this <see cref="T:MovingGardenObject"/> is capable of.
	/// </summary>
	private List<MovementAction> possibleMovements = new List<MovementAction>();

	/// <summary>
	/// Initializes a new instance of the <see cref="MovingGardenObject"/> class.
	/// </summary>
	/// <param name="position">The initial position.</param>
	public MovingGardenObject(GridPosition position)
		: base(position) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MovingGardenObject"/> class.
	/// </summary>
	/// <param name="x">The iniital x-position.</param>
	/// <param name="y">The initial y-position.</param>
	public MovingGardenObject(uint x, uint y)
		: this(new GridPosition((int)x, (int)y)) {

	}
		
	public List<MovementAction> PossibleMovements {
		get {
			return possibleMovements;
		}
		set {
			possibleMovements = value;
		}
	}


	/// <summary>
	/// Performs one action from the available actions.
	/// </summary>
	/// <param name="availableActions">All available actions. Must not be null or empty. </param>
	public abstract void PerformMovementActionFromAvailableActions (List<MovementAction> availableActions);


	/// <summary>
	/// Resets the state of the object to reflect the beginning of a new episode.
	/// </summary>
	protected virtual void ResetToInitialState() {
		Position = InitialPosition;
	}
}
