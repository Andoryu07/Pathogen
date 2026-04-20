using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Tilemap Layers")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap obstacleTilemap;

    public static MapManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Get the data of a tile at a specific world position
    public WorldTile GetTileAtPosition(Vector3 worldPosition)
    {
        // Convert world position to Cell position
        Vector3Int cellPosition = floorTilemap.WorldToCell(worldPosition);

        // Prioritize obstacles, then floor
        TileBase tile = obstacleTilemap.GetTile(cellPosition);
        if (tile == null)
        {
            tile = floorTilemap.GetTile(cellPosition);
        }

        return tile as WorldTile;
    }

    public bool IsPositionWalkable(Vector3 worldPosition)
    {
        WorldTile tile = GetTileAtPosition(worldPosition);
        return tile != null && tile.isWalkable;
    }
}