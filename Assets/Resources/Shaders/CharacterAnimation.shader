Shader "Custom/CharacterAnimation"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Base Color", Color) = (1,1,1,1)
        _Tint ("Tint Color", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"

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
				float2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			fixed4 _Tint;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // c.rgb *= 256;
                // _Tint.rgba *= 256;
                // // //(alpha * ((source + 256) - dest)) / 256 + dest - alpha

                // // ((ta * ((tr + 256) - colour.r)) >> 8) + colour.r - ta
                // c.rgb = ((_Tint.a * ((_Tint.rgb + 256) - c.rgb)) / 256) + c.rgb - _Tint.a;
                // c.rgb /= 256;

                // finalcolor = (source * alpha) + (dest * (1 - alpha))
                c.rgb = (_Tint.rgb * _Tint.a) + (c.rgb * (1 - _Tint.a));
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}