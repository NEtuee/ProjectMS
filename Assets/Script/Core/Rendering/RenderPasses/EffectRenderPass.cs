using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectRenderPass : AkaneRenderPass
{
    private static int effectLayer;
    private static int shadowScreenLayer;
    protected override int layerMasks { get; set; } = effectLayer | shadowScreenLayer;

    public override RenderTexture RenderTexture { get { return effectRenderTexture; } }
    [SerializeField] private RenderTexture effectRenderTexture;

    public void OnEnable()
    {
        effectLayer = (1 << LayerMask.NameToLayer("EffectEtc"));
        shadowScreenLayer = (1 << LayerMask.NameToLayer("EffectEtc"));

        effectRenderTexture = new RenderTexture(960, 640, 1, RenderTextureFormat.ARGBHalf, 1);
    }
    public override void Draw(Camera renderCamera, RenderTexture buffer)
    {
        renderCamera.targetTexture = buffer;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
