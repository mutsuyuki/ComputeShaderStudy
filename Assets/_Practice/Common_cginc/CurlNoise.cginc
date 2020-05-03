#ifndef __CurlNoise_cginc__
#define __CurlNoise_cginc__

#include "PerlinNoise.cginc"

// CurlNoiseは外積を使うため、３次元Vector --> 3次元Vectorのみ定義する。
float3 CurlNoise3D(float3 pos, float3 gridSize = 5){
    const float delta = 1e-4; 
    const float3 dx = float3(delta, 0.0, 0.0);
    const float3 dy = float3(0.0, delta, 0.0);
    const float3 dz = float3(0.0, 0.0, delta);

    // 各軸で偏微分（数値微分）
    float3 x0 = PerlinNoise3D(pos - dx, gridSize);
    float3 x1 = PerlinNoise3D(pos + dx, gridSize);
    float3 y0 = PerlinNoise3D(pos - dy, gridSize);
    float3 y1 = PerlinNoise3D(pos + dy, gridSize);
    float3 z0 = PerlinNoise3D(pos - dz, gridSize);
    float3 z1 = PerlinNoise3D(pos + dz, gridSize);
    
    // Corss積をとる
    float x = (y1.z - y0.z) - (z1.y - z0.y);
    float y = (z1.x - z0.x) - (x1.z - x0.z);
    float z = (x1.y - x0.y) - (y1.x - y0.x);

    return float3(x, y, z) / (delta * 2);
}

#endif