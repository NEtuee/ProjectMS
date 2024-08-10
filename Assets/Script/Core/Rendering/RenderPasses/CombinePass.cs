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

    private static int shadowScreenLayer;
    public override int layerMasks => shadowScreenLayer;
    public override string layerName => "ShadowScreen";

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
        renderMaterial?.SetTexture("_CharacterTexture", characterRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_MainTex", backgroundRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_PerspectiveDepthTexture", perspectiveDepthRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_ForwardScreenTexture", forwardScreenRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_DecalTexture", decalRenderPass?.RenderTexture);

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
