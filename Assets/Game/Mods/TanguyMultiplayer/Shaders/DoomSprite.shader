// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//https://github.com/unitycoder/DoomStyleBillboardTest

Shader "UnityCoder/DoomSpriteAnimated2" 
{

	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Frames ("Frames (rows)", Float) = 8
		_Columns ("Columns", Float) = 3
		_AnimSpeed ("Animation Speed", Float) = 1
	}

	SubShader 
	{
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True"}
        
    	ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
	    Pass 
	    {
	        CGPROGRAM

		    #pragma vertex vert
		    #pragma fragment frag
			#define PI 3.1415926535897932384626433832795
			#define RAD2DEG 57.2957795131
		    #define SINGLEFRAMEANGLE (360/_Frames)
		    #define UVOFFSETX (1/_Frames)
		    #include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			struct appdata {
			    float4 vertex : POSITION;
			    float4 texcoord : TEXCOORD0;
			};

			struct v2f {
			    float4 pos : SV_POSITION;
			    half2 uv : TEXCOORD0;
			};

			// float4x4 _CameraToWorld;
            float _Frames;
            float _Columns;
            float _AnimSpeed;
            
            float2 atan2Approximation(float2 y, float2 x) // http://http.developer.nvidia.com/Cg/atan2.html
			{
				float2 t0, t1, t2, t3, t4;
				t3 = abs(x);
				t1 = abs(y);
				t0 = max(t3, t1);
				t1 = min(t3, t1);
				t3 = float(1) / t0;
				t3 = t1 * t3;
				t4 = t3 * t3;
				t0 =         - float(0.013480470);
				t0 = t0 * t4 + float(0.057477314);
				t0 = t0 * t4 - float(0.121239071);
				t0 = t0 * t4 + float(0.195635925);
				t0 = t0 * t4 - float(0.332994597);
				t0 = t0 * t4 + float(0.999995630);
				t3 = t0 * t3;
				t3 = (abs(y) > abs(x)) ? float(1.570796327) - t3 : t3;
				t3 = (x < 0) ?  float(3.141592654) - t3 : t3;
				t3 = (y < 0) ? -t3 : t3;
				return t3;
			}

			v2f vert (appdata v) 
			{
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                
                // get direction
   				float3 cameraUp = UNITY_MATRIX_IT_MV[1].xyz;
				float3 cameraForward = normalize(UNITY_MATRIX_IT_MV[2].xyz);
				float3 towardsRight = normalize(cross(cameraUp, cameraForward));
                
                // get angle & current frame
   				float angle = (atan2Approximation(towardsRight.z,towardsRight.x)*RAD2DEG) % 360;
				int index = angle/SINGLEFRAMEANGLE;
                
   				// animated frames
				float animFrame= _Columns-(1+round(_Time.y*_AnimSpeed) % _Columns);
				               
                // set uv to display current frame
                o.uv = float2(v.texcoord.x*UVOFFSETX+UVOFFSETX*index,(v.texcoord.y+animFrame)/_Columns);
                          
               // billboard towards camera
  				float3 vpos=mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
 				float4 worldCoord=float4(unity_ObjectToWorld._m03,unity_ObjectToWorld._m13,unity_ObjectToWorld._m23,1);
				float4 viewPos=mul(UNITY_MATRIX_V,worldCoord)+float4(vpos,0);
				float4 outPos=mul(UNITY_MATRIX_P,viewPos);
				
				o.pos = UnityPixelSnap(outPos); // uses pixelsnap
               
                return o;
			}

			fixed4 frag(v2f i) : SV_Target 
			{
				return tex2D(_MainTex,i.uv);
			}
			
	        ENDCG
	    }
	}
	Fallback "Sprites-Diffuse"
}