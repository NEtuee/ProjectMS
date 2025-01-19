using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using AkaneSequencerGraph;

[CustomEditor(typeof(AkaneSequencerGraphDataImporter), true)]
public class AkaneSequencerGraphDataImporterEditor : ScriptedImporterEditor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Sequencer Graph Editor") == true)
        {
            AssetImporter importer = target as AssetImporter;
            var graphData = AssetDatabase.LoadAssetAtPath<AkaneSequencerGraphData>(importer.assetPath);
            AkaneSequencerGraphEditorWindow.OpenEditor(graphData, importer.assetPath);
        }
        
        ApplyRevertGUI(); 
    }
}
