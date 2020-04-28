#ifndef __ValueNoise_cginc__
#define __ValueNoise_cginc__

#include "Random.cginc"

float ValueNoise(float2 pos, float2 gridSize = 5){

    float2 scaledPos = pos * gridSize;
    float2 gridIndex = floor(scaledPos);
    float2 gapFromGrid = frac(scaledPos);
    
    // 左:0 右:1 / 下:0 上:1
    float point00 = Random(gridIndex);
    float point10 = Random(gridIndex + float2(1,0));
    float point01 = Random(gridIndex + float2(0,1));
    float point11 = Random(gridIndex + float2(1,1));
    
    // grid交点からのずれを滑らかにする
    float2 smooth = gapFromGrid * gapFromGrid * (3.0 - 2.0 * gapFromGrid);
    
    // 下２点、上２点を横方向にスムーズかけて、それを縦方向にスムーズする。
    float point0010 = lerp(point00, point10, smooth.x);
    float point0111 = lerp(point01, point11, smooth.x);
    float result = lerp(point0010, point0111, smooth.y);
    
    return result;
}

float ValueNoise(float3 pos, float3 gridSize = 5){

    float3 scaledPos = pos * gridSize;
    float3 gridIndex = floor(scaledPos);
    float3 gapFromGrid = frac(scaledPos);
    
    // 左:0 右:1 / 下:0 上:1 / 手前:0 奥:1
    float point000 = Random(gridIndex);
    float point100 = Random(gridIndex + float3(1,0,0));
    float point010 = Random(gridIndex + float3(0,1,0));
    float point110 = Random(gridIndex + float3(1,1,0));
    float point001 = Random(gridIndex + float3(0,0,1));
    float point101 = Random(gridIndex + float3(1,0,1));
    float point011 = Random(gridIndex + float3(0,1,1));
    float point111 = Random(gridIndex + float3(1,1,1));
   
    // grid交点からのずれを滑らかにする
    float3 smooth = gapFromGrid * gapFromGrid * (3.0 - 2.0 * gapFromGrid);
    
    // 面ごとにスムーズする。
    float point000100 = lerp(point000, point100, smooth.x);
    float point010110 = lerp(point010, point110, smooth.x);
    float pointFront = lerp(point000100, point010110, smooth.y);
    
    float point001101 = lerp(point001, point101, smooth.x);
    float point011111 = lerp(point011, point111, smooth.x);
    float pointBack = lerp(point001101, point011111, smooth.y);
    
    float result = lerp(pointFront, pointBack, smooth.z);
    
    return result;
}

#endif