﻿/*
 * Jose Manuel Herrera Vera 
 * @projects_ia (Twitter)
 * https://josemhv.itch.io
 *
 * FogOfWar v1.0
 * 
 * Date: 2021/06/21
 */
Shader "FogOfWar"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
		_SecondaryTex("Secondary Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags 
		{
			"Queue" = "Transparent+1"
		}
        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			sampler2D _SecondaryTex;

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = tex2D(_MainTex, i.uv) + tex2D(_SecondaryTex, i.uv);
				col.a = 2.0f - col.r * 1.5f - col.b * 0.5f - col.g;
                return fixed4(0,0,0,col.a);
            }
            ENDCG
        }
    }
}
