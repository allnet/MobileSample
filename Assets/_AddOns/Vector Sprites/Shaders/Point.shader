// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vector Sprites/Point" {
	Properties {
		size("Size", Int) = 0
		colour("Colour", Color) = (1, 1, 1, 1)
		clampXMin("Clamp X Min", Float) = 0
		clampXMax("Clamp X Max", Float) = 0
		clampYMin("Clamp Y Min", Float) = 0
		clampYMax("Clamp Y Max", Float) = 0
	}
	SubShader {
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vertexShader
				#pragma fragment fragmentShader
				#include "UnityCG.cginc"
				int size;
				float4 colour;
				float clampXMin, clampXMax, clampYMin, clampYMax;

				//Vertex to fragment structure.
				struct vertexToFragment {
					float4 position : SV_POSITION;
					float2 textureCoordinates : TEXCOORD0;
					float2 pixelPosition : TEXCOORD1;
				};

				//Vertex shader.
				vertexToFragment vertexShader(appdata_base v) {
					vertexToFragment o;
					o.position = UnityObjectToClipPos(v.vertex);
					o.textureCoordinates = v.texcoord;
					o.pixelPosition = o.position.xy;
					return o;
				}

				//Fragment shader.
				fixed4 fragmentShader(vertexToFragment i) : SV_Target {
					if ((i.pixelPosition.x < clampXMin) || (i.pixelPosition.x > clampXMax) || (i.pixelPosition.y < clampYMin) || (i.pixelPosition.y > clampYMax))
						return fixed4(0, 0, 0, 0);
					float distance = length((i.textureCoordinates - float2(0.5, 0.5)));
					if (distance > 0.5)
						return fixed4(0, 0, 0, 0);
					else if (distance > 0.5 - (2 / float(size)))
						return fixed4(0, 0, 0, 0.375);
					else
						return fixed4(colour.r, colour.g, colour.b, 0.375);
				}
			ENDCG
		}
	}
}
