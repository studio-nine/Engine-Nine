

sampler DefaultSampler : register(s0);

float2 PixelSize;


float3 FetchNormalVector( float2 tc )
{
	/*
	Coordinates are laid out as follows:

		0,0 | 1,0 | 2,0
		----+-----+----
		0,1 | 1,1 | 2,1
		----+-----+----
		0,2 | 1,2 | 2,2
	*/

	// Compute the necessary offsets:
	float2 o00 = tc + float2( -PixelSize.x, -PixelSize.y );
	float2 o10 = tc + float2(          0.0f, -PixelSize.y );
	float2 o20 = tc + float2(  PixelSize.x, -PixelSize.y );
	float2 o01 = tc + float2( -PixelSize.x, 0.0f          );
	float2 o21 = tc + float2(  PixelSize.x, 0.0f          );
	float2 o02 = tc + float2( -PixelSize.x,  PixelSize.y );
	float2 o12 = tc + float2(          0.0f,  PixelSize.y );
	float2 o22 = tc + float2(  PixelSize.x,  PixelSize.y );

	// Use of the sobel filter requires the eight samples
	// surrounding the current pixel:
	float h00 = tex2D( DefaultSampler, o00 ).r;
	float h10 = tex2D( DefaultSampler, o10 ).r;
	float h20 = tex2D( DefaultSampler, o20 ).r;
	float h01 = tex2D( DefaultSampler, o01 ).r;
	float h21 = tex2D( DefaultSampler, o21 ).r;
	float h02 = tex2D( DefaultSampler, o02 ).r;
	float h12 = tex2D( DefaultSampler, o12 ).r;
	float h22 = tex2D( DefaultSampler, o22 ).r;

	// The Sobel X kernel is:
	//
	// [ 1.0  0.0  -1.0 ]
	// [ 2.0  0.0  -2.0 ]
	// [ 1.0  0.0  -1.0 ]

	float Gx = h00 - h20 + 2.0f * h01 - 2.0f * h21 + h02 - h22;
				
	// The Sobel Y kernel is:
	//
	// [  1.0    2.0    1.0 ]
	// [  0.0    0.0    0.0 ]
	// [ -1.0   -2.0   -1.0 ]

	float Gy = h00 + 2.0f * h10 + h20 - h02 - 2.0f * h12 - h22;

	// Generate the missing Z component - tangent
	// space normals are +Z which makes things easier
	// The 0.5f leading coefficient can be used to control
	// how pronounced the bumps are - less than 1.0 enhances
	// and greater than 1.0 smoothes.
	float Gz = 0.5f * sqrt( 1.0f - Gx * Gx - Gy * Gy );

	// Make sure the returned normal is of unit length
	return normalize( float3( 2.0f * Gx, 2.0f * Gy, Gz ) );
}


float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
	float3 n = FetchNormalVector(texCoord);
	n.xy = n.xy * 0.5 + 0.5;
	
    return float4(n, 1);
}


technique Saturation
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}
