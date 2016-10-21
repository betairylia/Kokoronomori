Shader "Unlit/OceanOverall"
{
	Properties
	{
		NormalMap ("Normal Map", 2D) = "white" {}
		RefMap("Reflection Map", CUBE) = "" {}
		NightMap("Night Map", CUBE) = "" {}
		NormalMapPercent("NormalMap%", float) = 0.5
		lightPosition("Light position", Vector) = (0,1,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float3 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 tangent : TANGENT;
				float3 worldPos : POSITION1;
			};

			samplerCUBE RefMap, NightMap;
			sampler2D NormalMap;
			float4 NormalMap_ST;
			float3 lightPosition;
			float NormalMapPercent;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, NormalMap);
				o.normal = v.normal;
				o.tangent = v.tangent;
				o.worldPos = mul(_Object2World, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;

				// sample the texture
				/*float3 normalM = tex2D(NormalMap, i.uv);

				normalM = 2 * normalM - 1;
				normalM = normalize(normalM.xzy * NormalMapPercent + i.normal * (1 - NormalMapPercent));*/

				float3 dirc = i.worldPos - _WorldSpaceCameraPos;
				//float3 dirc = _WorldSpaceCameraPos - i.worldPos;
				
				float3 dircR = reflect(normalize(dirc), float3(0, 1, 0));
				if (dircR.y < 0)
				{
					dircR = -dircR;
				}

				col = texCUBE(RefMap, dircR);
				/*float len = length(col);

				if (len < 0.4 && len > 0.3)
				{
					float3 lightZ = cross(lightPosition, float3(1, 0, 0));

					col *= pow(len, 2) / 0.16;
					col += ((0.4 - len) * 10 / 4) * texCUBE(NightMap, float3(-dircR.x, 0, 0) + (-dircR.y) * lightPosition + (-dircR.z) * lightZ);
				}*/
				if (lightPosition.y < 0 && lightPosition.y > -0.1)
				{
					float3 lightZ = cross(lightPosition, float3(1, 0, 0));

					col *= pow((0.1 + lightPosition.y) / 0.1, 2);
					col += (( - lightPosition.y) / 0.1) * texCUBE(NightMap, float3(-dircR.x, 0, 0) + (-dircR.y) * lightPosition + (-dircR.z) * lightZ);
				}
				if (lightPosition.y < -0.1)
				{
					float3 lightZ = cross(lightPosition, float3(1, 0, 0));

					col = texCUBE(NightMap, float3(-dircR.x, 0, 0) + (-dircR.y) * lightPosition + (-dircR.z) * lightZ);
				}

				//col.rgb = normalize(reflect(dirc, normalM)).yyy * 1000;
				//col.rgb = normalM.ggg;//tex2D(NormalMap, i.uv);
				//col = texCUBE(RefMap, normalM);//reflect(normalize(dirc), normalM));
				//col.rgb = -dirc.ggg;

				return col;
			}
			ENDCG
		}
	}
}
