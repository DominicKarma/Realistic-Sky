sampler baseTexture : register(s0);

bool invertedGravity;
float cloudHorizontalOffset;
float globalTime;
float densityFactor;
float2 screenSize;
float2 worldPosition;
float3 sunPosition;
float4 sunColor;
float4 cloudColor;

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

// Density corresponds to how many particles one can expect at a given point.
float CalculateAtmosphereDensity(float3 p)
{
    float2 samplePosition = p.xy;
    samplePosition.x *= 0.5;
    samplePosition.x += cloudHorizontalOffset / p.z * 3;
    
    float swapInterpolant = cos(p.z * 10 + globalTime * 0.2) * 0.5 + 0.5;
    
    float4 densityData = saturate(tex2D(baseTexture, samplePosition / screenSize * 1.5) - tex2D(baseTexture, samplePosition / screenSize * 3) * 0.5) + 0.02;
    float densityA = densityData.r;
    float densityB = densityData.g;
    return lerp(densityA, densityB, swapInterpolant) * densityFactor;
}

float4 CalculateScatteredLight(float3 rayOrigin, float3 rayDirection)
{
    float inScatterPoints = 3;
    float sunIntersectionPoints = 4;
    float3 boxMin = float3(-99999, -99999, -1);
    float3 boxMax = float3(99999, 99999, 4);
    float2 intersectionDistances = GetRayBoxIntersectionOffsets(rayOrigin, rayDirection, boxMin, boxMax);
    
    // Calculate how far the atmosphere intersection must travel.
    // If no intersection happened, simply return 0.
    float atmosphereIntersectionLength = intersectionDistances.y - intersectionDistances.x;
    if (atmosphereIntersectionLength <= 0)
        return 0;
    
    // Calculate how much each step along the in-scatter ray must travel.
    float inScatterStep = atmosphereIntersectionLength / (inScatterPoints - 1);
    
    // Initialize the light accumulation value at 0.
    float4 light = 0;
    
    float3 start = rayOrigin + intersectionDistances.x * rayDirection;
    float3 inScatterSamplePosition = start;
    for (int i = 0; i < inScatterPoints; i++)
    {
        float inScatterDistance = length((inScatterSamplePosition - start) / float3(screenSize, 1));
        float inScatterDensity = CalculateAtmosphereDensity(inScatterSamplePosition);
        float inTransmittance = exp(-inScatterDistance * inScatterDensity);
        
        float3 directionToSun = normalize(sunPosition - inScatterSamplePosition);
        float2 sunRayLengthDistances = GetRayBoxIntersectionOffsets(inScatterSamplePosition, directionToSun, boxMin, boxMax);
        float sunIntersectionRayLength = sunRayLengthDistances.y - sunRayLengthDistances.x;
        
        float outTransmittance = 1;
        float3 outScatterSamplePosition = inScatterSamplePosition;
        for (int j = 0; j < sunIntersectionPoints; j++)
        {
            float outScatterDistance = length((outScatterSamplePosition - inScatterSamplePosition) / float3(screenSize, 1));
            float outScatterDensity = CalculateAtmosphereDensity(outScatterSamplePosition);
            outTransmittance *= exp(-outScatterDistance * outScatterDensity);
            
            outScatterSamplePosition += directionToSun * sunIntersectionRayLength / (sunIntersectionPoints - 1);
        }
        
        light += inTransmittance * cloudColor * outTransmittance * sunColor;
        
        // Move onto the next movement iteration by stepping forward on the in-scatter position.
        inScatterSamplePosition += rayDirection * inScatterStep;
    }
    
    // Combine the light with the scattering coefficients.
    return 1 - exp(light * -0.9);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
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