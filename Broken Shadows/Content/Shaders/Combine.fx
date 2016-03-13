texture colorMap;
texture lightMap;

sampler colorSampler = sampler_state
{
  Texture = (colorMap);
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  Mipfilter = LINEAR;
};
sampler lightSampler = sampler_state
{
  Texture = (lightMap);
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  Mipfilter = LINEAR;
};

struct VertexShaderInput
{
  float3 Position : SV_POSITION;
  float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
  float4 Position : SV_POSITION;
  float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
  VertexShaderOutput output;
  output.Position = float4(input.Position, 1);
  output.TexCoord = input.TexCoord;
  return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
  float3 diffuseColor = tex2D(colorSampler, input.TexCoord).rgb;
  float3 light = tex2D(lightSampler, input.TexCoord).rgb;   
  return float4((diffuseColor * light), 1);  
}

technique Technique1
{
  pass Pass1
  {
    VertexShader = compile vs_4_0 VertexShaderFunction();
    PixelShader = compile ps_4_0 PixelShaderFunction();
  }
}
