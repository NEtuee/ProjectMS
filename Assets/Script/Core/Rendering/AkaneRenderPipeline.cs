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
        CharacterRenderPass characterPass = ScriptableObject.CreateInstance<CharacterRenderPass>();
        ShadowRenderPass shadowPass = ScriptableObject.CreateInstance<ShadowRenderPass>();
        EffectRenderPass effectPass = ScriptableObject.CreateInstance<EffectRenderPass>();
        CombinePass combinePass = CombinePass.CreateInstance(backgroundPass, characterPass, shadowPass);

        renderPasses.Add(backgroundPass);
        renderPasses.Add(characterPass);
        renderPasses.Add(shadowPass);
        renderPasses.Add(effectPass);
        renderPasses.Add(combinePass);
    }

    private void Awake()
    {
        initializeRenderResources();
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
