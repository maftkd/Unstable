Shader "Unlit/box"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Main", Color) = (1,1,1,1)
		_Outline ("Outline", Color) = (1,1,1,1)
		_Threshold ("Thickness", Float) = 0.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _Outline;
			fixed _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed bord = step(_Threshold,abs(0.5-i.uv.x));
				bord+=step(_Threshold,abs(0.5-i.uv.y));
                fixed4 col = lerp(_Color,_Outline,bord);
                // apply fog
                return col;
            }
            ENDCG
        }
    }
}
