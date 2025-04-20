Shader "ToonShader/Unlit/Texture_ToonRamp_Advanced"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ToonRamp("Toon Ramp", 2D) = "white" {}

        _BaseColor("Base Color", Color) = (1,1,1,1)
        _HighlightColor("Highlight Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (0.2,0.2,0.2,1)
        
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _Shininess("Shininess", Range(1,100)) = 20
        [Enum(Regular,0,Cartoon,1)] _SpecularMode("Specular Mode", Float) = 1

        [Header(Stencil)]
        _Stencil ("Stencil ID [0;255]", Float) = 0
        _ReadMask ("ReadMask [0;255]", Int) = 255
        _WriteMask ("WriteMask [0;255]", Int) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0

        [Header(Rendering)]
        _Offset("Offset", float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Cull Mode", Int) = 2
        [Enum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
        [Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 15
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex, _ToonRamp;
    float4 _MainTex_ST;

    half4 _BaseColor, _HighlightColor, _ShadowColor, _SpecularColor;
    float _Shininess;
    float _SpecularMode;

    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float3 worldNormal : TEXCOORD1;
        float3 worldPos : TEXCOORD2;
        float4 vertex : SV_POSITION;
    };

    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.worldNormal = UnityObjectToWorldNormal(v.normal);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        return o;
    }

    half4 frag (v2f i) : SV_Target
    {
        half3 normal = normalize(i.worldNormal);
        half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
        half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
        float NdotL = max(0, dot(normal, lightDir));

        // Toon Ramp Light
        float2 rampUV = float2(NdotL, 0.5);
        half4 rampColor = tex2D(_ToonRamp, rampUV);

        // Diffuse
        half4 texColor = tex2D(_MainTex, i.uv);
        half4 litColor = _BaseColor * rampColor * texColor;

        // Highlight & Shadow blend
        litColor = lerp(_ShadowColor * texColor, litColor, NdotL);
        if (NdotL > 0.95)
            litColor.rgb = lerp(litColor.rgb, _HighlightColor.rgb * texColor.rgb, saturate((NdotL - 0.95) * 20)); // Smooth blend

        // Specular
        float3 halfDir = normalize(lightDir + viewDir);
        float specDot = max(0, dot(normal, halfDir));
        float specular = 0;

        if (_SpecularMode < 0.5)
        {
            // Regular Specular
            specular = pow(specDot, _Shininess);
        }
        else
        {
            // Cartoon Specular
            specular = step(0.95, pow(specDot, _Shininess));
        }

        half4 specColor = _SpecularColor * specular;

        return litColor + specColor;
    }

    struct v2fShadow {
        V2F_SHADOW_CASTER;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    v2fShadow vertShadow(appdata_base v)
    {
        v2fShadow o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
        return o;
    }

    float4 fragShadow(v2fShadow i) : SV_Target
    {
        SHADOW_CASTER_FRAGMENT(i)
    }

    ENDCG

    SubShader
    {
        Stencil
        {
            Ref [_Stencil]
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }

        Pass
        {
            Tags { "RenderType"="Opaque" "Queue"="Geometry" }
            LOD 100
            Cull [_Culling]
            Offset [_Offset], [_Offset]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            ColorMask [_ColorMask]

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            LOD 80
            Cull [_Culling]
            Offset [_Offset], [_Offset]
            ZWrite [_ZWrite]
            ZTest [_ZTest]

            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma target 2.0
            #pragma multi_compile_shadowcaster
            ENDCG
        }
    }
}
