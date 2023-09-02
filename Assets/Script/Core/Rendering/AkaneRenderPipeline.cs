using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AkaneRenderTexture2D
{
    public AkaneRenderTexture2D(string layer, int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGBHalf)
    {
        Texture = new RenderTexture(width, height, 1, format);
        Layer = layer;
    }
    public AkaneRenderTexture2D(string layer, RenderTexture renderTexture)
    {
        Texture = renderTexture;
        Layer = layer;
    }

    public RenderTexture Texture;
    public string Layer;
}

[ExecuteAlways]
public class AkaneRenderPipeline : MonoBehaviour
{
    [SerializeField] private Camera internalCamera;
    [SerializeField] private List<AkaneRenderPass> renderPasses;

    private void initializeRenderResources()
    {
        renderPasses = new List<AkaneRenderPass>();

        BackgroundRenderPass backgroundPass = ScriptableObject.CreateInstance<BackgroundRenderPass>();
        backgroundPass.Awake();

        CharacterRenderPass characterPass = ScriptableObject.CreateInstance<CharacterRenderPass>();
        characterPass.Awake();

        ShadowRenderPass shadowPass = ScriptableObject.CreateInstance<ShadowRenderPass>();
        shadowPass.Awake();

        EffectRenderPass effectPass = ScriptableObject.CreateInstance<EffectRenderPass>();
        effectPass.Awake();

        InterfaceRenderPass interfacePass = ScriptableObject.CreateInstance<InterfaceRenderPass>();
        interfacePass.Awake();

        CombinePass combinePass = CombinePass.CreateInstance(backgroundPass, characterPass, shadowPass);
        combinePass.Awake();

        combinePass.AddPass(backgroundPass);
        combinePass.AddPass(characterPass);
        combinePass.AddPass(shadowPass);
        combinePass.AddPass(interfacePass);

        renderPasses.Add(backgroundPass);
        renderPasses.Add(characterPass);
        renderPasses.Add(shadowPass);
        renderPasses.Add(effectPass);

        renderPasses.Add(combinePass);
        renderPasses.Add(interfacePass);
    }

    private void Awake()
    {
        initializeRenderResources();

        var pixelMult = 3; // scaling factor, assumes 100ppu unity default, and scales up to my desired 3 pixel squares.

        var camera = internalCamera;
        var camFrustWidthShouldBe = Screen.height / 100f;
        var frustrumInnerAngles = (180f - camera.fieldOfView) / 2f * Mathf.PI / 180f;
        var newCamDist = Mathf.Tan(frustrumInnerAngles) * (camFrustWidthShouldBe / 2);
        camera.transform.position = new Vector3(0, 0, -newCamDist / pixelMult);
    }

    [ExecuteAlways]
    private void LateUpdate()
    {
        if (Application.isPlaying == true)
        {
            internalDraw();
        }
        else
        {
            editor_internalDraw();
        }
    }

    private void OnApplicationQuit()
    {
        ReleaseResources();
    }
    void ReleaseResources()
    {
        renderPasses = null;
    }

    private void editor_internalDraw()
    {
        if (renderPasses == null)
        {
            initializeRenderResources();
        }

        internalDraw();
    }
    private void internalDraw()
    {
        for (int i = 0; i < renderPasses.Count; ++i)
        {
            var renderPass = renderPasses[i];

            renderPass.Draw(internalCamera, renderPass.RenderTexture);
        }
    }
}
