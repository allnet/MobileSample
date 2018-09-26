// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vector Sprites/Icons" {
	Properties {
		iconIndex("Icon Index", Int) = 0
		pixelYFrom("Pixel Y From", Int) = 0
		pixelYTo("Pixel Y To", Int) = 0
	}
	SubShader {
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vertexShader
				#pragma fragment fragmentShader
				#include "UnityCG.cginc"
				int iconIndex;
				int pixelYFrom, pixelYTo;

				//Constants.
				static const int icon_Expand = 0;
				static const int icon_Contract = 1;
				static const int icon_T = 2;
				static const int icon_L = 3;
				static const int icon_Translate = 4;
				static const int icon_Rotate = 5;
				static const int icon_Scale = 6;

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

					//Get the X and Y position of the pixel
					int pixelX = floor((i.textureCoordinates.x * (iconIndex <= icon_L ? 20 : 32)) + 0.01);
					int pixelY = floor((i.textureCoordinates.y * (iconIndex <= icon_L ? 20 : 32)) + 0.01);

					//Cull the pixel if necessary.
					if (pixelY < pixelYFrom || pixelY > pixelYTo)
						return float4(0, 0, 0, 0);

					//Draw the icon.
					if (iconIndex == icon_Expand || iconIndex == icon_Contract) {
						if (((pixelX == 4 || pixelX == 16) && (pixelY >= 4 && pixelY <= 16)) || ((pixelY == 4 || pixelY == 16) && (pixelX >= 4 && pixelX <= 16)) ||
								(pixelX >= 7 && pixelX <= 13 && pixelY == 10) || (iconIndex == icon_Expand && pixelY >= 7 && pixelY <= 13 && pixelX == 10))
							return fixed4(1, 1, 1, 1);
						else
							return fixed4(0, 0, 0, 0);
					}
					else if (iconIndex == icon_T || iconIndex == icon_L) {
						if ((pixelX == 10 && pixelY % 2 == 0 && (iconIndex == icon_T || pixelY >= 10)) || (pixelX > 10 && pixelX % 2 == 0 && pixelY == 10))
							return fixed4(1, 1, 1, 1);
						else
							return fixed4(0, 0, 0, 0);
					}
					else if (iconIndex == icon_Translate) {
						if ((pixelX >= 14 && pixelX <= 18 && pixelY >= 6 && pixelY <= 26) || (pixelY >= 14 && pixelY <= 18 && pixelX >= 6 && pixelX <= 26) ||
								(abs(16 - pixelX) - (32 - pixelY) < 0 && pixelY > 26 && pixelY < 31) ||
								(abs(16 - pixelX) - pixelY < 0 && pixelY < 6 && pixelY > 1) ||
								(abs(16 - pixelY) - (32 - pixelX) < 0 && pixelX > 26 && pixelX < 31) ||
								(abs(16 - pixelY) - pixelX < 0 && pixelX < 6 && pixelX > 1))
							return fixed4(1, 1, 0, 0.5);
						else if ((pixelX >= 13 && pixelX <= 19 && pixelY >= 6 && pixelY <= 26) || (pixelY >= 13 && pixelY <= 19 && pixelX >= 6 && pixelX <= 26) ||
								(abs(16 - pixelX) - (32 - pixelY) <= 0 && pixelY >= 26) || (abs(16 - pixelX) - pixelY <= 0 && pixelY <= 6 && pixelY > 0) ||
								(abs(16 - pixelY) - (32 - pixelX) <= 0 && pixelX >= 26) || (abs(16 - pixelY) - pixelX <= 0 && pixelX <= 6 && pixelX > 0))
							return fixed4(0, 0, 0, 0.5);
						else
							return fixed4(0, 0, 0, 0);
					}
					else if (iconIndex == icon_Rotate) {
						float distance = length(float2(pixelX, pixelY) - float2(15, 16));
						if ((distance > 7 && distance < 12 && (pixelX < 16 || pixelY > 16)) || (abs(24 - pixelX) - (pixelY - 9) < 0 && pixelY < 17 && pixelY > 9))
							return fixed4(1, 1, 0, 0.5);
						else if ((distance > 6 && distance < 13 && (pixelX < 17 || pixelY > 16)) ||
								(abs(24 - pixelX) - (pixelY - 9) <= 0 && pixelY < 18 && pixelY > 8))
							return fixed4(0, 0, 0, 0.5);
						else
							return fixed4(0, 0, 0, 0);
					}
					else if (iconIndex == icon_Scale) {
						if ((pixelX + pixelY > 29 && pixelX + pixelY < 33 && pixelX > 4 && pixelY < 27 && pixelX < 23 && pixelY > 8) ||
								(pixelX > 17 && pixelX < 26 && pixelY > 5 && pixelY < 14 && pixelX - pixelY > 12))
							return fixed4(1, 1, 0, 0.5);
						else if ((pixelX + pixelY > 28 && pixelX + pixelY < 34 && pixelX > 3 && pixelY < 28 && pixelX < 24 && pixelY > 7) ||
								(pixelX > 16 && pixelX < 27 && pixelY > 4 && pixelY < 15 && pixelX - pixelY > 11))
							return fixed4(0, 0, 0, 0.5);
						else if ((pixelX > 0 && pixelX < 10 && pixelY > 21 && pixelY < 31) || (pixelX > 12 && pixelX < 31 && pixelY > 0 && pixelY < 19))
							return fixed4(1, 1, 0, 0.5);
						else if ((pixelX < 11 && pixelY > 20) || (pixelX > 11 && pixelY < 20))
							return fixed4(0, 0, 0, 0.5);
						else
							return fixed4(0, 0, 0, 0);
					}
					else
						return fixed4(0, 0, 0, 0);
				}
			ENDCG
		}
	}
}
