using UnityEngine;
using System.Collections;

public class customMesh : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        //tutorial
        MeshRect();
	}

    void MeshRect()
    {
        MeshFilter mFilter = gameObject.GetComponent<MeshFilter>();
        MeshRenderer mRender = gameObject.GetComponent<MeshRenderer>();

        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(5, 0, 0);
        vertices[2] = new Vector3(5, 5, 0);
        vertices[3] = new Vector3(0, 5, 0);

        int[] triangles = new int[6] { 0, 2, 1, 2, 0, 3 };

        Vector3[] normals = new Vector3[4];
        normals[0] = new Vector3(0, 0, -1);
        normals[1] = new Vector3(0, 0, -1);
        normals[2] = new Vector3(0, 0, -1);
        normals[3] = new Vector3(0, 0, -1);

        Mesh mesh = new Mesh();

        mesh.hideFlags = HideFlags.DontSave;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        //mesh.normals = normals;

        mFilter.mesh = mesh;
    }



    // Update is called once per frame
    void Update ()
    {
	
	}
}
