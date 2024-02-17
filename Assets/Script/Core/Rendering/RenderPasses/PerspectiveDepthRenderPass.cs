using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveDepthRenderPass : AkaneRenderPass
{
    public override int layerMasks => perspectiveLayer;
    private static int perspectiveLayer;
    public override string layerName => "Background";

    public override RenderTexture RenderTexture { get { return perspectiveDepthRenderTexture; } }
    [SerializeField] private RenderTexture perspectiveDepthRenderTexture;
    public void Awake()
    {
        perspectiveLayer = (1 << LayerMask.NameToLayer(layerName));
        perspectiveDepthRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.Depth, 1);
        perspectiveDepthRenderTexture.filterMode = FilterMode.Point;
    }

    public override void Draw(Camera renderCamera)
    {
        Vector3 positionOrigin = renderCamera.transform.position;

        renderCamera.transform.position = MathEx.floorNoSign(renderCamera.transform.position, 2);
        renderCamera.targetTexture = RenderTexture;
        DepthTextureMode mode = renderCamera.depthTextureMode;
        renderCamera.depthTextureMode = DepthTextureMode.Depth;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.depthTextureMode = mode;
        renderCamera.transform.position = positionOrigin;
        renderCamera.cullingMask = 0;
    }
}
