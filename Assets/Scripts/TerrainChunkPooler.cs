using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkPooler : MonoBehaviour {

    public Transform chunkParent;

    List<TerrainChunkObject> terrainChunks = new List<TerrainChunkObject>();

    public void setUp(int size)
    {
        for (int i = 0; i < size; i++)
        {
            GameObject chunk = new GameObject();
            chunk.SetActive(false);
            terrainChunks.Add(chunk.AddComponent<TerrainChunkObject>());
            chunk.name = "TerrainChunk";
            chunk.transform.SetParent(chunkParent);
        }
    }

    public TerrainChunkObject supplyTCObject()
    {
        foreach (TerrainChunkObject t in terrainChunks)
        {
            if (!t.isReserved)
            {
                t.reserve();
                return t;
            }
        }
        return null;
    }
}
