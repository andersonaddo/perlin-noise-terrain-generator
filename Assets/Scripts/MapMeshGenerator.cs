using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapMeshGenerator {
    
    public static MeshData GenerateMesh(float[,] map, int levelOfDetail, AnimationCurve _heightMappingCurve, float heightMultiplier)
    {
        AnimationCurve heightMappingCurve = new AnimationCurve(_heightMappingCurve.keys); //We had to do this because for some reason _heightMappingCurve messes up when acessed from multiple threads, 
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        int vertixStep = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / vertixStep + 1; //Assuming that width = height, which should be the case

        float topLeftX = (width - 1) / -2f; //The top left X of the mesh if it is centered around the origin
        float topLeftZ = (height - 1) / 2f;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int currentVertexIndex = 0;

        for (int y = 0; y < height; y += vertixStep)
            for (int x = 0; x < width; x += vertixStep)
            {
                meshData.vertices[currentVertexIndex] = new Vector3(topLeftX + x, heightMappingCurve.Evaluate(map[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[currentVertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.addTriangle(currentVertexIndex, currentVertexIndex + verticesPerLine + 1, currentVertexIndex + verticesPerLine); //Triangles count their vertices clockwise
                    meshData.addTriangle(currentVertexIndex + verticesPerLine + 1, currentVertexIndex, currentVertexIndex + 1);
                }

                currentVertexIndex++;
            }
            

        return meshData; //Returning the meshData instead of the mesh to allow for multithreading
    }

}


/// <summary>
/// A class to hold mesh infromation for particular noise map
/// </summary>
public class MeshData{
    public Vector3[] vertices; //The positions of the vertices
    public Vector2[] uvs; //Contains vectors for the position that each vertix will represent on the mesh's texture. Each vector contains only percentages, so x and y have a max of 1
    public int[] triangles; //This array contains the indices of the positions in the veritces array that make up the meshe's triangles. Each set of 3 ints represents the vertices of a single triangle

    int nextTriangeIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshHeight * meshWidth];
        uvs = new Vector2[meshHeight * meshWidth];
        triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
    }

    public void addTriangle(int verticeA, int verticeB, int verticeC)
    {
        triangles[nextTriangeIndex] = verticeA;
        triangles[nextTriangeIndex + 1] = verticeB;
        triangles[nextTriangeIndex + 2] = verticeC;
        nextTriangeIndex += 3;
    }

    public Mesh produceMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals(); //For correct lighting
        return mesh;
    }
}
