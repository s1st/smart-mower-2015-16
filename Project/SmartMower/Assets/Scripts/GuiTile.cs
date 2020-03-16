using UnityEngine;
using System.Collections;

/// <summary>
/// The GuiTile is an UI representation of a <see cref="T:Tile"/> object.
/// </summary>
public class GuiTile : MonoBehaviour {

	/// <summary>
	/// The sprite renderer of the attached game object.
	/// </summary>
    SpriteRenderer spriteRenderer;

	/// <summary>
	/// Gets or sets the color of this GuiTile.
	/// </summary>
	/// <value>The color of this GuiTile.</value>
    public Color Color
    {
        get
        {
            return spriteRenderer.color;
        }

        set
        {
            spriteRenderer.color = value;
        }
    }

	/// <summary>
	/// Basic initialisation.
	/// </summary>
    void Awake ()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

	/// <summary>
	/// Sets the color of this GuiTile according to the given tile status.
	/// </summary>
	/// <param name="status">The tile status to determine the color from.</param>
    public void SetColorForState(Tile.MowStatus status)
    {
        switch (status)
        {
            case Tile.MowStatus.LongGrass:
                this.Color = Color.green;
                break;
            case Tile.MowStatus.ShortGrass:
                this.Color = Color.yellow;
                break;
			case Tile.MowStatus.Obstacle:
                this.Color = Color.grey;
                break;
            case Tile.MowStatus.ChargingStation:
                this.Color = Color.black;
                break;
        }
    }
}
