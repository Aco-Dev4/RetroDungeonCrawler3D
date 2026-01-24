using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiPassPass : ScriptableRenderPass
{
    private List<ShaderTagId> tags;

    public MultiPassPass(List<string> shaderTags)
    {
        tags = new();
        foreach (var tag in shaderTags)
            tags.Add(new ShaderTagId(tag));

        renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    [System.Obsolete]
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        FilteringSettings filtering = FilteringSettings.defaultValue;

        foreach (var tag in tags)
        {
            var drawing = CreateDrawingSettings(tag, ref renderingData, SortingCriteria.CommonOpaque);
            context.DrawRenderers(renderingData.cullResults, ref drawing, ref filtering);
        }
    }
}

