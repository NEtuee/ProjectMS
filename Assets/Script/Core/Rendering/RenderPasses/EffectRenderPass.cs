using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectRenderPass : AkaneRenderPass
{
    private static int effectLayer;
    private static int shadowScreenLayer;
    protected override int layerMasks { get; set; } = effectLayer | shadowScreenLayer;

    public void OnEnable()
    {
        effectLayer = (1 << LayerMask.NameToLayer("EffectEtc"));
        shadowScreenLayer = (1 << LayerMask.NameToLayer("EffectEtc"));
    }
    public override void Draw(Camera renderCamera, RenderTexture buffer)
    {
        renderCamera.targetTexture = buffer;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
