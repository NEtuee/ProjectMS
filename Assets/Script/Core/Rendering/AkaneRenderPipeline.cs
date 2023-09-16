using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

        PerspectiveRenderPass perspectivePass = ScriptableObject.CreateInstance<PerspectiveRenderPass>();
        perspectivePass.Awake();

        PerspectiveDepthRenderPass perspectiveDepthPass = ScriptableObject.CreateInstance<PerspectiveDepthRenderPass>();
        perspectiveDepthPass.Awake();

        CombinePass combinePass = CombinePass.CreateInstance(backgroundPass, characterPass, shadowPass, perspectivePass, interfacePass, perspectiveDepthPass);
        combinePass.Awake();

        EmptyRenderPass emptyPass = ScriptableObject.CreateInstance<EmptyRenderPass>();
        emptyPass.Awake();



        combinePass.AddPass(backgroundPass);
       // combinePass.AddPass(perspectivePass);
        combinePass.AddPass(characterPass);
        combinePass.AddPass(shadowPass);
        combinePass.AddPass(effectPass);
      //  combinePass.AddPass(interfacePass);

        renderPasses.Add(backgroundPass);
        renderPasses.Add(perspectivePass);
        renderPasses.Add(perspectiveDepthPass);
        renderPasses.Add(characterPass);
        renderPasses.Add(shadowPass);
        renderPasses.Add(effectPass);
        renderPasses.Add(combinePass);
        renderPasses.Add(interfacePass);
        renderPasses.Add(combinePass);
        renderPasses.Add(emptyPass);
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
     //   UnityEditorInternal.RenderDoc.BeginCaptureRenderDoc(UnityEditor.EditorWindow.GetWindow<SceneView>("Scene"));
        for (int i = 0; i < renderPasses.Count; i++)
        {
            var renderPass = renderPasses[i];

            renderPass.Draw(internalCamera, renderPass.RenderTexture);
        }
//.RenderDoc.EndCaptureRenderDoc(UnityEditor.EditorWindow.GetWindow<SceneView>("Scene"));
    }
}
