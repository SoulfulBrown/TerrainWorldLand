using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private int density = 1;

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
                vertices[i] = new Vector3((float)(x) / density, 0  , (float)(z) / density); 
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
                    var y = Mathf.PerlinNoise(x * 0.3f, z*0.3f)  * 2.0f;            
                    vertices[i] = new Vector3((float)(x) / density, y, (float)(z) / density);
                }
                i++;
            }
        }

        //lerp in density points
        //TODO: This curently looks at the next on the x and not the y make it do the thing looking at neighbours
        for (int i = 0, z = 0; z <= zSize * density; z++)
        {
            var nextVector = 0;
            var currentVector = 0;
            for (int x = 0; x <= xSize * density; x++)
            {         
                if(x % density == 0 && z % density == 0)
                {
                    nextVector = i + density;
                    currentVector = i;
                }
                else
                {
                    vertices[i] = new Vector3((float)(x) / density, vertices[currentVector].y - ((vertices[currentVector].y - vertices[nextVector].y) / (nextVector-currentVector)), (float)(z) / density); 
                    currentVector = i;
                }

                i++;
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

