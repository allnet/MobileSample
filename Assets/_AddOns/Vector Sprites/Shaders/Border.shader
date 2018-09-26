// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vector Sprites/Border" {
	Properties {
		boxWidth("Box Width", Int) = 0
		boxHeight("Box Height", Int) = 0
		colour("Colour", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vertexShader
				#pragma fragment fragmentShader
				#include "UnityCG.cginc"
				int boxWidth, boxHeight;
				float4 colour;

				//Vertex to fragment structure.
				struct vertexToFragment {
					float4 position : SV_POSITION;
					float2 textureCoordinates : TEXCOORD0;
				};

				//Vertex shader.
				vertexToFragment vertexShader(appdata_base v) {
					vertexToFragment o;
					o.position = UnityObjectToClipPos(v.vertex);
					o.textureCoordinates = v.texcoord;
					return o;
				}

				//Fragment shader.
				fixed4 fragmentShader(vertexToFragment i) : SV_Target {
					int pixelX = floor((i.textureCoordinates.x * boxWidth) + 0.01);
					int pixelY = floor((i.textureCoordinates.y * boxHeight) + 0.01);
					if (pixelX < 2 || pixelY < 2 || pixelX > boxWidth - 3 || pixelY > boxHeight - 3)
						return colour;
					else
						return fixed4(0, 0, 0, 0);
				}
			ENDCG
		}
	}
}
