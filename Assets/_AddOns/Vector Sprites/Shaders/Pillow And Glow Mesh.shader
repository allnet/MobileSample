// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vector Sprites/Pillow And Glow Mesh" {
	Properties {
		colour1("Colour 1", Color) = (1, 1, 1, 1)
		colour2("Colour 2", Color) = (1, 1, 1, 1)
		colourBias("Colour Bias", Float) = 0
		colourBands("Colour Bands", Float) = 0
		stencilTestValue("Stencil Test Value", Int) = 0
	}
	SubShader {

		//First pass - for drawing the fill mesh and setting the stencil buffer pixel values to ensure the pillow/glow is only drawn inside/outside the shape
		//respectively.
		Pass {
			Stencil {
				Ref 1
				Pass Replace
				Fail Zero
			}
			ColorMask 0
		}

		//Second pass - for drawing the pillow/glow mesh, using a stencil test as set up in the first pass.
		Pass {
			Stencil {
				Ref [stencilTestValue]
				Comp Equal
			}
			Blend SrcAlpha Zero
			BlendOp Max
			CGPROGRAM
				#pragma vertex vertexShader
				#pragma fragment fragmentShader
				#include "UnityCG.cginc"

				//Variables.
				float4 colour1, colour2;
				float colourBias;
				float colourBands;

				//Vertex to fragment structure.
				struct vertexToFragment {
					float4 position : SV_POSITION;
					float pillowOrGlowShadingAmount : TEXCOORD0;
				};

				//Vertex shader.
				vertexToFragment vertexShader(appdata_base v) {
					vertexToFragment o;
					o.position = UnityObjectToClipPos(v.vertex);
					o.pillowOrGlowShadingAmount = v.texcoord.x;
					return o;
				}

				//Fragment shader.
				fixed4 fragmentShader(vertexToFragment i) : SV_Target {
					i.pillowOrGlowShadingAmount = floor(saturate(i.pillowOrGlowShadingAmount + ((colourBias - 0.5) * 2)) * colourBands) / colourBands;
					return (colour1 * (1 - i.pillowOrGlowShadingAmount)) + (colour2 * i.pillowOrGlowShadingAmount);
				}
			ENDCG
		}
	}
}
