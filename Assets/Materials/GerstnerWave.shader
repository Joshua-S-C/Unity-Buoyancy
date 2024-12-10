Shader "Custom/Waves"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _NumWaves ("Number of Waves", Integer) = 1
        _WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0

        // Uniforms
        sampler2D _MainTex;
        int _NumWaves = 0;
        float4 _Waves[10];
        float4 _WaveA;

        struct Input
        {
            float2 uv_MainTex;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 GerstnerWave (float4 wave, float3 p, inout float3 tangent, inout float3 binormal) {
		    // Used in Calcs --------------------------------------------------------*/
            float steepness = wave.z;
		    float wavelength = wave.w;
		    float k = 2 * UNITY_PI / wavelength;        // Wave Number. 2 PI is what makes it pretty
			float c = sqrt(9.8 / k);                    // Speed, input is gravity but assuming 9.8
			float2 d = normalize(wave.xy);              // Direction normalized
			float f = k * (dot(d, p.xz) - c * _Time.y); 
			float a = steepness / k;                    // Amplitude

            // Normals --------------------------------------------------------------*/
			tangent += float3(
				-d.x * d.x * (steepness * sin(f)),
				d.x * (steepness * cos(f)),
				-d.x * d.y * (steepness * sin(f))
			);
			binormal += float3(
				-d.x * d.y * (steepness * sin(f)),
				d.y * (steepness * cos(f)),
				-d.y * d.y * (steepness * sin(f))
			);

            // Undulation -----------------------------------------------------------*/
			return float3(
				d.x * (a * cos(f)),
				a * sin(f),
				d.y * (a * cos(f))
			);
		}

        void vert (inout appdata_full vertexData) {
            float3 gridPoint = vertexData.vertex.xyz;

            float3 tangent = float3(1, 0, 0);
			float3 binormal = float3(0, 0, 1);
            float3 p = gridPoint;

            for(int i = 0; i < _NumWaves; i++) {
                if (_Waves[i].w == 0)
                    continue;
                p += GerstnerWave(_Waves[i], gridPoint, tangent, binormal);
            }

            float3 normal = normalize(cross(binormal, tangent));

            vertexData.vertex.xyz = p;
            vertexData.normal = normal;
        }

        // Uniforms
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
