Shader "Unlit/Flowers_leave"
{
	Properties
	{
		colorTex("Diffuse Map", 2D) = "white" {}
		colorMulTex("Color Mul Tex", 2D) = "white" {}
		//normalTex("Normal Map", 2D) = "white" {}

		shadowTex("Shadow Map", 2D) = "white" {}
		shadowRev("Shadow Map Rev", 2D) = "white"{}

		lightPosition("light position (normalized)", Vector) = (0, 1, 0, 0)
		lightColor("Sun light color", Color) = (1, 1, 1, 1)
		ambientColor("Ambient color", Color) = (1, 1, 1, 1)

		originPosition("origin position", Vector) = (0, 6.57, 0, 0)
		cameraPosition("local position of the shadow map camera (toOrigin)", Vector) = (0, 17.57, 0, 0)
		cameraRevPosition("local position of the shadow map camera reversed(toOrigin)", Vector) = (0, 0, 0, 0)
		cameraY("-Y Axis of the camera", Vector) = (0, -1, 0, 0)
		cameraX("X Axis of the camera", Vector) = (1, 0, 0, 0)
		cameraZ("Z Axis of the camera", Vector) = (0, 0, 1, 0)
		cameraRadius("Size of the ortho shadow map camera", float) = 16
		cameraNear("Near plane", float) = 1
		cameraFar("Far plane", float) = 32
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 billboardPos : TEXCOORD1;
				float4 billboardX : TANGENT;
				float4 normal : NORMAL;
				float2 vertexType : TEXCOORD3;
				float2 mulTex : TEXCOORD2;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 position: POSITION1;
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float3 tangent : TANGENT;
				float2 vertexTyp : TEXCOORD3;
				float2 mulTex : TEXCOORD2;
			};

			uniform sampler2D colorTex, shadowTex, colorMulTex;

			uniform float3 cameraPosition, cameraY, cameraX, cameraZ, originPosition, lightPosition, cameraRevPosition;
			uniform float4 lightColor, ambientColor;
			uniform float cameraRadius, cameraNear, cameraFar;

			v2f vert(appdata v)
			{
				v2f o;

				float3 v3VertexPos = mul(UNITY_MATRIX_MV, v.vertex).xyz;
				float3 billboardX = mul(UNITY_MATRIX_MV, v.billboardX.xyz);

				float3 billboardZ = cross(mul(UNITY_MATRIX_MV, v.normal.xyz), billboardX.xyz);

				float3 billboardCenter = v3VertexPos + billboardX * (-v.billboardPos.x) + billboardZ * (-v.billboardPos.y);

				float4 vertexPos;
				vertexPos.xyz = billboardCenter + float3(v.billboardPos.x, v.billboardPos.y, 0);
				//vertexPos.xyz = v3VertexPos + float3(v.billboardPos.x, v.billboardPos.y, 0);
				vertexPos.w = 1;

				o.vertex = mul(UNITY_MATRIX_P, vertexPos);

				//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;

				o.position = mul(_Object2World, v.vertex.xyz) - originPosition;
				o.normal = v.normal.xyz;
				o.tangent = v.billboardX;
				o.vertexTyp = v.vertexType;
				o.mulTex = v.mulTex;
				//o.position = v.billboardX;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//get texture
				fixed4 col = tex2D(colorTex, i.uv);

				//alpha culling
				//Our shadow map camera's near plane is 1. so use _ProjectionParams.y can detect which camera is current in use.
				//don't make alpha culling on shadow maps.
				if (col.a < 0.5)// && _ProjectionParams.y < 0.8)
				{
					clip(-1.0);
				}

				float x = (i.position.x * cameraX.x + i.position.y * cameraX.y + i.position.z * cameraX.z) / (cameraRadius * 2) + 0.5;
				float z = (i.position.x * cameraZ.x + i.position.y * cameraZ.y + i.position.z * cameraZ.z) / (cameraRadius * 2) + 0.5;

				float4 depthTex = tex2D(shadowTex, float2(x, z));

				//Calc dis to camera
				float3 v3ToCam = cameraPosition.xyz - i.position.xyz;
				float dist = -dot(v3ToCam, cameraY);
				float depth = (dist - cameraNear) / (cameraFar - cameraNear);

				if (length(lightColor.rgb) < 0.3)
				{
					ambientColor *= 1.4;
				}
				col.rgb = col.rgb * ambientColor * 0.5 + col.rgb * lightColor * abs(dot(lightPosition, i.normal));

				//col.rgb *= 0.5;

				//col.a = 1;
				//col.rgb = normal;
				//return col;

				if (depth - depthTex.r > 0.05)
				{
					col *= 0.35;

					col.r *= 1;
					col.r += col.g * 0.1;
				}
				else
				{
					col *= 0.8;

					col.r *= 0.85;
					col.r += col.g * 0.1;
				}

				/*float factorMul = (length(col.rgb) - 0.7) / 0.3;
				
				if (factorMul > 0)
				{
					col = (1 - factorMul) * col + factorMul * tex2D(colorMulTex, i.mulTex);
				}*/

				return col;
			}
		ENDCG
		}
	}
}
