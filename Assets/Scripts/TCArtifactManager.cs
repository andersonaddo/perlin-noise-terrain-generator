using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Each TerrainChunk has an instance of this to generate & manage it artifacts
/// </summary>
public class TCArtifactManager
{
    bool hasGeneratedArtifacts;
    MapData mapData;
    Biome biome;
    int preferredLOD = 0; //Only meshes that have reached this LOD get their artifacts generated/shown

    Vector3 centerOfReference; //Center of the terrain chunk mesh
    Vector2 relativeChunkCoord;
    float chunkScale;

    List<GameObject> artifacts = new List<GameObject>();
    Transform artifactParent;

    public TCArtifactManager(MapData mapData, Biome biome, int preferredLOD, Vector3 center, float scale, Vector2 relativeChunkCoord)
    {
        this.mapData = mapData;
        this.biome = biome;
        this.preferredLOD = preferredLOD;
        centerOfReference = center;
        this.relativeChunkCoord = relativeChunkCoord;
        this.chunkScale = scale;
    }

    //Called inside of enableArtifacts
    public void generateArtifacts()
    {
        int numOfArtifacts = Mathf.RoundToInt(MapGenerator.mapChunkSize * biome.artifactChancePerVertix);
        numOfArtifacts += Random.Range(-5, 5);
        numOfArtifacts = Mathf.Clamp(numOfArtifacts, 1, MapGenerator.mapChunkSize);

        List<Vector3> points = selectPoints(numOfArtifacts);

        artifactParent = new GameObject().transform;
        artifactParent.gameObject.name = "Artifacts TC " + relativeChunkCoord;
        artifactParent.position = centerOfReference;
        artifactParent.SetParent(ArtifactGenerationManager._artifactSuperParent);

        //Will hold the candidate artifacts for each point
        List<BiomeArtifact> candidateArtifacts = new List<BiomeArtifact>();
        
        //Gotten from mesh generator script
        float topLeftX = (MapGenerator.mapChunkSize - 1) / -2f; //The top left X of the mesh if it is centered around the origin
        float topLeftZ = (MapGenerator.mapChunkSize - 1) / 2f;

        foreach (Vector3 point in points)
        {
            candidateArtifacts.Clear();

            foreach (BiomeArtifact artifact in biome.artifacts)
                if (point.z >= artifact.perlinValueLowerBound && point.z <= artifact.perlinValueUpperBound)
                    candidateArtifacts.Add(artifact);

            if (candidateArtifacts.Count != 0)
            {
                GameObject chosenArtifact = candidateArtifacts[Random.Range(0, candidateArtifacts.Count)].gameObject;
                GameObject instanciatedChosenArtifact = Object.Instantiate(chosenArtifact, new Vector3 (topLeftX + point.x, 0, topLeftZ - point.y ) * chunkScale, Quaternion.identity);
                instanciatedChosenArtifact.transform.Translate(centerOfReference);
                instanciatedChosenArtifact.transform.Translate(Vector3.up * biome.heightMultiplier * biome.heightMultiplierCurve.Evaluate(point.z) * chunkScale);
                instanciatedChosenArtifact.transform.localScale *= chunkScale;
                artifacts.Add(instanciatedChosenArtifact);
                instanciatedChosenArtifact.transform.SetParent(artifactParent);
            }
        }
    }

    /// <summary>
    /// Only actually generates and/or displays the artifats if the LOD = the preferred LOD
    /// </summary>
    public void enableArtifacts(int LOD)
    {
        if (LOD > preferredLOD) return;
        if (biome.artifactChancePerVertix == 0 || biome.artifacts.Count == 0) return;
        if (!hasGeneratedArtifacts)
        {
            generateArtifacts();
            hasGeneratedArtifacts = true;
        }
        else
        {
            artifactParent.gameObject.SetActive(true);
        }
    }

    public void disableAtifacts()
    {
        if (hasGeneratedArtifacts) artifactParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Randomly selects a certain number of points from the map in mapData
    /// </summary>
    /// <param name="numOfArtifacts"></param>
    /// <returns>A List of vector3s. The z coord in these vectors repesent the perlin value at that point in the map</returns>
    List<Vector3> selectPoints(int numOfArtifacts)
    {
        List<Vector3> points = new List<Vector3>();
        int successfulSelections = 0;
        while (successfulSelections < numOfArtifacts)
        {
            int x = Random.Range(0, MapGenerator.mapChunkSize);
            int y = Random.Range(0, MapGenerator.mapChunkSize);
            Vector3 point = new Vector3(x, y, mapData.noiseMap[x, y]);
            if (points.Contains(point)) continue;
            points.Add(point);
            successfulSelections++;
        }

        return points;
    }
}

