using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardScreenRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return forwardScreenRenderTexture; } }
    [SerializeField] private RenderTexture forwardScreenRenderTexture;

    private static int forwardScreenLayer;
    public override int layerMasks => forwardScreenLayer;
    public override string layerName => "ForwardScreen";

    public void Awake()
    {
        forwardScreenLayer = (1 << LayerMask.NameToLayer("ForwardScreen"));
        forwardScreenRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1);
        forwardScreenRenderTexture.filterMode = FilterMode.Point;
        forwardScreenRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;
    }
    public override void Draw(Camera renderCamera)
    {
        Vector3 positionOrigin = renderCamera.transform.position;

        renderCamera.transform.position = MathEx.floorNoSign(renderCamera.transform.position,2);
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.transform.position = positionOrigin;
        renderCamera.cullingMask = 0;
    }
}
