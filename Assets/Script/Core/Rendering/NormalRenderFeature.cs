using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

#pragma warning disable 612, 618, 672
public class NormalRenderFeature : ScriptableRendererFeature
{
    public class NormalPass : ScriptableRenderPass
    {
        private Material normalMaterial;
        private FilteringSettings filteringSettings;
        private string profilerTag = "NormalRenderPass";

        public NormalPass(Material normalMaterial)
        {
            this.normalMaterial = normalMaterial;
            filteringSettings = new FilteringSettings(RenderQueueRange.all);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (normalMaterial == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
            {
                RenderTargetIdentifier cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                cmd.SetRenderTarget(cameraTarget);
                cmd.ClearRenderTarget(true, true, Color.black);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera);
            DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("UniversalForward"), sortingSettings)
            {
                overrideMaterial = normalMaterial,
                overrideMaterialPassIndex = 0
            };
            drawingSettings.SetShaderPassName(1, new ShaderTagId("SRPDefaultUnlit"));

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public Material replacementMaterial;

    NormalPass _replacementPass;

    public bool _renderNormal = false;

    public override void Create()
    {
        if (replacementMaterial == null)
            return;

        _replacementPass = new NormalPass(replacementMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRendering 
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (replacementMaterial == null || _renderNormal == false)
            return;

        renderer.EnqueuePass(_replacementPass);
    }
}
#pragma warning restore 612, 618, 672