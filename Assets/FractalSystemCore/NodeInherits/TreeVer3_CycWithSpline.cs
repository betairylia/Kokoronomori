using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.FractalSystemCore.NodeInherits
{
    public class TreeSplineNode
    {
        public Vector3 positionLocal = Vector3.zero;
        public Quaternion rotationGlobal = Quaternion.identity;
        public Quaternion rotationLocal = Quaternion.identity;
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
        public float gravityAngelsPerFactor = 50.0f;

        float lengthStep = 0.1f;//每一段的长度

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
            lengthStep = height * growRate / (spline.Count);

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

                //旋转，并保存一个相对与上一个节点的旋转( rotationLocal )
                spline[i].rotationLocal = Quaternion.AngleAxis(noiseA[i], rotateAxis);
                spline[i].rotationGlobal = spline[i].rotationLocal * spline[i].rotationGlobal;
                spline[i].tangentGlobal = spline[i].rotationLocal * spline[i].tangentGlobal;

                //处理当前结点截面的大小
                spline[i].radius = ((endRadius - startRadius) * (i / ((float)spline.Count)) + startRadius) * Random.Range(0.9f, 1.1f);
            }
        }

        public void AddGravity(float gravity = 1.0f)
        {
            if(branchType == BranchType.Trank)
            {
                gravity *= 0.5f;//主干就给我直起来！
            }

            //先产生一个重力影响分布。假设每个点受到的重力影响是固定的。（最后的朝向是一个积分）+ 白噪声（startStep为1的PerlinNoise）
            float gravityInfectionNoiseIdentity = 0.0f;

            float[] gravityInfection = MathCore.PerlinNoise(spline.Count, spline.Count / 5, gravityInfectionNoiseIdentity, gravity);

            //一个一会需要重复计算的东西
            float gravityFactorStepsGlobal = Mathf.Pow(gravityAngelsPerFactor, (height / nodes)) - 1;
            Debug.Log(gravityFactorStepsGlobal);

            int i;
            //假设初始节点不受重力影响
            for (i = 1; i < spline.Count; i++)
            {
                //先根据上一个节点的rotation和自身的rotationLocal更新自己的rotationGlobal和位置
                spline[i].positionLocal = spline[i - 1].positionLocal + spline[i - 1].rotationGlobal * new Vector3(0, lengthStep, 0);
                spline[i].rotationGlobal = spline[i].rotationLocal * spline[i - 1].rotationGlobal;

                /*
                为了求出要施加重力作用的旋转轴，我们需要先做生长方向对一个与地面平行的平面的投影，找出这个面上与这个投影正交的向量。
                它就是我们的转轴。
                */
                //求出转轴
                Vector3 gravityAxis = new Vector3(
                    ((spline[i].rotationGlobal * Vector3.up).z), 
                    0,
                    -((spline[i].rotationGlobal * Vector3.up).x));

                Vector3 dirc = spline[i].rotationGlobal * Vector3.up;

                //计算重力因子，这里先假设它和生长方向与地面的夹角成余弦关系
                float deg = Vector3.Angle(dirc, new Vector3(dirc.x, 0, dirc.z));

                float gravityFactor = 
                    ((1.0f - Mathf.Cos(deg)) //余弦关系因子
                    //- (deg / 100.0f) //趋光性
                    /*- 1.2f * ((i+1) / (float)spline.Count) * ((i+1) / (float)spline.Count)*/) //强制性尖端上扬（平方正比）
                    * gravityFactorStepsGlobal //根据分段总数和总长度求得的补正
                    * gravityInfection[i]; //加噪声的重力影响

                //施加重力
                spline[i].rotationLocal = Quaternion.AngleAxis(gravityFactor, gravityAxis) * spline[i].rotationLocal;
                spline[i].rotationGlobal = spline[i].rotationLocal * spline[i - 1].rotationGlobal;
                spline[i].tangentGlobal = spline[i].rotationLocal * spline[i - 1].tangentGlobal;
            }

            //最后要把这根树枝的rotation更新成最后一个节点的rotation
            rotation = spline[spline.Count - 1].rotationGlobal;
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
            AddGravity(1.0f);
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

            //“封口”（因为是圆形封闭截面，需要把最后一个和第一个也连起来）
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

        public static int steps = 4;
        public static int count = 3;
        public static float branchStart = 0.1f;
        public static float branchEnd = 0.9f;
        public static float spread = 0.1f;
        public static float tiltStart = 60f;//degrees
        public static float tiltEnd = 30f;
        public static float tiltRand = 10f;
        public static float lenthStart = 1.4f;
        public static float lenthEnd = 1.0f;
        public static float lenthRand = 0.1f;

        public override void generateChildren()
        {
            //todo
        }
    }
}
