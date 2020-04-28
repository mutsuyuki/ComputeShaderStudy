#ifndef __BlockNoise_cginc__
#define __BlockNoise_cginc__

#include "Random.cginc"

float BlockNoise(float2 pos, float2 gridSize = 5){
    return  Random(floor(pos * gridSize));
}

float BlockNoise(float3 pos, float3 gridSize = 5){
    return  Random(floor(pos * gridSize));
}

#endif