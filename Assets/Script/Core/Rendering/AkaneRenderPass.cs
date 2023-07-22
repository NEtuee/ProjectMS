using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AkaneRenderPass : ScriptableObject
{
    protected abstract int layerMasks { get; set; }
    public abstract void Draw(Camera renderCamera, RenderTexture buffer);
}