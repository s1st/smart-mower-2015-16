using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridPosition = Garden.GridPosition;

/// <summary>
/// Abstract base class for an arbitrary object inside the garden.
/// The class provides the position of this object.
/// </summary>
public abstract class GardenObject {

	/// <summary>
	/// The position on the 2D grid.
	/// </summary>
	private GridPosition position;

	/// <summary>
	/// The initial position on the 2D grid when the object was created.
	/// </summary>
	private GridPosition initialPosition;

	/// <summary>
	/// Initializes a new instance of the <see cref="GardenObject"/> class.
	/// </summary>
	/// <param name="position">The initial position.</param>
	public GardenObject(GridPosition position) {
		this.Position = new GridPosition(position);
		this.InitialPosition = Position;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GardenObject"/> class.
	/// </summary>
	/// <param name="x">The iniital x-position.</param>
	/// <param name="y">The initial y-position.</param>
	public GardenObject(uint x, uint y) 
		: this(new GridPosition((int)x, (int)y)) {

	}

	public GridPosition Position {
		get {
			return position;
		}
		set {
			position = value;
		}
	}

	public GridPosition InitialPosition {
		get {
			return initialPosition;
		}
		private set {
			initialPosition = value;
		}
	}
}
