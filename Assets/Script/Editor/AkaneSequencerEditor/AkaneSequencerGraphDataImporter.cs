using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "sgd", AllowCaching = true)]
public class AkaneSequencerGraphDataImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var asset = ScriptableObject.CreateInstance<AkaneSequencerGraphData>();

        var assetPath = ctx.assetPath;

        // asset.FilePath = assetPath;
        // asset.Load();
        
        ctx.AddObjectToAsset(GUID.Generate().ToString(), asset);
        ctx.SetMainObject(asset);
    }
}
