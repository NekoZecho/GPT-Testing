using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class TilemapNavMesh : MonoBehaviour
{
    public Tilemap tilemap;  // Reference to your tilemap
    public NavMeshSurface navMeshSurface;  // Reference to the NavMeshSurface
    public float updateInterval = 0.1f;  // Interval to refresh the NavMesh after tilemap generation

    void Start()
    {
        // Assuming your tilemap generation happens here, if not, trigger the generation
        GenerateTilemap(); // Your procedural generation function or code

        // Bake the NavMesh after the tilemap is generated
        UpdateNavMesh();
    }

    void GenerateTilemap()
    {
        // Your procedural tilemap generation code goes here.
        // After generating the new layout, the NavMesh needs to be rebaked.
    }

    void UpdateNavMesh()
    {
        // Rebuild the NavMesh surface to adapt to the new tilemap layout
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("NavMeshSurface is not assigned!");
        }
    }

    // Optionally, you can bake the NavMesh periodically, for example, after every few tilemap updates
    void Update()
    {
        // If your game requires dynamic updates to the tilemap, you can call this in Update
        // to bake the NavMesh periodically. This is just an example.
        // You could also hook this up to an event when the tilemap is regenerated.
        if (Time.time % updateInterval < 0.1f)
        {
            UpdateNavMesh();
        }
    }
}