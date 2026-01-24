using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiPassFeature : ScriptableRendererFeature
{
    public List<string> shaderTags = new() { "UniversalForward" };

    private MultiPassPass pass;

    public override void Create()
    {
        pass = new MultiPassPass(shaderTags);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}

