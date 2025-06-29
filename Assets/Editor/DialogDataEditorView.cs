using UnityEngine;
using UnityEditor;
using Codice.Utils;

[CustomEditor(typeof(DialogData))]
class DialogDataEditorView : Editor 
{
	public override void OnInspectorGUI() 
    {
        if(GUILayout.Button("Open Editor"))
            DialogEditor.Open(target as DialogData);
	}
}