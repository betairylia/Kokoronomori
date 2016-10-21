﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/*
只考虑这个工程中的使用情形：
    a.  只要绘制，就是绘制整棵树。
    b.  因为要生长，所以采用相对位置和相对旋转。
*/

namespace Assets.FractalSystemCore
{
    public struct FractalRenderState
    {
        public Vector3 centerPos;
        public Quaternion rotation;
        //想到再加
    }

    abstract public class FractalSystemNode
    {
        public int randomSeed = 1;

        public float startGrowRate = 0;

        public List<FractalSystemNode> child = new List<FractalSystemNode>();

        public FractalSystemNode father;

        public Vector3 centerPos = Vector3.zero;//相对于自己的父节点
        public Quaternion rotation = Quaternion.identity;//相对于自己的父节点

        public Vector3 globalPos = Vector3.zero;
        public Quaternion globalRotation = Quaternion.identity;

        public float growRate = 1.0f;

        public int fractalMode = 0;

        public int submeshCount = 1;

        abstract public void Express(
            Vector3[] vertices,
            ref int verticesCount,
            List<int[]> indices,
            ref List<int> indicesCount,
            Vector3[] normals,
            ref int normalsCount,
            Vector2[] uvs,
            Vector2[] uv2s, 
            Vector2[] uv3s,
            Vector2[] uv4s,
            Vector4[] tangents,
            ref int tangentsCount,
            ref FractalRenderState state
            );

        virtual public void init()
        {

        }

        abstract public void generateChildren();
        virtual public void update()
        {

        }

        #region treeOperation
        public FractalSystemNode GetMostLeft()
        {
            if(child.Count > 0)
            {
                return child[0].GetMostLeft();
            }
            else
            {
                return this;
            }
        }

        public FractalSystemNode GetMostRight()
        {
            if (child.Count > 0)
            {
                return child[child.Count - 1].GetMostRight();
            }
            else
            {
                return this;
            }
        }

        public void ClearNode()
        {
            foreach(FractalSystemNode node in child)
            {
                node.ClearNode();
            }

            child.Clear();
        }
        #endregion

    }
}
