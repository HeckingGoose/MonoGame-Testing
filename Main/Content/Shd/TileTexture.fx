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
    uint subDivisionSize;
    
    // Get texture sizes
    InputTexture.GetDimensions(0, baseX, baseY, uintWaste);
    OutputTexture.GetDimensions(stretchX, stretchY);
    
    // Get the size of each subdivision
    subDivisionSize = baseX / SUBDIVISIONS;
    
    // Figure out what section we're operating in
    if (globalID.y < subDivisionSize)
    {
        // Operating somewhere in the top region
        if (globalID.x < subDivisionSize)
        {
            // Operating in the top left, so just do a direct pixel copy
            OutputTexture[globalID.xy] = pixel;
        }
        else if (globalID.x < baseX - subDivisionSize)
        {
            // Operating in the top middle
            
            // Declare stretch scale
            uint scaleX = (stretchX - subDivisionSize * 2) / (baseX - subDivisionSize * 2);
            
            // Stretch
            for (uint x = 0; x <= scaleX; x++)
            {
                if (
                    globalID.x + x + ((globalID.x - subDivisionSize) * scaleX) >= subDivisionSize
                    && globalID.x + x + ((globalID.x - subDivisionSize) * scaleX) < stretchX - subDivisionSize
                    )
                {
                    // Draw each pixel one at a time
                    OutputTexture[uint2(globalID.x + x + ((globalID.x - subDivisionSize) * scaleX), globalID.y)] = pixel;
                }
            }
        }
        else
        {
            // Operating in the top right, so do a translated pixel copy
            OutputTexture[uint2(stretchX - (subDivisionSize - (globalID.x - subDivisionSize * 2)), globalID.y)] = pixel;
        }
    }
    else if (globalID.y < baseY - subDivisionSize)
    {
        // Operating somewhere in the middle region
        if (globalID.x < subDivisionSize)
        {
            // Operating in the middle left
           
            // Declare stretch scale
            uint scaleY = (stretchY - subDivisionSize * 2) / (baseY - subDivisionSize * 2);
            
            // Stretch
            for (uint y = 0; y <= scaleY; y++)
            {
                if (
                    globalID.y + y + ((globalID.y - subDivisionSize) * scaleY) >= subDivisionSize
                    && globalID.y + y + ((globalID.y - subDivisionSize) * scaleY) < stretchY - subDivisionSize
                    )
                {
                    // Draw each pixel one at a time
                    OutputTexture[
                        uint2(
                        globalID.x,
                        globalID.y + y + ((globalID.y - subDivisionSize) * scaleY)
                        )] = pixel;
                }
            }
        }
        else if (globalID.x < baseX - subDivisionSize)
        {
            // Operating in the middle middle
            
            // Calculate scales
            uint scaleX = (stretchX - subDivisionSize * 2) / (baseX - subDivisionSize * 2);
            uint scaleY = (stretchY - subDivisionSize * 2) / (baseY - subDivisionSize * 2);
            
            // Loop through every pixel in the target
            for (uint y = 0; y <= scaleY; y++)
            {
                for (uint x = 0; x <= scaleX; x++)
                {
                    // If the pixel to plot is in a valid spot (bad math patch)
                    if (
                        globalID.x + x + ((globalID.x - subDivisionSize) * scaleX) <= stretchX - subDivisionSize
                        && globalID.y + y + ((globalID.y - subDivisionSize) * scaleY) <= stretchY - subDivisionSize
                        )
                    {
                        // Copy across a single pixel of data
                        OutputTexture[
                            uint2(
                            globalID.x + x + ((globalID.x - subDivisionSize) * scaleX),
                            globalID.y + y + ((globalID.y - subDivisionSize) * scaleY)
                            )] = pixel;
                    }
                }
            }
        }
        else
        {
            // Operating in the middle right
            
            // Declare stretch scale
            uint scaleY = (stretchY - subDivisionSize * 2) / (baseY - subDivisionSize * 2);
            
            // Stretch
            for (uint y = 0; y <= scaleY; y++)
            {
                if (
                    globalID.y + y + ((globalID.y - subDivisionSize) * scaleY) >= subDivisionSize
                    && globalID.y + y + ((globalID.y - subDivisionSize) * scaleY) < stretchY - subDivisionSize
                    )
                {
                    // Draw each pixel one at a time
                    OutputTexture[
                        uint2(
                        stretchX - (subDivisionSize - (globalID.x - subDivisionSize * 2)),
                        globalID.y + y + ((globalID.y - subDivisionSize) * scaleY)
                        )] = pixel;
                }
            }
        }
    }
    else
    {
        // Operating somewhere in the bottom region
        if (globalID.x < subDivisionSize)
        {
            // Operating in the bottom left, so just do a translated pixel copy
            OutputTexture[uint2(globalID.x, stretchY - (subDivisionSize - (globalID.y - subDivisionSize * 2)))] = pixel;
        }
        else if (globalID.x < baseX - subDivisionSize)
        {
            // Operating in the bottom middle
            
            // Declare stretch scale
            uint scaleX = (stretchX - subDivisionSize * 2) / (baseX - subDivisionSize * 2);
            
            // Stretch
            for (uint x = 0; x <= scaleX; x++)
            {
                if (
                    globalID.x + x + ((globalID.x - subDivisionSize) * scaleX) >= subDivisionSize
                    && globalID.x + x + ((globalID.x - subDivisionSize) * scaleX) < stretchX - subDivisionSize
                    )
                {
                    // Draw each pixel one at a time
                    OutputTexture[
                        uint2(
                        globalID.x + x + ((globalID.x - subDivisionSize) * scaleX),
                        stretchY - (subDivisionSize - (globalID.y - subDivisionSize * 2))
                        )] = pixel;
                }
            }
        }
        else
        {
            // Operating in the bottom right, so just do a translated pixel copy
            OutputTexture[uint2(stretchX - (subDivisionSize - (globalID.x - subDivisionSize * 2)), stretchY - (subDivisionSize - (globalID.y - subDivisionSize * 2)))] = pixel;
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