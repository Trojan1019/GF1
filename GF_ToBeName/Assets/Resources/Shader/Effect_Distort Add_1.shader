Shader "Effect/Distort Add_1" {
	Properties {
		[HDR] _TintColor ("Tint Color", Vector) = (0.5,0.5,0.5,0.5)
		_MainTex ("Main Texture", 2D) = "white" {}
		_DistortTex ("Distort Texture (RG)", 2D) = "white" {}
		_Mask ("Mask ( R Channel )", 2D) = "white" {}
		_HeatTime ("Heat Time", Range(-1, 1)) = 0
		_ForceX ("Strength X", Range(0, 1)) = 0
		_ForceY ("Strength Y", Range(0, 1)) = 0
		_Bright ("Bright", Range(1, 5)) = 2
		_MainScrollUV_X ("Main UV Scroll X", Float) = 0
		_MainScrollUV_Y ("Main UV Scroll Y", Float) = 0
		_MaskScrollUV_X ("Mask UV Scroll X", Float) = 0
		_MaskScrollUV_Y ("Mask UV Scroll Y", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}