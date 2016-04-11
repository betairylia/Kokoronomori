using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.FractalSystemCore.NodeInherits
{
    public class BoxTest : FractalSystemNode
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

            #region Vertices

            vertices[verticesCount + 0] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f,  0, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 1] = state.centerPos + state.rotation * (rotation * (new Vector3( 0.2f,  0, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 2] = state.centerPos + state.rotation * (rotation * (new Vector3( 0.2f,  0,  0.2f) * growRate) + centerPos);
            vertices[verticesCount + 3] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f,  0,  0.2f) * growRate) + centerPos);
            vertices[verticesCount + 4] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f,  1, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 5] = state.centerPos + state.rotation * (rotation * (new Vector3( 0.2f,  1, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 6] = state.centerPos + state.rotation * (rotation * (new Vector3( 0.2f,  1,  0.2f) * growRate) + centerPos);
            vertices[verticesCount + 7] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f,  1,  0.2f) * growRate) + centerPos);

            vertices[verticesCount + 0 + 8] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f, 0, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 1 + 8] = state.centerPos + state.rotation * (rotation * (new Vector3(0.2f, 0, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 2 + 8] = state.centerPos + state.rotation * (rotation * (new Vector3(0.2f, 0, 0.2f) * growRate) + centerPos);
            vertices[verticesCount + 3 + 8] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f, 0, 0.2f) * growRate) + centerPos);
            vertices[verticesCount + 4 + 8] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f, 1, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 5 + 8] = state.centerPos + state.rotation * (rotation * (new Vector3(0.2f, 1, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 6 + 8] = state.centerPos + state.rotation * (rotation * (new Vector3(0.2f, 1, 0.2f) * growRate) + centerPos);
            vertices[verticesCount + 7 + 8] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f, 1, 0.2f) * growRate) + centerPos);

            vertices[verticesCount + 0 + 16] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f, 0, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 1 + 16] = state.centerPos + state.rotation * (rotation * (new Vector3(0.2f, 0, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 2 + 16] = state.centerPos + state.rotation * (rotation * (new Vector3(0.2f, 0, 0.2f) * growRate) + centerPos);
            vertices[verticesCount + 3 + 16] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f, 0, 0.2f) * growRate) + centerPos);
            vertices[verticesCount + 4 + 16] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f, 1, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 5 + 16] = state.centerPos + state.rotation * (rotation * (new Vector3(0.2f, 1, -0.2f) * growRate) + centerPos);
            vertices[verticesCount + 6 + 16] = state.centerPos + state.rotation * (rotation * (new Vector3(0.2f, 1, 0.2f) * growRate) + centerPos);
            vertices[verticesCount + 7 + 16] = state.centerPos + state.rotation * (rotation * (new Vector3(-0.2f, 1, 0.2f) * growRate) + centerPos);

            #endregion

            #region Normals
            //-Z
            normals[normalsCount + 0] = state.rotation * rotation * Vector3.back;
            normals[normalsCount + 1] = state.rotation * rotation * Vector3.back;
            normals[normalsCount + 4] = state.rotation * rotation * Vector3.back;
            normals[normalsCount + 5] = state.rotation * rotation * Vector3.back;

            //+Z
            normals[normalsCount + 10] = state.rotation * rotation * Vector3.forward;
            normals[normalsCount + 3] = state.rotation * rotation * Vector3.forward;
            normals[normalsCount + 7] = state.rotation * rotation * Vector3.forward;
            normals[normalsCount + 14] = state.rotation * rotation * Vector3.forward;

            //-X
            normals[normalsCount + 8] = state.rotation * rotation * Vector3.left;
            normals[normalsCount + 11] = state.rotation * rotation * Vector3.left;
            normals[normalsCount + 12] = state.rotation * rotation * Vector3.left;
            normals[normalsCount + 15] = state.rotation * rotation * Vector3.left;

            //+X
            normals[normalsCount + 9] = state.rotation * rotation * Vector3.right;
            normals[normalsCount + 2] = state.rotation * rotation * Vector3.right;
            normals[normalsCount + 13] = state.rotation * rotation * Vector3.right;
            normals[normalsCount + 6] = state.rotation * rotation * Vector3.right;

            //-Y
            normals[normalsCount + 16] = state.rotation * rotation * Vector3.down;
            normals[normalsCount + 17] = state.rotation * rotation * Vector3.down;
            normals[normalsCount + 18] = state.rotation * rotation * Vector3.down;
            normals[normalsCount + 19] = state.rotation * rotation * Vector3.down;

            //+Y
            normals[normalsCount + 20] = state.rotation * rotation * Vector3.up;
            normals[normalsCount + 21] = state.rotation * rotation * Vector3.up;
            normals[normalsCount + 22] = state.rotation * rotation * Vector3.up;
            normals[normalsCount + 23] = state.rotation * rotation * Vector3.up;

            #endregion

            #region Indices

            int[] tmpIndices = new int[36] {
                verticesCount + 0,
                verticesCount + 5,
                verticesCount + 1,
                verticesCount + 0,
                verticesCount + 4,
                verticesCount + 5,
                verticesCount + 9,
                verticesCount + 6,
                verticesCount + 2,
                verticesCount + 9,
                verticesCount + 13,
                verticesCount + 6,
                verticesCount + 10,
                verticesCount + 7,
                verticesCount + 3,
                verticesCount + 10,
                verticesCount + 14,
                verticesCount + 7,
                verticesCount + 8,
                verticesCount + 15,
                verticesCount + 12,
                verticesCount + 8,
                verticesCount + 11,
                verticesCount + 15,
                verticesCount + 20,
                verticesCount + 22,
                verticesCount + 21,
                verticesCount + 20,
                verticesCount + 23,
                verticesCount + 22,
                verticesCount + 16,
                verticesCount + 18,
                verticesCount + 19,
                verticesCount + 16,
                verticesCount + 17,
                verticesCount + 18 };
            tmpIndices.CopyTo(indices, indicesCount);

            #endregion

            verticesCount += 24;
            indicesCount += 36;
            normalsCount += 24;

            state.centerPos += state.rotation * (rotation * new Vector3(0, 1, 0) * growRate + centerPos);
            state.rotation = rotation * state.rotation;
        }

        public override void generateChildren()
        {
            FractalSystemNode node = new BoxTest();
            node.rotation = Quaternion.AngleAxis(20, new Vector3(0, 0, 1));
            node.growRate = growRate * 0.9f;

            child.Add(node);

            node = new BoxTest();
            node.rotation = Quaternion.AngleAxis(-20, new Vector3(0, 0, 1));
            node.growRate = growRate * 0.9f;

            child.Add(node);
        }

        //abo.
        //public override void updateChildren()
        //{
        //    foreach(FractalSystemNode node in child)
        //    {
        //        node.centerPos = new Vector3(0, 0, 0);
        //        node.growRate = growRate * 0.5f;
        //    }
        //}
    }
}
