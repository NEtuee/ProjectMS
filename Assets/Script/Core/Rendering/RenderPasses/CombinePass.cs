using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinePass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return null; } }
    private Material renderMaterial;

    BackgroundRenderPass backgroundRenderPass;
    CharacterRenderPass characterRenderPass;
    PerspectiveDepthRenderPass perspectiveDepthRenderPass;
    ForwardScreenRenderPass forwardScreenRenderPass;
    DecalRenderPass decalRenderPass;

    private RenderTexture _combineBackgroundTexture;

    private static int shadowScreenLayer;
    public override int layerMasks => shadowScreenLayer;
    public override string layerName => "ShadowScreen";

    public Material _backgroundCombineMaterial;

    public void Awake()
    {
        shadowScreenLayer = (1 << LayerMask.NameToLayer(layerName));

        if (renderMaterial == null)
        {
            var quad = GameObject.FindGameObjectWithTag("ScreenResultMesh");

            var renderer = quad.GetComponent<Renderer>();
            renderMaterial = renderer.sharedMaterial;
        }

        _backgroundCombineMaterial = Material.Instantiate(ResourceContainerEx.Instance().GetMaterial("Material/BackgroundDecalCombine"));
        _combineBackgroundTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1);
        _combineBackgroundTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;
    }

    public static CombinePass CreateInstance(BackgroundRenderPass backgroundPass, CharacterRenderPass characterPass, PerspectiveDepthRenderPass perspectiveDepthPass, ForwardScreenRenderPass forwardScreenPass, DecalRenderPass decalPass)
    {
        var pass = ScriptableObject.CreateInstance<CombinePass>();
        pass.backgroundRenderPass = backgroundPass;
        pass.characterRenderPass = characterPass;
        pass.perspectiveDepthRenderPass = perspectiveDepthPass;
        pass.forwardScreenRenderPass = forwardScreenPass;
        pass.decalRenderPass = decalPass;

        return pass;
    }

    public override void Draw(Camera renderCamera)
    {
        _backgroundCombineMaterial.SetTexture("_DecalTex", decalRenderPass?.RenderTexture);
        _backgroundCombineMaterial.SetTexture("_PerspectiveDepthTexture",perspectiveDepthRenderPass?.RenderTexture);
        _backgroundCombineMaterial.SetTexture("_CharacterTexture",characterRenderPass?.RenderTexture);

        CameraControlEx.Instance().getPostProcessProfileControl()?.syncShadowValueToMaterial(_backgroundCombineMaterial);

        Graphics.Blit(backgroundRenderPass?.RenderTexture, _combineBackgroundTexture,_backgroundCombineMaterial);

        renderMaterial?.SetTexture("_CharacterTexture", characterRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_MainTex", _combineBackgroundTexture);
        renderMaterial?.SetTexture("_PerspectiveDepthTexture", perspectiveDepthRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_ForwardScreenTexture", forwardScreenRenderPass?.RenderTexture);

        float orthoSize = Camera.main.orthographicSize;
        float aspectRatio = Camera.main.aspect;
        float height = orthoSize * 2f;
        float width = height * aspectRatio;

        Vector4 cameraSize = Vector4.zero;
        cameraSize.x = width;
        cameraSize.y = height;
        
        renderMaterial?.SetVector("_RealCameraSize",cameraSize * 100.0f);
    }
}
