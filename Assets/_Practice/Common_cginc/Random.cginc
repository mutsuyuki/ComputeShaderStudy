#ifndef __Random_cginc__
#define __Random_cginc__

// 2次元Vector --> スカラー
float Random(float2 pos){
    return frac(sin(dot(pos, float2(12.9898,78.233))) * 43758.5453);
}

// 3次元Vector --> スカラー
float Random(float3 pos){
    return frac(sin(dot(pos, float3(12.9898, 78.233, 56.787))) * 43758.5453);
}

// 2次元Vector --> 2次元Vector
float2 Random2D(float2 pos){
    return float2(
       Random(pos + Random(pos * 12.345)),
       Random(pos + Random(pos * 98.765))
    );
}

// 3次元Vector --> 3次元Vector
float3 Random3D(float3 pos){
    return float3(
       Random(pos + Random(pos * 12.345)),
       Random(pos + Random(pos * 98.765)),
       Random(pos + Random(pos * 45.678))
    );
}


// 2次元Vector --> スカラー
float RandomRange(float2 pos, float min, float max){
    return (max - min) * Random(pos) + min;
}

// 3次元Vector --> スカラー
float RandomRange(float3 pos, float min, float max){
    return (max - min) * Random(pos) + min;
}

// 2次元Vector --> 2次元Vector
float2 RandomRange2D(float2 pos, float min, float max){
    return (max - min) * Random2D(pos) + min;
}

// 3次元Vector --> 3次元Vector
float3 RandomRange3D(float3 pos, float min, float max){
    return (max - min) * Random3D(pos) + min;
}

#endif 
