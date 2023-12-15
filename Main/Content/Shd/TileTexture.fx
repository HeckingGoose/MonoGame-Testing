#define GroupSizeXY 8
#define SUBDIVISIONS 3

Texture2D<float4> InputTexture;
RWTexture2D<float4> OutputTexture;

[numthreads(GroupSizeXY, GroupSizeXY, 1)]
void CS(uint3 localID: SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint localIndex: SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    float4 pixel = InputTexture[globalID.xy];
    
    // Var
    uint baseX;
    uint baseY;
    uint stretchX;
    uint stretchY;
    uint uintWaste;
    uint scaleX;
    uint scaleY;
    uint extraX;
    uint extraY;
    uint subDivisionSize;
    
    // Get texture sizes
    InputTexture.GetDimensions(0, baseX, baseY, uintWaste);
    OutputTexture.GetDimensions(stretchX, stretchY);
    
    // Get the size of each subdivision
    subDivisionSize = baseX / SUBDIVISIONS;
    
    // Calculate scales
    scaleX = stretchX / baseX;
    scaleY = stretchY / baseY;
    
    // Calculate repeats
    extraX = stretchX % baseX;
    
    extraY = stretchY % baseY;
    
    
    // Loop through y direction
    for (int y = 0; y < scaleY; y++)
    {
        // Loop through x direction
        for (int x = 0; x < scaleX; x++)
        {
            // Draw each pixel one at a time
            OutputTexture[uint2((globalID.x * scaleX) + x, (globalID.y * scaleY) + y)] = pixel;
        }
    }
    
}

technique Tech0
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 CS();
    }
}