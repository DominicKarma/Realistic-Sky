sampler baseTexture : register(s0);
sampler planetTexture : register(s1);

bool invertedGravity;
float globalTime;
float atmosphereRadius;
float planetRadius;
float screenHeight;
float3 sunPosition;
float3 planetPosition;
float3 rgbLightWavelengths;

float2 GetRaySphereIntersectionOffsets(float3 rayOrigin, float3 rayDirection, float3 spherePosition, float sphereRadius)
{
    float3 offsetFromSphere = rayOrigin - spherePosition;
    float a = dot(rayDirection, rayDirection);
    float b = dot(rayDirection, offsetFromSphere) * 2;
    float c = dot(offsetFromSphere, offsetFromSphere) - (sphereRadius * sphereRadius);
    float discriminant = b * b - 4.0 * a * c;
    if (discriminant < 0.0)
    {
        return -1;
    }
    else
    {
        return float2(-b - sqrt(discriminant), -b + sqrt(discriminant)) / (a * 2);
    }
}

// Density corresponds to how many particles one can expect at a given point.
// Points further up into the atmosphere become less dense, as more and more particles float away and cease to be present.
// Once past the hard limit of the atmosphere radius, the density is considered to henceforth be 0.
float CalculateAtmosphereDensityAtPoint(float3 p)
{
    float distanceFromPlanetCenter = distance(p, planetPosition);
    float distanceFromPlanetSurface = distanceFromPlanetCenter - planetRadius * 0.7;
    float amountOfSpaceTakenByAtmosphere = atmosphereRadius - planetRadius * 0.7;
    float atmosphereInterpolant = smoothstep(0, amountOfSpaceTakenByAtmosphere, distanceFromPlanetSurface);
    
    return exp(-atmosphereInterpolant * 1.5) * (1 - atmosphereInterpolant);
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
    float inScatterPoints = 10;
    float sunIntersectionPoints = 4;
    float2 intersectionDistances = GetRaySphereIntersectionOffsets(rayOrigin, rayDirection, planetPosition, atmosphereRadius);
    float3 scatteringCoefficients = pow(400 / rgbLightWavelengths, 4);
    
    // Calculate how far the atmosphere intersection must travel.
    // If no intersection happened, simply return 0;
    float atmosphereIntersectionLength = intersectionDistances.y - intersectionDistances.x;
    if (atmosphereIntersectionLength <= 0)
        return 0;
    
    // Calculate how much each step along the in-scatter ray must travel.
    float inScatterStep = atmosphereIntersectionLength / (inScatterPoints - 1);
    
    // Initialize the light accumulation value at 0.
    float3 light = 0;
    
    // Start the in-scatter sample position at the edge of the sphere.
    // This process attempts to discretely model the integral used along the ray in real-world atmospheric scattering calculations.
    float3 inScatterSamplePosition = rayOrigin + intersectionDistances.x * rayDirection;
    for (int i = 0; i < inScatterPoints; i++)
    {
        // Calculate the direction from the in-scatter point to the sun.
        float3 directionToSun = normalize(sunPosition - inScatterSamplePosition);
        
        // Perform a ray intersection from the sample position towards the sun.
        // This does not need a safety "is there any intersection at all?" check since by definition the sample position is already in the sphere, since it's an intersection
        // of a line in said sphere.
        float2 sunRayLengthDistances = GetRaySphereIntersectionOffsets(inScatterSamplePosition, directionToSun, planetPosition, atmosphereRadius);
        float sunIntersectionRayLength = sunRayLengthDistances.y - sunRayLengthDistances.x;
        
        // Calculate the optical depth along the ray from the sample point to the sun.
        float sunIntersectionOpticalDepth = CalculateOpticalDepth(inScatterSamplePosition, directionToSun, sunIntersectionRayLength, sunIntersectionPoints);
        
        // Calculate the optical depth along the ray from the starting position up to the sample point.
        float inScatterOpticalDepth = CalculateOpticalDepth(rayOrigin, rayDirection, inScatterStep * i, sunIntersectionPoints);
        
        // Combine the two optical depths via exponential decay and incorporate scattering coefficients.
        float3 localScatteredLight = exp(-(sunIntersectionOpticalDepth + inScatterOpticalDepth) * scatteringCoefficients);
        
        // Combine the local scattered light, along with the density of the current position.
        light += CalculateAtmosphereDensityAtPoint(inScatterSamplePosition) * localScatteredLight;
        
        // Move onto the next movement iteration by stepping forward on the in-scatter position.
        inScatterSamplePosition += rayDirection * inScatterStep;
    }
    
    // Calculate the rayleigh scattering phase value for the given ray between it and the sun.
    float cosTheta = dot(normalize(sunPosition - rayOrigin), rayDirection);
    float rayleighPhase = (cosTheta * cosTheta + 1.0) * 0.0596831; // This constant is 3/(16pi).
    
    // Combine the light with the scattering coefficients.
    return float4(light * scatteringCoefficients * rayleighPhase * inScatterStep * 15.1, 0);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    if (invertedGravity)
        position.y = screenHeight - position.y;
    
    //return exp(distance(position.xy, sunPosition.xy) * -0.03);
    float distanceFromPlanet = distance(position.xy, planetPosition.xy);
    float4 atmosphereLight = CalculateScatteredLight(float3(position.xy, -atmosphereRadius - 5), float3(0, 0, 1));
    atmosphereLight.rgb = 1 - exp(-atmosphereLight.rgb);
    
    return atmosphereLight * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}