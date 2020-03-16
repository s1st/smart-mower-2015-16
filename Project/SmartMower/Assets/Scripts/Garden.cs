using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The Garden is the main data structure representing the 2D garden world an the objects moving within.
/// It acts as the main source of progress by asking each moving object within the garden to perform one action in each time step.
/// </summary>
public class Garden : StepGenerator.IStepReceiver,MovingObstacle.IMovingObstacleObserver, Mower.IMowerObserver, EpisodeManager.IEpisodeChangesReceiver
{
	/// <summary>
	/// Interface for observers of the garden.
	/// Observers are notified about changes of the individual tiles and their mow states.
	/// </summary>
	public	interface IGardenObserver {

		/// <summary>
		/// Notifies the observer that the mow status of the given tile changed.
		/// </summary>
		/// <param name="tile">The tile where the change occured.</param>
		/// <param name="oldMowStatus">Its old mow status.</param>
		/// <param name="newMowStatus">Its new mow status.</param>
		void MowStatusOfTileChanged(Tile tile, Tile.MowStatus oldMowStatus, Tile.MowStatus newMowStatus);
	}
	
	/// <summary>
	/// Wrapper class for a pair of discrete coordinates. Signedness is preserved to allow easy calculation.
	/// </summary>
	public class GridPosition {
		
		/// <summary>
		/// Gets or sets the x coordinate.
		/// </summary>
		/// <value>The x coordinate.</value>
		public int X {get; set;}

		/// <summary>
		/// Gets or sets the y .
		/// </summary>
		/// <value>The y coordinate.</value>
		public int Y {get; set;}


		public static GridPosition operator+ (GridPosition a, GridPosition b) {
			return new GridPosition(a.X + b.X, a.Y + b.Y);
		}

		/// <summary>
		/// Default empty contructor.
		/// Initializes a new instance of the <see cref="Garden+GridPosition"/> class.
		/// Necessary for serialization.
		/// </summary>
		public GridPosition() 
			:this(0,0) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Garden+GridPosition"/> class.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public GridPosition(int x, int y) {
			this.X = x;
			this.Y = y;
		}

		/// <summary>
		/// Copy constructor.
		/// Initializes a new instance of the <see cref="Garden+GridPosition"/> class.
		/// </summary>
		/// <param name="other">The other <see cref="T:Garden+GridPosition"/> that is copied.</param>
		public GridPosition(GridPosition other) {
			this.X = other.X;
			this.Y = other.Y;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Garden+GridPosition"/> class.
		/// Adds a <see cref="T:MovingGardenObject+MovementAction"/> to another <see cref="Garden+GridPosition"/>.
		/// </summary>
		/// <param name="other">The other <see cref="Garden+GridPosition"/>.</param>
		/// <param name="movementAction">The <see cref="T:MovingGardenObject+MovementAction"/> that is applied to the other object.</param>
		public GridPosition(GridPosition other, MovementAction movementAction) 
			: this(other + movementAction.GridMovement){

		}

		/// <summary>
		/// Applies the <see cref="T:MovingGardenObject+MovementAction"/> to this object.
		/// </summary>
		/// <param name="movementAction">The <see cref="T:MovingGardenObject+MovementAction"/> to apply.</param>
		public void ApplyMovementAction(MovementAction movementAction) {
			GridPosition tmp = this + movementAction.GridMovement;
			this.X = tmp.X;
			this.Y = tmp.Y;
		}

		public bool Equals(GridPosition other) {
			return (this.X == other.X && this.Y == other.Y);
		}

		public override string ToString () {
			return string.Format ("(X={0}, Y={1})", X, Y);
		}

		public override int GetHashCode () {
			int hash = 7;
			hash = hash * 13 + this.X;
			hash = hash * 13 + this.Y;
			return hash;
		}
	}

	/// <summary>
	/// The tiles that form the two dimensional space of the garden.
	/// </summary>
    private List<Tile> tiles = new List<Tile>();

	/// <summary>
	/// The width of the garden (i.e the number of tiles in horizontal direction).
	/// </summary>
    uint gardenWidth;

	/// <summary>
	/// The height of the garden (i.e. the number of tiles in vertical direction).
	/// </summary>
    uint gardenHeigth;

	/// <summary>
	/// The number of tiles in the garden, that have grass on it (and therefore can be mown).
	/// </summary>
	uint numGrassTiles;

	/// <summary>
	/// The mower robot.
	/// </summary>
	private Mower mower;

	/// <summary>
	/// The starting position of the mower.
	/// Default value is only for testing purposes.
	/// </summary>
	private GridPosition mowerStartPosition = new GridPosition(0,0);

	/// <summary>
	/// All static obstacles in the garden.
	/// </summary>
	private List<StaticObstacle> staticObstacles = new List<StaticObstacle>();

	/// <summary>
	/// All moving obstacles in the garden.
	/// </summary>
	private List<MovingObstacle> movingObstacles = new List<MovingObstacle>();

	/// <summary>
	/// The list of garden observers.
	/// </summary>
	private List<IGardenObserver> observers = new List<IGardenObserver>();

	/// <summary>
	/// Flag to signal that the current episode is ending.
	/// Used to allow arbitray movements for garden objects due to their reset behaviour
	/// </summary>
	private bool episodeIsEnding = false;
	

    internal List<Tile> Tiles
    {
        get
        {
            return tiles;
        }
		private set {
			tiles = value;
		}
    }

	public uint GardenWidth {
		get {
			return gardenWidth;
		}
		private set {
			gardenWidth = value;
		}
	}	

	public uint GardenHeigth {
		get {
			return gardenHeigth;
		}
		private set {
			gardenHeigth = value;
		}
	}

	public uint NumGrassTiles {
		get {
			return numGrassTiles;
		}
		private set {
			numGrassTiles = value;
		}
	}

	public Mower Mower {
		get {
			return mower;
		}
	}

	public GridPosition MowerStartPosition {
		get {
			return mowerStartPosition;
		}
		private set {
			mowerStartPosition = value;
		}
	}

	public List<StaticObstacle> StaticObstacles {
		get {
			return staticObstacles;
		}
		private set {
			staticObstacles = value;
		}
	}

	public List<MovingObstacle> MovingObstacles {
		get {
			return movingObstacles;
		}
		private set {
			movingObstacles = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Garden"/> class.
	/// Since this is a heavy-weight constructor with lots of error-potential if used incorrectly,
	/// all Garden objects should be created using the <see cref="T:GardenFactory"/>. 
	/// </summary>
	/// <param name="gardenWidth">The garden width.</param>
	/// <param name="gardenHeight">The garden height.</param>
	/// <param name="tiles">A list of all tiles of the garden. Must contain <paramref name="gardenWidth"/> times <paramref name="gardenHeight"/> tiles</param>.
	/// <param name="mowerStartPosition">The start position of the mower robot.</param>
	/// <param name="movingObstacles">A list of all moving obstacles in the garden.</param>
	/// <param name="staticObstacles">A list of all static obstacles in the garden.</param>
	/// <param name="paramSet">The set of parameters for this project.</param>
	public Garden(uint gardenWidth, uint gardenHeight,
				List<Tile> tiles, 
				GridPosition mowerStartPosition, 
				List<MovingObstacle> movingObstacles,
				List<StaticObstacle> staticObstacles,
				ParamSet paramSet) {

		this.GardenWidth = gardenWidth;
		this.GardenHeigth = gardenHeight;
		this.Tiles = tiles;
		this.MowerStartPosition = mowerStartPosition;
		this.MovingObstacles = movingObstacles;
		this.StaticObstacles = staticObstacles;

		this.NumGrassTiles = 0;
		foreach (var tile in Tiles) {
			if (tile.GetMowStatus() == Tile.MowStatus.LongGrass) {
				this.NumGrassTiles ++;
			}
		}
		Debug.Log(string.Format("Garden initialized with {0} tiles to mow", this.NumGrassTiles));

		// Create the mower agent.
		mower = new Mower(MowerStartPosition, this, paramSet);
		mower.AddObserver(this);

		foreach (var movingObstacle in MovingObstacles) {
			movingObstacle.AddObserver(this);
		}

		// Receive episode changes.
		EpisodeManager.AddEpisodeChangesReceiver(this);

		// Receive a step-call after each time-step.
		GameObject stepGenGameObject = GameObject.FindGameObjectWithTag("StepGenerator");
		StepGenerator stepGen = stepGenGameObject.GetComponent<StepGenerator>();
		stepGen.AddStepReceiver(this);
	}
		

	/// <summary>
	/// Returns the tile at the given position in the garden.
	/// </summary>
	/// <returns>The tile.</returns>
	/// <param name="x">The x-position of the tile to return.</param>
	/// <param name="y">The y-position of the tile to return.</param>
    private Tile GetTile(uint x, uint y)
    {
		uint index = y*GardenWidth + x;
		if (IsValidPosition(x,y) && index < Tiles.Count) {
			return Tiles[(int)index];
		}
		return null;
    }

	/// <summary>
	/// Returns the tile at the given position in the garden.
	/// </summary>
	/// <returns>The tile.</returns>
	/// <param name="position">The position of the tile to return.</param>
	private Tile GetTile(GridPosition position) {
		return GetTile((uint)position.X, (uint)position.Y);
	}

    /// <summary>
    /// Sets the status of the tile at the given position in the garden.
    /// </summary>
    /// <param name="x">The x-position of the tile.</param>
    /// <param name="y">The y-position of the tile.</param>
    /// <param name="status">The status to set.</param>
    public void SetTileStatus(uint x, uint y, Tile.MowStatus status)
    {
        Tile t = GetTile(x, y);
        if (t == null) {
            return;
        }
		Tile.MowStatus oldStatus = t.GetMowStatus();
        t.SetMowStatus(status);
		foreach (var observer in observers) {
			observer.MowStatusOfTileChanged(t, oldStatus, status);
		}
    }

	/// <summary>
	/// Sets the status of the tile at the given position in the garden.
	/// </summary>
	/// <param name="position">The Ppsition of the tile.</param>
	/// <param name="status">The status to set.</param>
	public void SetTileStatus(GridPosition position, Tile.MowStatus status) {
		SetTileStatus((uint)position.X, (uint)position.Y, status);
	}

	/// <summary>
	/// Returns the status of the tile at the given position in the garden.
	/// </summary>
	/// <returns>The tile status.</returns>
	/// <param name="x">The x-position of the tile.</param>
	/// <param name="y">The y-position of the tile.</param>
    public Tile.MowStatus GetTileStatus(uint x, uint y)
    {
        Tile t = GetTile(x, y);
        if (t == null)
        {
			return Tile.MowStatus.Obstacle;
        }
        return t.GetMowStatus();
    }
		
	/// <summary>
	/// Returns the status of the tile at the given position in the garden.
	/// </summary>
	/// <returns>The tile status.</returns>
	/// <param name="position">The position of the tile.</param>
	public Tile.MowStatus GetTileStatus(GridPosition position) {
		return GetTileStatus((uint)position.X, (uint)position.Y);
	}

	/// <summary>
	/// Sanity check returning whether the given grid-position is a valid position in the garden.
	/// </summary>
	/// <returns><c>true</c> if the position is a valid one; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x-position</param>
	/// <param name="y">The y-position</param>
    public bool IsValidPosition(uint x, uint y)
    {
        if (x >= GardenWidth || 
            y >= GardenHeigth) {
            return false;
        } else {
            return true;
        }
    }

	/// <summary>
	/// Sanity check returning whether the given grid-position is a valid position in the garden.
	/// </summary>
	/// <returns><c>true</c> if the position is a valid one; otherwise, <c>false</c>.</returns>
    /// <param name="position">The position to check.</param>
	public bool IsValidPosition(GridPosition position) {
		return IsValidPosition((uint)position.X, (uint)position.Y);
	}


	/// <summary>
	/// Registers a new observer of the garden.
	/// </summary>
	/// <returns><c>true</c>, if the observer was added, <c>false</c> otherwise.</returns>
	/// <param name="observer">The new observer to register.</param>
	public bool AddObserver (IGardenObserver observer) {
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
	public bool RemoveObserver(IGardenObserver observer) {
		if (observers.Contains(observer)) {
			observers.Remove(observer);
			return true;
		} else {
			return false;
		}
	}


	/// <summary>
	/// Perform one cycle / time step.
	/// Implementation of <see cref="StepGenerator.IStepReceiver"/>
	/// </summary>
	public void DoStep() {
		// Ask all moving objects in the garden to perfrom one step aka one movement action.
		// The mower is always asked last.

		// Create a list of all moving objects in the garden with the robot at the last index.
		List<MovingGardenObject> movingObjects = new List<MovingGardenObject>(1 + movingObstacles.Count);
		foreach (var obstacle in movingObstacles) {
			movingObjects.Add(obstacle);
		}
        movingObjects.Add(Mower);

        // Check for all movements the object can perform if they are permitted.
        foreach (var movingObject in movingObjects) {
			List<MovementAction> permittedMovements = new List<MovementAction>(movingObject.PossibleMovements.Count);
			foreach (var movement in movingObject.PossibleMovements) {
				GridPosition resultingPosition = new GridPosition(movingObject.Position + movement.GridMovement);
				Tile resultingTile = GetTile(resultingPosition);
				if (IsValidPosition(resultingPosition) && !(resultingTile.Occupied) && (resultingTile.GetMowStatus() != Tile.MowStatus.Obstacle)) {
					permittedMovements.Add(movement);
				}
			}
			if (permittedMovements.Count > 0) {
				movingObject.PerformMovementActionFromAvailableActions(permittedMovements);
			}
		}
        
		// Chech if the episode is finished.
		if (Mower.MowingFinished) {
			EpisodeManager.NextEpisode();
		}


	}

	/// <summary>
	/// Notification that the obstacle moved from one position to another.
	/// Implementation of the <see cref="MovingObstacle.IMovingObstacleObserver"/>.
	/// </summary>
	/// <param name="obstacle">The moving obstacle.</param>
	/// <param name="oldPosition">Its old position.</param>
	/// <param name="newPosition">Its new position.</param>
	public void MovingObstacleMoved(MovingObstacle obstacle, GridPosition oldPosition, GridPosition newPosition) {
		if (! episodeIsEnding) {
			Tile oldPositionTile = GetTile(oldPosition);
			Tile newPositionTile = GetTile(newPosition);
			if (oldPositionTile != null && newPositionTile != null) {
				oldPositionTile.Occupied = false;
				newPositionTile.Occupied = true;
				//Debug.Log(string.Format("Obstacle moved to {0}", newPosition));
			} else {
				//TODO: Throw error or write to an error log.
				Debug.Log(string.Format("MovingObstacle moved illegally: {0} -> {1}", oldPosition, newPosition));
			}	
		}
	}

	/// <summary>
	/// Notification that the mower moved from one position to another.
	/// Implementation of <see cref="Mower.IMowerObserver"/>.
	/// </summary>
	/// <param name="mower">The mower.</param>
	/// <param name="oldPosition">Its old position.</param>
	/// <param name="newPosition">Its new position.</param>
	public void MowerMoved(Mower mower, GridPosition oldPosition, GridPosition newPosition) {
		if (! episodeIsEnding) {
			Tile oldPositionTile = GetTile(oldPosition);
			Tile newPositionTile = GetTile(newPosition);
			if (oldPositionTile != null && newPositionTile != null) {
				oldPositionTile.Occupied = false;
				newPositionTile.Occupied = true;
				//Debug.Log(string.Format("Mower moved to {0}", newPosition));
			} else {
				//TODO: Throw error or write to an error log.
				Debug.Log(string.Format("Mower moved illegally: {0} -> {1}", oldPosition, newPosition));
			}
		}
	}

	/// <summary>
	/// Notification that the current episode will end.
	/// </summary>
	/// <param name="episode">The current episode to end.</param>
	public void EpisodeWillEnd(uint episode) {
		episodeIsEnding = true;
	}

	/// <summary>
	/// Notification that the current episode did end.
	/// </summary>
	/// <param name="episode">The episode that ended.</param>
	public void EpisodeDidEnd(uint episode) {
		foreach (var tile in Tiles) {
			tile.Occupied = tile.InitialOccupiedState;
			// Use SetTileStatus(...) to inform observers.
			SetTileStatus(tile.Position, tile.InitialMowStatus);
		}
	}

	/// <summary>
	/// Notification that a new episode will start.
	/// </summary>
	/// <param name="episode">The episode that will start.</param>
	public void EpisodeWillStart(uint newEpisode) {
		episodeIsEnding = false;
	}

	/// <summary>
	/// Notification that a new episode did start.
	/// </summary>
	/// <param name="episode">The episode that did start.</param>
	public void EpisodeDidStart(uint newEpisode) {
		// Do nothing.
	}

}