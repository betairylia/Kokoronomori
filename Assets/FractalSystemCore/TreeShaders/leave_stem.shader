Shader "Unlit/Leaves_2"
{
	Properties
	{
		colorTex("Diffuse Map", 2D) = "white" {}

		lightPosition("light position (normalized)", Vector) = (0, 1, 0, 0)
		lightColor("Sun light color", Color) = (1, 1, 1, 1)
		ambientColor("Ambient color", Color) = (1, 1, 1, 1)

		originPosition("origin position", Vector) = (0, 6.57, 0, 0)
		cameraPosition("local position of the shadow map camera (toOrigin)", Vector) = (0, 17.57, 0, 0)
		cameraRevPosition("local position of the shadow map camera reversed(toOrigin)", Vector) = (0, 0, 0, 0)
		cameraY("-Y Axis of the camera", Vector) = (0, -1, 0, 0)
		cameraX("X Axis of the camera", Vector) = (1, 0, 0, 0)
		cameraZ("Z Axis of the camera", Vector) = (0, 0, 1, 0)
		cameraRadius("Size of the ortho shadow map camera", float) = 9
		cameraNear("Near plane", float) = 1
		cameraFar("Far plane", float) = 24

		doubleSideRatio("double side yellow ratio(like spec alpha)", float) = 3
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float2 billboardPos : TEXCOORD1;
					float4 billboardX : TANGENT;
					float4 normal : NORMAL;
					float2 vertexType : TEXCOORD3;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float3 position: POSITION1;
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float3 tangent : TANGENT;
					float2 vertexTyp : TEXCOORD3;
				};

				uniform sampler2D colorTex;

				uniform float3 cameraPosition, cameraY, cameraX, cameraZ, originPosition, lightPosition, cameraRevPosition;
				uniform float4 lightColor, ambientColor;
				uniform float cameraRadius, cameraNear, cameraFar, doubleSideRatio;

				v2f vert(appdata v)
				{
					v2f o;

					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = v.uv;

					o.position = mul(_Object2World, v.vertex.xyz) - originPosition;
					o.normal = v.normal.xyz;
					o.tangent = v.billboardX;
					o.vertexTyp = v.vertexType;
					//o.position = v.billboardX;

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					//get texture
					fixed4 col = tex2D(colorTex, i.uv);

				//alpha culling
				if (col.a < 0.5)// && _ProjectionParams.y < 0.8)
				{
					clip(-1.0);
				}

				//col.rgb *= 0.5 + 0.5 * abs(dot(lightPosition, normalM));
				if (length(lightColor.rgb) < 0.3)
				{
					ambientColor *= 1.4;
				}
				col.rgb = col.rgb * ambientColor * 0.5 + col.rgb * lightColor * abs(dot(lightPosition, i.normal));

				//Double-Sided:
				float3 lookDir = i.position - _WorldSpaceCameraPos;
				lookDir = normalize(lookDir);
				fixed4 colYellow = fixed4(col.g * 0.9, col.g, col.g * 0.2, 1.0) * lightColor * 1.8;
				col *= ambientColor;
				float factor = pow(abs(dot(lookDir, lightPosition)), doubleSideRatio);

				col = (1 - factor) * col + factor * colYellow;

				return col;
			}
			ENDCG
		}
	}
}
