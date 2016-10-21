using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.FractalSystemCore.NodeInherits
{
    //这里所有都为Global。
    class TreeVer3_withGravity : FractalSystemNode
    {
        public int circleFragments = 6, nodes = 10;
        public float radiusRate = 0.025f, height = 1.0f;

        public float gravityLenthNormalized = 0.5f;
        public float gnarlLenthNormalized = 0.5f;

        float lengthStep = 0.1f;//每一段的长度
        public float[] gravityConst = new float[10] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
        public float[] gnarlConst = new float[10] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

        public Vector3 parentAxis, parentCenterPos;
        public Vector3 gravityAxis = Vector3.down;

        List<TreeSplineNode> spline = new List<TreeSplineNode>();

        BranchType branchType = BranchType.Trank;

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
            float[] gnarlFactor = MathCore.PerlinNoise(spline.Count, noiseStep, 0.4f, gnarlConst);
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

                dirc += tang * gnarlInfectionStep;
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

        public override void init()
        {
            base.init();

            //添加样条线节点
            for (int i = 0; i < nodes; i++)
            {
                spline.Add(new TreeSplineNode());
            }

            //处理样条线的形态
            UpdateSpline();
        }

        static float panStepStart = 60f;
        static float panStepStop = 120f;
        static float panOffsetMax = 60f;
        static float tiltStart = 20f;
        static float tiltEnd = 70f;

        /*
            生长率控制，在这里生长率为和这个树枝与主干的夹角（tilt）线性相关的一个量和一个加性高斯噪声叠加而成。start与end定义出这个线性相关量（与tilt相对应），noiseRad定义出噪声的方差（功率）。
        */
        static float growRateStart = 0.6f;
        static float growRateEnd = 0.8f;
        static float growNoiseRad = 0.04f;

        public override void generateChildren()
        {
            float nowDeg = 0f, offset = Random.Range(0f, panOffsetMax);

            while (nowDeg < 360.0f)
            {
                nowDeg += Random.Range(panStepStart, panStepStop);
                float tilt = Random.Range(tiltStart, tiltEnd);

                TreeVer3_withGravity node = new TreeVer3_withGravity();
                node.rotation = Quaternion.AngleAxis(tilt,
                    Quaternion.AngleAxis(offset, Vector3.up) * Quaternion.AngleAxis(nowDeg, Vector3.up) * new Vector3(0, 0, 1)) * rotation;

                node.centerPos = spline[spline.Count - 1].position;

                //与旋转角度的余弦线性相关
                node.growRate = growRate * (((Mathf.Cos(tiltStart / 180.0f * Mathf.PI) - Mathf.Cos(tilt / 180.0f * Mathf.PI)) / (Mathf.Cos(tiltStart / 180.0f * Mathf.PI) - Mathf.Cos(tiltEnd / 180.0f * Mathf.PI))) *
                    ((growRateEnd) - (growRateStart)) + growRateStart + Random.Range(-growNoiseRad, growNoiseRad));

                node.globalRotation = node.rotation;// * globalRotation;
                //node.centerPos = node.centerPos + globalPos;

                //朝向下方的惩罚（得不到光照etc.）
                Vector3 final = node.globalRotation * Vector3.up;
                if (final.y < 0.1f)
                {
                    float factor = (0.1f - final.y) / 1.1f;
                    node.growRate *= ((factor - 1) * (factor - 1)) * 0.4f + 0.6f;
                }

                node.parentAxis = spline[spline.Count - 1].rotationGlobal * (-gravityAxis);
                node.parentCenterPos = centerPos;

                node.init();

                child.Add(node);
            }
        }
    }
}