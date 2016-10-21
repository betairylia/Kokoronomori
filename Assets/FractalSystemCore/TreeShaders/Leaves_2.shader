Shader "Unlit/Leaves_2"
{
	Properties
	{
		colorTex("Diffuse Map", 2D) = "white" {}
		normalTex("Normal Map", 2D) = "white" {}

		shadowTar("Shadow Map Target", 2D) = "white" {}
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

			uniform sampler2D colorTex, normalTex, shadowTex, shadowRev;

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

				if (_ProjectionParams.y > 0.3)
				{
					float x = (i.position.x * cameraX.x + i.position.y * cameraX.y + i.position.z * cameraX.z) / (cameraRadius * 2) + 0.5;
					float z = (i.position.x * cameraZ.x + i.position.y * cameraZ.y + i.position.z * cameraZ.z) / (cameraRadius * 2) + 0.5;
					float4 depthTex = tex2D(shadowTex, float2(x, z));

					float3 v3ToCam = cameraPosition.xyz - i.position.xyz;
					float dist = -dot(v3ToCam, cameraY);
					float depth = (dist - cameraNear) / (cameraFar - cameraNear);

					//Draw our RGBA shadow map
					if (_ProjectionParams.y == 0.5)
					{
						//1st, red channel
						if (depthTex.r > depth)
						{
							return fixed4(depth, depthTex.gba);
						}
						else
						{
							clip(-1.0);
						}
					}

					if (_ProjectionParams.y == 0.6)
					{
						return depthTex;

						//2nd, green channel
						if (depthTex.r < depth && depthTex.g > depth)
						//if (depthTex.g - depth > 0.05)
						{
							return fixed4(depthTex.r, depth, depthTex.ba);
						}
						return depthTex;
					}
				}
				else
				{
					float x = (i.position.x * cameraX.x + i.position.y * cameraX.y + i.position.z * cameraX.z) / (cameraRadius * 2) + 0.5;
					float z = (i.position.x * cameraZ.x + i.position.y * cameraZ.y + i.position.z * cameraZ.z) / (cameraRadius * 2) + 0.5;

					float4 depthTex = tex2D(shadowTex, float2(x, z));
					float4 depthRev = tex2D(shadowRev, float2(x, 1 - z));

					float3 normalM = tex2D(normalTex, i.uv);

					//Calc dis to camera
					float3 v3ToCam = cameraPosition.xyz - i.position.xyz;
					float dist = -dot(v3ToCam, cameraY);
					float depth = (dist - cameraNear) / (cameraFar - cameraNear);

					//Calc thickness
					float distCamera = length(cameraRevPosition - cameraPosition);
					float depF = depthTex.r * (cameraFar - cameraNear) + cameraNear;
					float depR = depthRev.r * (cameraFar - cameraNear) + cameraNear;
					float thickness = distCamera - depF - depR;

					//Normalmapping : get TBN Space
					float3 N = i.normal;
					float3 T = normalize(i.tangent - N * i.tangent * N);
					float3 B = cross(N, T);
					float3x3 T2W = float3x3(T, B, N);

					normalM = 2 * normalM - 1;
					normalM = normalize(mul(normalM, T2W));

					//col.rgb *= 0.5 + 0.5 * abs(dot(lightPosition, normalM));
					if (length(lightColor.rgb) < 0.3)
					{
						ambientColor *= 1.4;
					}
					col.rgb = col.rgb * ambientColor * 0.5 + col.rgb * lightColor * abs(dot(lightPosition, normalM));

					//col.rgb *= 0.5;

					//col.a = 1;
					//col.rgb = normal;
					//return col;

					if (depth > depthTex.r)
					{
						//col = fixed4(1, 1, 1, 0);
						col *= 0.6;
					}
					else
					{
						//col = fixed4(0, 0, 0, 0);
						col *= 1.1;
					}

					float fac = pow(length(float3(i.position.x / 5.0, 0/*i.position.y / 7.0*/, i.position.z / 5.0)), 1);
					if (fac > 1) fac = 1;
					if (fac < 0.3) fac = 0.3;
					col *= fac;

					//Double-Sided:
					float3 lookDir = i.position - _WorldSpaceCameraPos;
					lookDir = normalize(lookDir);
					fixed4 colYellow = fixed4(col.g * 0.9, col.g, col.g * 0.2, 1.0) * lightColor * 1.8;
					col *= ambientColor;
					float factor = pow(abs(dot(lookDir, lightPosition)), doubleSideRatio);

					col = (1 - factor) * col + factor * colYellow;

					/*if (thickness < 1 && thickness > -0.1)
					{
					col *= 2.0;
					col = fixed4(col.g * 0.9, col.g, col.g * 0.2, 1.0);
					//col = fixed4(1, 1, 1, 1);
					}*/

					//col *= 1 + col * 0.6;

					//col = fixed4(depth - depthTex.r, 0, 0, 1);
					//col = fixed4(0, (thickness < 0.5 ? 1 : 0), 0, 1);
					//col = fixed4(10*(x - 0.5), 0, 10*(1-z-0.5), 1);

					return col;
				}
				return fixed4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
}
