Shader "Unlit/SimpleOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineWidth("Outline Width", float) = 0.1
        _OutlineColor("Outline Color", Color) = (0,0,0)
        [Toggle(_)] _Is_BakedNormal("Is_BakedNormal", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            Cull Back
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
        ENDCG
    }
         Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            half _OutlineWidth;
            fixed4 _OutlineColor;
            half _Is_BakedNormal;
            half _Offset_Z;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
                float2 uv : TEXCOORD0;
                float3 bakedNormal:TEXCOORD7;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 TransformTBN(float2 bakedNormal, float3x3 tbn)
            {
                float3 normal = float3(bakedNormal, 0);
                normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
                return  (mul(normal, tbn));
            }
     
            v2f vert(appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float3 normalOS = normalize(v.normal);
                float3 tangentOS = v.tangent;
                tangentOS = normalize(tangentOS);
                float3 bitangentOS = normalize(cross(normalOS, tangentOS) * v.tangent.w);
                float3x3 tbn = float3x3(tangentOS, bitangentOS, normalOS);

                float3 _BakedNormalDir = (TransformTBN(v.bakedNormal, tbn));

                float4 pos = UnityObjectToClipPos(v.vertex);
                float Set_OutlineWidth = pos.w * _OutlineWidth;
                Set_OutlineWidth = min(Set_OutlineWidth, _OutlineWidth);
                Set_OutlineWidth *= _OutlineWidth;
                Set_OutlineWidth = min(Set_OutlineWidth, _OutlineWidth) * 0.001;
                float3 Set_NormalDir = lerp(v.normal, _BakedNormalDir, _Is_BakedNormal);

                o.pos = UnityObjectToClipPos(v.vertex + Set_NormalDir * Set_OutlineWidth);

                return o; 
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
        ENDCG
    }

       
    }
}
