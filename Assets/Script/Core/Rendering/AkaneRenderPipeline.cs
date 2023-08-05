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
    // 인덱스가 레이어 번호, 내용물이 레이어 이름
    // 기본 RGBA16 포맷
    // 특정 레이어만 다른 포맷 지정하려면 layerFormatSpecifier에 레이어 이름 추가하고 포맷 설정해주면 됨
    // 위에 주석은 무시하면 됨
    private List<string> layerOrders;

    [SerializeField] private List<AkaneRenderTexture2D> renderTextures;
    private Dictionary<string, AkaneRenderTexture2D> layerToRenderTexture;
    [SerializeField] private Camera internalCamera;
    private List<AkaneRenderPass> renderPasses;
    #region Debug
    [SerializeField] private List<RenderTexture> _debugRenderTextures;
    #endregion

    public const int RENDERING_LAYER_FROM = 6;
    public const int RENDERING_LAYER_TO = 18;

    private void initializeRenderResources()
    {
        renderPasses = new List<AkaneRenderPass>();
        layerOrders = new List<string>();
        renderTextures = new List<AkaneRenderTexture2D>();
        layerToRenderTexture = new Dictionary<string, AkaneRenderTexture2D>();
        
        renderPasses.Add(EffectRenderPass.CreateInstance<EffectRenderPass>());

        int debugCount = 0;
        for (int i = RENDERING_LAYER_FROM; i < RENDERING_LAYER_TO; ++i)
        {
            string engineLayer = LayerMask.LayerToName(i);

            if (engineLayer == string.Empty)
            {
                continue;
            }

            if (_debugRenderTextures.Count > debugCount)
            {
                var renderTexture = new AkaneRenderTexture2D(engineLayer, _debugRenderTextures[debugCount]);
                layerOrders.Add(engineLayer);
                renderTextures.Add(renderTexture);
                layerToRenderTexture.Add(engineLayer, renderTexture);
                debugCount++;
            }
            else
            {
                if (debugCount != 0)
                {
                    return;
                }
                var renderTexture = new AkaneRenderTexture2D(engineLayer, Screen.width, Screen.height);
                layerOrders.Add(engineLayer);
                renderTextures.Add(renderTexture);
                layerToRenderTexture.Add(engineLayer, renderTexture);
            }
        }
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
            Debug.Log("Game Update");
            internalDraw();
        }
        else
        {
            Debug.Log("Editor Update");
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
        _debugRenderTextures = null;
        layerToRenderTexture = null;
        layerOrders = null;
    }

    private void editor_internalDraw()
    {
        if (renderPasses == null || layerToRenderTexture == null || layerOrders == null || _debugRenderTextures == null)
        {
            initializeRenderResources();
        }

        for (int i = 0; i < layerOrders.Count; ++i)
        {
            string keyLayer = layerOrders[i];
            RenderTexture renderTexture = layerToRenderTexture[keyLayer].Texture;
            //Graphics.SetRenderTarget(layerToRenderTexture[keyLayer].Texture);

            internalCamera.cullingMask |= 1 << LayerMask.NameToLayer(layerOrders[i]);

            internalCamera.targetTexture = renderTexture;
            internalCamera.Render();

            internalCamera.cullingMask = 0;
        }

        string effectLayer = layerOrders[3];
        RenderTexture effectRenderTexture = layerToRenderTexture[effectLayer].Texture;
        renderPasses[0].Draw(internalCamera, effectRenderTexture);
    }
    private void internalDraw()
    {
        for (int i = 0; i < layerOrders.Count; ++i)
        {
            string keyLayer = layerOrders[i];
            RenderTexture renderTexture = layerToRenderTexture[keyLayer].Texture;
            //Graphics.SetRenderTarget(layerToRenderTexture[keyLayer].Texture);

            internalCamera.cullingMask |= 1 << LayerMask.NameToLayer(layerOrders[i]);

            internalCamera.targetTexture = renderTexture;
            internalCamera.Render();

            internalCamera.cullingMask = 0;
        }

        string effectLayer = layerOrders[3];
        RenderTexture effectRenderTexture = layerToRenderTexture[effectLayer].Texture;
        renderPasses[0].Draw(internalCamera, effectRenderTexture);
    }
}
