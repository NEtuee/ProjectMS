using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinePass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return null; } }
    private Material renderMaterial;

    BackgroundRenderPass backgroundRenderPass;
    CharacterRenderPass characterRenderPass;
    ShadowRenderPass shadowRenderPass;

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

    public static CombinePass CreateInstance(BackgroundRenderPass backgroundPass, CharacterRenderPass characterPass, ShadowRenderPass shadowPass)
    {
        var pass = ScriptableObject.CreateInstance<CombinePass>();
        pass.backgroundRenderPass = backgroundPass;
        pass.characterRenderPass = characterPass;
        pass.shadowRenderPass = shadowPass;

        return pass;
    }
    public override void Draw(Camera renderCamera, RenderTexture buffer)
    {
        renderMaterial?.SetTexture("_CharacterTexture", characterRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_MainTex", backgroundRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_ShadowMapTexture", shadowRenderPass?.RenderTexture);

        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
