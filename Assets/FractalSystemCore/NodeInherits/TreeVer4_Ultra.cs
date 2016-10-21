﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.FractalSystemCore.NodeInherits
{
    //这里所有都为Global。
    class TreeVer4_Ultra : FractalSystemNode
    {
        public int circleFragments = 6, nodes = 10;
        public float radiusRate = 0.025f, height = 1.0f;

        public float gravityLenthNormalized = 1.5f;
        public float gnarlLenthNormalized = 2.0f;

        float lengthStep = 0.1f;//每一段的长度
        public float[] gravityConst = new float[10] { 0.2f, 0.3f, 0.5f, 1.1f, 2.0f, 1.2f, 0.4f, -0.2f, -0.8f, -1.0f };
        public float[] gnarlConst = new float[10] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

        public Vector3 parentAxis, parentCenterPos;
        public Vector3 gravityAxis = Vector3.down;

        public int iterationStep = 0;

        List<TreeSplineNode> spline = new List<TreeSplineNode>();

        BranchType branchType = BranchType.Trank;

        public TreeVer4_Ultra()
        {
            submeshCount = 2;
        }

        #region 形态处理函数

        public void UpdateSpline()
        {
            int i = 0;

            float startRadius, endRadius;
            startRadius = radiusRate * growRate;

            if (branchType == BranchType.Leaves)
            {
                return;
            }
            if (branchType == BranchType.Trank)
            {
                endRadius = startRadius * 0.15f;
            }
            else
            {
                endRadius = startRadius * 0.02f;
            }

            Vector3 startPos = centerPos;
            Quaternion q = rotation;
            lengthStep = height * growRate / (spline.Count);

            /*
            在样条线上加一个PerlinNoise，两层（一层是幅度一层是相位）
            转轴旋转平面垂直于上一个节点的生长方向，从0开始旋转noiseP度；
            然后绕转轴旋转noiseA度。
            */

            int noiseStep = spline.Count / 4;
            float noiseAmpl = 8;//(height * growRate) / 6.0;

            float[] noiseA = MathCore.PerlinNoise(spline.Count, noiseStep, noiseAmpl, null);
            float[] noiseP = MathCore.PerlinNoise(spline.Count, noiseStep, 180, null);//相位先设定为2pi

            //重力
            float[] gravityFactor = MathCore.PerlinNoise(spline.Count, noiseStep, 0.1f, gravityConst);
            float gravityInfectionStep = gravityLenthNormalized / nodes;

            //向心旋转（它的主要组成就是perlinNoise）
            float[] gnarlFactor = MathCore.PerlinNoise(spline.Count, noiseStep, 1.0f, gnarlConst);
            float gnarlInfectionStep = gnarlLenthNormalized / nodes;

            //样条线
            //第一个点和当前node的数据一致
            spline[i].position = centerPos;
            spline[i].rotationGlobal = rotation;
            spline[i].tangentGlobal = (rotation * new Vector3(1, 0, 0));//假定切线在x轴
            spline[i].radius = startRadius;

            //如果是主干，根部加粗
            if (branchType == BranchType.Trank)
            {
                spline[i].radius *= 2.0f;
            }

            for (i = 1; i < spline.Count; i++)
            {
                //位置是由上一个node的生长方向决定的
                spline[i].position = spline[i - 1].position + spline[i - 1].rotationGlobal * new Vector3(0, lengthStep, 0);

                //先让这个节点的方向和上一个节点的方向保持一致
                spline[i].rotationGlobal = spline[i - 1].rotationGlobal;
                spline[i].tangentGlobal = spline[i - 1].tangentGlobal;

                //当前生长方向
                Vector3 dirc = spline[i].rotationGlobal * Vector3.up;

                ////////////////////
                //处理重力
                ////////////////////

                /*
                重力直接作用于生长方向上。
                相加，规范化，乘以步长。
                */
                dirc += gravityInfectionStep * gravityFactor[i] * gravityAxis;
                dirc.Normalize();

                ////////////////////
                //处理旋力
                ////////////////////

                /*
                为了处理“旋力”，我们需要求出在以主干为圆心，过当前点的切线向量。
                并将这个向量和当前的生长方向相加，规范化，乘以步长。
                最后根据它求得这一段的rotation。
                */

                //先拿到当前点相对父树枝原点的位置
                Vector3 posLocal = spline[i].position - parentCenterPos;
                //把这个相对位置绕父树枝的方向旋转2度，与原向量相减规范化得到近似的切线朝向
                Vector3 tang = Quaternion.AngleAxis(1.0f, parentAxis) * posLocal - posLocal;
                tang.Normalize();

                dirc += tang * gnarlInfectionStep * gnarlFactor[i];
                dirc.Normalize();

                ////////////////////
                //计算rotation
                ////////////////////

                spline[i].rotationLocal = Quaternion.FromToRotation(spline[i - 1].rotationGlobal * Vector3.up, dirc);

                ////////////////////
                //处理噪声
                ////////////////////

                //当前旋转方向由噪声决定
                //处理旋转
                //转轴
                Vector3 rotateAxis = Quaternion.AngleAxis(noiseP[i], spline[i - 1].rotationGlobal * new Vector3(0, lengthStep, 0)) * spline[i - 1].tangentGlobal;
                rotateAxis.Normalize();

                //旋转，并保存一个相对与上一个节点的旋转( rotationLocal )
                spline[i].rotationLocal = Quaternion.AngleAxis(noiseA[i], rotateAxis) * spline[i].rotationLocal;

                //更新global
                spline[i].rotationGlobal = spline[i].rotationLocal * spline[i].rotationGlobal;
                spline[i].tangentGlobal = spline[i].rotationLocal * spline[i].tangentGlobal;

                //处理当前结点截面的大小
                spline[i].radius = ((endRadius - startRadius) * (i / ((float)spline.Count)) + startRadius) * Random.Range(0.9f, 1.1f);
            }

            //处理一下rotation
            for (i = 1; i < spline.Count; i++)
            {
                spline[i].rotationLocal = Quaternion.FromToRotation(spline[i - 1].rotationGlobal * Vector3.up, spline[i].position - spline[i - 1].position);
                spline[i].rotationGlobal = spline[i].rotationLocal * spline[i].rotationGlobal;
            }

            rotation = spline[i - 1].rotationGlobal;
        }

        #endregion

        public override void Express(
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
            )
        {
            /////////////////////
            //树叶的情况
            /////////////////////

            float leaveRadius = 4.0f * startGrowRate / 24.0f;

            if(branchType == BranchType.Leaves)
            {
                //centerPos += new Vector3(0, 2.4f, 0);

                vertices[verticesCount] = rotation * new Vector3(-leaveRadius, -leaveRadius, 0) + centerPos;
                uvs[verticesCount] = new Vector2(0.5f, 0f);
                uv2s[verticesCount] = new Vector2(-leaveRadius, -leaveRadius);
                uv4s[verticesCount] = new Vector2(0, 0);                    //0, 0: 叶子
                normals[verticesCount] = rotation * Vector3.back;
                tangents[verticesCount] = rotation * Vector3.right;
                verticesCount++;

                vertices[verticesCount] = rotation * new Vector3(-leaveRadius, +leaveRadius, 0) + centerPos;
                uvs[verticesCount] = new Vector2(0.5f, 0.5f);
                uv2s[verticesCount] = new Vector2(-leaveRadius, +leaveRadius);
                uv4s[verticesCount] = new Vector2(0, 0);
                normals[verticesCount] = rotation * Vector3.back;
                tangents[verticesCount] = rotation * Vector3.right;
                verticesCount++;

                vertices[verticesCount] = rotation * new Vector3(+leaveRadius, +leaveRadius, 0) + centerPos;
                uvs[verticesCount] = new Vector2(0f, 0.5f);
                uv2s[verticesCount] = new Vector2(+leaveRadius, +leaveRadius);
                uv4s[verticesCount] = new Vector2(0, 0);
                normals[verticesCount] = rotation * Vector3.back;
                tangents[verticesCount] = rotation * Vector3.right;
                verticesCount++;

                vertices[verticesCount] = rotation * new Vector3(+leaveRadius, -leaveRadius, 0) + centerPos;
                uvs[verticesCount] = new Vector2(0f, 0f);
                uv2s[verticesCount] = new Vector2(+leaveRadius, -leaveRadius);
                uv4s[verticesCount] = new Vector2(0, 0);
                normals[verticesCount] = rotation * Vector3.back;
                tangents[verticesCount] = rotation * Vector3.right;
                verticesCount++;

                indices[1][indicesCount[1]++] = verticesCount - 4;
                indices[1][indicesCount[1]++] = verticesCount - 2;
                indices[1][indicesCount[1]++] = verticesCount - 3;
                indices[1][indicesCount[1]++] = verticesCount - 4;
                indices[1][indicesCount[1]++] = verticesCount - 1;
                indices[1][indicesCount[1]++] = verticesCount - 2;

                indices[1][indicesCount[1]++] = verticesCount - 4;
                indices[1][indicesCount[1]++] = verticesCount - 3;
                indices[1][indicesCount[1]++] = verticesCount - 2;
                indices[1][indicesCount[1]++] = verticesCount - 4;
                indices[1][indicesCount[1]++] = verticesCount - 2;
                indices[1][indicesCount[1]++] = verticesCount - 1;
                return;
            }

            /////////////////////
            //树枝的情况
            /////////////////////

            /*
            按圆柱表面坐标系上点的坐标给点标号。圆为横轴，高为纵轴。
            顶点（x,y）坐标：
                rad = x * (2f * Mathf.PI / circleFragments);
Vertex =        (cos(rad) * radius, y * heightStep, sin(rad) * radius);
            顶点（x,y）法线：
                rad = x * (2f * Mathf.PI / circleFragments);
Normal =        (cos(rad), 0, sin(rad))
            构成整个子结构的面：
                for(x = 0; x < circleFragments - 1; x++)
                    for(y = 0; y < heightFragments - 1; y++)
Indices =               ( x, y ) ( x + 1, y + 1 ) ( x + 1, y ); ( x, y ) ( x , y + 1 ) ( x + 1, y + 1 )
                
                不封口。反正也看不见（
            */

            //int vert = 0, x, y;
            //float radius = radiusRate * growRate, heightStep = height * growRate / (spline.Count);
            //float rad;

            //绘制
            int vert = 0, x, index;
            float rad, radiusReal;

            #region Vertices & Normals

            for (index = 0; index < spline.Count; index++)
            {
                for (x = 0; x < circleFragments; x++)
                {
                    radiusReal = spline[index].radius * Random.Range(0.9f, 1.1f);

                    rad = x * (2f * Mathf.PI / circleFragments);

                    vertices[verticesCount + (x + index * circleFragments)] =
                        state.centerPos + (spline[index].rotationGlobal * new Vector3(
                            Mathf.Cos(rad) * radiusReal,
                            0,
                            Mathf.Sin(rad) * radiusReal)) + spline[index].position;

                    normals[verticesCount + (x + index * circleFragments)] = spline[index].rotationGlobal * new Vector3(
                        Mathf.Cos(rad),
                        0,
                        Mathf.Sin(rad));

                    uv4s[verticesCount + (x + index * circleFragments)] = new Vector2(1, 0); //1, 0: 树枝 / 树干

                    vert++;
                }
            }

            #endregion

            #region Indices

            for (x = 0; x < circleFragments - 1; x++)
                for (index = 0; index < (spline.Count - 1); index++)
                {
                    //Indices = ( x, y ) ( x + 1, y + 1 ) ( x + 1, y ); ( x, y ) ( x , y + 1 ) ( x + 1, y + 1 )
                    indices[0][indicesCount[0]++] = verticesCount + (x + index * circleFragments);
                    indices[0][indicesCount[0]++] = verticesCount + (x + 1 + (index + 1) * circleFragments);
                    indices[0][indicesCount[0]++] = verticesCount + (x + 1 + index * circleFragments);
                    indices[0][indicesCount[0]++] = verticesCount + (x + index * circleFragments);
                    indices[0][indicesCount[0]++] = verticesCount + (x + (index + 1) * circleFragments);
                    indices[0][indicesCount[0]++] = verticesCount + (x + 1 + (index + 1) * circleFragments);
                }

            //“封口”（因为是圆形封闭截面，需要把最后一个和第一个也连起来）
            for (index = 0; index < (spline.Count - 1); index++)
            {
                //Indices = ( x, y ) ( x + 1, y + 1 ) ( x + 1, y ); ( x, y ) ( x , y + 1 ) ( x + 1, y + 1 )
                indices[0][indicesCount[0]++] = verticesCount + (x + index * circleFragments);
                indices[0][indicesCount[0]++] = verticesCount + (0 + (index + 1) * circleFragments);
                indices[0][indicesCount[0]++] = verticesCount + (0 + index * circleFragments);
                indices[0][indicesCount[0]++] = verticesCount + (x + index * circleFragments);
                indices[0][indicesCount[0]++] = verticesCount + (x + (index + 1) * circleFragments);
                indices[0][indicesCount[0]++] = verticesCount + (0 + (index + 1) * circleFragments);
            }

            #endregion

            verticesCount += vert;
            normalsCount += vert;
            //indicesCount已经在上面加过了

            state.centerPos = Vector3.zero;
            state.rotation = Quaternion.identity;
        }

        bool inited = false;
        public override void init()
        {
            if(!inited)
            {
                if (branchType == BranchType.Trank)
                {
                    radiusRate *= 1.5f;
                    gravityConst = new float[10] { 0.0f, 0.15f, 0.25f, 0.50f, 0.25f, 0.0f, -0.15f, -0.25f, -0.40f, 0.0f };
                    gravityLenthNormalized = 0.25f;
                }
                if (branchType == BranchType.Branch)
                {
                    height *= 1.1f;

                    step = 1;
                    steps = 5;
                    count = 1;
                    branchStart = 2;
                    branchEnd = (Random.Range(-1.0f, 1.0f) > 0.0f ? 7 : 8);
                    spread = 0.05f;
                    stepRand = 180.0f;
                    overAllRand = 180.0f;
                    forceOverall = 0.0f;
                    overallRoll = 90.0f;

                    radStart = 0.6f;
                    radEnd = 0.8f;

                    heightStart = 1.4f;
                    heightEnd = 1.5f;

                    tiltStart = 60f;
                    tiltEnd = 35f;
                }
                if (iterationStep == 2)
                {
                    step = 3;
                    steps = 2;
                    count = 1;
                    branchStart = 6;
                    branchEnd = 7;
                }
                inited = true;
            }

            //添加样条线节点
            spline.Clear();
            for (int i = 0; i < nodes; i++)
            {
                spline.Add(new TreeSplineNode());
            }

            //处理样条线的形态
            UpdateSpline();
        }

        public int step = 2;
        public int steps = 4;
        public int count = 3;
        public int branchStart = 3;
        public int branchEnd = 9;
        public float spread = 0.05f;
        public float tiltStart = 55f;//degrees
        public float tiltEnd = 30f;
        public float tiltRand = 5f;
        public float radStart = 0.4f;
        public float radEnd = 0.65f;
        public float radRand = 0.1f;
        public float overAllRand = 10.0f;
        public float stepRand = 10.0f;
        public float heightStart = 1.2f;
        public float heightEnd = 1.3f;
        public float forceOverall = 0.0f;
        public float overallRoll = 0.0f;

        public override void update()
        {
            base.update();

            UpdateSpline();

            float dAngle = 360.0f / count;

            if (count == 1)
            {
                dAngle = 0f;
            }

            float overallAngle = overallRoll, stepAngle = 0f;

            int i, j, stepCount = 0;
            for (i = branchStart; i <= branchEnd; i += step)
            {
                stepCount++;
                stepAngle = overallAngle;
                for (j = 0; j < count; j++)
                {
                    TreeVer4_Ultra branch = (TreeVer4_Ultra)child[j];
                    if (iterationStep == 2)
                    {
                        if (Random.Range(0, 10) < 10)
                        {
                            //这是一片叶子
                            branch.branchType = BranchType.Leaves;

                            //这片叶子的旋转
                            float tilt = Random.Range(-tiltRand, tiltRand) -
                                (tiltStart - tiltEnd) * ((stepCount - 1) / (float)(steps - 1)) + tiltStart;

                            branch.rotation = Quaternion.AngleAxis(tilt,
                                Quaternion.AngleAxis(stepAngle, Vector3.up) * Vector3.forward) * spline[i].rotationGlobal;

                            //这个树枝的起始位置
                            float spreadF = Random.Range(-spread, spread);

                            if (spreadF > 0.0f || i == 0)
                            {
                                branch.centerPos = spline[i].position + spline[i].rotationGlobal * Vector3.up *
                                    spreadF * height * growRate;
                            }
                            else
                            {
                                branch.centerPos = spline[i].position - spline[i - 1].rotationGlobal * Vector3.up *
                                    (-spreadF) * height * growRate;
                            }

                            //树叶偏离树枝一点
                            branch.centerPos += branch.rotation * (Vector3.up * 0.7f);

                            //这个树枝的生长率（长度与粗细）
                            float radius = spline[i].radius * (((radEnd - radStart) * ((stepCount - 1) / (float)(steps - 1)) + radStart) + Random.Range(-radRand, radRand));
                            branch.growRate = radius / branch.radiusRate;

                            //下一个树叶要旋转一下
                            stepAngle += dAngle + Random.Range(-stepRand, stepRand);
                        }
                    }
                    else
                    {
                        //这是一个树枝
                        branch.branchType = BranchType.Branch;

                        //这个树枝依附于当前结点
                        branch.parentCenterPos = spline[i].position;
                        branch.parentAxis = spline[i].rotationGlobal * Vector3.up;

                        //这个树枝的起始位置
                        float spreadF = Random.Range(-spread, spread);

                        //hotfix：如果已经在顶上就往下走
                        if (i == (nodes - 1))
                        {
                            spreadF = Random.Range(spread - 0.05f, -0.05f);
                        }

                        if (spreadF > 0.0f || i == 0)
                        {
                            branch.centerPos = spline[i].position + spline[i].rotationGlobal * Vector3.up *
                                spreadF * height * growRate;
                        }
                        else
                        {
                            branch.centerPos = spline[i].position - spline[i - 1].rotationGlobal * Vector3.up *
                                (-spreadF) * height * growRate;
                        }

                        //这个树枝的旋转
                        float tilt = Random.Range(-tiltRand, tiltRand) -
                            (tiltStart - tiltEnd) * ((stepCount - 1) / (float)(steps - 1)) + tiltStart;

                        if (branchType == BranchType.Branch)
                        {
                            stepAngle = stepAngle % 360f;
                            if (stepAngle < 90f || stepAngle > 270f)
                            {
                                stepAngle += 90.0f;
                                stepAngle = stepAngle % 360.0f;

                                stepAngle = (stepAngle - 90f) * 0.3f + 90f;
                                stepAngle -= 90.0f;
                            }
                            else
                            {
                                stepAngle = (stepAngle - 180f) * 0.3f + 180f;
                            }
                        }

                        branch.rotation = Quaternion.AngleAxis(tilt,
                            Quaternion.AngleAxis(stepAngle, Vector3.up) * Vector3.forward) * spline[i].rotationGlobal;

                        //这个树枝的生长率（长度与粗细）
                        float radius = spline[i].radius * (((radEnd - radStart) * ((stepCount - 1) / (float)(steps - 1)) + radStart) + Random.Range(-radRand, radRand));
                        branch.growRate = radius / branch.radiusRate;

                        //这个树枝会越来越“细长”
                        branch.height *= ((heightEnd - heightStart) * ((stepCount - 1) / (float)(steps - 1)) + heightStart);

                        //下一个树枝要旋转一下
                        stepAngle += dAngle + Random.Range(-stepRand, stepRand);

                        if (branchType == BranchType.Branch)
                        {
                            //除了第一级枝干，之后的每一级重力常数都要减小
                            branch.gravityLenthNormalized *= 0.7f;
                            branch.gnarlLenthNormalized = 6.0f;
                        }
                    }

                    //Check iteration steps
                    branch.iterationStep = iterationStep + 1;
                    branch.startGrowRate = startGrowRate;
                }

                //交错分布
                overallAngle += (dAngle / 2f) + Random.Range(-overAllRand, overAllRand) + forceOverall;
            }
        }

        public override void generateChildren()
        {
            float dAngle = 360.0f / count;

            if (count == 1)
            {
                dAngle = 0f;
            }

            float overallAngle = overallRoll, stepAngle = 0f;

            int i, j, stepCount = 0;
            for (i = branchStart; i <= branchEnd; i += step)
            {
                stepCount++;
                stepAngle = overallAngle;
                for(j = 0; j < count; j++)
                {
                    //创建树枝
                    TreeVer4_Ultra branch = new TreeVer4_Ultra();
                    
                    if(iterationStep == 2)
                    {
                        if(Random.Range(0, 10) < 10)
                        {
                            //这是一片叶子
                            branch.branchType = BranchType.Leaves;

                            //这片叶子的旋转
                            float tilt = Random.Range(-tiltRand, tiltRand) -
                                (tiltStart - tiltEnd) * ((stepCount - 1) / (float)(steps - 1)) + tiltStart;

                            branch.rotation = Quaternion.AngleAxis(tilt,
                                Quaternion.AngleAxis(stepAngle, Vector3.up) * Vector3.forward) * spline[i].rotationGlobal;

                            //这个树枝的起始位置
                            float spreadF = Random.Range(-spread, spread);

                            if (spreadF > 0.0f || i == 0)
                            {
                                branch.centerPos = spline[i].position + spline[i].rotationGlobal * Vector3.up *
                                    spreadF * height * growRate;
                            }
                            else
                            {
                                branch.centerPos = spline[i].position - spline[i - 1].rotationGlobal * Vector3.up *
                                    (-spreadF) * height * growRate;
                            }

                            //树叶偏离树枝一点
                            branch.centerPos += branch.rotation * (Vector3.up * 0.7f);

                            //这个树枝的生长率（长度与粗细）
                            float radius = spline[i].radius * (((radEnd - radStart) * ((stepCount - 1) / (float)(steps - 1)) + radStart) + Random.Range(-radRand, radRand));
                            branch.growRate = radius / branch.radiusRate;

                            //下一个树叶要旋转一下
                            stepAngle += dAngle + Random.Range(-stepRand, stepRand);
                        }
                    }
                    else
                    {
                        //这是一个树枝
                        branch.branchType = BranchType.Branch;

                        //这个树枝依附于当前结点
                        branch.parentCenterPos = spline[i].position;
                        branch.parentAxis = spline[i].rotationGlobal * Vector3.up;

                        //这个树枝的起始位置
                        float spreadF = Random.Range(-spread, spread);

                        //hotfix：如果已经在顶上就往下走
                        if (i == (nodes - 1))
                        {
                            spreadF = Random.Range(spread - 0.05f, -0.05f);
                        }

                        if (spreadF > 0.0f || i == 0)
                        {
                            branch.centerPos = spline[i].position + spline[i].rotationGlobal * Vector3.up *
                                spreadF * height * growRate;
                        }
                        else
                        {
                            branch.centerPos = spline[i].position - spline[i - 1].rotationGlobal * Vector3.up *
                                (-spreadF) * height * growRate;
                        }

                        //这个树枝的旋转
                        float tilt = Random.Range(-tiltRand, tiltRand) -
                            (tiltStart - tiltEnd) * ((stepCount - 1) / (float)(steps - 1)) + tiltStart;

                        if (branchType == BranchType.Branch)
                        {
                            stepAngle = stepAngle % 360f;
                            if (stepAngle < 90f || stepAngle > 270f)
                            {
                                stepAngle += 90.0f;
                                stepAngle = stepAngle % 360.0f;

                                stepAngle = (stepAngle - 90f) * 0.3f + 90f;
                                stepAngle -= 90.0f;
                            }
                            else
                            {
                                stepAngle = (stepAngle - 180f) * 0.3f + 180f;
                            }
                        }

                        branch.rotation = Quaternion.AngleAxis(tilt,
                            Quaternion.AngleAxis(stepAngle, Vector3.up) * Vector3.forward) * spline[i].rotationGlobal;

                        //这个树枝的生长率（长度与粗细）
                        float radius = spline[i].radius * (((radEnd - radStart) * ((stepCount - 1) / (float)(steps - 1)) + radStart) + Random.Range(-radRand, radRand));
                        branch.growRate = radius / branch.radiusRate;

                        //这个树枝会越来越“细长”
                        branch.height *= ((heightEnd - heightStart) * ((stepCount - 1) / (float)(steps - 1)) + heightStart);

                        //下一个树枝要旋转一下
                        stepAngle += dAngle + Random.Range(-stepRand, stepRand);

                        if (branchType == BranchType.Branch)
                        {
                            //除了第一级枝干，之后的每一级重力常数都要减小
                            branch.gravityLenthNormalized *= 0.7f;
                            branch.gnarlLenthNormalized = 6.0f;
                        }
                    }

                    //Check iteration steps
                    branch.iterationStep = iterationStep + 1;
                    branch.startGrowRate = startGrowRate;

                    branch.init();

                    child.Add(branch);
                }

                //交错分布
                overallAngle += ( dAngle / 2f ) + Random.Range(-overAllRand, overAllRand) + forceOverall;
            }
        }
    }
}