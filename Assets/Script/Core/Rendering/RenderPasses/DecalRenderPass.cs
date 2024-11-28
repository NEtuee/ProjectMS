using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DecalRenderPass : AkaneRenderPass
{
    private static int effectLayer;
    private static int shadowScreenLayer;
    public override int layerMasks => effectLayer | shadowScreenLayer;
    public override string layerName => "Decal";
    public override RenderTexture RenderTexture { get { return decalRenderTexture; } }
    [SerializeField] private RenderTexture decalRenderTexture;

    public void Awake()
    {
        effectLayer = (1 << LayerMask.NameToLayer(layerName));

        decalRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1)
        {
            stencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
        };
        
        decalRenderTexture.filterMode = FilterMode.Point;
        decalRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;
    }
    public override void Draw(Camera renderCamera)
    {
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
