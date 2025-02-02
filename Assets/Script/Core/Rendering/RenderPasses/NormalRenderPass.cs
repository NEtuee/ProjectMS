using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NormalRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return normalRenderTexture; } }
    [SerializeField] private RenderTexture normalRenderTexture;

    private static int backgroundLayer;
    public override int layerMasks => backgroundLayer;
    public override string layerName => "Background";

    private NormalRenderFeature _normalRenderFeature;

    public void Awake()
    {
        backgroundLayer = (1 << LayerMask.NameToLayer("Background"));
        normalRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1);
        normalRenderTexture.filterMode = FilterMode.Point;
        normalRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;

        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        foreach(UniversalRendererData rendererData in urpAsset.rendererDataList)
        {
            _normalRenderFeature = rendererData.rendererFeatures.Find(f => f is NormalRenderFeature) as NormalRenderFeature;
            if(_normalRenderFeature != null)
                break;
        }
        
        if(_normalRenderFeature == null)
            Debug.Log("render feature가 없음!");
    }
    public override void Draw(Camera renderCamera)
    {
        Vector3 positionOrigin = renderCamera.transform.position;

        renderCamera.transform.position = MathEx.floorNoSign(renderCamera.transform.position,2);
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        _normalRenderFeature._renderNormal = true;
        renderCamera.Render();
        _normalRenderFeature._renderNormal = false;

        renderCamera.transform.position = positionOrigin;
        renderCamera.cullingMask = 0;
    }
}
