sampler baseTexture : register(s0);

bool invertedGravity;
float globalTime;
float cloudDensity;
float horizontalOffset;
float2 parallax;
float2 screenSize;
float2 worldPosition;
float3 sunPosition;

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
};

// Density corresponds to how many particles one can expect at a given point.
// Points further up into the atmosphere become less dense, as more and more particles float away and cease to be present.
// Once past the hard limit of the atmosphere radius, the density is considered to henceforth be 0.
float CalculateAtmosphereDensityAtPoint(float3 p)
{
    // Store the XY world position of this point for calculations later on.
    float2 localWorldPosition = p.xy + worldPosition;
    
    // Apply parallax.
    p.xy += worldPosition * parallax;
    
    // Convert input position from screen space coordinates to UV coordinates.
    p /= float3(screenSize.xy, 1);
    
    // Move horizontally based on wind.
    p.x += horizontalOffset;
    
    // Squish the noise, so that it looks more like a condensed cloud.
    p.xy *= float2(0.5, 1);
    
    // Calculate a UV offset for the given point.
    // This will be used to accent the clouds and make it look less like it's just a scrolling noise texture.
    float2 uvOffset = tex2D(baseTexture, p.xy * 3 + globalTime * 0.02) * 0.015;
    
    // Acquire density data from the three color channels of the noise. This uses the UV offset from above.
    float3 densityData = tex2D(baseTexture, p.xy * 0.34 + uvOffset);
    
    // Sample two density values from the color channels. These will be interpolated between based on the Z position, to create the illusion of
    // 3D noise without having to dedicate a ton of memory to a cube texture of it.
    float densityA = densityData.r;
    float densityB = densityData.b;
    float density = lerp(densityA, densityB, sin(p.z * 6.283) * 0.5 + 0.5);
    
    // Add secondary cloud density values in accordance with the general cloud density shader parameter.
    density += densityData.g * cloudDensity - p.y * (0.1 - cloudDensity) * 0.5;
    
    // Make the density taper off near the top of the world.
    density *= smoothstep(3200, 4000, localWorldPosition.y);
    
    // Combine things together.
    return density * cloudDensity * 0.7;
}

// Optical depth in this context basically is a measure of how much air is present along a given ray.
float CalculateOpticalDepth(float3 rayOrigin, float3 rayDirection, float rayLength, float numOpticalDepthPoints)
{
    float3 densitySamplePoint = rayOrigin;
    float stepSize = rayLength / (numOpticalDepthPoints - 1);
    float opticalDepth = 0;

    for (int i = 0; i < numOpticalDepthPoints; i++)
    {
        float localDensity = CalculateAtmosphereDensityAtPoint(densitySamplePoint);
        opticalDepth += localDensity * stepSize;
        densitySamplePoint += rayDirection * stepSize;
    }
    return opticalDepth;
}

// Credit to Sebastian Lague's atmospheric rendering shader for much of this (as well as his video on the subject for explaining the concepts excellently).
float4 CalculateScatteredLight(float3 rayOrigin, float3 rayDirection)
{
    float3 boxMin = float3(-999999, -999999, 0);
    float3 boxMax = float3(999999, 999999, 2);
    
    float inScatterPoints = 4;
    float sunIntersectionPoints = 4;
    float2 intersectionDistances = GetRayBoxIntersectionOffsets(rayOrigin, rayDirection, boxMin, boxMax);
    
    // Calculate how far the atmosphere intersection must travel.
    // If no intersection happened, simply return 0;
    float atmosphereIntersectionLength = intersectionDistances.y - intersectionDistances.x;
    if (atmosphereIntersectionLength <= 0)
        return 0;
    
    // Calculate how much each step along the in-scatter ray must travel.
    float inScatterStep = atmosphereIntersectionLength / (inScatterPoints - 1);
    
    // Initialize the light accumulation value at 0.
    float4 light = 0;
    
    // Start the in-scatter sample position at the edge of the sphere.
    // This process attempts to discretely model the integral used along the ray in real-world atmospheric scattering calculations.
    float3 boxStart = rayOrigin + intersectionDistances.x * rayDirection;
    float3 inScatterSamplePosition = boxStart;
    for (int i = 0; i < inScatterPoints; i++)
    {
        // Calculate the direction from the in-scatter point to the sun.
        float3 directionToSun = normalize(sunPosition - inScatterSamplePosition);
        
        // Perform a ray intersection from the sample position towards the sun.
        // This does not need a safety "is there any intersection at all?" check since by definition the sample position is already in the sphere, since it's an intersection
        // of a line in said sphere.
        float2 sunRayLengthDistances = GetRayBoxIntersectionOffsets(inScatterSamplePosition, directionToSun, boxMin, boxMax);
        float sunIntersectionRayLength = sunRayLengthDistances.y - sunRayLengthDistances.x;
        
        // Calculate the optical depth along the ray from the sample point to the sun.
        float sunIntersectionOpticalDepth = CalculateOpticalDepth(inScatterSamplePosition, directionToSun, sunIntersectionRayLength, sunIntersectionPoints);
        
        // Combine the two optical depths via exponential decay.
        float3 localScatteredLight = exp(-sunIntersectionOpticalDepth);
        
        // Combine the local scattered light, along with the density of the current position.
        light += CalculateAtmosphereDensityAtPoint(inScatterSamplePosition) * float4(localScatteredLight, 1);
        
        // Move onto the next movement iteration by stepping forward on the in-scatter position.
        inScatterSamplePosition += rayDirection * inScatterStep;
    }

    // Perform Mie scattering on the result.
    float g = 0.85;
    float gSquared = g * g;
    float cosTheta = dot(rayDirection, normalize(sunPosition - boxStart));
    float cosThetaSquared = cosTheta * cosTheta;
    float phaseMie = ((1 - gSquared) * (cosThetaSquared + 1)) / (pow(1 + gSquared - cosTheta * g * 2, 1.5) * (gSquared + 2)) * 0.1193662; // This constant is equal to 3/(8pi)
    return light * inScatterStep * phaseMie * 300;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Account for the pesky gravity potions...
    if (invertedGravity)
        position.y = screenSize.y - position.y;
    
    // Calculate how much scattered light will end up in the current fragment.
    float4 atmosphereLight = CalculateScatteredLight(float3(position.xy, -1), float3(0, 0, 1));
    atmosphereLight.rgb = 1 - exp(atmosphereLight.rgb * -1.3);
    
    // Combine the scattered light with the sample color, allowing for dynamic colorations and opacities to the final result.
    return atmosphereLight * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}