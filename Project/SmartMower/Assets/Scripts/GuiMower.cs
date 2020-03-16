using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The GuiMower is an UI representation of a <see cref="T:Mower"/> object.
/// </summary>
public class GuiMower : MonoBehaviour, Mower.IMowerObserver {

	/// <summary>
	/// The <see cref="T:GuiGarden"/>, that houses this <see cref="T:GuiMower"/>.
	/// </summary>
	private GuiGarden guiGarden;

	/// <summary>
	/// The <see cref="T:Mower"/> that is represented by this <see cref="T:GuiMower"/>.
	/// </summary>
	private Mower mower;

	public GuiGarden GuiGarden {
		get {
			return guiGarden;
		}
		private set {
			guiGarden = value;
			mower = guiGarden.Garden.Mower;
		}
	}

	public Mower Mower {
		get {
			return mower;
		}
	}

	/// <summary>
	/// Initializes this <see cref="T:GuiMower"/>.
	/// </summary>
	/// <param name="guiGarden">The <see cref="T:GuiGarden/> where this <see cref="T:GuiMower"/> lives in.</param>
    public void Init(GuiGarden guiGarden) {
		this.GuiGarden = guiGarden;
		//set initial position
		this.MoveToPosition(Mower.Position);
		Mower.AddObserver(this);
    }
		

	/// <summary>
	/// Notification that the mower moved from one position to another.
	/// </summary>
	/// <param name="mower">The mower.</param>
	/// <param name="oldPosition">Its old position.</param>
	/// <param name="newPosition">Its new position.</param>
	public void MowerMoved(Mower mower, Garden.GridPosition oldPosition, Garden.GridPosition newPosition) {
		this.MoveToPosition(newPosition);
	}

	/// <summary>
	/// Moves to the given <see cref="T:Garden.GridPosition"/>.
	/// </summary>
	/// <param name="gridPosition">The <see cref="T:Garden.GridPosition"/> to move to.</param>
	private void MoveToPosition(Garden.GridPosition gridPosition) {
		this.transform.position = GuiGarden.GridPositionToUnityPosition(gridPosition);
	}
		
}
