#ifndef __FBMNoise_cginc__
#define __FBMNoise_cginc__

#include "FBMNoise.cginc"

float FBMNoise(float2 pos, float2 gridSize = 5, int octaves = 3, float roughness = 0.5){
    float2 currentFrequency = gridSize;
    float currentRoughness = roughness;
    float ajustRate = currentRoughness;

    float result = 0.0;
    for (int i = 0; i < octaves; i++){
        result += currentRoughness * PerlinNoise(pos, currentFrequency); 
        currentFrequency *= 2.0f;
        currentRoughness *= roughness;
        ajustRate += currentRoughness;
    }
    ajustRate -= currentRoughness;

    return result / ajustRate;
}

float FBMNoise(float3 pos, float3 gridSize = 5, int octaves = 3, float roughness = 0.5){
    float3 currentFrequency = gridSize;
    float currentRoughness = 1.0;
    float ajustRate = currentRoughness;

    float result = 0.0;
    for (int i = 0; i < octaves; i++){
        result += currentRoughness * PerlinNoise(pos, currentFrequency); 
        currentFrequency *= 2.0f;
        currentRoughness *= roughness;
        ajustRate += currentRoughness;
    }
    ajustRate -= currentRoughness;

    return result / ajustRate;
}

float2 FBMNoise2D(float2 pos, float2 gridSize = 5, int octaves = 3, float roughness = 0.5){
    float2 currentFrequency = gridSize;
    float currentRoughness = 1.0;
    float ajustRate = currentRoughness;

    float2 result = float2(0.0, 0.0);
    for (int i = 0; i < octaves; i++){
        result += currentRoughness * PerlinNoise2D(pos, currentFrequency); 
        currentFrequency *= 2.0f;
        currentRoughness *= roughness;
        ajustRate += currentRoughness;
    }
    ajustRate -= currentRoughness;

    return result / ajustRate;
}

float3 FBMNoise3D(float3 pos, float3 gridSize = 5, int octaves = 3, float roughness = 0.5){
    float3 currentFrequency = gridSize;
    float currentRoughness = 1.0;
    float ajustRate = currentRoughness;

    float3 result = float3(0.0, 0.0, 0.0);
    for (int i = 0; i < octaves; i++){
        result += currentRoughness * PerlinNoise3D(pos, currentFrequency); 
        currentFrequency *= 2.0f;
        currentRoughness *= roughness;
        ajustRate += currentRoughness;
    }
    ajustRate -= currentRoughness;

    return result / ajustRate;
}

#endif