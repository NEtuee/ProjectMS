using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PerspectiveRenderPass : AkaneRenderPass
{
    public override int layerMasks => perspectiveLayer;
    private static int perspectiveLayer;
    public override string layerName => "Perspective";

    //   public override RenderTexture RenderTexture => perspectiveRenderTexture;
    public override RenderTexture RenderTexture { get { return perspectiveRenderTexture; } }
    [SerializeField] private RenderTexture perspectiveRenderTexture;
    public void Awake()
    {
        perspectiveLayer = (1 << LayerMask.NameToLayer(layerName));
        perspectiveRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1);
        perspectiveRenderTexture.filterMode = FilterMode.Point;
    }

    public override void Draw(Camera renderCamera, RenderTexture buffer)
    {
        renderCamera.targetTexture = buffer;
        renderCamera.cullingMask = layerMasks;
        
        renderCamera.orthographic = false;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
        renderCamera.orthographic = true;
    }
}
