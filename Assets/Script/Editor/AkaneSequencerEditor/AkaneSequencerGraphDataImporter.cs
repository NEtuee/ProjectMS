using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "sgd", AllowCaching = true)]
public class AkaneSequencerGraphDataImporter : ScriptedImporter
{
    public static bool CurrentIsSave = false;
    public static AkaneSequencerGraphData CurrentData;
    
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var asset = CurrentData == null ? ScriptableObject.CreateInstance<AkaneSequencerGraphData>() : CurrentData;
        
        ctx.AddObjectToAsset(GUID.Generate().ToString(), asset);
        ctx.SetMainObject(asset);
    }
}
