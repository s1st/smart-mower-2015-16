using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridPosition = Garden.GridPosition;

/// <summary>
/// A static obstacle inside the garden.
/// It contains a type that specifies, what kind of obstacle it is.
/// </summary>
public class StaticObstacle : GardenObject {

	/// <summary>
	/// The possible types of static obstacles.
	/// </summary>
	public enum StaticObstacleType {
		Rock,
		Water
	}

	/// <summary>
	/// The type of the static obstacle.
	/// </summary>
	private StaticObstacleType type;

	/// <summary>
	/// Initializes a new instance of the <see cref="StaticObstacle"/> class.
	/// </summary>
	/// <param name="position">The initial position.</param>
	/// <param name="type">The type of the obstacle.</param>
	public StaticObstacle(GridPosition position, StaticObstacleType type)
		: base(position) {
		this.type = type;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StaticObstacle"/> class.
	/// </summary>
	/// <param name="x">The initial x-position.</param>
	/// <param name="y">The initial y-position.</param>
	/// <param name="type">The type of the obstacle.</param>
	public StaticObstacle(uint x, uint y, StaticObstacleType type)
		: this(new GridPosition((int)x, (int)y), type) {

	}

	public StaticObstacleType Type {
		get {
			return type;
		}
	}
}
