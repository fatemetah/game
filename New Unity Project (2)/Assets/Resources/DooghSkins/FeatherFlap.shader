Shader "SugarRush/FeatherFlap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("Renderer color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable external alpha", Float) = 0
        _Roots ("Wing roots (left, right)", Vector) = (.27,.73,0,0)
        _Band ("Feather area (bottom, top, right slope)", Vector) = (.08,.94,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="False" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment Flap
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            float4 _Roots, _Band;

            fixed4 Flap(v2f input) : SV_Target
            {
                float2 uv = input.texcoord;
                // A continuous bend starts outside the vessel. At the roots the
                // offset and its slope are zero, so the painted outline stays joined.
                float left = 1 - smoothstep(0, _Roots.x, uv.x);
                float right = smoothstep(_Roots.y, 1, uv.x);
                float top = _Band.y + _Band.z * saturate((uv.x - _Roots.y) / (1 - _Roots.y));
                float feathers = smoothstep(_Band.x, _Band.x + .085, uv.y)
                               * (1 - smoothstep(top - .085, top, uv.y));
                uv.y -= .075 * sin(_Time.y * 7) * (left + right) * feathers;
                fixed4 c = SampleSpriteTexture(uv) * input.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
