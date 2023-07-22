using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectRenderPass : AkaneRenderPass
{
    protected override int layerMasks { get; set; } = (1 << LayerMask.NameToLayer("EffectEtc")) | (1 << LayerMask.NameToLayer("ShadowScreen"));
    public override void Draw(Camera renderCamera, RenderTexture buffer)
    {
        renderCamera.targetTexture = buffer;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
