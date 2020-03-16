using UnityEngine;
using System.Collections;

/// <summary>
/// The GuiMovingObstalce is an UI representation of a <see cref="T:MovingObstacle"/> object.
/// </summary>
public class GuiMovingObstacle : MonoBehaviour, MovingObstacle.IMovingObstacleObserver {


	/// <summary>
	/// The <see cref="T:GuiGarden"/>, that houses this <see cref="T:GuiMower"/>.
	/// </summary>
	private GuiGarden guiGarden;

	/// <summary>
	/// The <see cref="T:MovingObstacle"/> that is represented by this <see cref="T:GuiMovingObstacle"/>.
	/// </summary>
	private MovingObstacle movingObstacle;

	public GuiGarden GuiGarden {
		get {
			return guiGarden;
		}
		private set {
			guiGarden = value;
		}
	}

	public MovingObstacle MovingObstacle {
		get {
			return movingObstacle;
		}
		private set {
			movingObstacle = value;
		}
	}		

	/// <summary>
	/// Initializes this <see cref="T:GuiMovingObstacle"/>.
	/// </summary>
	/// <param name="guiGarden">The surrounding <see cref="T:GuiGarden"/>.</param>
	/// <param name="movingObstacle">The <see cref="T:MovingObstacle/> that is represented by this object.</param>
	public void Init(GuiGarden guiGarden, MovingObstacle movingObstacle) {
		this.GuiGarden = guiGarden;
		this.MovingObstacle = movingObstacle;
		//set initial position
		this.MoveToPosition(MovingObstacle.Position);
		MovingObstacle.AddObserver(this);
	}
		
	/// <summary>
	/// Notification that the obstacle moved from one position to another.
	/// </summary>
	/// <param name="obstacle">The moving obstacle.</param>
	/// <param name="oldPosition">Its old position.</param>
	/// <param name="newPosition">Its new position.</param>
	public void MovingObstacleMoved(MovingObstacle obstacle, Garden.GridPosition oldPosition, Garden.GridPosition newPosition) {
		MoveToPosition(newPosition);
	}


	/// <summary>
	/// Moves this object to the given position on the 2D grid.
	/// </summary>
	/// <param name="gridPosition">Grid position.</param>
	private void MoveToPosition(Garden.GridPosition gridPosition) {
		this.transform.position = GuiGarden.GridPositionToUnityPosition(gridPosition);
	}
}
