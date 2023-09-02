using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return characterRenderTexture; } }
    [SerializeField] private RenderTexture characterRenderTexture;

    private static int characterLayer;
    protected override int layerMasks { get; set; } = characterLayer;

    public void OnEnable()
    {
        characterLayer = (1 << LayerMask.NameToLayer("Character"));
        characterRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1);
        characterRenderTexture.filterMode = FilterMode.Point;
    }
    public override void Draw(Camera renderCamera, RenderTexture buffer)
    {
        renderCamera.targetTexture = buffer;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
