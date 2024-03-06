sampler starTexture : register(s1);

float opacity;
float glowIntensity;
float globalTime;
float2 sunPosition;
matrix projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, projection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float2 position = input.Position.xy;
    float4 color = input.Color * tex2D(starTexture, coords);
    
    // Add an inner glow to the texture.
    float2 centerOffset = coords - 0.5;
    float distanceSqrFromCenter = dot(centerOffset, centerOffset);
    float twinkle = lerp(0.2, 1.87, cos(globalTime + color.b * 125 + color.r * 120) * 0.5 + 0.5);
    float glow = (color.r + color.b) * twinkle / (distanceSqrFromCenter * 4 + 2);
    
    float distanceSqrFromSun = dot(position - sunPosition, position - sunPosition);
    float localOpacity = opacity - smoothstep(67600, 26500, distanceSqrFromSun);
    
    return (color + glow) * localOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
