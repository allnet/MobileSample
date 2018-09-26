// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vector Sprites/Pillow And Glow Render Texture" {
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            CGPROGRAM
                #pragma vertex vertexShader
                #pragma fragment fragmentShader
                #include "UnityCG.cginc"
				uniform sampler2D _MainTex;

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
				fixed4 fragmentShader(vertexToFragment i) : SV_Target{
					return tex2D(_MainTex, i.textureCoordinates);
				}
		ENDCG
	}
	}
}
