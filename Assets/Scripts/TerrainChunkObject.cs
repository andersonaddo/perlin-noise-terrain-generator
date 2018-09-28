using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TerrainChunkObject : MonoBehaviour {

    public bool isReserved { get; private set; }

    public void reserve() {  //Called by pooler

        gameObject.SetActive(true);
        isReserved = true;
    }

    public void release() //Called by TerrainChunks
    {
        gameObject.SetActive(false);
        isReserved = false;
    }

    public void setMesh(Mesh mesh)
    {
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void setMaterial(Material mat)
    {
        GetComponent<MeshRenderer>().material = mat;
    }

    public void setTexture(Texture texture)
    {
        GetComponent<MeshRenderer>().material.mainTexture = texture;
    }
}
