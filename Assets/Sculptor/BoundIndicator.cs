using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
public class BoundIndicator : MonoBehaviour {

    private int extent = 1;
    Mesh indicatorMesh;


	// Use this for initialization
	void Start () {

        ProceduralTerrainVolume volume = this.gameObject.GetComponent<ProceduralTerrainVolume>();
        if (volume)
        {
            extent = volume.planetRadius;
        }

        indicatorMesh = CreatePlaneMesh();
    }

    public void Show()
    {
        GetComponent<MeshFilter>().mesh = indicatorMesh;
    }

    public void Hide()
    {
        GetComponent<MeshFilter>().mesh = null;
    }

    Mesh CreatePlaneMesh()
    {
        Mesh mesh = new Mesh();
        //vertices
        Vector3[] vertices = new Vector3[]
        {
            new Vector3( extent, -extent,  extent),
            new Vector3( extent, -extent, -extent),
            new Vector3(-extent, -extent,  extent),
            new Vector3(-extent, -extent, -extent),

            new Vector3( extent, -extent,  extent),
            new Vector3( extent, -extent, -extent),
            new Vector3( extent,  extent,  extent),
            new Vector3( extent,  extent, -extent),

            new Vector3( extent, -extent, -extent),
            new Vector3(-extent, -extent, -extent),
            new Vector3( extent,  extent, -extent),
            new Vector3(-extent,  extent, -extent),

            new Vector3(-extent, -extent,  extent),
            new Vector3(-extent, -extent, -extent),
            new Vector3(-extent,  extent,  extent),
            new Vector3(-extent,  extent, -extent),

            new Vector3( extent, -extent,  extent),
            new Vector3(-extent, -extent,  extent),
            new Vector3( extent,  extent,  extent),
            new Vector3(-extent,  extent,  extent),

            new Vector3( extent,  extent,  extent),
            new Vector3( extent,  extent, -extent),
            new Vector3(-extent,  extent,  extent),
            new Vector3(-extent,  extent, -extent),
        };
        //UV
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1),

            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1),

            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1),

            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1),

            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1),

            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1),
        };
        //index
        int[] triangles = new int[72];
        for (int tempi = 0; tempi < 6; tempi++)
        {
            triangles[12 * tempi] = tempi * 4;
            triangles[12 * tempi + 1] = tempi * 4 + 2;
            triangles[12 * tempi + 2] = tempi * 4 + 1;
            triangles[12 * tempi + 3] = tempi * 4 + 1;
            triangles[12 * tempi + 4] = tempi * 4 + 2;
            triangles[12 * tempi + 5] = tempi * 4 + 3;
            triangles[12 * tempi + 6] = tempi * 4 + 0;
            triangles[12 * tempi + 7] = tempi * 4 + 1;
            triangles[12 * tempi + 8] = tempi * 4 + 2;
            triangles[12 * tempi + 9] = tempi * 4 + 1;
            triangles[12 * tempi + 10] = tempi * 4 + 3;
            triangles[12 * tempi + 11] = tempi * 4 + 2;
        }

        //0, 2, 1,
        //1, 2, 3,
        //0, 1, 2,
        //1, 3, 2,

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }

    // Update is called once per frame
    void Update () {
	
	}
}
