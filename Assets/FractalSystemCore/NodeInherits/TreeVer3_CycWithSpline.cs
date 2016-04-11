using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.FractalSystemCore.NodeInherits
{
    public class TreeSplineNode
    {
        public Vector3 positionLocal = Vector3.zero;
        public Quaternion rotationGlobal = Quaternion.identity;
        public Vector3 tangentGlobal = Vector3.zero;
        public float radius;
    }

    public enum BranchType
    {
        Trank,
        Branch,
        Root,
        Leaves
    }

    //这里所有旋转为Global，位置为local。
    class TreeVer3_CycWithSpline : FractalSystemNode
    {
        public int circleFragments = 6, nodes = 10;
        public float radiusRate = 0.025f, height = 1.0f;

        List<TreeSplineNode> spline = new List<TreeSplineNode>();

        BranchType branchType = BranchType.Trank;

        #region 形态处理函数

        public void UpdateSpline()
        {
            int i = 0;

            float startRadius, endRadius;
            startRadius = radiusRate * growRate;

            if(branchType == BranchType.Leaves)
            {
                return;
            }
            if(branchType == BranchType.Trank)
            {
                endRadius = startRadius * 0.15f;
            }
            else
            {
                endRadius = startRadius * 0.02f;
            }

            Vector3 startPos = centerPos;
            Quaternion q = rotation;
            float lengthStep = height * growRate / (spline.Count);

            /*
            在样条线上加一个PerlinNoise，两层（一层是幅度一层是相位）

            转轴旋转平面垂直于上一个节点的生长方向，从0开始旋转noiseP度；
            然后绕转轴旋转noiseA度。
            */

            int noiseStep = spline.Count / 4;
            float noiseAmpl = 8;//(height * growRate) / 6.0;

            float[] noiseA = MathCore.PerlinNoise(spline.Count, noiseStep, noiseAmpl, 0);
            float[] noiseP = MathCore.PerlinNoise(spline.Count, noiseStep, 180, 0);//相位先设定为2pi

            //样条线
            //第一个点和当前node的数据一致
            spline[i].positionLocal = centerPos;
            spline[i].rotationGlobal = rotation;
            spline[i].tangentGlobal = (rotation * new Vector3(1, 0, 0));//假定切线在x轴
            spline[i].radius = startRadius;

            //如果是主干，根部加粗
            if(branchType == BranchType.Trank)
            {
                spline[i].radius *= 2.0f;
            }

            for (i = 1; i < spline.Count; i++)
            {
                //位置是由上一个node的生长方向决定的
                spline[i].positionLocal = spline[i - 1].positionLocal + spline[i - 1].rotationGlobal * new Vector3(0, lengthStep, 0);

                //当前旋转方向由噪声决定
                //先让这个节点的方向和上一个节点的方向保持一致
                spline[i].rotationGlobal = spline[i - 1].rotationGlobal;
                spline[i].tangentGlobal = spline[i - 1].tangentGlobal;

                //再处理旋转
                //转轴
                Vector3 rotateAxis = Quaternion.AngleAxis(noiseP[i], spline[i - 1].rotationGlobal * new Vector3(0, lengthStep, 0)) * spline[i - 1].tangentGlobal;
                rotateAxis.Normalize();

                //旋转
                Quaternion quaternion = Quaternion.AngleAxis(noiseA[i], rotateAxis);
                spline[i].rotationGlobal = quaternion * spline[i].rotationGlobal;
                spline[i].tangentGlobal = quaternion * spline[i].tangentGlobal;

                //处理当前结点截面的大小
                spline[i].radius = ((endRadius - startRadius) * (i / ((float)spline.Count)) + startRadius) * Random.Range(0.9f, 1.1f);
            }
        }

        #endregion

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

            //添加样条线节点
            for(int i = 0; i < nodes; i++)
            {
                spline.Add(new TreeSplineNode());
            }

            //先处理样条线的形态
            UpdateSpline();
            //施加重力
            //施加径向扭曲力

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
                            Mathf.Sin(rad) * radiusReal)) + spline[index].positionLocal;

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
                    indices[indicesCount++] = verticesCount + (x + index * circleFragments);
                    indices[indicesCount++] = verticesCount + (x + 1 + (index + 1) * circleFragments);
                    indices[indicesCount++] = verticesCount + (x + 1 + index * circleFragments);
                    indices[indicesCount++] = verticesCount + (x + index * circleFragments);
                    indices[indicesCount++] = verticesCount + (x + (index + 1) * circleFragments);
                    indices[indicesCount++] = verticesCount + (x + 1 + (index + 1) * circleFragments);
                }

            for (index = 0; index < (spline.Count - 1); index++)
            {
                //Indices = ( x, y ) ( x + 1, y + 1 ) ( x + 1, y ); ( x, y ) ( x , y + 1 ) ( x + 1, y + 1 )
                indices[indicesCount++] = verticesCount + (x + index * circleFragments);
                indices[indicesCount++] = verticesCount + (0 + (index + 1) * circleFragments);
                indices[indicesCount++] = verticesCount + (0 + index * circleFragments);
                indices[indicesCount++] = verticesCount + (x + index * circleFragments);
                indices[indicesCount++] = verticesCount + (x + (index + 1) * circleFragments);
                indices[indicesCount++] = verticesCount + (0 + (index + 1) * circleFragments);
            }

            #endregion

            verticesCount += vert;
            normalsCount += vert;
            //indicesCount已经在上面加过了

            state.centerPos += spline[spline.Count - 1].positionLocal;
            state.rotation = Quaternion.identity;
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

                FractalSystemNode node = new TreeVer3_CycWithSpline();
                node.rotation = Quaternion.AngleAxis(tilt,
                    Quaternion.AngleAxis(offset, Vector3.up) * Quaternion.AngleAxis(nowDeg, Vector3.up) * new Vector3(0, 0, 1)) * rotation;

                node.centerPos = new Vector3(0, 0, 0);

                //与旋转角度的余弦线性相关
                node.growRate = growRate * (((Mathf.Cos(tiltStart / 180.0f * Mathf.PI) - Mathf.Cos(tilt / 180.0f * Mathf.PI)) / (Mathf.Cos(tiltStart / 180.0f * Mathf.PI) - Mathf.Cos(tiltEnd / 180.0f * Mathf.PI))) *
                    ((growRateEnd) - (growRateStart)) + growRateStart + Random.Range(-growNoiseRad, growNoiseRad));

                node.globalRotation = node.rotation;// * globalRotation;
                node.centerPos = node.centerPos + globalPos;

                //朝向下方的惩罚（得不到光照etc.）
                Vector3 final = node.globalRotation * Vector3.up;
                if (final.y < 0.1f)
                {
                    float factor = (0.1f - final.y) / 1.1f;
                    node.growRate *= ((factor - 1) * (factor - 1)) * 0.4f + 0.6f;
                }

                child.Add(node);
            }
        }
    }
}
