using UnityEngine;
using System.Collections;

using Assets.FractalSystemCore;
using Assets.FractalSystemCore.NodeInherits;
using System.Collections.Generic;

public enum FractalType
{
    boxTest,
    treeVer1,
    treeVer1_ReducedVertices,
    treeVer2Cyc_ConcretedNormals,
    treeVer3Cyc_Spline,
    treeVer3_G,
    treeVer4,
    treeVer4_Frond
}

public class fractalRenderer : MonoBehaviour
{
    FractalSystemNode startNode;
    public int iterationMax = 10;
    public float iterGrowRate = 1.0f;

    public const int verticesMax = 60000;//normals, uvs, tangents = vertices
    public const int indicesMax = 524286;

    public FractalType fractalType = FractalType.treeVer4_Frond;
    public float startGrowRate = 128.0f;

    public bool renderNormals = true;
    public bool renderUV1s = true;
    public bool renderUV2s = true;
    public bool renderUV3s = true;
    public bool renderUV4s = true;
    public bool renderTangents = true;

    public int fractalMode = 0;

    public bool noUpdate = true;

    public int randomSeed = 1219;

    int stoppedCount = 0;//DEBUGGING VARIABLE

    MeshFilter mFilter;
    Vector3[] vertices = new Vector3[verticesMax];
    List<int[]> indices = new List<int[]>();
    Vector3[] normals = new Vector3[verticesMax];
    Vector2[] uvs = new Vector2[verticesMax];
    Vector2[] uv2s = new Vector2[verticesMax];
    Vector2[] uv3s = new Vector2[verticesMax];
    Vector2[] uv4s = new Vector2[verticesMax];
    Vector4[] tangents = new Vector4[verticesMax];
    int verticesCount = 0, normalsCount = 0, tmp;
    List<int> indicesCount = new List<int>();

    // Use this for initialization
    void Start()
    {
        ReDraw();
    }

    public void Descript(FractalSystemNode node, int depth)
    {
        if (node.growRate < iterGrowRate)
        {
            stoppedCount++;
            node.ClearNode();
            return;
        }
        if (depth >= iterationMax)
        {
            //因为在这个工程中，只会渲染整棵树，所以不需要下面串起来的链表。
            node.ClearNode();
            return;
        }

        if (node.child.Count == 0)//这个节点还没有展开过
        {
            node.generateChildren();
        }
        //node.updateChildren();
        foreach (FractalSystemNode child in node.child)
        {
            Descript(child, depth + 1);
        }

        //同样因为没有链表，所以不用进行后续的处理。
    }

    public void RenderMesh()
    {
        //Resources.UnloadUnusedAssets();

        int i;

        mFilter = gameObject.GetComponent<MeshFilter>();
        FractalRenderState state;
        state.centerPos = new Vector3(0, 0, 0);
        state.rotation = Quaternion.identity;

        RenderNodeRec(state, startNode);

        int indicesTotal = 0;
        for (i = 0; i < startNode.submeshCount; i++)
        {
            indicesTotal += indicesCount[i];
        }

        Debug.Log("Render summary: Vertices count = " + verticesCount + " Indices count = " + indicesTotal + " (" + fractalType.ToString() + ")");

        //Mesh mesh = new Mesh();
        mFilter.mesh.subMeshCount = startNode.submeshCount;
        //mFilter.mesh.hideFlags = HideFlags.None;
        mFilter.mesh.vertices = vertices;

        for (i = 0; i < startNode.submeshCount; i++)
        {
            mFilter.mesh.SetTriangles(indices[i], i);
        }

        if (renderNormals) { mFilter.mesh.normals = normals; }
        if (renderUV1s) { mFilter.mesh.uv = uvs; }
        if (renderUV2s) { mFilter.mesh.uv2 = uv2s; }
        if (renderUV3s) { mFilter.mesh.uv3 = uv3s; }
        if (renderUV4s) { mFilter.mesh.uv4 = uv4s; }
        if (renderTangents) { mFilter.mesh.tangents = tangents; }

        //mFilter.mesh = mesh;
    }

    void RenderNodeRec(FractalRenderState state, FractalSystemNode node)
    {
        node.Express(
            vertices, ref verticesCount,    //Vertices
            indices, ref indicesCount,      //Indices
            normals, ref normalsCount,      //Normals
            uvs, uv2s, uv3s, uv4s,          //TexCoord(uv)
            tangents, ref tmp,              //Tangents
            ref state);

        foreach (FractalSystemNode child in node.child)
        {
            RenderNodeRec(state, child);
        }
    }

    public void ReDraw()
    {
        UnityEngine.Random.seed = randomSeed;
        
        switch (fractalType) //Simple factory
        {
            case FractalType.boxTest:
                startNode = new BoxTest();
                startNode.growRate = startGrowRate;
                break;

            case FractalType.treeVer1:
                startNode = new TreeVer1Beta();
                startNode.growRate = startGrowRate;
                break;

            case FractalType.treeVer1_ReducedVertices:
                startNode = new TreeVer1_ReducedVertices();
                startNode.growRate = startGrowRate;
                break;

            case FractalType.treeVer2Cyc_ConcretedNormals:
                startNode = new TreeVer2Cyc();
                startNode.growRate = startGrowRate;
                break;

            case FractalType.treeVer3Cyc_Spline:
                startNode = new TreeVer3_CycWithSpline();
                startNode.growRate = startGrowRate;
                break;

            case FractalType.treeVer3_G:
                startNode = new TreeVer3_withGravity();
                startNode.growRate = startGrowRate;
                break;

            case FractalType.treeVer4:
                startNode = new TreeVer4_Ultra();
                startNode.growRate = startGrowRate;
                break;

            case FractalType.treeVer4_Frond:
                startNode = new TreeVer4_Frond();
                startNode.growRate = startGrowRate;
                break;
        }
        startNode.startGrowRate = startGrowRate;

        indices.Clear();
        indicesCount.Clear();
        verticesCount = 0;
        for (int i = 0; i < startNode.submeshCount; i++)
        {
            indices.Add(new int[indicesMax]);
            indicesCount.Add(0);
        }

        startNode.randomSeed *= randomSeed;
        startNode.fractalMode = fractalMode;

        startNode.init();

        Descript(startNode, 0);

        //Debug.Log(stoppedCount);

        RenderMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (noUpdate)
        {
            return;
        }

        UnityEngine.Random.seed = randomSeed;

        //startGrowRate += 1.0f * Time.deltaTime;
        if(startGrowRate < 24.0f)
        {
            startGrowRate *= 1.0f + (0.08f * Time.deltaTime);
        }
        else
        {
            return;
        }

        ReDraw();
    }

    public void changeRandomSeed(int seed)
    {
        randomSeed = seed;
        ReDraw();
    }

    public void changeGrowthRate(float dRate)
    {
        startGrowRate += dRate;

        if (startGrowRate < 1) startGrowRate = 1;
        if (startGrowRate > 19) startGrowRate = 19;

        ReDraw();
    }
}