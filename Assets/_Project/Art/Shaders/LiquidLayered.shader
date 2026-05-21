// Inspired by WaterSort BottleColor shadergraph.
// Built-in pipeline (Sprite). 4 horizontal liquid layers + fill amount + rotation tilt.
//
//   _MainTex   : Bottle/Tube mask sprite (alpha = bottle shape).
//   _C1.._C4   : Layer colors (bottom -> top).
//   _FillAmount: 0..1 — total liquid level.
//   _SARM      : -1..1 — Scale-Adjusted Rotation Multiplier. Tilts liquid surface
//                opposite to bottle rotation so it stays roughly horizontal.
//   _LayerCount: 1..4 — how many active layers (others fully transparent).
//
// Layer boundaries are equally spaced over [0..fillAmount]:
//   y < fillAmount * (i/N)  → layer i color
//
// Tilt: shift uv.y by _SARM * (uv.x - 0.5) before sampling layer boundary.
Shader "ChemistryGame/LiquidLayered"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _C1 ("Color 1 (bottom)", Color) = (0.4, 0.8, 1, 0.85)
        _C2 ("Color 2",          Color) = (0.4, 0.8, 1, 0.85)
        _C3 ("Color 3",          Color) = (0.4, 0.8, 1, 0.85)
        _C4 ("Color 4 (top)",    Color) = (0.4, 0.8, 1, 0.85)
        _FillAmount ("Fill Amount", Range(0,1)) = 0.5
        _SARM       ("Surface Tilt (SARM)", Range(-1,1)) = 0
        _LayerCount ("Layer Count", Range(1,4)) = 4
        _EdgeSoft   ("Edge Softness", Range(0,0.05)) = 0.01

        // For UI compatibility (CanvasRenderer & RectMask2D)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil     ("Stencil ID", Float) = 0
        _StencilOp   ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask   ("Color Mask", Float) = 15
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _C1, _C2, _C3, _C4;
            float _FillAmount;
            float _SARM;
            float _LayerCount;
            float _EdgeSoft;

            v2f vert(appdata IN)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(IN.vertex);
                o.uv  = TRANSFORM_TEX(IN.uv, _MainTex);
                o.color = IN.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 maskCol = tex2D(_MainTex, i.uv);
                // Tilted Y by surface SARM relative to center X.
                float yTilted = i.uv.y + _SARM * (i.uv.x - 0.5);

                // Where the liquid top is.
                float fill = _FillAmount;
                if (yTilted > fill + _EdgeSoft) discard;

                // Determine layer index based on yTilted within [0..fill].
                float n = max(1.0, _LayerCount);
                float layerSize = fill / n;
                float bandIdx = floor(yTilted / max(layerSize, 1e-5));
                bandIdx = clamp(bandIdx, 0, n - 1);

                // Pick color.
                fixed4 c;
                if (bandIdx < 0.5)      c = _C1;
                else if (bandIdx < 1.5) c = _C2;
                else if (bandIdx < 2.5) c = _C3;
                else                    c = _C4;

                // Smooth top edge (anti-alias)
                float topEdge = smoothstep(fill + _EdgeSoft, fill - _EdgeSoft, yTilted);
                c.a *= topEdge * maskCol.a * i.color.a;
                c.rgb *= i.color.rgb;
                return c;
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
