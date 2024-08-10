using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AkaneRenderPipeline : MonoBehaviour
{
    [SerializeField] private Camera internalCamera;
    [SerializeField] private List<AkaneRenderPass> renderPasses;
    [SerializeField] private float fieldOfView;
    static public float FieldOfView;
    private void initializeRenderResources()
    {
        renderPasses = new List<AkaneRenderPass>();

        BackgroundRenderPass backgroundPass = ScriptableObject.CreateInstance<BackgroundRenderPass>();
        backgroundPass.Awake();
        CharacterRenderPass characterPass = ScriptableObject.CreateInstance<CharacterRenderPass>();
        characterPass.Awake();
        EffectRenderPass effectPass = ScriptableObject.CreateInstance<EffectRenderPass>();
        effectPass.Awake();

        ForwardScreenRenderPass forwardScreenPass = ScriptableObject.CreateInstance<ForwardScreenRenderPass>();
        forwardScreenPass.Awake();

        DecalRenderPass decalPass = ScriptableObject.CreateInstance<DecalRenderPass>();
        decalPass.Awake();

        PerspectiveDepthRenderPass perspectiveDepthPass = ScriptableObject.CreateInstance<PerspectiveDepthRenderPass>();
        perspectiveDepthPass.Awake();

        CombinePass combinePass = CombinePass.CreateInstance(backgroundPass, characterPass, perspectiveDepthPass,forwardScreenPass,decalPass);
        combinePass.Awake();

        // EmptyRenderPass emptyPass = ScriptableObject.CreateInstance<EmptyRenderPass>();
        // emptyPass.Awake();

        renderPasses.Add(backgroundPass);
        renderPasses.Add(perspectiveDepthPass);
        renderPasses.Add(characterPass);
        renderPasses.Add(forwardScreenPass);
        renderPasses.Add(decalPass);
        renderPasses.Add(effectPass);
        renderPasses.Add(combinePass);
        //renderPasses.Add(emptyPass);
    }

    private void Awake()
    {
        initializeRenderResources();
    }

    [ExecuteAlways]
    private void LateUpdate()
    {
        FieldOfView = fieldOfView;
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
        for (int i = 0; i < renderPasses.Count; i++)
        {
            var renderPass = renderPasses[i];

            renderPass.Draw(internalCamera);
        }
    }
}
