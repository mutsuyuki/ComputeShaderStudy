#ifndef __PerlinNoise_cginc__
#define __PerlinNoise_cginc__

#include "Random.cginc"

float PerlinNoise(float2 pos, float2 gridSize = 5){
    float2 scaledPos = pos * gridSize;
    float2 gridIndex = floor(scaledPos);
    float2 gapFromGrid = frac(scaledPos);
    
    // 左:0 右:1 / 下:0 上:1  格子上にランダムなベクトルを生成
    float2 randomVector00 = RandomRange2D(gridIndex + float2(0,0), -1, 1);
    float2 randomVector10 = RandomRange2D(gridIndex + float2(1,0), -1, 1);
    float2 randomVector01 = RandomRange2D(gridIndex + float2(0,1), -1, 1);
    float2 randomVector11 = RandomRange2D(gridIndex + float2(1,1), -1, 1);
    
    // 現在位置から格子上の点に向かうベクトル
    float2 toGridVector00 = gridIndex + float2(0,0) - scaledPos;
    float2 toGridVector10 = gridIndex + float2(1,0) - scaledPos;
    float2 toGridVector01 = gridIndex + float2(0,1) - scaledPos;
    float2 toGridVector11 = gridIndex + float2(1,1) - scaledPos;
    
    // 格子状のランダムなベクトルと、格子上の点に向かうベクトルとで内積をとる
    float dot00 = dot(randomVector00, toGridVector00);
    float dot10 = dot(randomVector10, toGridVector10);
    float dot01 = dot(randomVector01, toGridVector01);
    float dot11 = dot(randomVector11, toGridVector11);
     
    // 各点の内積を滑らかにする
    float2 smooth = gapFromGrid * gapFromGrid * (3.0 - 2.0 * gapFromGrid);
    float point0010 = lerp(dot00, dot10, smooth.x);
    float point0111 = lerp(dot01, dot11, smooth.x);
    float result = lerp(point0010, point0111, smooth.y);
    
    return clamp(result + 0.5, 0.0, 1.0);
}

float PerlinNoise(float3 pos, float3 gridSize = 5){
    float3 scaledPos = pos * gridSize;
    float3 gridIndex = floor(scaledPos);
    float3 gapFromGrid = frac(scaledPos);
    
    // 左:0 右:1 / 下:0 上:1 / 手前:0 奥:1
    float3 randomVector000 = RandomRange3D(gridIndex + float3(0,0,0), -1, 1);
    float3 randomVector100 = RandomRange3D(gridIndex + float3(1,0,0), -1, 1);
    float3 randomVector010 = RandomRange3D(gridIndex + float3(0,1,0), -1, 1);
    float3 randomVector110 = RandomRange3D(gridIndex + float3(1,1,0), -1, 1);
    float3 randomVector001 = RandomRange3D(gridIndex + float3(0,0,1), -1, 1);
    float3 randomVector101 = RandomRange3D(gridIndex + float3(1,0,1), -1, 1);
    float3 randomVector011 = RandomRange3D(gridIndex + float3(0,1,1), -1, 1);
    float3 randomVector111 = RandomRange3D(gridIndex + float3(1,1,1), -1, 1);
    
    // 現在位置から格子上の点に向かうベクトル
    float3 toGridVector000 = gridIndex + float3(0,0,0) - scaledPos;
    float3 toGridVector100 = gridIndex + float3(1,0,0) - scaledPos;
    float3 toGridVector010 = gridIndex + float3(0,1,0) - scaledPos;
    float3 toGridVector110 = gridIndex + float3(1,1,0) - scaledPos;
    float3 toGridVector001 = gridIndex + float3(0,0,1) - scaledPos;
    float3 toGridVector101 = gridIndex + float3(1,0,1) - scaledPos;
    float3 toGridVector011 = gridIndex + float3(0,1,1) - scaledPos;
    float3 toGridVector111 = gridIndex + float3(1,1,1) - scaledPos;
   
    // 格子状のランダムなベクトルと、格子上の点に向かうベクトルとで内積をとる
    float dot000 = dot(randomVector000, toGridVector000);
    float dot100 = dot(randomVector100, toGridVector100);
    float dot010 = dot(randomVector010, toGridVector010);
    float dot110 = dot(randomVector110, toGridVector110);
    float dot001 = dot(randomVector001, toGridVector001);
    float dot101 = dot(randomVector101, toGridVector101);
    float dot011 = dot(randomVector011, toGridVector011);
    float dot111 = dot(randomVector111, toGridVector111);
    
    // grid交点からのずれを滑らかにする
    float3 smooth = gapFromGrid * gapFromGrid * (3.0 - 2.0 * gapFromGrid);
    
    // 面ごとにスムーズする。
    float point000100 = lerp(dot000, dot100, smooth.x);
    float point010110 = lerp(dot010, dot110, smooth.x);
    float pointFront = lerp(point000100, point010110, smooth.y);
    
    float point001101 = lerp(dot001, dot101, smooth.x);
    float point011111 = lerp(dot011, dot111, smooth.x);
    float pointBack = lerp(point001101, point011111, smooth.y);
    
    float result = lerp(pointFront, pointBack, smooth.z);
    
    return clamp(result + 0.5, 0.0, 1.0);
}

float2 PerlinNoise2D(float2 pos, float2 gridSize){
    return float2(
       PerlinNoise(float2(pos.x, pos.y) + 12.345, gridSize),
       PerlinNoise(float2(pos.y, pos.x) + 87.654, gridSize)
    );
}

float3 PerlinNoise3D(float3 pos, float3 gridSize){
    return float3(
       PerlinNoise(float3(pos.x, pos.y, pos.z) + 12.345, gridSize),
       PerlinNoise(float3(pos.z, pos.x, pos.y) + 87.654, gridSize),
       PerlinNoise(float3(pos.y, pos.z, pos.x) + 45.678, gridSize)
    );
}

#endif