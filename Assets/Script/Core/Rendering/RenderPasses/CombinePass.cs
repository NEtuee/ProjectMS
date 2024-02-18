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
    InterfaceRenderPass interfaceRenderPass;

    Dictionary<string, AkaneRenderPass> renderPasses = new Dictionary<string, AkaneRenderPass>();

    private static int shadowScreenLayer;
    public override int layerMasks => shadowScreenLayer;
    public override string layerName => "ShadowScreen";

    public void AddPass<T>(T renderPass) where T : AkaneRenderPass
    {
        renderPasses.Add(renderPass.layerName, renderPass);
    }

    public void Awake()
    {
        shadowScreenLayer = (1 << LayerMask.NameToLayer(layerName));

        if (renderMaterial == null)
        {
            var quad = GameObject.FindGameObjectWithTag("ScreenResultMesh");

            var renderer = quad.GetComponent<Renderer>();
            renderMaterial = renderer.sharedMaterial;
        }
    }

    public static CombinePass CreateInstance(BackgroundRenderPass backgroundPass, CharacterRenderPass characterPass, InterfaceRenderPass interfacePass, PerspectiveDepthRenderPass perspectiveDepthPass)
    {
        var pass = ScriptableObject.CreateInstance<CombinePass>();
        pass.backgroundRenderPass = backgroundPass;
        pass.characterRenderPass = characterPass;
        pass.interfaceRenderPass = interfacePass;
        pass.perspectiveDepthRenderPass = perspectiveDepthPass;
        return pass;
    }
    public override void Draw(Camera renderCamera)
    {
        renderMaterial?.SetTexture("_CharacterTexture", characterRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_MainTex", backgroundRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_InterfaceTexture", interfaceRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_PerspectiveDepthTexture", perspectiveDepthRenderPass?.RenderTexture);

        float orthoSize = Camera.main.orthographicSize;
        float aspectRatio = Camera.main.aspect;
        float height = orthoSize * 2f;
        float width = height * aspectRatio;

        Vector4 cameraSize = Vector4.zero;
        cameraSize.x = width;
        cameraSize.y = height;
        
        renderMaterial?.SetVector("_RealCameraSize",cameraSize * 100.0f);

        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
