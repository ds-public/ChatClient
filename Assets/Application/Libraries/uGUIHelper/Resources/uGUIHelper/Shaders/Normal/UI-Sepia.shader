// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/Sepia"
{
	Properties
	{
		[PerRendererData] _MainTex( "Sprite Texture", 2D ) = "white" {}
		_Color( "Tint", Color ) = ( 1,1,1,1 )
		
		_StencilComp( "Stencil Comparison", Float ) = 8
		_Stencil( "Stencil ID", Float ) = 0
		_StencilOp( "Stencil Operation", Float ) = 0
		_StencilWriteMask( "Stencil Write Mask", Float ) = 255
		_StencilReadMask( "Stencil Read Mask", Float ) = 255
		_Darkness( "Dark", Range( 0, 0.2 ) ) = 0.1
		_Strength( "Strength", Range( 0.05, 0.15 ) ) = 0.1
		_Interpolation( "Interpolation", Range( 0, 1 ) ) = 1

		_ColorMask( "Color Mask", Float ) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Offset -1, -1
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
            half _Darkness;
            half _Strength;
			half _Interpolation;

			bool _UseClipRect;
			float4 _ClipRect;

			bool _UseAlphaClip;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
				#endif
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				half4 original_color = color;

				half gray = color.r * 0.3 + color.g * 0.6 + color.b * 0.1 - _Darkness;
                gray = ( gray < 0 ) ? 0 : gray;

                half R = gray + _Strength;
                half B = gray - _Strength;

                R = ( R > 1.0 ) ? 1.0 : R;
                B = ( B < 0 ) ? 0 : B;
                color.rgb = fixed3(R, gray, B);

				color = lerp(original_color, color, _Interpolation);

				if (_UseClipRect)
					color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				if (_UseAlphaClip)
					clip (color.a - 0.001);

				return color;
			}
		ENDCG
		}
	}
}
