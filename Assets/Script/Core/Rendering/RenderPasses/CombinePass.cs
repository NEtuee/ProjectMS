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

    private static int shadowScreenLayer;
    protected override int layerMasks { get; set; } = shadowScreenLayer;

    public static CombinePass CreateInstance(BackgroundRenderPass backgroundPass, CharacterRenderPass characterPass, ShadowRenderPass shadowPass)
    {
        var pass = ScriptableObject.CreateInstance<CombinePass>();
        pass.backgroundRenderPass = backgroundPass;
        pass.characterRenderPass = characterPass;
        pass.shadowRenderPass = shadowPass;

        return pass;
    }

    public void OnEnable()
    {
        shadowScreenLayer = (1 << LayerMask.NameToLayer("ShadowScreen"));

        if (renderMaterial == null)
        {
            var quad = GameObject.FindGameObjectWithTag("ScreenResultMesh");

            var renderer = quad.GetComponent<Renderer>();
            renderMaterial = renderer.sharedMaterial;
        }
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
