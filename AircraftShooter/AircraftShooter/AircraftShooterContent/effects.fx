//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xLightDirection;
float xAmbient;
bool xEnableLighting;
bool xShowNormals;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSize;

float xTime;
float xOvercast;

//------- Texture Samplers --------

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

//Pretransformed

struct VertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

VertexToPixel PretransformedVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;
	Output.Color = inColor;
    
	return Output;    
}

PixelToFrame PretransformedPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = PSIn.Color;

	return Output;
}

technique Pretransformed
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 PretransformedVS();
		PixelShader  = compile ps_2_0 PretransformedPS();
	}
}

//Colored

VertexToPixel ColoredVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
	
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));	
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

PixelToFrame ColoredPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;
	Output.Color.rgb *= saturate(PSIn.LightingFactor) + xAmbient;

	return Output;
}

technique Colored
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 ColoredVS();
		PixelShader  = compile ps_2_0 ColoredPS();
	}
}

//ColoredNoShading

VertexToPixel ColoredNoShadingVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
    
	return Output;    
}

PixelToFrame ColoredNoShadingPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;

	return Output;
}

technique ColoredNoShading
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 ColoredNoShadingVS();
		PixelShader  = compile ps_2_0 ColoredNoShadingPS();
	}
}


//Technique: Textured

VertexToPixel TexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);	
	Output.TextureCoords = inTexCoords;
	
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));	
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

PixelToFrame TexturedPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
	Output.Color.rgb *= saturate(PSIn.LightingFactor) + xAmbient;

	return Output;
}

technique Textured
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 TexturedVS();
		PixelShader  = compile ps_2_0 TexturedPS();
	}
}

//TexturedNoShading

VertexToPixel TexturedNoShadingVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);	
	Output.TextureCoords = inTexCoords;
    
	return Output;    
}

PixelToFrame TexturedNoShadingPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);

	return Output;
}

technique TexturedNoShading
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 TexturedNoShadingVS();
		PixelShader  = compile ps_2_0 TexturedNoShadingPS();
	}
}

//PointSprites

VertexToPixel PointSpriteVS(float3 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
    VertexToPixel Output = (VertexToPixel)0;

    float3 center = mul(inPos, xWorld);
    float3 eyeVector = center - xCamPos;

    float3 sideVector = cross(eyeVector,xCamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector,eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (inTexCoord.x-0.5f)*sideVector*0.5f*xPointSpriteSize;
    finalPosition += (0.5f-inTexCoord.y)*upVector*0.5f*xPointSpriteSize;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul (xView, xProjection);
    Output.Position = mul(finalPosition4, preViewProjection);

    Output.TextureCoords = inTexCoord;

    return Output;
}

PixelToFrame PointSpritePS(VertexToPixel PSIn) : COLOR0
{
    PixelToFrame Output = (PixelToFrame)0;
    Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
    return Output;
}

technique PointSprites
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 PointSpriteVS();
		PixelShader  = compile ps_2_0 PointSpritePS();
	}
}

//perlin
struct PNVertexToPixel
{
	float4 Position	: POSITION;
	float2 TexCoords	: TEXCOORD0;
};

struct PNPixelToFrame
{
	float4 Color	: COLOR0;
};

PNVertexToPixel PerlinNoiseVS(float4 inPos: POSITION, float2 inTexCoords: TEXCOORD0)
{
	PNVertexToPixel Output = (PNVertexToPixel)0;

	Output.Position = inPos;
	Output.TexCoords = inTexCoords;

	return Output;
}

PNPixelToFrame PerlinNoisePS(PNVertexToPixel PSIn)
{
	PNPixelToFrame Output = (PNPixelToFrame)0;

	float2 move = float2(0,2);
	float4 perlin = tex2D(TextureSampler, (PSIn.TexCoords)+xTime*move)/2;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*2+xTime*move)/4;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*4+xTime*move)/8;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*8+xTime*move)/16;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*16+xTime*move)/32;
	//perlin += tex2D(TextureSampler, (PSIn.TexCoords)*32+xTime*move)/32;

	Output.Color.rgb = 1.0f - pow(abs(perlin.r), xOvercast)*2.0f;
	Output.Color.a = 1;

	return Output;
}

technique PerlinNoise
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 PerlinNoiseVS();
		PixelShader = compile ps_2_0 PerlinNoisePS();
	}
}

//skyDome
struct SDVertexToPixel
{
	float4 Position:		POSITION0;
	float2 TexCoords:		TEXCOORD0;
	float4 ObjectPosition:	TEXCOORD1;
};

struct SDPixelToFrame
{
	float4 Color:	COLOR0;
};

SDVertexToPixel SkyDomeVS(float4 inPos: POSITION, float2 inTexCoords: TEXCOORD0)
{
	SDVertexToPixel Output = (SDVertexToPixel)0;

	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	Output.Position = mul(inPos, preWorldViewProjection);
	Output.TexCoords = inTexCoords;
	Output.ObjectPosition = inPos;

	return Output;
}

SDPixelToFrame SkyDomePS(SDVertexToPixel PSIn)
{
	SDPixelToFrame Output = (SDPixelToFrame)0;

	float4 topColor = float4(0.3f, 0.3f, 0.8f, 1);
	float4 bottomColor = 1;

	float4 baseColor = lerp(bottomColor, topColor, saturate((PSIn.ObjectPosition.y)/0.4f));
	float4 cloudValue = tex2D(TextureSampler, PSIn.TexCoords).r;

	Output.Color = lerp(baseColor, 1, cloudValue);

	return Output;
}

technique SkyDome
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 SkyDomeVS();
		PixelShader = compile ps_2_0 SkyDomePS();
	}
}