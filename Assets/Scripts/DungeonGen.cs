using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap for floors
    public Tilemap wallTilemap; // Reference to the Tilemap for walls (backdrop)
    public RuleTile roomRuleTile; // RuleTile for floors
    public RuleTile hallwayRuleTile; // RuleTile for hallways
    public RuleTile wallRuleTile; // RuleTile for walls (backdrop)
    public Vector2Int mapSize = new Vector2Int(30, 30); // Size of the dungeon
    public int minRoomSize = 5; // Minimum room size
    public int maxRoomSize = 10; // Maximum room size
    public int maxRooms = 10; // Maximum number of rooms

    private List<RectInt> rooms = new List<RectInt>(); // List to store room positions

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        tilemap.ClearAllTiles(); // Clear any previous floor tiles
        wallTilemap.ClearAllTiles(); // Clear any previous wall tiles
        rooms.Clear();

        // Generate starting room at (0, 0)
        RectInt startRoom = CreateRoom(Vector2Int.zero);
        rooms.Add(startRoom);
        DrawRoom(startRoom);

        // Generate additional rooms
        for (int i = 0; i < maxRooms; i++)
        {
            Vector2Int newRoomPosition = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
            RectInt newRoom = CreateRoom(newRoomPosition);

            // Ensure the room doesn't overlap with existing rooms
            if (!RoomOverlaps(newRoom))
            {
                rooms.Add(newRoom);
                DrawRoom(newRoom);
                CreateHallway(rooms[Random.Range(0, rooms.Count)], newRoom);
            }
        }

        // Add wall tiles around the rooms
        AddWallBackdrop();
    }

    RectInt CreateRoom(Vector2Int position)
    {
        int roomWidth = Random.Range(minRoomSize, maxRoomSize);
        int roomHeight = Random.Range(minRoomSize, maxRoomSize);

        // Ensure the room fits within the map bounds
        position.x = Mathf.Clamp(position.x, 0, mapSize.x - roomWidth);
        position.y = Mathf.Clamp(position.y, 0, mapSize.y - roomHeight);

        return new RectInt(position.x, position.y, roomWidth, roomHeight);
    }

    bool RoomOverlaps(RectInt newRoom)
    {
        foreach (var room in rooms)
        {
            if (newRoom.Overlaps(room)) return true;
        }
        return false;
    }

    void DrawRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), roomRuleTile);
            }
        }
    }

    void CreateHallway(RectInt from, RectInt to)
    {
        Vector2Int fromCenter = new Vector2Int(from.xMin + from.width / 2, from.yMin + from.height / 2);
        Vector2Int toCenter = new Vector2Int(to.xMin + to.width / 2, to.yMin + to.height / 2);

        // Randomly determine whether to create a vertical or horizontal hallway first
        if (Random.Range(0, 2) == 0)
        {
            CreateHorizontalHallway(fromCenter.x, toCenter.x, fromCenter.y);
            CreateVerticalHallway(fromCenter.y, toCenter.y, toCenter.x);
        }
        else
        {
            CreateVerticalHallway(fromCenter.y, toCenter.y, fromCenter.x);
            CreateHorizontalHallway(fromCenter.x, toCenter.x, toCenter.y);
        }
    }

    void CreateHorizontalHallway(int xStart, int xEnd, int y)
    {
        for (int x = Mathf.Min(xStart, xEnd); x <= Mathf.Max(xStart, xEnd); x++)
        {
            tilemap.SetTile(new Vector3Int(x, y, 0), hallwayRuleTile);
        }
    }

    void CreateVerticalHallway(int yStart, int yEnd, int x)
    {
        for (int y = Mathf.Min(yStart, yEnd); y <= Mathf.Max(yStart, yEnd); y++)
        {
            tilemap.SetTile(new Vector3Int(x, y, 0), hallwayRuleTile);
        }
    }

    // Method to add wall backdrop tiles around the rooms
    void AddWallBackdrop()
    {
        foreach (var room in rooms)
        {
            // Place wall tiles around the room (as a border)
            for (int x = room.xMin - 1; x <= room.xMax; x++)
            {
                for (int y = room.yMin - 1; y <= room.yMax; y++)
                {
                    if (x == room.xMin - 1 || x == room.xMax || y == room.yMin - 1 || y == room.yMax)
                    {
                        wallTilemap.SetTile(new Vector3Int(x, y, 0), wallRuleTile);
                    }
                }
            }
        }
    }
}
