using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SULWRenderTexture2D
{
    public SULWRenderTexture2D(string layer, int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGBHalf)
    {
        Texture = new RenderTexture(width, height, 1, format);
        Layer = layer;
    }

    public SULWRenderTexture2D(string layer, RenderTexture renderTexture)
    {
        Texture = renderTexture;
        Layer = layer;
    }

    public RenderTexture Texture;
    public string Layer;
}

public class SULWRenderPipeline : MonoBehaviour
{

    // �ε����� ���̾� ��ȣ, ���빰�� ���̾� �̸�
    // �⺻ RGBA16 ����
    // Ư�� ���̾ �ٸ� ���� �����Ϸ��� layerFormatSpecifier�� ���̾� �̸� �߰��ϰ� ���� �������ָ� ��
    private List<string> layerOrders = new List<string>();
    
    [SerializeField] private List<SULWRenderTexture2D> renderTextures = new List<SULWRenderTexture2D>();
    private Dictionary<string, SULWRenderTexture2D> layerToRenderTexture = new Dictionary<string, SULWRenderTexture2D>();
    [SerializeField] private Camera internalCamera;

    #region Debug
    [SerializeField] private List<RenderTexture> _debugRenderTextures;
    #endregion

    public const int RENDERING_LAYER_FROM = 6;
    public const int RENDERING_LAYER_TO = 18;

    private void Awake()
    {
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
                var renderTexture = new SULWRenderTexture2D(engineLayer, _debugRenderTextures[debugCount]);
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
                var renderTexture = new SULWRenderTexture2D(engineLayer, Screen.width, Screen.height);
                layerOrders.Add(engineLayer);
                renderTextures.Add(renderTexture);
                layerToRenderTexture.Add(engineLayer, renderTexture);
            }

        }
    }
    private void LateUpdate()
    {
        internalDraw();
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
    }
}
