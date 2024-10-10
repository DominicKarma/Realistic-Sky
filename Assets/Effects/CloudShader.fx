sampler baseTexture : register(s0);
sampler atmosphereTexture : register(s1);

bool invertedGravity;
float globalTime;
float2 screenSize;
float2 worldPosition;
float3 sunPosition;

const float oneOver4Pi = 0.0795774;

float HenyeyGreensteinPhase(float g, float cos_theta)
{
    float numerator = 1 - g * g;
    float denominator = pow(1 + g * g - g * cos_theta * 2, 1.5);
    
    return numerator / denominator * oneOver4Pi;
}

float2 GetRayBoxIntersectionOffsets(float3 rayOrigin, float3 rayDirection, float3 boxMin, float3 boxMax)
{
    // Add a tiny nudge to the ray direction, since the compiler gets upset about the potential for division by zero otherwise.
    rayDirection += 1e-8;
    
    float3 tMin = (boxMin - rayOrigin) / rayDirection;
    float3 tMax = (boxMax - rayOrigin) / rayDirection;
    
    float3 t1 = min(tMin, tMax);
    float3 t2 = max(tMin, tMax);
    
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
    
    return float2(tNear, tFar);
}

float4 CalculateScatteredLight(float3 rayOrigin, float3 rayDirection)
{
    float3 boxMin = float3(-999999, -999999, 0);
    float3 boxMax = float3(999999, 999999, 2);
    
    float inScatterPoints = 4;
    float sunIntersectionPoints = 4;
    float2 intersectionDistances = GetRayBoxIntersectionOffsets(rayOrigin, rayDirection, boxMin, boxMax);
    
    // Calculate how far the cloud intersection must travel.
    // If no intersection happened, simply return 0.
    float cloudIntersectionLength = intersectionDistances.y - intersectionDistances.x;
    if (cloudIntersectionLength <= 0)
        return 0;
    
    // Calculate how much each step along the in-scatter ray must travel.
    float inScatterStep = cloudIntersectionLength / (inScatterPoints - 1);
    
    // Initialize the light accumulation value at 0.
    float4 light = 0;
    
    return light;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    position.xy = round(position.xy / 0.25) * 0.25;
    
    // Account for the pesky gravity potions...
    if (invertedGravity)
        position.y = screenSize.y - position.y;
    
    // Calculate how much scattered light will end up in the current fragment.
    float4 cloudLight = CalculateScatteredLight(float3(position.xy, -1), float3(0, 0, 1));
    
    // Combine the scattered light with the sample color, allowing for dynamic colorations and opacities to the final result.
    return cloudLight * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}