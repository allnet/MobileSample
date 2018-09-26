// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vector Sprites/Grid" {
	Properties {
		gridWidth("Grid Width", Int) = 0
		gridHeight("Grid Height", Int) = 0
		gridDivisionsX("Grid Divisions X Direction", Int) = 0
		gridDivisionsY("Grid Divisions Y Direction", Int) = 0
		drawGuides("Draw Guides", Int) = 0
		spriteAspectRatio("Sprite Aspect Ratio", Float) = 0
		drawSpriteCropLines("Draw Sprite Crop Lines", Int) = 0
		isProfessionalSkin("Professional Skin", Int) = 0
		zoom("Zoom", Float) = 0
		zoomCentreX("Zoom Centre X", Float) = 0
		zoomCentreY("Zoom Centre Y", Float) = 0
	}
	SubShader {
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vertexShader
				#pragma fragment fragmentShader
				#include "UnityCG.cginc"
				int gridWidth, gridHeight;
				int gridDivisionsX, gridDivisionsY;
				int drawGuides;
				float spriteAspectRatio;
				int drawSpriteCropLines;
				int isProfessionalSkin;
				float zoom, zoomCentreX, zoomCentreY;

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
					float pixelX = ((i.textureCoordinates.x / zoom) + zoomCentreX - (0.5 / zoom)) * gridWidth;
					float pixelY = ((i.textureCoordinates.y / zoom) + zoomCentreY - (0.5 / zoom)) * gridHeight;
					if (zoom < 1.001) {
						pixelX = (int) pixelX;
						pixelY = (int) pixelY;
					}
					int quarterWidth = gridWidth / 4;
					int quarterHeight = gridHeight / 4;
					int cropLineXLeft = pixelY % 24 < 12 ? lerp(gridWidth / 2, gridWidth / 4, min(spriteAspectRatio, 1)) : -1;
					int cropLineXRight = gridWidth - cropLineXLeft;
					int cropLineYTop = pixelX % 24 < 12 ? lerp(gridHeight / 2, gridHeight / 4, min(1 / spriteAspectRatio, 1)) : -1;
					int cropLineYBottom = gridHeight - cropLineYTop;
					if ((drawGuides == 1 &&
							((pixelX >= quarterWidth - 2 && pixelX <= quarterWidth + 1) ||
							(pixelX >= (quarterWidth * 3) - 2 && pixelX <= (quarterWidth * 3) + 1) ||
							(pixelY >= quarterHeight - 2 && pixelY <= quarterHeight + 1) ||
							(pixelY >= (quarterHeight * 3) - 2 && pixelY <= (quarterHeight * 3) + 1))) ||
							((pixelX + 1) % (float(gridWidth) / float(gridDivisionsX)) < 1.9999 && (pixelY + 1) % (float(gridHeight) / gridDivisionsY) < 1.9999))
						return isProfessionalSkin == 1 ? fixed4(0.75, 0.75, 0.75, 1) : fixed4(0.5, 0.5, 0.5, 1);
					else if (drawSpriteCropLines == 1 && (
							(pixelX >= cropLineXLeft - 1 && pixelX <= cropLineXLeft) || (pixelX >= cropLineXRight - 1 && pixelX <= cropLineXRight) ||
							(pixelY >= cropLineYTop - 1 && pixelY <= cropLineYTop) || (pixelY >= cropLineYBottom - 1 && pixelY <= cropLineYBottom)))
						return fixed4(1, 0.5, 0, 1);
					else
						return fixed4(0, 0, 0, 0);
				}
			ENDCG
		}
	}
}
