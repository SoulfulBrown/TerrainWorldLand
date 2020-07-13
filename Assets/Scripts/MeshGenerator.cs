using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;
using UnityEngine.Rendering;

public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    [SerializeField] Vector3[] vertices;
    [SerializeField] int[] triangles;

    [SerializeField] private int xSize = 20;
    [SerializeField] private int zSize = 20;
    [SerializeField][Range(1,2)] private int density = 1;

    private int prevX = 0, prevZ = 0, prevD = 0;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        if(prevX == xSize && prevZ == zSize  && prevD == density) { return ; }
        if(zSize <= 0 || xSize <= 0 || density <= 0) { return; }

        CreateShape();
        UpdateMesh();

        prevZ = zSize;
        prevX = xSize;    
        prevD = density;
    }

    public void GenerateMesh()
    {
        CreateShape();
        UpdateMesh();
    }

    private void CreateShape()
    {
        vertices = new Vector3 [((xSize * density) + 1) * ((zSize * density) + 1)];
        
        //create flat grid
        for (int i = 0, z = 0; z <= zSize * density; z++)
        {
            for (int x = 0; x <= xSize * density; x++)
            {         
                vertices[i] = new Vector3((float)(x) / density, 0, (float)(z) / density); 
                i++;
            }
        }

        //create spikes at standard points
        for (int i = 0, z = 0; z <= zSize * density; z++)
        {
            for (int x = 0; x <= xSize * density ; x++)
            {
                if( x % density == 0 && z % density == 0)
                {
                    var y = Mathf.PerlinNoise((x/density)*0.3f, (z/density)*0.3f)  * 2.0f;            
                    vertices[i] = new Vector3((float)(x) / density, y, (float)(z) / density);
                }
                i++;
            }
        }

        if(density > 1)
        {
            var nextVector = 0;
            var currentVector = 0;
            
            var quads = new int[(xSize * density) * (zSize * density) *4];

            int qVert = 0, quad = 0;

            for(int z = 0; z < (zSize * density); z++)
            {
                for (int x = 0; x < (xSize * density); x++)
                {
                    quads[quad + 0] = qVert + 0;
                    quads[quad + 1] = qVert + xSize* density + 1;
                    quads[quad + 2] = qVert + xSize* density + 2;
                    quads[quad + 3] = qVert + 1;
                    qVert++;
                    quad += 4;
                }
                qVert++;
            }

            //centre points
            for (int i = 0, z = 0; z <= zSize ; z += density)
            {  
                for (int x = 0; x <= xSize ; x += density)
                {         
                    var centerIndex = i + xSize * density + 2;
                    var yAvg = (vertices[centerIndex + xSize * density].y + vertices[centerIndex - xSize * density].y  + vertices[centerIndex + xSize * density + 2].y + vertices[centerIndex - (xSize * density + 2)].y) /4;
                    vertices[centerIndex] = new Vector3(vertices[centerIndex].x, yAvg, vertices[centerIndex].z);
                    i+=density;           
                }
            }

            //centre neighbours
            for (int i = 0, z = 0; z <= zSize ; z += density)
            {  
                for (int x = 0; x <= xSize ; x += density)
                {         
                    var leftIndex = i + (xSize * density) + 1;
                    var rightIndex = i + (xSize * density + 1) + 2;
                    var upIndex = i + 1 + ((xSize * density + 1)* 2);
                    var downIndex = i + 1;
                    //left
                    vertices[leftIndex] = new Vector3(vertices[leftIndex].x, 
                                                    (vertices[leftIndex + 1].y + vertices[leftIndex - xSize * density - 1].y  + vertices[leftIndex + xSize * density + 1].y) /3,
                                                    vertices[leftIndex].z);
                    //right
                    vertices[rightIndex] = new Vector3(vertices[rightIndex].x, 
                                                    (vertices[rightIndex - 1].y + vertices[rightIndex - xSize * density - 1].y  + vertices[rightIndex + xSize * density + 1].y) /3,
                                                    vertices[rightIndex].z);   
                    //up
                    vertices[upIndex] = new Vector3(vertices[upIndex].x, 
                                                    (vertices[upIndex - 1].y + vertices[upIndex + 1].y  + vertices[upIndex - xSize * density - 1].y) /3,
                                                    vertices[upIndex].z);  
                    //down
                    vertices[downIndex] = new Vector3(vertices[downIndex].x, 
                                                    (vertices[downIndex - 1].y + vertices[downIndex + 1].y  + vertices[downIndex + xSize * density + 1].y) /3,
                                                    vertices[downIndex].z); 
                }
            }
        }

        
        triangles = new int[(xSize * density) * (zSize * density) * 6];
        int vert = 0, tris = 0;

        for(int z = 0; z < (zSize * density); z++)
        {
            for (int x = 0; x < (xSize * density); x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize* density + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize * density + 1;
                triangles[tris + 5] = vert + xSize * density + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
        
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    // private void OnDrawGizmos() 
    // {
    //     if(vertices == null) { return ; }
    //     foreach(Vector3 v in vertices)
    //     {
    //         Gizmos.DrawSphere(v,0.25f/density);
    //     }    
    // }
}

