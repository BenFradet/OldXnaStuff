//constants
float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;

float3 xLightDirection;
float xAmbient;
bool xEnableLighting;

bool Clipping;
float4 ClipPlane0;

float4x4 xReflectionView;

float xWaveLength;
float xWaveHeight;

float3 xCamPos;

float3 xWindDirection;
float xWindForce;
float xTime;

float xOvercast;

float3 xAllowedRotDir;

//texture samplers
Texture xTexture;
sampler TextureSampler = sampler_state{texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xTexture0;
sampler TextureSampler0 = sampler_state{texture = <xTexture0>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap;};
Texture xTexture1;
sampler TextureSampler1 = sampler_state{texture = <xTexture1>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap;};
Texture xTexture2;
sampler TextureSampler2 = sampler_state{texture = <xTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xTexture3;
sampler TextureSampler3 = sampler_state{texture = <xTexture3>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xReflectionMap;
sampler ReflectionSampler = sampler_state{texture = <xReflectionMap>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror;};
Texture xRefractionMap;
sampler RefractionSampler = sampler_state{texture = <xRefractionMap>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xWaterBumpMap;
sampler WaterBumpMapSampler = sampler_state{texture = <xWaterBumpMap>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror;};

Texture xBillboardTexture;
sampler textureSampler = sampler_state{texture = <xBillboardTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = CLAMP; AddressV = CLAMP;};

Texture xSkyDomeTexture;
sampler SkyDomeTextureSampler = sampler_state{texture = <xSkyDomeTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = CLAMP; AddressV = CLAMP;};

//colored
struct ColVertexToPixel
{
	float4 Position			: POSITION;
	float4 Color			: COLOR0;
	float LightingFactor	: TEXCOORD0;
};

struct ColPixelToFrame
{
	float4 Color: COLOR0;
};

ColVertexToPixel ColoredVS(float4 inPos: POSITION, float4 inColor: COLOR0, float3 inNormal: NORMAL)
{
	ColVertexToPixel Output = (ColVertexToPixel)0;

	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;

	float3 Normal = normalize(mul(normalize(inNormal), xWorld));

	Output.LightingFactor = 1;

	if(xEnableLighting)
		Output.LightingFactor = saturate(dot(Normal, -xLightDirection));

	return Output;
}

ColPixelToFrame ColoredPS(ColVertexToPixel PSIn)
{
	ColPixelToFrame Output = (ColPixelToFrame)0;

	Output.Color = PSIn.Color;
	Output.Color.rgb *= saturate(PSIn.LightingFactor + xAmbient);

	return Output;
}

technique Colored
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 ColoredVS();
		PixelShader = compile ps_2_0 ColoredPS();
	}
}

//textured
struct TexVertexToPixel
{
	float4 Position			: POSITION;
	float4 Color			: COLOR0;
	float LightingFactor	: TEXCOORD0;
	float2 TextureCoords	: TEXCOORD1;
};

struct TexPixelToFrame
{
	float4 Color			: COLOR0;
};

TexVertexToPixel TexturedVS(float4 inPos: POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{
	TexVertexToPixel Output = (TexVertexToPixel)0;

	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	Output.Position = mul(inPos, preWorldViewProjection);
	Output.TextureCoords = inTexCoords;

	float3 Normal = normalize(mul(normalize(inNormal), xWorld));
	
	Output.LightingFactor = 1;

	if(xEnableLighting)
		Output.LightingFactor = saturate(dot(Normal, -xLightDirection));

	return Output;
}

TexPixelToFrame TexturedPS(TexVertexToPixel PSIn)
{
	TexPixelToFrame Output = (TexPixelToFrame)0;

	Output.Color = tex2D(TextureSampler0, PSIn.TextureCoords);
	Output.Color.rgb *= saturate(PSIn.LightingFactor + xAmbient);

	return Output;
}

technique Textured
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 TexturedVS();
		PixelShader = compile ps_2_0 TexturedPS();
	}
}

//multitextured
struct MTVertexToPixel
{
	float4 Position			: POSITION;
	float4 Color			: COLOR0;
	float3 Normal			: TEXCOORD0;
	float2 TextureCoords	: TEXCOORD1;
	float4 LightDirection	: TEXCOORD2;
	float4 TextureWeights	: TEXCOORD3;
	float Depth				: TEXCOORD4;
	float4 ClipDistances	: TEXCOORD5;
};

struct MTPixelToFrame
{
	float4 Color			: COLOR0;
};

MTVertexToPixel MultiTexturedVS(float4 inPos: POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float4 inTexWeights: TEXCOORD1)
{
	MTVertexToPixel Output = (MTVertexToPixel)0;

	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Normal = mul(normalize(inNormal), xWorld);
	Output.TextureCoords = inTexCoords;
	Output.LightDirection.xyz = -xLightDirection;
	Output.LightDirection.w = 1;
	Output.TextureWeights = inTexWeights;
	Output.Depth = Output.Position.z/Output.Position.w;
	Output.ClipDistances = dot(inPos, ClipPlane0);

	return Output;
}

MTPixelToFrame MultiTexturedPS(MTVertexToPixel PSIn)
{
	MTPixelToFrame Output = (MTPixelToFrame)0;

	float lightingFactor = 1;
	float blendDistance = 0.99f;
	float blendWidth = 0.005f;
	float blendFactor = clamp((PSIn.Depth-blendDistance)/blendWidth, 0, 1);//0 = near clipping distance, 1 = far clipping distance

	if(xEnableLighting)
		lightingFactor = saturate(saturate(dot(PSIn.Normal, PSIn.LightDirection)) + xAmbient);

	float4 farColor;
	farColor = tex2D(TextureSampler0, PSIn.TextureCoords)*PSIn.TextureWeights.x;
	farColor += tex2D(TextureSampler1, PSIn.TextureCoords)*PSIn.TextureWeights.y;
	farColor += tex2D(TextureSampler2, PSIn.TextureCoords)*PSIn.TextureWeights.z;
	farColor += tex2D(TextureSampler3, PSIn.TextureCoords)*PSIn.TextureWeights.w;

	/*float4 mediumColor;
	float2 mediumTextureCoords = PSIn.TextureCoords*3;
	mediumColor = tex2D(TextureSampler0, mediumTextureCoords)*PSIn.TextureWeights.x;
	mediumColor += tex2D(TextureSampler1, mediumTextureCoords)*PSIn.TextureWeights.y;
	mediumColor += tex2D(TextureSampler2, mediumTextureCoords)*PSIn.TextureWeights.z;
	mediumColor += tex2D(TextureSampler3, mediumTextureCoords)*PSIn.TextureWeights.w;*/

	float4 nearColor;
	float2 nearTextureCoords = PSIn.TextureCoords*3;//makes the texture 3 times smaller so a better def in close view
	nearColor = tex2D(TextureSampler0, nearTextureCoords)*PSIn.TextureWeights.x;
	nearColor += tex2D(TextureSampler1, nearTextureCoords)*PSIn.TextureWeights.y;
	nearColor += tex2D(TextureSampler2, nearTextureCoords)*PSIn.TextureWeights.z;
	nearColor += tex2D(TextureSampler3, nearTextureCoords)*PSIn.TextureWeights.w;

	Output.Color = lerp(nearColor, farColor, blendFactor);//linear interpolation
	//Output.Color *= lerp(mediumColor, farColor, blendFactor);
	Output.Color *= lightingFactor;

	if(Clipping)
		clip(PSIn.ClipDistances);

	return Output;
}

technique MultiTextured
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 MultiTexturedVS();
		PixelShader = compile ps_2_0 MultiTexturedPS();
	}
}

//water
struct WVertexToPixel
 {
     float4 Position					: POSITION;
     float4 ReflectionMapSamplingPos	: TEXCOORD1;
	 float2 BumpMapSamplingPos			: TEXCOORD2;
	 float4 RefractionMapSamplingPos	: TEXCOORD3;
	 float4 Position3D					: TEXCOORD4;
 };
 
 struct WPixelToFrame
 {
     float4 Color						: COLOR0;
 };
 
 WVertexToPixel WaterVS(float4 inPos : POSITION, float2 inTex: TEXCOORD)
 {    
     WVertexToPixel Output = (WVertexToPixel)0;
 
     float4x4 preViewProjection = mul (xView, xProjection);
     float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
     float4x4 preReflectionViewProjection = mul (xReflectionView, xProjection);
     float4x4 preWorldReflectionViewProjection = mul (xWorld, preReflectionViewProjection);
 
     Output.Position = mul(inPos, preWorldViewProjection);
     Output.ReflectionMapSamplingPos = mul(inPos, preWorldReflectionViewProjection);
	 
	 float3 winDir = normalize(xWindDirection);
	 float3 perpDir = cross(xWindDirection, float3(0,1,0));
	 float ydot = dot(inTex, xWindDirection.xz);
	 float xdot = dot(inTex, perpDir.xz);
	 float2 moveVector = float2(xdot, ydot);
	 moveVector.y += xTime*xWindForce;
	 Output.BumpMapSamplingPos = moveVector/xWaveLength;

	 Output.RefractionMapSamplingPos = mul(inPos, preWorldViewProjection);
	 Output.Position3D = mul(inPos, xWorld);//useless
 
     return Output;
 }
 
 WPixelToFrame WaterPS(WVertexToPixel PSIn)
 {
     WPixelToFrame Output = (WPixelToFrame)0;        
     
     float2 ProjectedTexCoords;
     ProjectedTexCoords.x = PSIn.ReflectionMapSamplingPos.x/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;
     ProjectedTexCoords.y = -PSIn.ReflectionMapSamplingPos.y/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	 float4 bumpColor = tex2D(WaterBumpMapSampler, PSIn.BumpMapSamplingPos);
	 float2 perturbation = xWaveHeight*(bumpColor.rg - 0.5f)*2.0f;
	 float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;

	 float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedTexCoords);

	 float2 ProjectedRefrTexCoords;
	 ProjectedRefrTexCoords.x = PSIn.RefractionMapSamplingPos.x/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;
	 ProjectedRefrTexCoords.y = -PSIn.RefractionMapSamplingPos.y/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;
	 float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation;

	 float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);

	 float3 eyeVector = normalize(xCamPos - PSIn.Position3D);
	 float3 normalVector = (bumpColor.rbg - 0.5f)*2.0f;
	 float fresnelTerm = dot(eyeVector, normalVector);

	 float4 combinedColor = lerp(reflectiveColor, refractiveColor, fresnelTerm);

	 float4 dullColor = float4(0.3f, 0.3f, 0.5f, 1.0f);
 
     Output.Color = lerp(combinedColor, dullColor, 0.2f);

	 float3 reflectionVector = -reflect(xLightDirection, normalVector);
	 float specular = dot(normalize(reflectionVector), normalize(eyeVector));
	 specular = abs(specular);

	 specular = pow(specular, 256);
	 Output.Color.rbg += specular;
     
     return Output;
 }
 
 technique Water
 {
     pass Pass0
     {
         VertexShader = compile vs_2_0 WaterVS();
         PixelShader = compile ps_2_0 WaterPS();
     }
 }

 //perlin noise
 struct PNVertexToPixel
 {
	float4 Position		: POSITION;
	float2 TexCoords	: TEXCOORD0;
 };

 struct PNPixelToFrame
 {
	float4 Color	:COLOR0;
};

PNVertexToPixel PerlinVS(float4 inPos: POSITION, float2 inTexCoords: TEXCOORD0)
{
	PNVertexToPixel Output = (PNVertexToPixel)0;

	Output.Position = inPos;
	Output.TexCoords = inTexCoords;

	return Output;
}

PNPixelToFrame PerlinPS(PNVertexToPixel PSIn)
{
	PNPixelToFrame Output = (PNPixelToFrame)0;

	float2 move = float2(0, 1);
	float4 perlin = tex2D(TextureSampler, (PSIn.TexCoords) + xTime*move)/2;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*2 + xTime*move)/4;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*4 + xTime*move)/8;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*8 + xTime*move)/16;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*16 + xTime*move)/32;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords)*32 + xTime*move)/32;

	Output.Color.rgb = 1.0f - pow(abs(perlin.r), xOvercast)*2.0f;
	Output.Color.a = 1;
	return Output;
}

technique PerlinNoise
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 PerlinVS();
		PixelShader = compile ps_2_0 PerlinPS();
	}
}

//skydome
struct SDVertexToPixel
{
	float4 Position:		POSITION;
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

	float4 topColor = float4(0.3f, 0.3f, 0.8f, 1);//kinda blue
	float4 bottomColor = 1;//white

	float4 baseColor = lerp(bottomColor, topColor, saturate((PSIn.ObjectPosition.y)/0.4f));//interpolate on the height of the pixel, 0.5 maxHeight
	float4 cloudValue = tex2D(SkyDomeTextureSampler, PSIn.TexCoords).r;

	Output.Color = lerp(baseColor, 1, cloudValue);//interpolate between skyColor and white

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

//billboard
struct BBVertexToPixel
{
    float4 Position : POSITION;
	float2 TexCoord	: TEXCOORD0;
};

struct BBPixelToFrame
{
    float4 Color : COLOR0;
};

BBVertexToPixel CylBillboardVS(float3 inPos: POSITION, float2 inTexCoord: TEXCOORD0)
{
    BBVertexToPixel Output = (BBVertexToPixel)0;

	float3 center = mul(inPos, xWorld);
	float3 eyeVector = center - xCamPos;

	float3 upVector = xAllowedRotDir;
	upVector = normalize(upVector);
	float3 sideVector = cross(eyeVector, upVector);
	sideVector = normalize(sideVector);

	float3 finalPosition = center;
	finalPosition += (inTexCoord.x-0.5f)*sideVector;
	finalPosition += (1.5f-inTexCoord.y*1.5f)*upVector;

	float4 finalPosition4 = float4(finalPosition, 1);

	float4x4 preViewProjection = mul(xView, xProjection);
	Output.Position = mul(finalPosition4, preViewProjection);

	Output.TexCoord = inTexCoord;

    return Output;
}

BBPixelToFrame BillboardPS(BBVertexToPixel PSIn): COLOR0
{
	BBPixelToFrame Output = (BBPixelToFrame)0;

	Output.Color = tex2D(textureSampler, PSIn.TexCoord);

	clip(Output.Color.w -.07843f);

	return Output;
}

technique CylBillboard
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 CylBillboardVS();
        PixelShader = compile ps_2_0 BillboardPS();
    }
}
