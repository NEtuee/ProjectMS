using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AkaneRenderPass : ScriptableObject
{
    protected abstract int layerMasks { get; set; }
    public abstract RenderTexture RenderTexture { get; }
    public abstract void Draw(Camera renderCamera, RenderTexture buffer);
}