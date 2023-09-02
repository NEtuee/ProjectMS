using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ShadowRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return shadowRenderTexture; } }
    [SerializeField] private RenderTexture shadowRenderTexture;

    private static int shadowLayer;
    protected override int layerMasks { get; set; } = shadowLayer;

    public void OnEnable()
    {
        shadowLayer = (1 << LayerMask.NameToLayer("ShadowMap"));
        shadowRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.Shadowmap, 1);
        shadowRenderTexture.filterMode = FilterMode.Point;
    }
    public override void Draw(Camera renderCamera, RenderTexture buffer)
    {
        renderCamera.targetTexture = buffer;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
