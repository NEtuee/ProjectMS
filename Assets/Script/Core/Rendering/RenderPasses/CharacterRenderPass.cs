using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return characterRenderTexture; } }
    private RenderTexture characterRenderTexture;

    private string _renderTexturePath = "RenderTexture/CharacterRenderLayer";

    private static int characterLayer;
    public override int layerMasks => characterLayer;
    public override string layerName => "Character";

    public void Awake()
    {
        characterLayer = (1 << LayerMask.NameToLayer(layerName));

        characterRenderTexture = Resources.Load(_renderTexturePath, typeof(RenderTexture)) as RenderTexture;
        if(characterRenderTexture == null)
            DebugUtil.assert(false,$"RenderTexture [{_renderTexturePath}]가 없습니다. 확인 필요");
        characterRenderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;
    }
    public override void Draw(Camera renderCamera)
    {
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;

    }
}
