using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Uses object pooling for the infinite effect
public class endlessTerrain : MonoBehaviour {

    public levelOfDetailLimit[] LODLimits;
    public static float viewerSightLimit; //How far the viewer can see. Automtically set to the last limit of the LODLimits array
    public float meshChunkScale = 1;
    public static float _meshChunkScale;

    public Transform viewer;
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
    static List<TerrainChunk> terrainChunksVisibleLastFrame = new List<TerrainChunk>();

    void Start()
    {
        generator = FindObjectOfType<MapGenerator>();
        _meshChunkScale = meshChunkScale;

        viewerSightLimit = LODLimits[LODLimits.Length - 1].distanceUpperBound;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewerDistance = Mathf.RoundToInt(viewerSightLimit / chunkSize);

        distanceThresholdSquared = moveDistanceUpdateThreshold * moveDistanceUpdateThreshold;
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        oldViewerPosition = viewerPosition;

        int chunksActiveAroundViewer = (int) Mathf.Pow(((chunksVisibleInViewerDistance * 2) + 1), 2);
        GetComponent<TerrainChunkPooler>().setUp(Mathf.RoundToInt(chunksActiveAroundViewer * 1.2f)); //* 1.5f to create surplus to prevent any edge case scenarios

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / _meshChunkScale;
        if ((oldViewerPosition - viewerPosition).sqrMagnitude > distanceThresholdSquared) //Faster than vecotor3.distance() since we don't have to constantly get the square root
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

        //Removing chunks not visisble to the player...

        Bounds playerVisibilityBounds = new Bounds(new Vector2(currentChunkX, currentChunkY), Vector2.one * (chunksVisibleInViewerDistance * 2 + 1));

        foreach (TerrainChunk chunk in terrainChunksVisibleLastFrame)
        {       
            if (!playerVisibilityBounds.Contains(chunk.relativeCoord))
            {
                chunk.disableTCObject();
            }
        }

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
                }
                else
                {
                    terrainChunks.Add(viewableChunkCoord, new TerrainChunk(viewableChunkCoord, chunkSize, LODLimits, meshMaterial));
                }
            }
        }

    }



    public class TerrainChunk
    {
        bool isVisible = true;

        Bounds worldBounds;
        Vector3 worldPosition;

        public readonly Vector2 relativeCoord;

        Texture recievedTexture;
        Material material;

        TerrainChunkObject assignedTCObject; //Will be assigned to when we get a mesh

        MapData mapData;
        bool hasRecievedMapData;

        levelOfDetailLimit[] LODLimits;
        LODMesh[] levelOfDetailMeshes;

        int currentLevelOfDetailIndex = -1;

        public TerrainChunk(Vector2 relativeCoord, int size, levelOfDetailLimit[] LODLimits, Material material)
        {
            this.LODLimits = LODLimits;

            worldPosition = new Vector3(relativeCoord.x * size, 0, relativeCoord.y * size);

            this.relativeCoord = relativeCoord;

            this.material = material;

            worldBounds = new Bounds(relativeCoord * size, Vector2.one * size); //Bounds calculations will only take X and Z world axes into consideration, hence the 2D position

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

            recievedTexture = TextureGenerator.GenerateColorTexture(mapData.noiseMap, mapData.colorMap);

            UpdateVisibilityOrLOD(); //TO start rendering our chunk now
        }

        //Disables itself if far enough
        public void UpdateVisibilityOrLOD()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(worldBounds.SqrDistance(viewerPosition));
            bool shouldBeVisible = viewerDistanceFromNearestEdge <= viewerSightLimit;


            if (!shouldBeVisible && assignedTCObject != null) //Needed because this method can also be called from callbacks
            {
                assignedTCObject.release();
                assignedTCObject = null;
            }

            //Going through all the LOD limits (but the last one, because the player cannot see past that one, and choosing the corrent LOD
            if (shouldBeVisible && hasRecievedMapData)
            {
                int lodIndex = 0;

                //Finding the correct LOD...
                for (int i = 0; i < LODLimits.Length - 1; i++)
                {
                    if (viewerDistanceFromNearestEdge > LODLimits[i].distanceUpperBound)
                        lodIndex = i + 1;
                    else break;
                }

                
                if (shouldBeVisible && assignedTCObject == null)
                {
                    assignedTCObject = FindObjectOfType<TerrainChunkPooler>().supplyTCObject();
                    setUpTCObject();
                }

                if (currentLevelOfDetailIndex != lodIndex)
                {
                    LODMesh lodMesh = levelOfDetailMeshes[lodIndex];
                    if (lodMesh.hasRecievedMesh)
                    {
                        currentLevelOfDetailIndex = lodIndex;
                        assignedTCObject.setMesh(lodMesh.mesh);
                    }
                    else if (!lodMesh.hasRequestedMesh) lodMesh.RequestMesh(mapData); 
                    //Otherwise we'll just have to wait and see if data has been recieved yet
                }

                if (shouldBeVisible) terrainChunksVisibleLastFrame.Add(this);
            }
        }

        public void setUpTCObject()
        {
            assignedTCObject.setMaterial(material);
            assignedTCObject.setTexture(recievedTexture);
            assignedTCObject.transform.position = worldPosition * _meshChunkScale;
            assignedTCObject.transform.localScale = Vector3.one * _meshChunkScale;
        }

        public void disableTCObject()
        {
            if (assignedTCObject == null) return;
            assignedTCObject.release();
            assignedTCObject = null;
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
