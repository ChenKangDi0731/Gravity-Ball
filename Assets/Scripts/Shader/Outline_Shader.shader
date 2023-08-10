// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Outline_Shader"
{
    Properties
    {

        _Diffuse("Diffuse Color",color)=(1,1,1,1)
        [HDR]_OutlineColor("Outline Color",Color)=(0,0,0,1)
        _Outline("Outline",float)=0.1
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [HDR]_HDRColor("Emission Color",Color)=(1,1,1,1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue"="Transparent-1"}
        LOD 100

        Pass{

            Cull off
            Zwrite Off
            offset 1,1
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma vertex vert
        #pragma fragment frag

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
#include "UnityCG.cginc"
        fixed4 _OutlineColor;
        float _Outline;
        float4 _MainTex_ST;
        struct appdata {
            float4 vertex : POSITION;
            float3 normal:NORMAL;
            float2 uv:TEXCOORD0;
        };
        struct v2f {
             float4 vertex:SV_POSITION;
             float2 uv:TEXCOORD0;
        };
        
        v2f vert(appdata v) { 
            v2f o;
            //设置一下xy
            //v.vertex.xy *= 1.1;
            v.vertex.xyz += v.normal * _Outline;
            o.vertex = UnityObjectToClipPos(v.vertex);

            o.uv = v.uv;
            return o;
        }

        fixed4 frag(v2f i) :SV_Target{
            return _OutlineColor;
        }
            ENDCG

        }

             Pass{
            CGPROGRAM

#include "Lighting.cginc"

        #pragma vertex vert
        #pragma fragment frag

        struct appdata {
            float4 vertex:POSITION;
            float3 normal:NORMAL;
            float2 uv:TEXCOORD0;
            float4 color:COLOR;
        };
        struct v2f {
            float4 vertex:SV_POSITION;
            float2 uv:TEXCOORD0;
            float3 color:Color;
        };

        float4 _Diffuse;
        sampler2D _MainTex;
        float4 _HDRColor;

        v2f vert(appdata v) {
            v2f o;

            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;

            fixed3 worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
            fixed3 worldLight = normalize(_WorldSpaceLightPos0.xyz);

            fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
           
            fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLight));
            o.color = ambient + diffuse;
            return o;
        }

        fixed4 frag(v2f i) :SV_Target{
            float4 c = tex2D(_MainTex,i.uv);
            int sign = 1;
            float4 hdr = _HDRColor * sign + (1 - sign) * float4(1,1,1,1);
            return c * fixed4(i.color,1) * hdr;
        }

            ENDCG
        }
    }
       


    FallBack "Diffuse"
}
