using UnityEngine;
//using UnityEditor;
using System.Collections;

public class grassCreator : MonoBehaviour
{
    public float length = 60, step = 1, offset = 0, billboardMRadius = 1.0f;
    public Terrain targetTerrain;

    MeshFilter mFilter;
    Vector3[] vertices = new Vector3[12101];  // rad * height
    Vector2[] uvs = new Vector2[12101];
    Vector3[] normals = new Vector3[12101];
    int[] indices = new int[36303];           // rad * height * 3

    // Use this for initialization
    void Start()
    {
        mFilter = gameObject.GetComponent<MeshFilter>();

        DrawGrass();

        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.None;
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs;
        mesh.normals = normals;

        mFilter.mesh = mesh;

        //AssetDatabase.CreateAsset(mesh, "Assets/grassSmall.asset");
    }

    private void DrawGrass()
    {
        int indicesCount = 0;
        float x, y;
        int verticesCount = 0;

        for (x = offset + transform.position.x; x < length + transform.position.x; x += step)
        {
            for (y = offset + transform.position.z; y < length + transform.position.z; y += step)
            {
                float billboardRadius = billboardMRadius * Random.Range(0.8f, 1.2f);
                Quaternion rotation = Quaternion.AngleAxis(Random.Range(-180f, 180f), Vector3.up);

                Vector2 tmp = Random.insideUnitCircle;
                Vector3 centerPos = new Vector3(x - transform.position.x, targetTerrain.SampleHeight(new Vector3(x, 0, y)) + billboardRadius - 0.3f, y - transform.position.z) + 
                    new Vector3(tmp.x, 0, tmp.y);

                vertices[verticesCount] = rotation * new Vector3(-billboardRadius, -billboardRadius, 0) + centerPos;
                uvs[verticesCount] = new Vector2(0f, 0f);
                normals[verticesCount] = rotation * Vector3.forward;
                verticesCount++;

                vertices[verticesCount] = rotation * new Vector3(-billboardRadius, +billboardRadius, 0) + centerPos;
                uvs[verticesCount] = new Vector2(0f, 1.0f);
                normals[verticesCount] = rotation * Vector3.forward;
                verticesCount++;

                vertices[verticesCount] = rotation * new Vector3(+billboardRadius, +billboardRadius, 0) + centerPos;
                uvs[verticesCount] = new Vector2(1.0f, 1.0f);
                normals[verticesCount] = rotation * Vector3.forward;
                verticesCount++;

                vertices[verticesCount] = rotation * new Vector3(+billboardRadius, -billboardRadius, 0) + centerPos;
                uvs[verticesCount] = new Vector2(1.0f, 0f);
                normals[verticesCount] = rotation * Vector3.forward;
                verticesCount++;

                indices[indicesCount++] = verticesCount - 4;
                indices[indicesCount++] = verticesCount - 2;
                indices[indicesCount++] = verticesCount - 3;
                indices[indicesCount++] = verticesCount - 4;
                indices[indicesCount++] = verticesCount - 1;
                indices[indicesCount++] = verticesCount - 2;

                indices[indicesCount++] = verticesCount - 4;
                indices[indicesCount++] = verticesCount - 3;
                indices[indicesCount++] = verticesCount - 2;
                indices[indicesCount++] = verticesCount - 4;
                indices[indicesCount++] = verticesCount - 2;
                indices[indicesCount++] = verticesCount - 1;
            }
        }

        Debug.Log("Render summary: Vertices count = " + verticesCount + " Indices count = " + indicesCount + " (grassCreator)");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
