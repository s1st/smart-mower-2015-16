using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using UnityEngine;

/// <summary>
/// Factory class to create objects of the <see cref="T:Garden"/> class from a XML file.
/// </summary>
public class GardenFactory {

	/// <summary>
	/// Invalid garden XML file exception.
	/// </summary>
	[Serializable]
	public class InvalidGardenXMLException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="T:InvalidGardenXMLException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public InvalidGardenXMLException (string message) : base (message) {
		
		}
	}

	//-- All XML-node and XML-attribute names that are allowed in the garden XML file --//

	private const string gardenNodeName = "Garden";
	private const string startPosNodeName = "MowerStartPosition";
	private const string tilesNodeName = "Tiles";
	private const string tileNodeName = "Tile";
	private const string movingObstaclesNodeName = "MovingObstacles";
	private const string movingObstacleNodeName = "MovingObstacle";

	private const string widthAttributeName = "width";
	private const string heightAttributeName = "height";
	private const string xPosAttributeName = "x";
	private const string yPosAttributeName = "y";
	private const string typeAttributeName = "type";

	//-- All constants that are allowed in the garden XML file --//

	private const string movingObstacleTypeAnimal = "Animal";
	private const string movingObstacleTypePerson = "Person";

	private const string tileTypeLongGrass = "Long grass";
	private const string tileTypeShortGrass = "Short grass";
	private const string tileTypeRock = "Rock";
	private const string tileTypeWater = "Water";
	private const string tileTypeChargingStation = "Charging station";

	/// <summary>
	/// Creates a Garden object using a xml-based representation.
	/// </summary>
	/// <returns>The garden from the xml file.</returns>
	/// <param name="filename">The filename of the xml-file defining the garden.</param>
	/// <param name="paramSet">The set of parameters for this project.</param>
	public static Garden CreateGardenFromFile(string filename, ParamSet paramSet) {

		// Init the xml document
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(filename);

		// Init containers for the extracted information
		uint gardenWidth, gardenHeight;
		Garden.GridPosition mowerStartPosition;
		List<Tile> tiles;
		List<MovingObstacle> movingObstacles;
		List<StaticObstacle> staticObstacles = new List<StaticObstacle>();

		bool parseResult = true;
		XmlNode gardenNode = xmlDoc.SelectSingleNode("/" + gardenNodeName);

		// Parse the garden
		if(gardenNode == null) {
			throw new InvalidGardenXMLException(MissingNodeErrorMessage(gardenNodeName));
		}
		XmlNode widthAttribute = gardenNode.SelectSingleNode("@" + widthAttributeName);
		if (widthAttribute == null) {
			throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(gardenNodeName, widthAttributeName));
		}
		parseResult = uint.TryParse(widthAttribute.InnerText, out gardenWidth);
		XmlNode heightAttribute = gardenNode.SelectSingleNode("@" + heightAttributeName);
		if (heightAttribute == null) {
			throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(gardenNodeName, heightAttributeName));
		}
		parseResult = uint.TryParse(heightAttribute.InnerText, out gardenHeight);

		// Parse the mower starting position
		XmlNode startPosNode = gardenNode.SelectSingleNode(startPosNodeName);
		if(startPosNode == null) {
			throw new InvalidGardenXMLException(MissingNodeErrorMessage(startPosNodeName));
		}
		XmlNode xAttribute = startPosNode.SelectSingleNode("@" + xPosAttributeName);
		if (xAttribute == null) {
			throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(startPosNodeName, xPosAttributeName));
		}
		uint x,y;
		parseResult = uint.TryParse(xAttribute.InnerText, out x);
		XmlNode yAttribute = startPosNode.SelectSingleNode("@" + yPosAttributeName);
		if (yAttribute == null) {
			throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(startPosNodeName, yPosAttributeName));
		}
		parseResult = uint.TryParse(yAttribute.InnerText, out y);
		mowerStartPosition = new Garden.GridPosition((int)x, (int)y);

		// Parse the tiles
		XmlNodeList tileNodes = gardenNode.SelectNodes(tilesNodeName + "/" + tileNodeName);
		if (tileNodes.Count != (gardenWidth * gardenHeight)) {
			throw new InvalidGardenXMLException(string.Format("Only found {0} <" + tileNodeName + "> nodes. Expecting {1}.", tileNodes.Count, gardenWidth * gardenHeight));
		}
		tiles = new List<Tile>(tileNodes.Count);
		foreach (XmlNode tileNode in tileNodes) {
			xAttribute = tileNode.SelectSingleNode("@" + xPosAttributeName);
			if (xAttribute == null) {
				throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(tileNodeName, xPosAttributeName));
			}
			parseResult = uint.TryParse(xAttribute.InnerText, out x);
			yAttribute = tileNode.SelectSingleNode("@" + yPosAttributeName);
			if (yAttribute == null) {
				throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(tileNodeName, yPosAttributeName));
			}
			parseResult = uint.TryParse(yAttribute.InnerText, out y);
			XmlNode typeAttribute = tileNode.SelectSingleNode("@" + typeAttributeName);
			if (typeAttribute == null) {
				throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(tileNodeName, typeAttributeName));
			}
			Garden.GridPosition position = new Garden.GridPosition((int)x, (int)y);
			Tile.MowStatus tileStatus = Tile.MowStatus.LongGrass;
			switch (typeAttribute.InnerText) {
			case tileTypeLongGrass:
				tileStatus = Tile.MowStatus.LongGrass;
				break;
			case tileTypeShortGrass:
				tileStatus = Tile.MowStatus.ShortGrass;
				break;
			case tileTypeRock:
				tileStatus = Tile.MowStatus.Obstacle;
				staticObstacles.Add(new StaticObstacle(position, StaticObstacle.StaticObstacleType.Rock));
				break;
			case tileTypeWater:
				tileStatus = Tile.MowStatus.Obstacle;
				staticObstacles.Add(new StaticObstacle(position, StaticObstacle.StaticObstacleType.Water));
				break;
			case tileTypeChargingStation:
				tileStatus = Tile.MowStatus.ChargingStation;
				break;
			default:
				throw new InvalidGardenXMLException(
					InvalidAttributeValueInNodeErrorMessage(tileNodeName, new KeyValuePair<string, string>(typeAttributeName, typeAttribute.InnerText))
				);
			}
			bool occupiedByMower = position.Equals(mowerStartPosition);
			tiles.Add(new Tile(position, tileStatus, occupiedByMower));
		}

		// Parse the moving obstacles
		XmlNodeList obstacleNodes = gardenNode.SelectNodes(movingObstaclesNodeName + "/" + movingObstacleNodeName);
		movingObstacles = new List<MovingObstacle>(obstacleNodes.Count);
		foreach (XmlNode obstacleNode in obstacleNodes) {
			xAttribute = obstacleNode.SelectSingleNode("@" + xPosAttributeName);
			if (xAttribute == null) {
				throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(movingObstacleNodeName, xPosAttributeName));
			}
			parseResult = uint.TryParse(xAttribute.InnerText, out x);
			yAttribute = obstacleNode.SelectSingleNode("@" + yPosAttributeName);
			if (yAttribute == null) {
				throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(movingObstacleNodeName, yPosAttributeName));
			}
			parseResult = uint.TryParse(yAttribute.InnerText, out y);
			XmlNode typeAttribute = obstacleNode.SelectSingleNode("@" + typeAttributeName);
			if (typeAttribute == null) {
				throw new InvalidGardenXMLException(MissingAttributeInNodeErrorMessage(movingObstacleNodeName, typeAttributeName));
			}
			Garden.GridPosition position = new Garden.GridPosition((int)x, (int)y);
			MovingObstacle.MovingObstacleType obstacleType = MovingObstacle.MovingObstacleType.Animal;
			switch (typeAttribute.InnerText) {
			case movingObstacleTypeAnimal:
				obstacleType = MovingObstacle.MovingObstacleType.Animal;
				break;
			case movingObstacleTypePerson:
				obstacleType = MovingObstacle.MovingObstacleType.Person;
				break;
			default:
				throw new InvalidGardenXMLException(
					InvalidAttributeValueInNodeErrorMessage(movingObstacleNodeName, new KeyValuePair<string, string>(typeAttributeName, typeAttribute.InnerText))
				);
			}
			MovingObstacle obstacle = new MovingObstacle(position, obstacleType);
			// Register the tile as occupied
			foreach (var tile in tiles) {
				if (tile.Position.Equals(position)) {
					if (tile.Occupied == true) {
						throw new InvalidGardenXMLException(string.Format("Multiple objects positioned at {0}.", position));
					} else {
						tile.Occupied = true;
					}
					break;
				}
			}
			movingObstacles.Add(obstacle);

			// Save the initial state of all tiles
			foreach (var tile in tiles) {
				tile.SetCurrentStateAsInitialState();
			}
		}

		// Create the garden (finally...)
		return new Garden(gardenWidth, 
			gardenHeight,
			tiles,
			mowerStartPosition,
			movingObstacles,
			staticObstacles,
			paramSet);

	}
		
	/// <summary>
	/// Convenience method to create an error message when a node was not found in the XML file.
	/// </summary>
	/// <returns>The node-not-found error message.</returns>
	/// <param name="nodeName">The name of the node that was not found.</param>
	private static string MissingNodeErrorMessage(string nodeName) {
		return "Node <" + nodeName + "> not found.";
	}

	/// <summary>
	/// Convenience method to create an error message when an attribute in an XML node was not found.
	/// </summary>
	/// <returns>The attribute-not-found error message.</returns>
	/// <param name="nodeName">The name of the node that did not contain the desired attribute.</param>
	/// <param name="attributeName">The name of the attribute that was missing.</param>
	private static string MissingAttributeInNodeErrorMessage(string nodeName, string attributeName) {
		return "Attribute \"" + attributeName + "\" in Node <\" + nodeName + \"> not found.";
	}

	/// <summary>
	/// Convenience method to create an error message when an attribute whith an invalid value was found in an XML node.
	/// </summary>
	/// <returns>The invalid-attribute-value error message.</returns>
	/// <param name="nodeName">The name of the node that contained the invalid attribute value.</param>
	/// <param name="attribute">Key-value pair of the name of the attribute in question and its invalid value.</param>
	private static string InvalidAttributeValueInNodeErrorMessage(string nodeName, KeyValuePair<string, string> attribute) {
		return "Value \"" + attribute.Value + "\" invalid for Attribute \"" + attribute.Key + "\" in Node <" + nodeName + ">.";
	}

}
