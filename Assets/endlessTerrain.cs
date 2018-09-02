using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endlessTerrain : MonoBehaviour {

    public levelOfDetailLimit[] LODLimits;
    public static float viewerSightLimit; //How far the viewer can see. Automtically set to the last limit of the LODLimits array

    public Transform viewer, chunkParent;
    public static Vector2 viewerPosition;
    Vector2 oldViewerPosition;

    [Tooltip("The player must have moved more than this distance before we update the terrain chunks")]
    [SerializeField] private float moveDistanceUpdateThreshold = 25;
    float distanceThresholdSquared;

    [HideInInspector]public static MapGenerator generator;
    public Material meshMaterial;

    int chunkSize; //Size of the meshes (in vertices per side)
    int chunksVisibleInViewerDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastFrame = new List<TerrainChunk>();

    void Start()
    {
        generator = FindObjectOfType<MapGenerator>();

        viewerSightLimit = LODLimits[LODLimits.Length - 1].distanceUpperBound;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewerDistance = Mathf.RoundToInt(viewerSightLimit / chunkSize);

        distanceThresholdSquared = moveDistanceUpdateThreshold * moveDistanceUpdateThreshold;
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        oldViewerPosition = viewerPosition;
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if ((oldViewerPosition - viewerPosition).sqrMagnitude > distanceThresholdSquared) //Faster than vecotor3.distance()
        {
            oldViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    public void UpdateVisibleChunks()
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
                    terrainChunks[viewableChunkCoord].UpdateVisibilityOrLOD();
                    terrainChunksVisibleLastFrame.Add(terrainChunks[viewableChunkCoord]);
                }
                else
                {
                    terrainChunks.Add(viewableChunkCoord, new TerrainChunk(viewableChunkCoord, chunkSize, LODLimits, chunkParent, meshMaterial));
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

        MapData mapData;
        bool hasRecievedMapData;

        levelOfDetailLimit[] LODLimits;
        LODMesh[] levelOfDetailMeshes;

        int currentLevelOfDetailIndex = -1;

        public TerrainChunk(Vector2 relativeCoord, int size, levelOfDetailLimit[] LODLimits, Transform parent, Material material)
        {
            this.LODLimits = LODLimits;

            worldPosition = new Vector3(relativeCoord.x * size, 0, relativeCoord.y * size);

            meshObject = new GameObject("Terrain Chunk");
            meshFilter = meshObject.AddComponent<MeshFilter>(); //These two will get their data from some multithreading callbacks soon
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;

            meshObject.transform.position = worldPosition;
            setVisible(false); //We'll enable this in the next frame

            worldBounds = new Bounds(relativeCoord * size, Vector2.one * size); //Bounds calculations will only take X and Z world axes into consideration, hence the 2D position
            meshObject.transform.SetParent(parent);

            levelOfDetailMeshes = new LODMesh[LODLimits.Length];
            for (int i = 0; i < levelOfDetailMeshes.Length; i++)
            {
                levelOfDetailMeshes[i] = new LODMesh(LODLimits[i].levelOfDetail, UpdateVisibilityOrLOD);
            }

            generator.GenerateMapDataAsync(new Vector2(relativeCoord.x * size, relativeCoord.y * size), OnMapDataReceived);
        }

        void OnMapDataReceived(MapData data)
        {
            this.mapData = data;
            hasRecievedMapData = true;

            Texture mapTexture = TextureGenerator.GenerateColorTexture(mapData.noiseMap, mapData.colorMap);
            meshRenderer.material.mainTexture = mapTexture;

            UpdateVisibilityOrLOD(); //TO start rendering our chunk now
        }

        //Disables itself if far enough
        public void UpdateVisibilityOrLOD()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(worldBounds.SqrDistance(viewerPosition));
            bool shouldBeVisible = viewerDistanceFromNearestEdge <= viewerSightLimit;
            setVisible(shouldBeVisible);

            //Going through all the LOD limits (but the last one, because the player cannot see past that one, and choosing the corrent LOD
            if (shouldBeVisible && hasRecievedMapData)
            {
                int lodIndex = 0;

                for (int i = 0; i < LODLimits.Length - 1; i++)
                {
                    if (viewerDistanceFromNearestEdge > LODLimits[i].distanceUpperBound)
                        lodIndex = i + 1;
                    else break;
                }

                if (currentLevelOfDetailIndex != lodIndex)
                {
                    LODMesh lodMesh = levelOfDetailMeshes[lodIndex];
                    if (lodMesh.hasRecievedMesh)
                    {
                        currentLevelOfDetailIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh) lodMesh.RequestMesh(mapData); 
                    //Otherwise we'll just have to wait and see if data has been recieved yet
                }
            }
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


    /// <summary>
    /// Used by instances of TerrainChunk to hold different meshes for different levels of detail
    /// </summary>
    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh, hasRecievedMesh;
        int meshLevelOfDetail;

        //To allow us to manually call UpdateVisibleChunks() when mesh data is recieved, since the only other chance to call that would be when the player has moved significantly
        System.Action updateCallback; 

        public LODMesh(int meshLevelOfDetail, System.Action updateCallback)
        {
            this.meshLevelOfDetail = meshLevelOfDetail;
            this.updateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            generator.GenerateMeshDataAsync(mapData, OnMeshDataRecieved, meshLevelOfDetail);
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            hasRecievedMesh = true;
            mesh = meshData.produceMesh();
            updateCallback();
        }
    }

    //A list of these will be used to determine the level of detail of meshes based off their distance from the viewer
    [System.Serializable]
    public struct levelOfDetailLimit
    {
        [Tooltip("Shoud be 0-6")]
        public int levelOfDetail;
        public float distanceUpperBound;
    }

}
