// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vector Sprites/Shape" {
	Properties {
		colourSourceBlend("Colour Source Blend", Int) = 0
		colourDestinationBlend("Colour Destination Blend", Int) = 0
		alphaSourceBlend("Alpha Source Blend", Int) = 0
		alphaDestinationBlend("Alpha Destination Blend", Int) = 0
		alphaBlendOperation("Alpha Blend Operation", Int) = 0
		fillStyle("Fill Style", Int) = 0
		colour1("Colour 1", Color) = (1, 1, 1, 1)
		colour2("Colour 2", Color) = (1, 1, 1, 1)
		areaFromX("Area From X", Float) = 0
		areaToX("Area To X", Float) = 0
		areaFromY("Area From Y", Float) = 0
		areaToY("Area To Y", Float) = 0
		angle("Angle", Float) = 0
		bars("Bars", Int) = 0
		noiseType("Noise Type", Int) = 0
		noiseLevel("Noise Level", Float) = 0
		centreX("Centre X", Float) = 0
		centreY("Centre Y", Float) = 0
		radialSize("Radial Size", Float) = 0
		colourBias("Colour Bias", Float) = 0
		colourBands("Colour Bands", Float) = 0
		zoom("Zoom", Float) = 0
		zoomCentreX("Zoom Centre X", Float) = 0
		zoomCentreY("Zoom Centre Y", Float) = 0
	}
	SubShader {
		Pass {
			Blend [colourSourceBlend] [colourDestinationBlend], [alphaSourceBlend] [alphaDestinationBlend]
			BlendOp Add, [alphaBlendOperation]
			ZTest Less
			CGPROGRAM
				#pragma vertex vertexShader
				#pragma fragment fragmentShader
				#pragma target 3.0
				#include "UnityCG.cginc"

				//Constants.
				static const int fillStyle_None = 0;
				static const int fillStyle_AngledBars = 1;
				static const int fillStyle_AngledBilinear = 2;
				static const int fillStyle_AngledLinear = 3;
				static const int fillStyle_Checkerboard = 4;
				static const int fillStyle_HorizontalBars = 5;
				static const int fillStyle_HorizontalBilinear = 6;
				static const int fillStyle_HorizontalLinear = 7;
				static const int fillStyle_Noise = 8;
				static const int fillStyle_Radial = 9;
				static const int fillStyle_SolidColour = 10;
				static const int fillStyle_VerticalBars = 11;
				static const int fillStyle_VerticalBilinear = 12;
				static const int fillStyle_VerticalLinear = 13;
				static const int noiseType_Random = 0;
				static const int noiseType_RGB = 1;

				//Variables.
				int fillStyle;
				float4 colour1, colour2;
				float areaFromX, areaToX, areaFromY, areaToY;
				float angle;
				int bars;
				int noiseType;
				float noiseLevel;
				float centreX, centreY;
				float radialSize;
				float colourBias;
				float colourBands;
				float zoom, zoomCentreX, zoomCentreY;

				//Vertex to fragment structure.
				struct vertexToFragment {
					float4 position : SV_POSITION;
					float2 pixelPosition : TEXCOORD0;
				};

				//Vertex shader.
				vertexToFragment vertexShader(appdata_base v) {
					vertexToFragment o;
					o.position = UnityObjectToClipPos(v.vertex);					
					o.pixelPosition = o.position.xy;
					o.position.x = (o.position.x * zoom) - (zoomCentreX * zoom * 2) + zoom;
					o.position.y = (o.position.y * zoom) - (zoomCentreY * zoom * 2) + zoom;
					return o;
				}

				//Fragment shader.
				fixed4 fragmentShader(vertexToFragment i) : SV_Target {

					//Immediately return transparent if the fill style is set to "none".
					if (fillStyle == fillStyle_None)
						return fixed4(0, 0, 0, 0);

					//If this is a fill style that uses the "area" properties, scale the pixel range such that X and Y co-ordinates are within the range -0.5 to 0.5
					//and the centre point is at centre of the area.
					if (fillStyle != fillStyle_Noise && fillStyle != fillStyle_SolidColour)
						i.pixelPosition = float2((((i.pixelPosition.x / 2) + 0.5 - areaFromX) / (areaToX - areaFromX)) - 0.5,
								(((-i.pixelPosition.y / 2) + 0.5 - areaFromY) / (areaToY - areaFromY)) - 0.5);

					//If this is a fill style that takes angles into account, rotate the pixel position around the centre point.
					if (fillStyle == fillStyle_AngledBars || fillStyle == fillStyle_AngledLinear || fillStyle == fillStyle_AngledBilinear ||
							fillStyle == fillStyle_Checkerboard)
						i.pixelPosition = float2((i.pixelPosition.x * cos(-angle)) + (i.pixelPosition.y * sin(angle)),
								(i.pixelPosition.x * sin(-angle)) + (i.pixelPosition.y * cos(angle)));

					//Handle fill styles that directly modify the output colour...
					if (fillStyle == fillStyle_Noise)
						return colour1 + float4(
							(frac(sin(dot(i.pixelPosition, float2(15.665, 54.99 * noiseLevel))) * 31225.554) - 0.5) * noiseLevel *
							(noiseType == noiseType_RGB ? colour1.r : 1),
							(frac(sin(dot(i.pixelPosition, float2(8.565 * noiseLevel, 99.2033))) * 20221.918) - 0.5) * noiseLevel *
							(noiseType == noiseType_RGB ? colour1.g : 1),
							(frac(sin(dot(i.pixelPosition, float2(-55.2033, 2.015 * noiseLevel))) * 15229.662) - 0.5) * noiseLevel *
							(noiseType == noiseType_RGB ? colour1.b : 1), 0);

					//Calculate the amount of each colour to use based on the fill style and pixel position.
					float amount = 0;
					if (fillStyle == fillStyle_AngledLinear || fillStyle == fillStyle_HorizontalLinear)
						amount = i.pixelPosition.x + 1 - centreX;
					else if (fillStyle == fillStyle_AngledBilinear || fillStyle == fillStyle_HorizontalBilinear)
						amount = abs((i.pixelPosition.x * 2) + 0.5 - centreX);
					else if (fillStyle == fillStyle_VerticalLinear)
						amount = i.pixelPosition.y + 1 - centreY;
					else if (fillStyle == fillStyle_VerticalBilinear)
						amount = abs((i.pixelPosition.y * 2) + 0.5 - centreY);
					else if (fillStyle == fillStyle_HorizontalBars)
						amount = floor((-i.pixelPosition.y + 2 - centreY) * bars) % 2;
					else if (fillStyle == fillStyle_AngledBars || fillStyle == fillStyle_VerticalBars)
						amount = floor((i.pixelPosition.x + 2 - centreX) * bars) % 2;
					else if (fillStyle == fillStyle_Checkerboard)
						amount = (floor((-i.pixelPosition.y + 2 - centreY) * bars) + floor((i.pixelPosition.x + 2 - centreX) * bars)) % 2;
					else if (fillStyle == fillStyle_Radial)
						amount = length(i.pixelPosition + float2(0.5 - centreX, 0.5 - centreY)) / radialSize;

					//For linear gradients/radials, adjust the amount according to the colour bias.
					if (fillStyle == fillStyle_AngledLinear || fillStyle == fillStyle_HorizontalLinear || fillStyle == fillStyle_Radial || fillStyle ==
							fillStyle_VerticalLinear || fillStyle == fillStyle_AngledBilinear || fillStyle == fillStyle_HorizontalBilinear ||
							fillStyle == fillStyle_VerticalBilinear)
						amount = floor(saturate(amount + ((colourBias - 0.5) * 2)) * colourBands) / colourBands;

					//Return the colour as a percentage of colour 1 and 2.
					return (colour1 * (1 - amount)) + (colour2 * amount);
				}
			ENDCG
		}
	}
}
