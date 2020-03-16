using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridPosition = Garden.GridPosition;

/// <summary>
/// A moving obstacle inside the garden.
/// It contains a type that specifies, what kind of obstacle it is.
/// </summary>
public class MovingObstacle : MovingGardenObject, EpisodeManager.IEpisodeChangesReceiver {

	/// <summary>
	/// Interface for observers of MovingObstacle objects.
	/// The observers are notified about position changes.
	/// </summary>
	public interface IMovingObstacleObserver {
		
		/// <summary>
		/// Notification that the obstacle moved from one position to another.
		/// </summary>
		/// <param name="obstacle">The moving obstacle.</param>
		/// <param name="oldPosition">Its old position.</param>
		/// <param name="newPosition">Its new position.</param>
		void MovingObstacleMoved(MovingObstacle obstacle, GridPosition oldPosition, GridPosition newPosition);
	}
	
	/// <summary>
	/// The possible types of moving obstacles.
	/// </summary>
	public enum MovingObstacleType {
		Person,
		Animal
	}

	/// <summary>
	/// The type of the moving obstacle.
	/// </summary>
	private MovingObstacleType type;

	/// <summary>
	/// The list of observers.
	/// </summary>
	private List<IMovingObstacleObserver> observers = new List<IMovingObstacleObserver>();

	/// <summary>
	/// Initializes a new instance of the <see cref="MovingObstacle"/> class.
	/// </summary>
	/// <param name="position">The initial position.</param>
	/// <param name="type">The type of the obstacle.</param>
	public MovingObstacle(GridPosition position, MovingObstacleType type)
	: base(position) {
		this.type = type;
		this.PossibleMovements = MovementAction.VonNeumannNeighbourhoodMovements();
		EpisodeManager.AddEpisodeChangesReceiver(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MovingObstacle"/> class.
	/// </summary>
	/// <param name="x">The iniital x-position.</param>
	/// <param name="y">The initial y-position.</param>
	/// <param name="type">The type of the obstacle.</param>
	public MovingObstacle(uint x, uint y, MovingObstacleType type)
		: this(new GridPosition((int)x, (int)y), type) {

	}
	
	public MovingObstacleType Type {
		get {
			return type;
		}
	}


	public override void PerformMovementActionFromAvailableActions (List<MovementAction> availableActions) {
		// For now, all moving obstacles simply perform random movements.
		// Note: Maybe replace with implementation depending on the type.
		// (e.g. Animals are chasing the robot, Persons are evading it).
		int randIndex = UnityEngine.Random.Range(0, availableActions.Count);
		MovementAction chosenMovement = availableActions[randIndex];
		PerformMovement(chosenMovement);
	}

	/// <summary>
	/// Registers a new observer of the mower.
	/// </summary>
	/// <returns><c>true</c>, if the observer was added, <c>false</c> otherwise.</returns>
	/// <param name="observer">The new observer to register.</param>
	public bool AddObserver (IMovingObstacleObserver observer) {
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
	public bool RemoveObserver(IMovingObstacleObserver observer) {
		if (observers.Contains(observer)) {
			observers.Remove(observer);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Resets the state of the object to reflect the beginning of a new episode.
	/// </summary>
	protected override void ResetToInitialState() {
		GridPosition oldPosition = Position;
		base.ResetToInitialState();
		foreach (var observer in observers) {
			observer.MovingObstacleMoved(this, oldPosition, Position);
		}
	}

	/// <summary>
	/// Performs the given movement action.
	/// </summary>
	/// <param name="movementAction">The Movement action to perform.</param>
	private void PerformMovement(MovementAction movementAction) {
		GridPosition oldPosition = this.Position;
		this.Position = oldPosition + movementAction.GridMovement;
		foreach (var observer in observers) {
			observer.MovingObstacleMoved(this, oldPosition, this.Position);
		}
	}

	/// <summary>
	/// Notification that the current episode will end.
	/// </summary>
	/// <param name="episode">The current episode to end.</param>
	public void EpisodeWillEnd(uint episode) {
		// Do nothing
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


}
