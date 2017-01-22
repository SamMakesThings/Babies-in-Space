//
// SunShader 1.0
//
// Panteleymonov Aleksandr 2016
//
// foxes@bk.ru
// manil@panteleymonov.ru
//

Shader "Space/Star/Body_rnd"
{
	Properties
	{
		_Radius("Radius", Float) = 0.5
		_Light("Light",Color) = (1,1,1,1)
		_Color("Color", Color) = (1,1,0,1)
		_Base("Base", Color) = (1,0,0,1)
		_Dark("Dark", Color) = (1,0,1,1)
		_Detail("Detail Body", Range(0,5)) = 3
		_Glow("Glow", Range(1.0,100.0)) = 4.0
		_Zoom("Zoom", Float) = 1.0
		_SpeedHi("Speed Hi", Range(0.0,10)) = 2.0
		_SpeedLow("Speed Low", Range(0.0,10)) = 2.0
		_Seed("Seed", Range(-10,10)) = 0
		_BodyNoiseL("Body Noise Light", Vector) = (0.625,0.125,0.0625,0.03125)
		_BodyNoiseS("Body Noise Scale", Vector) = (3.6864,61.44,307.2,600.0)
		//_RND("", 3D) = "white" {}
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		//Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			//Tags{ "Queue" = "Transparent" }
			Blend One OneMinusSrcAlpha
			//ZWrite Off
			//Cull Front
			//ColorMask 0
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			//#pragma glsl
			//#pragma multi_compile SIMPLE_SHADING
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
			#if UNITY_5_0
				UNITY_FOG_COORDS(1)
			#endif
				float4 vertex : SV_POSITION;
			//#if SHADER_API_D3D11 
				float3 uv1 : TEXCOORD1;
			//#endif
			};

			sampler3D _RND;
			//float4 _MainTex_ST;
			float _Radius;
			float _RayString;
			fixed4 _Light;
			fixed4 _Color;
			fixed4 _Base;
			fixed4 _Dark;
			fixed4 _Ray;
			fixed4 _RayLight;
			int _Detail;
			float _Rays;
			float _RayRing;
			float _RayGlow;
			float _Zoom;
			float _SpeedHi;
			float _SpeedLow;
			float _SpeedRay;
			float _SpeedRing;
			float _Glow;
			float _Seed;
			float4 _BodyNoiseL;
			float4 _BodyNoiseS;

			float4 posGlob; // center position
									
			v2f vert (appdata v)
			{
				v2f o;
				//o.uv1 = (float3)v.vertex;
				//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				
				posGlob = float4(UNITY_MATRIX_MV[0].w, UNITY_MATRIX_MV[1].w, UNITY_MATRIX_MV[2].w,0);
				//float3x3 r=transpose((float3x3)UNITY_MATRIX_MV);
				float3x3 m;
				m[2]=normalize(mul((float3x3)UNITY_MATRIX_T_MV,(float3)posGlob));
				m[1]=cross(m[2],float3(0.0, 1.0, 0.0));
				float l=length(m[1]);
				if (l==0.0) m[1]=cross(m[2],float3(1.0, 0.0, 0.0));
				m[1]=normalize(m[1]);
				m[0]=normalize(cross(m[1],m[2]));
				o.uv1 = mul(transpose(m), (float3)v.vertex);
            	o.vertex = mul(UNITY_MATRIX_MVP, float4(o.uv1, 1.0));
				
				#if UNITY_5_0
				UNITY_TRANSFER_FOG(o,o.vertex);
				#endif
				return o;
			}

			// mix noise for alive animation
			fixed noise4t(fixed4 x)
			{
				//return lerp(dot(tex3D(_RND, (float3)x*0.015625), float4(0.25, 0.25, 0.25, 0.25)), dot(tex3D(_RNDnext, (float3)x*0.015625), float4(0.25, 0.25, 0.25, 0.25)), frac(x.w));
				x*= 0.0078125;
				x.w*=0.5;
				fixed4 x1=x+fixed4(x.w,x.w,x.w,0);
				fixed4 x2=x+fixed4(-x.w,-x.w,-x.w,0);
				fixed4 x3=x+fixed4(-x.w,x.w,x.w,0);
				fixed4 x4=x+fixed4(x.w,-x.w,-x.w,0);
				float s=tex3D(_RND, (float3)x1).r;
				s+=tex3D(_RND, (float3)x2).g;
				s+=tex3D(_RND, (float3)x3).b;
				s+=tex3D(_RND, (float3)x4).a;
				//float s=dot(tex3D(_RND, (float3)x1*0.0078125), float4(0.25, 0.25, 0.25, 0.25));
				//s+=dot(tex3D(_RND, (float3)x2*0.0078125), float4(0.25, 0.25, 0.25, 0.25));
				//s+=dot(tex3D(_RND, (float3)x3*0.0078125), float4(0.25, 0.25, 0.25, 0.25));
				//s+=dot(tex3D(_RND, (float3)x4*0.0078125), float4(0.25, 0.25, 0.25, 0.25));
				return s*0.25;
			}
							
			float RayProj;
			float sqRadius; // sphere radius
			float fragTime;
			float sphere; // sphere distance
			float3 surfase; // position on surfase

			// body of a star
			fixed noiseSpere(float zoom, float3 subnoise, float anim)
			{
				fixed s = 0.0;

				if (sphere <sqRadius) {
					if (_Detail>0.0) s = noise4t(fixed4(surfase*zoom*_BodyNoiseS.x + subnoise, fragTime*_SpeedHi))*_BodyNoiseL.x;//*0.625;
					if (_Detail>1.0) s =s*0.85+noise4t(fixed4(surfase*zoom*_BodyNoiseS.y + subnoise*3.0, fragTime*_SpeedHi*3.0))*_BodyNoiseL.y;//*0.125;
					if (_Detail>2.0) s =s*0.94+noise4t(fixed4(surfase*zoom*_BodyNoiseS.z + subnoise*5.0, anim*5.0))*_BodyNoiseL.z;//*0.0625;//*0.03125;
					if (_Detail>3.0) s =s*0.98+noise4t(fixed4(surfase*zoom*_BodyNoiseS.w + subnoise*6.0, fragTime*_SpeedLow*6.0))*_BodyNoiseL.w;//*0.03125;
					if (_Detail>4.0) s =s*0.98+noise4t(fixed4(surfase*zoom*_BodyNoiseS.w*2.0 + subnoise*9.0, fragTime*_SpeedLow*9.0))*_BodyNoiseL.w*0.36; //0.01125
					//if (_Detail>0.0) s = noise4t(fixed4(surfase*zoom*3.6864 + subnoise, fragTime*_SpeedHi))*0.625;
					//if (_Detail>1.0) s =s*0.85+noise4t(fixed4(surfase*zoom*61.44 + subnoise*3.0, fragTime*_SpeedHi*3.0))*0.125;
					//if (_Detail>2.0) s =s*0.94+noise4t(fixed4(surfase*zoom*307.2 + subnoise*5.0, anim*5.0))*0.0625;//*0.03125;
					//if (_Detail>3.0) s =s*0.98+noise4t(fixed4(surfase*zoom*600.0 + subnoise*6.0, fragTime*_SpeedLow*6.0))*0.03125;
					//if (_Detail>4.0) s =s*0.98+noise4t(fixed4(surfase*zoom*1200.0 + subnoise*9.0, fragTime*_SpeedLow*9.0))*0.01125;
				}

				return s;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float invz =1/_Zoom;
				_Radius*=invz;
				fragTime=_Time.x*10.0;
				posGlob = float4(UNITY_MATRIX_MV[0].w, UNITY_MATRIX_MV[1].w, UNITY_MATRIX_MV[2].w,0);
				float3x3 m = (float3x3)UNITY_MATRIX_MV;
				float3 ray = normalize(mul(m, i.uv1) + posGlob.xyz);
				m = transpose((float3x3)UNITY_MATRIX_V);

				RayProj = dot(ray, posGlob.xyz);
				float sqDist=dot(posGlob.xyz, posGlob.xyz);
				sphere = sqDist - RayProj*RayProj;
				sqRadius = _Radius*_Radius;
				if (RayProj<=0.0) sphere=sqRadius;
				float3 pr = ray*abs(RayProj) - (float3)posGlob;
				
				fixed4 col = fixed4(0, 0, 0, 0);
				if (sqDist<=sqRadius) {
					surfase=(float3)-posGlob;
					sphere=sqDist;
				} else if (sphere <sqRadius) {
					float l1 = sqrt(sqRadius - sphere);
					surfase = mul(m,pr - ray*l1);
					col.w = 1.0;
				} else {
					surfase=(float3)0;
				}

				if (_Detail >= 1.0) {
					float s1 = noiseSpere(0.5*_Zoom, float3(45.78, 113.04, 28.957)*_Seed, fragTime*_SpeedLow);
					s1 = pow(s1*2.4, 2.0);
					float s2 = noiseSpere(4.0*_Zoom, float3(83.23, 34.34, 67.453)*_Seed, fragTime*_SpeedHi);
					s2 = s2*2.2;
					
					col.xyz = fixed3(lerp((float3)_Color, (float3)_Light, pow(s1, 60.0))*s1);
					col.xyz += fixed3(lerp(lerp((float3)_Base, (float3)_Dark, s2*s2), (float3)_Light, pow(s2, 10.0))*s2);
				}
				
				col = clamp(col, 0, 1);

#if UNITY_5_0
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
#endif
				return col;
			}
			ENDCG
		}
	}
}
