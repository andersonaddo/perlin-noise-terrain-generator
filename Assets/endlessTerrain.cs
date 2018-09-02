using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endlessTerrain : MonoBehaviour {

    [HideInInspector]public static MapGenerator generator;
    public Material meshMaterial;

    public const float viewerSightLimit = 500; //How far the viewer can see 
    public Transform viewer, chunkParent;

    public static Vector2 viewerPosition;
    int chunkSize; //Size of the meshes (in vertices per side)
    int chunksVisibleInViewerDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastFrame = new List<TerrainChunk>();

    void Start()
    {
        generator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewerDistance = Mathf.RoundToInt(viewerSightLimit / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        //In this method, we're not looking at chunks with their world positions, but their relative positions
        //So  the chunk directly north-east of the chunk at the world origin will have  a relative cord
        //of (1,1)
        int currentChunkX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        foreach (TerrainChunk terrain in terrainChunksVisibleLastFrame) terrain.setVisible(false);
        terrainChunksVisibleLastFrame.Clear();

        //Looking at surrounding chunks that should be visible to the player
        for (int yOffset = -chunksVisibleInViewerDistance; yOffset <= chunksVisibleInViewerDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewerDistance; xOffset <= chunksVisibleInViewerDistance; xOffset++)
            {
                Vector2 viewableChunkCoord = new Vector2(xOffset + currentChunkX, yOffset + currentChunkY);

                //Checking if this chunk that should be visible already exists
                if (terrainChunks.ContainsKey(viewableChunkCoord))
                {
                    terrainChunks[viewableChunkCoord].UpdateVisibility();
                    terrainChunksVisibleLastFrame.Add(terrainChunks[viewableChunkCoord]);
                }
                else
                {
                    terrainChunks.Add(viewableChunkCoord, new TerrainChunk(viewableChunkCoord, chunkSize, chunkParent, meshMaterial));
                }
            }
        }

    }



    public class TerrainChunk
    {
        GameObject meshObject;
        Bounds worldBounds;
        Vector3 worldPosition;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        public TerrainChunk(Vector2 relativeCoord, int size, Transform parent, Material material)
        {
            worldPosition = new Vector3(relativeCoord.x * size, 0, relativeCoord.y * size);

            meshObject = new GameObject("Terrain Chunk");
            meshFilter = meshObject.AddComponent<MeshFilter>(); //These two will get their data from some multithreading callbacks soon
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;

            meshObject.transform.position = worldPosition;
            setVisible(false); //We'll enable this in the next frame

            worldBounds = new Bounds(relativeCoord * size, Vector2.one * size); //Bounds calculations will only take X and Z world axes into consideration, hence the 2D position

            meshObject.transform.SetParent(parent);

            generator.GenerateMapDataAsync(OnMapDataReceived);
        }

        void OnMapDataReceived(MapData data)
        {
            generator.GenerateMeshDataAsync(data, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData data)
        {
            meshFilter.mesh = data.produceMesh();
        }

        //Disables itself if far enough
        public void UpdateVisibility()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(worldBounds.SqrDistance(viewerPosition));
            bool shouldBeVisible = viewerDistanceFromNearestEdge <= viewerSightLimit;
            setVisible(shouldBeVisible);
        }

        public void setVisible(bool shouldBeVisible)
        {
            meshObject.SetActive(shouldBeVisible);
        }

        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
    }

}
