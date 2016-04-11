using System.Collections;
using UnityEngine;

namespace Assets.FractalSystemCore.NodeInherits
{
    class TreeVer1_ReducedVertices : FractalSystemNode
    {
        public override void Express(
            Vector3[] vertices,
            ref int verticesCount,
            int[] indices,
            ref int indicesCount,
            Vector3[] normals,
            ref int normalsCount,
            Vector2[] uvs,
            ref int uvsCount,
            Vector2[] uv2s,
            ref int uv2sCount,
            Vector4[] tangents,
            ref int tangentsCount,
            ref FractalRenderState state
            )
        {
            /*
                                        z
                             y        -+
                            /|\       /|                    Normals:            +0          +8          +16
                             | (7)--------(6)           (0) (-1, -1, -1)        -Z          -X          -Y
                             | / |  /     / |           (1) ( 1, -1, -1)        -Z          +X          -Y
                             |/    /     /  |           (2) ( 1, -1,  1)        +X          +Z          -Y
                            (4)--+-----(5)  |           (3) (-1, -1,  1)        +Z          -X          -Y
                             |  (3) - - + -(2)          (4) (-1,  1, -1)        -Z          -X          +Y
                             | /        |  /            (5) ( 1,  1, -1)        -Z          +X          +Y
                             |/         | /             (6) ( 1,  1,  1)        +X          +Z          +Y
                        ----(0)--------(1)-----> x      (7) (-1,  1,  1)        +Z          -X          +Y
                            /|
                           / |
                          /  |

                        0154    015 054                             0   5   1   0   4   5
                        1265    126 165                             9   6   2   9   13  6 
                        2376    237 276                             10  7   3   10  14  7 
                        0473    047 073                             8   15  12  8   11  15
                        4567    456 467                             20  22  21  20  23  22
                        0321    032 021                             16  18  19  16  17  18
            */
            float lenth = 0.05f, width = 0.05f, height = 1.0f;//X, Z, Y

            #region Vertices

            vertices[verticesCount + 0] = state.centerPos + state.rotation * (rotation * (new Vector3(-lenth / 2.0f, 0, -width / 2.0f) * growRate) + centerPos);
            vertices[verticesCount + 1] = state.centerPos + state.rotation * (rotation * (new Vector3(+lenth / 2.0f, 0, -width / 2.0f) * growRate) + centerPos);
            vertices[verticesCount + 2] = state.centerPos + state.rotation * (rotation * (new Vector3(+lenth / 2.0f, 0, +width / 2.0f) * growRate) + centerPos);
            vertices[verticesCount + 3] = state.centerPos + state.rotation * (rotation * (new Vector3(-lenth / 2.0f, 0, +width / 2.0f) * growRate) + centerPos);
            vertices[verticesCount + 4] = state.centerPos + state.rotation * (rotation * (new Vector3(-lenth / 2.0f, height, -width / 2.0f) * growRate) + centerPos);
            vertices[verticesCount + 5] = state.centerPos + state.rotation * (rotation * (new Vector3(+lenth / 2.0f, height, -width / 2.0f) * growRate) + centerPos);
            vertices[verticesCount + 6] = state.centerPos + state.rotation * (rotation * (new Vector3(+lenth / 2.0f, height, +width / 2.0f) * growRate) + centerPos);
            vertices[verticesCount + 7] = state.centerPos + state.rotation * (rotation * (new Vector3(-lenth / 2.0f, height, +width / 2.0f) * growRate) + centerPos);

            #endregion

            #region Normals

            normals[normalsCount + 0] = state.rotation * rotation * new Vector3(-1, 0, -1);
            normals[normalsCount + 1] = state.rotation * rotation * new Vector3( 1, 0, -1);
            normals[normalsCount + 2] = state.rotation * rotation * new Vector3( 1, 0,  1);
            normals[normalsCount + 3] = state.rotation * rotation * new Vector3(-1, 0,  1);
            normals[normalsCount + 4] = state.rotation * rotation * new Vector3(-1, 0, -1);
            normals[normalsCount + 5] = state.rotation * rotation * new Vector3(1, 0, -1);
            normals[normalsCount + 6] = state.rotation * rotation * new Vector3(1, 0, 1);
            normals[normalsCount + 7] = state.rotation * rotation * new Vector3(-1, 0, 1);

            #endregion

            #region Indices

            int[] tmpIndices = new int[36] {
                verticesCount + 0,
                verticesCount + 5,
                verticesCount + 1,
                verticesCount + 0,
                verticesCount + 4,
                verticesCount + 5,
                verticesCount + 1,
                verticesCount + 6,
                verticesCount + 2,
                verticesCount + 1,
                verticesCount + 5,
                verticesCount + 6,
                verticesCount + 2, 
                verticesCount + 7,
                verticesCount + 3,
                verticesCount + 2, 
                verticesCount + 6, 
                verticesCount + 7,
                verticesCount + 0,
                verticesCount + 7, 
                verticesCount + 4, 
                verticesCount + 0, 
                verticesCount + 3, 
                verticesCount + 7, 
                verticesCount + 4, 
                verticesCount + 6, 
                verticesCount + 5, 
                verticesCount + 4, 
                verticesCount + 7, 
                verticesCount + 6, 
                verticesCount + 0, 
                verticesCount + 2, 
                verticesCount + 3, 
                verticesCount + 0, 
                verticesCount + 1, 
                verticesCount + 2 };
            tmpIndices.CopyTo(indices, indicesCount);

            #endregion

            verticesCount += 8;
            indicesCount += 36;
            normalsCount += 8;

            state.centerPos += state.rotation * (rotation * new Vector3(0, 1, 0) * growRate + centerPos);
            state.rotation = rotation * state.rotation;
        }

        static float panStepStart = 60f;
        static float panStepStop = 120f;
        static float panOffsetMax = 60f;
        static float tiltStart = 20f;
        static float tiltEnd = 70f;

        /*
            生长率控制，在这里生长率为和这个树枝与主干的夹角（tilt）线性相关的一个量和一个加性高斯噪声叠加而成。start与end定义出这个线性相关量（与tilt相对应），noiseRad定义出噪声的方差（功率）。
        */
        static float growRateStart = 0.4f;
        static float growRateEnd = 0.8f;
        static float growNoiseRad = 0.1f;

        public override void generateChildren()
        {
            float nowDeg = 0f, offset = Random.Range(0f, panOffsetMax);

            while (nowDeg < 360.0f)
            {
                nowDeg += Random.Range(panStepStart, panStepStop);
                float tilt = Random.Range(tiltStart, tiltEnd);

                FractalSystemNode node = new TreeVer1_ReducedVertices();
                node.rotation = Quaternion.AngleAxis(tilt,
                    Quaternion.AngleAxis(offset, Vector3.up) * Quaternion.AngleAxis(nowDeg, Vector3.up) * new Vector3(0, 0, 1));

                node.centerPos = new Vector3(0, 0, 0);

                //与旋转角度的余弦线性相关
                node.growRate = growRate * (((Mathf.Cos(tiltStart / 180.0f * Mathf.PI) - Mathf.Cos(tilt / 180.0f * Mathf.PI)) / (Mathf.Cos(tiltStart / 180.0f * Mathf.PI) - Mathf.Cos(tiltEnd / 180.0f * Mathf.PI))) *
                    ((growRateEnd) - (growRateStart)) + growRateStart + Random.Range(-growNoiseRad, growNoiseRad));

                child.Add(node);
            }
        }

        //abo.
        //public override void updateChildren()
        //{
        //    foreach (FractalSystemNode node in child)
        //    {
        //        node.centerPos = new Vector3(0, 0, 0);
        //        node.growRate = growRate * 0.75f;
        //    }
        //}
    }
}