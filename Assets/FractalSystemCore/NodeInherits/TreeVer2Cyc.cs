using System.Collections;
using UnityEngine;

namespace Assets.FractalSystemCore.NodeInherits
{
    class TreeVer2Cyc : FractalSystemNode
    {
        public int circleFragments = 8;
        public int heightFragments = 2;
        public float radiusRate = 0.05f, height = 1.0f;

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

            int vert = 0, x, y;
            float radius = radiusRate * growRate, heightStep = height * growRate / heightFragments;
            float rad;

            #region Vertices & Normals
            
            for (x = 0; x < circleFragments; x++)
            {
                for(y = 0; y < heightFragments + 1; y++)
                {
                    rad = x * (2f * Mathf.PI / circleFragments);

                    vertices[verticesCount + (x + y * circleFragments)] =
                        state.centerPos + state.rotation * (rotation * (new Vector3(
                        Mathf.Cos(rad) * radius,
                        y * heightStep,
                        Mathf.Sin(rad) * radius)) + centerPos);

                    normals[verticesCount + (x + y * circleFragments)] = state.rotation * rotation * new Vector3(
                        Mathf.Cos(rad),
                        0,
                        Mathf.Sin(rad));

                    vert++;
                }
            }

            #endregion

            #region Indices

            for (x = 0; x < circleFragments - 1; x++)
                for (y = 0; y < heightFragments; y++)
                {
                    //Indices = ( x, y ) ( x + 1, y + 1 ) ( x + 1, y ); ( x, y ) ( x , y + 1 ) ( x + 1, y + 1 )
                    indices[indicesCount++] = verticesCount + (x   + y     * circleFragments);
                    indices[indicesCount++] = verticesCount + (x+1 + (y+1) * circleFragments);
                    indices[indicesCount++] = verticesCount + (x+1 + y     * circleFragments);
                    indices[indicesCount++] = verticesCount + (x   + y     * circleFragments);
                    indices[indicesCount++] = verticesCount + (x   + (y+1) * circleFragments);
                    indices[indicesCount++] = verticesCount + (x+1 + (y+1) * circleFragments);
                }

            for (y = 0; y < heightFragments; y++)
            {
                x = circleFragments - 1;
                //Indices = ( x, y ) ( x + 1, y + 1 ) ( x + 1, y ); ( x, y ) ( x , y + 1 ) ( x + 1, y + 1 )
                indices[indicesCount++] = verticesCount + (x + y * circleFragments);
                indices[indicesCount++] = verticesCount + (0 + (y + 1) * circleFragments);
                indices[indicesCount++] = verticesCount + (0 + y * circleFragments);
                indices[indicesCount++] = verticesCount + (x + y * circleFragments);
                indices[indicesCount++] = verticesCount + (x + (y + 1) * circleFragments);
                indices[indicesCount++] = verticesCount + (0 + (y + 1) * circleFragments);
            }

            #endregion

            verticesCount += vert;
            normalsCount += vert;
            //indicesCount已经在上面加过了

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

                FractalSystemNode node = new TreeVer2Cyc();
                node.rotation = Quaternion.AngleAxis(tilt,
                    Quaternion.AngleAxis(offset, Vector3.up) * Quaternion.AngleAxis(nowDeg, Vector3.up) * new Vector3(0, 0, 1));

                node.centerPos = new Vector3(0, 0, 0);

                //与旋转角度的余弦线性相关
                node.growRate = growRate * (((Mathf.Cos(tiltStart / 180.0f * Mathf.PI) - Mathf.Cos(tilt / 180.0f * Mathf.PI)) / (Mathf.Cos(tiltStart / 180.0f * Mathf.PI) - Mathf.Cos(tiltEnd / 180.0f * Mathf.PI))) *
                    ((growRateEnd) - (growRateStart)) + growRateStart + Random.Range(-growNoiseRad, growNoiseRad));

                node.globalRotation = node.rotation * globalRotation;
                node.centerPos = node.centerPos + globalPos;

                //朝向下方的惩罚（得不到光照etc.）
                Vector3 final = node.globalRotation * Vector3.up;
                if(final.y < 0.1f)
                {
                    float factor = (0.1f - final.y) / 1.1f;
                    node.growRate *= ((factor - 1) * (factor - 1)) * 0.4f + 0.6f;
                }

                child.Add(node);
            }
        }
    }
}
