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
        CharacterRenderPass characterPass = ScriptableObject.CreateInstance<CharacterRenderPass>();
        ShadowRenderPass shadowPass = ScriptableObject.CreateInstance<ShadowRenderPass>();
        EffectRenderPass effectPass = ScriptableObject.CreateInstance<EffectRenderPass>();
<<<<<<< HEAD
        CombinePass combinePass = CombinePass.CreateInstance(backgroundPass, characterPass, shadowPass);
=======
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
>>>>>>> 91e846b5 (스프라이트 크기 버그 수정, 3d 2d 배경 분리, 인터페이스 패스 추가, 태그 추가, 빈 렌더패스 추가)

        renderPasses.Add(backgroundPass);
        renderPasses.Add(perspectivePass);
        renderPasses.Add(perspectiveDepthPass);
        renderPasses.Add(characterPass);
        renderPasses.Add(shadowPass);
        renderPasses.Add(effectPass);
<<<<<<< HEAD
        renderPasses.Add(combinePass);
=======
        renderPasses.Add(interfacePass);
        renderPasses.Add(combinePass);
        renderPasses.Add(emptyPass);

>>>>>>> 91e846b5 (스프라이트 크기 버그 수정, 3d 2d 배경 분리, 인터페이스 패스 추가, 태그 추가, 빈 렌더패스 추가)
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
