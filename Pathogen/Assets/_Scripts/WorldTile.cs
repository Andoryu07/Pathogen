using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New World Tile", menuName = "Tiles/World Tile")]
public class WorldTile : Tile
{
    public bool isWalkable;
    public int movementCost = 1;
    public bool slowsPlayer;
    // Add any other logic-specific data here
}