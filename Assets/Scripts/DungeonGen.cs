using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    public Vector2Int startRoomSize = new Vector2Int(10, 10);
    public Vector2Int roomMinSize = new Vector2Int(5, 5);
    public Vector2Int roomMaxSize = new Vector2Int(15, 15);

    public int hallwayWidth = 2;
    public int maxHallwayLength = 10;
    public int numberOfRooms = 10;

    private List<RectInt> rooms = new List<RectInt>();
    private HashSet<Hallway> existingHallways = new HashSet<Hallway>();

    public int roomPadding = 4;          // Minimum spacing between rooms
    public int placementRange = 50;       // Range within which rooms are placed

    public List<Region> regions = new List<Region>();
    public TileBase[] floorRegionTiles;
    public TileBase[] wallRegionTiles;

    void Start()
    {
        GenerateRegions();
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        Vector2Int currentPosition = Vector2Int.zero;
        // Generate starting room at the player's position
        RectInt startRoom = new RectInt(currentPosition - startRoomSize / 2, startRoomSize);
        rooms.Add(startRoom);
        CreateRoom(startRoom);

        // Generate additional rooms
        for (int i = 1; i < numberOfRooms; i++)
        {
            RectInt newRoom = GenerateRoom();
            if (newRoom.width == 0 || newRoom.height == 0) continue; // Skip invalid rooms

            // Find the nearest room to connect
            RectInt nearestRoom = FindNearestRoom(newRoom);

            // Get random points in the nearest and new rooms for hallway start/end
            Vector2Int hallwayStart = new Vector2Int(
                Mathf.FloorToInt(GetRandomPointInRoom(nearestRoom).x),
                Mathf.FloorToInt(GetRandomPointInRoom(nearestRoom).y)
            );

            Vector2Int hallwayEnd = new Vector2Int(
                Mathf.FloorToInt(GetRandomPointInRoom(newRoom).x),
                Mathf.FloorToInt(GetRandomPointInRoom(newRoom).y)
            );

            // Normalize the hallway to avoid duplicates
            Hallway newHallway = new Hallway(hallwayStart, hallwayEnd).Normalize();

            // Check if the hallway already exists or if there's an already existing hallway between the rooms in the same direction
            if (!existingHallways.Contains(newHallway) && !CheckForParallelHallways(newHallway, nearestRoom, newRoom))
            {
                // Create hallway and add it to the set of existing hallways
                CreateHallway(hallwayStart, hallwayEnd);
                existingHallways.Add(newHallway);
            }

            // Create the new room
            CreateRoom(newRoom);
            rooms.Add(newRoom);
        }

        // Add walls around rooms and hallways
        CreateWalls();
    }


    void GenerateRegions()
    {
        int numberOfRegions = Random.Range(2, 5); // Random number of regions
        for (int i = 0; i < numberOfRegions; i++)
        {
            Vector2Int size = new Vector2Int(
                Random.Range(roomMinSize.x, roomMaxSize.x),
                Random.Range(roomMinSize.y, roomMaxSize.y)
            );

            Vector2Int position = new Vector2Int(
                Random.Range(-placementRange, placementRange),
                Random.Range(-placementRange, placementRange)
            );

            Rect regionBounds = new Rect(position, size);
            if (!DoesRegionOverlap(regionBounds))
            {
                TileBase floorTile = floorRegionTiles[Random.Range(0, floorRegionTiles.Length)];
                TileBase wallTile = wallRegionTiles[Random.Range(0, wallRegionTiles.Length)];

                regions.Add(new Region(regionBounds, floorTile, wallTile));

                Debug.Log($"Created Region at {regionBounds.position} with size {regionBounds.size}");
            }
        }
    }


    bool DoesRegionOverlap(Rect newRegion)
    {
        foreach (Region region in regions)
        {
            if (region.Bounds.Overlaps(newRegion))
            {
                return true;
            }
        }
        return false;
    }


    bool CheckForParallelHallways(Hallway newHallway, RectInt room1, RectInt room2)
    {
        // Check for parallel hallways in the same direction
        foreach (Hallway existingHallway in existingHallways)
        {
            // Ensure we're not comparing the same hallway
            if (existingHallway.IsEqual(newHallway))
                continue;

            // Check if both hallways are connected to the same rooms in the same direction
            if ((existingHallway.start.Equals(room1.center) && existingHallway.end.Equals(room2.center)) ||
                (existingHallway.start.Equals(room2.center) && existingHallway.end.Equals(room1.center)))
            {
                if (existingHallway.GetDirection() == newHallway.GetDirection())
                {
                    return true; // Parallel hallways found
                }
            }
        }

        return false;
    }


    // Method to find the nearest room from a given room
    RectInt FindNearestRoom(RectInt room)
    {
        RectInt nearestRoom = rooms[0];
        float minDistance = float.MaxValue;

        foreach (RectInt existingRoom in rooms)
        {
            // Convert Vector2Int to Vector2 for distance calculation
            float distance = Vector2.Distance((Vector2)room.center, (Vector2)existingRoom.center);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestRoom = existingRoom;
            }
        }

        return nearestRoom;
    }


    RectInt GenerateRoom(int maxAttempts = 10)
    {
        int attemptPadding = roomPadding;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2Int size = new Vector2Int(
                Random.Range(roomMinSize.x, roomMaxSize.x),
                Random.Range(roomMinSize.y, roomMaxSize.y)
            );

            Vector2Int position = new Vector2Int(
                Random.Range(-placementRange, placementRange),
                Random.Range(-placementRange, placementRange)
            );

            RectInt room = new RectInt(position, size);

            // Create padded bounds for overlap checking
            RectInt paddedRoom = new RectInt(
                room.xMin - attemptPadding,
                room.yMin - attemptPadding,
                room.width + attemptPadding * 2,
                room.height + attemptPadding * 2
            );

            // Check for overlap with existing rooms
            bool overlaps = false;
            foreach (RectInt existingRoom in rooms)
            {
                RectInt paddedExistingRoom = new RectInt(
                    existingRoom.xMin - attemptPadding,
                    existingRoom.yMin - attemptPadding,
                    existingRoom.width + attemptPadding * 2,
                    existingRoom.height + attemptPadding * 2
                );

                if (paddedRoom.Overlaps(paddedExistingRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            // If no overlap, return the room
            if (!overlaps)
            {
                return room;
            }

            // Reduce padding after a few failed attempts to increase placement chances
            if (attempt > maxAttempts / 2 && attemptPadding > 0)
            {
                attemptPadding--;
            }
        }

        // Log warning if max attempts are reached
        Debug.LogWarning("Failed to generate a non-overlapping room with padding after max attempts.");
        return new RectInt(Vector2Int.zero, Vector2Int.zero);
    }




    void CreateRoom(RectInt room)
    {
        TileBase floor = GetFloorTileForPosition(room.center);

        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floor);
            }
        }
    }

    void CreateHallway(Vector2Int start, Vector2Int end)
    {
        Vector2Int currentPosition = start;
        TileBase floor = GetFloorTileForPosition(currentPosition);

        bool moveHorizontallyFirst = Random.value > 0.5f;

        if (moveHorizontallyFirst)
        {
            while (currentPosition.x != end.x)
            {
                int step = currentPosition.x < end.x ? 1 : -1;
                currentPosition.x += step;
                floor = GetFloorTileForPosition(currentPosition);

                for (int j = -hallwayWidth / 2; j <= hallwayWidth / 2; j++)
                {
                    floorTilemap.SetTile(new Vector3Int(currentPosition.x, currentPosition.y + j, 0), floor);
                }
            }

            while (currentPosition.y != end.y)
            {
                int step = currentPosition.y < end.y ? 1 : -1;
                currentPosition.y += step;
                floor = GetFloorTileForPosition(currentPosition);

                for (int j = -hallwayWidth / 2; j <= hallwayWidth / 2; j++)
                {
                    floorTilemap.SetTile(new Vector3Int(currentPosition.x + j, currentPosition.y, 0), floor);
                }
            }
        }
        else
        {
            while (currentPosition.y != end.y)
            {
                int step = currentPosition.y < end.y ? 1 : -1;
                currentPosition.y += step;
                floor = GetFloorTileForPosition(currentPosition);

                for (int j = -hallwayWidth / 2; j <= hallwayWidth / 2; j++)
                {
                    floorTilemap.SetTile(new Vector3Int(currentPosition.x + j, currentPosition.y, 0), floor);
                }
            }

            while (currentPosition.x != end.x)
            {
                int step = currentPosition.x < end.x ? 1 : -1;
                currentPosition.x += step;
                floor = GetFloorTileForPosition(currentPosition);

                for (int j = -hallwayWidth / 2; j <= hallwayWidth / 2; j++)
                {
                    floorTilemap.SetTile(new Vector3Int(currentPosition.x, currentPosition.y + j, 0), floor);
                }
            }
        }
    }

    TileBase GetFloorTileForPosition(Vector2 position)
    {
        foreach (Region region in regions)
        {
            if (region.Bounds.Contains(position))
            {
                Debug.Log($"Using region tile at {position}");
                return region.FloorTile;
            }
        }
        Debug.Log($"Using default tile at {position}");
        return floorTile; // Default tile
    }




    Vector2 GetRandomPointInRoom(RectInt room)
    {
        return new Vector2(
            Random.Range(room.xMin + 1, room.xMax - 1),
            Random.Range(room.yMin + 1, room.yMax - 1)
        );
    }


    void CreateWalls()
    {
        BoundsInt bounds = floorTilemap.cellBounds;

        // Iterate through all tiles in the bounds of the floor tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // If the current tile is a floor tile
                if (floorTilemap.HasTile(pos))
                {
                    // Place walls above (2 tiles thick)
                    for (int dy = 1; dy <= 2; dy++)
                    {
                        Vector3Int wallPos = pos + new Vector3Int(0, dy, 0);
                        if (!floorTilemap.HasTile(wallPos) && !wallTilemap.HasTile(wallPos))
                        {
                            wallTilemap.SetTile(wallPos, wallTile);
                        }
                    }

                    // Place walls below (1 tile thick)
                    Vector3Int bottomWallPos = pos + new Vector3Int(0, -1, 0);
                    if (!floorTilemap.HasTile(bottomWallPos) && !wallTilemap.HasTile(bottomWallPos))
                    {
                        wallTilemap.SetTile(bottomWallPos, wallTile);
                    }

                    // Place walls to the left (1 tile thick)
                    Vector3Int leftWallPos = pos + new Vector3Int(-1, 0, 0);
                    if (!floorTilemap.HasTile(leftWallPos) && !wallTilemap.HasTile(leftWallPos))
                    {
                        wallTilemap.SetTile(leftWallPos, wallTile);
                    }

                    // Place walls to the right (1 tile thick)
                    Vector3Int rightWallPos = pos + new Vector3Int(1, 0, 0);
                    if (!floorTilemap.HasTile(rightWallPos) && !wallTilemap.HasTile(rightWallPos))
                    {
                        wallTilemap.SetTile(rightWallPos, wallTile);
                    }
                }
            }
        }
    }

}
public struct Hallway
{
    public Vector2Int start;
    public Vector2Int end;
    public enum Direction { Horizontal, Vertical }

    public Hallway(Vector2Int start, Vector2Int end)
    {
        this.start = start;
        this.end = end;
    }

    public Direction GetDirection()
    {
        if (start.x == end.x)
            return Direction.Vertical;
        else
            return Direction.Horizontal;
    }

    // Normalize the hallway: Always store it in a consistent order.
    public Hallway Normalize()
    {
        if (start.x > end.x || (start.x == end.x && start.y > end.y))
        {
            Vector2Int temp = start;
            start = end;
            end = temp;
        }
        return this;
    }

    public bool IsEqual(Hallway other)
    {
        return this.start.Equals(other.start) && this.end.Equals(other.end);
    }

    public override bool Equals(object obj)
    {
        if (obj is Hallway)
        {
            Hallway other = (Hallway)obj;
            return this.IsEqual(other);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return start.GetHashCode() ^ end.GetHashCode();
    }
}
public class Region
{
    public Rect Bounds;
    public TileBase FloorTile;
    public TileBase WallTile;

    public Region(Rect bounds, TileBase floorTile, TileBase wallTile)
    {
        Bounds = bounds;
        FloorTile = floorTile;
        WallTile = wallTile;
    }
}