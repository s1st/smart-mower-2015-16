using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The GuiGarden is an UI representation of a <see cref="T:Garden"/> object.
/// </summary>
public class GuiGarden : MonoBehaviour, Garden.IGardenObserver {

	/// <summary>
	/// The <see cref="T:Garden"/>that is represented by this <see cref="T:GuiGarden"/>.
	/// </summary>
	private Garden garden;

	/// <summary>
	/// Prefab for a <see cref="T:GuiTile"/> game object.
	/// </summary>
	public GameObject TilePrefab;

	/// <summary>
	/// The renderer width of a <see cref="T:GuiTile"/> game object.
	/// </summary>
	private float tilePrefabWidth;

	/// <summary>
	/// The renderer height of a <see cref="T:GuiTile"/> game object.
	/// </summary>
	private float tilePrefabHeight;

	/// <summary>
	/// Prefab for a <see cref="GuiMower"/> game object.
	/// </summary>
	public GameObject MowerPrefab;

	/// <summary>
	/// Prefab for a <see cref="GuiMovingObject"/> game object of type "Animal".
	/// </summary>
	public GameObject AnimalPrefab;

	/// <summary>
	/// A list of <see cref="T:GuiTile"/> game objects the <see cref="T:GuiGarden"/> consists of.
	/// </summary>
	private List<GameObject> tiles = new List<GameObject>();

	public Garden Garden {
		get {
			return garden;
		}
		private set {
			garden = value;
		}
	}

	public float TilePrefabWidth {
		get {
			return tilePrefabWidth;
		}
		private set {
			tilePrefabWidth = value;
		}
	}

	public float TilePrefabHeight {
		get {
			return tilePrefabHeight;
		}
		private set {
			tilePrefabHeight = value;
		}
	}

	private List<GameObject> Tiles {
		get {
			return tiles;
		}
	}

	/// <summary>
	/// Initialize this <see cref="T:GuiGarden"/>.
	/// </summary>
	/// <description>
	/// Note: This Method has to be called once before Start() gets called.
	/// </description>
	/// <param name="garden">The <see cref="T:Garden"/> used for initialization.</param>
	public void Init(Garden garden) {
        this.garden = garden;

    }
    
    /// <summary>
    /// Creates the initial garden UI.
    /// </summary>
	void Start () {
        Application.runInBackground = true;

        
		// Extract size from tile prefab
		TilePrefabWidth = TilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
		TilePrefabHeight = TilePrefab.GetComponent<SpriteRenderer>().bounds.size.y;
		// Initi GuiTiles
        for (int row = 0; row < garden.GardenHeigth; row++)
        {
            for (int col = 0; col < garden.GardenWidth; col++)
            {
				Garden.GridPosition gridPosition = new Garden.GridPosition(col, row);
				GameObject go = (GameObject)Instantiate(TilePrefab, transform.position, Quaternion.identity);
				go.transform.SetParent(this.transform);
				go.layer = this.gameObject.layer;
				go.transform.position = GridPositionToUnityPosition(gridPosition);
                go.GetComponent<GuiTile>().SetColorForState(garden.GetTileStatus(gridPosition));

                tiles.Add(go);
            }
        }
		Garden.AddObserver(this);

		// Init GuiMower
		GameObject mowerGo = Instantiate(MowerPrefab);
		mowerGo.GetComponent<GuiMower>().Init(this);

		// Init GuiMovingObstacles
		foreach (var movingObstacle in Garden.MovingObstacles) {
			GameObject obstacleGO = Instantiate(AnimalPrefab);
			obstacleGO.GetComponent<GuiMovingObstacle>().Init(this, movingObstacle);
		}
	}


	/// <summary>
	/// Gets the <see cref="T:GuiTile"/> game object at the given logical <see cref="T:Garden.GridPosition"/>.
	/// </summary>
	/// <returns>The <see cref="T:GuiTile"/> game object at the given position.</returns>
	/// <param name="gridPosition">The <see cref="T:Garden.GridPosition"/>.</param>
	public GuiTile GuiTileAtPosition(Garden.GridPosition gridPosition) {
		int index = gridPosition.Y * (int)Garden.GardenWidth + gridPosition.X;
		if (Garden.IsValidPosition(gridPosition) && index < Tiles.Count) {
			return Tiles[index].GetComponent<GuiTile>();
		} else {
			return null;
		}
	}

	/// <summary>
	/// Converts a logical <see cref="T:Garden.GridPosition"/> into an absolute position in the Unity coordinate space.
	/// </summary>
	/// <description>
	/// Note: Do not call this method prior to Start() being called on this <see cref="T:GuiGarden"/> object.
	/// </description>
	/// <returns>The absolute position in Unity coordinate space.</returns>
	/// <param name="gridPosition">The <see cref="T:Garden.GridPosition"/> to convert.</param>
	public Vector3 GridPositionToUnityPosition(Garden.GridPosition gridPosition) {
		Vector3 position = transform.position + new Vector3(-(float)(garden.GardenWidth) / 
			2f * TilePrefabWidth, (garden.GardenHeigth) / 2f * TilePrefabHeight, transform.position.z);
		position = position + new Vector3(gridPosition.X * TilePrefabWidth, -gridPosition.Y * TilePrefabHeight);
		return position;
	}

	/// <summary>
	/// Notification that the mow status of the given tile changed.
	/// </summary>
	/// <param name="tile">The tile where the change occured.</param>
	/// <param name="oldMowStatus">Its old mow status.</param>
	/// <param name="newMowStatus">Its new mow status.</param>
	public void MowStatusOfTileChanged(Tile tile, Tile.MowStatus oldMowStatus, Tile.MowStatus newMowStatus) {
		GuiTile guiTile = GuiTileAtPosition(tile.Position);
		if (! guiTile) {
			Debug.Log(string.Format("Tile at position {0} has no representing GuiTile!", tile.Position));
		} else {
			guiTile.SetColorForState(newMowStatus);
		}
	}
}
