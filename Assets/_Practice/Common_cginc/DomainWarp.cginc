#ifndef __DomainWarp_cginc__
#define __DomainWarp_cginc__

#include "FBMNoise.cginc"

float DomainWarp(float2 pos, float time, float2 gridSize = 5, int octaves = 3, float roughness = 0.5){
    float noise1 = FBMNoise(pos + float2(1.7, 4.2), gridSize, octaves, roughness);
    float noise2 = FBMNoise(pos + noise1 + time * 0.3, gridSize, octaves, roughness);
    float noise3 = FBMNoise(pos + noise2, gridSize, octaves, roughness);
    return noise3;
}

float DomainWarp(float3 pos, float time, float3 gridSize = 5, int octaves = 3, float roughness = 0.5){
    float noise1 = FBMNoise(pos + float3(1.7, 4.2, 3.5), gridSize, octaves, roughness);
    float noise2 = FBMNoise(pos + noise1 + time * 0.3, gridSize, octaves, roughness);
    float noise3 = FBMNoise(pos + noise2, gridSize, octaves, roughness);
    return noise3;
}

float2 DomainWarp2D(float2 pos, float time, float2 gridSize = 5, int octaves = 3, float roughness = 0.5){
    float2 noise1 = FBMNoise2D(pos + float2(1.7, 4.2), gridSize, octaves, roughness);
    float2 noise2 = FBMNoise2D(pos + noise1 + time * 0.3, gridSize, octaves, roughness);
    float2 noise3 = FBMNoise2D(pos + noise2, gridSize, octaves, roughness);
    return noise3;
}

float3 DomainWarp3D(float3 pos, float time, float3 gridSize = 5, int octaves = 3, float roughness = 0.5){
    float3 noise1 = FBMNoise3D(pos + float3(1.7, 4.2, 3.5), gridSize, octaves, roughness);
    float3 noise2 = FBMNoise3D(pos + noise1 + time * 0.3, gridSize, octaves, roughness);
    float3 noise3 = FBMNoise3D(pos + noise2, gridSize, octaves, roughness);
    return noise3;
}

#endif
