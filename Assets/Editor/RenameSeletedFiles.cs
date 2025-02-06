using UnityEngine;
using UnityEditor;
using System.IO;

public class RenameSelectedFiles : EditorWindow
{
    private string searchWord = "";
    private string replaceWord = "";

    [MenuItem("Tools/Rename Selected Files")]
    public static void ShowWindow()
    {
        GetWindow<RenameSelectedFiles>("Rename Files");
    }

    private void OnGUI()
    {
        GUILayout.Label("Rename Files in Project", EditorStyles.boldLabel);

        searchWord = EditorGUILayout.TextField("Search Word", searchWord);
        replaceWord = EditorGUILayout.TextField("Replace Word", replaceWord);

        if (GUILayout.Button("Execute"))
        {
            RenameFiles();
        }
    }

    private void RenameFiles()
    {
        foreach (Object obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath)) continue;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);
            string directory = Path.GetDirectoryName(assetPath);

            if (fileName.Contains(searchWord))
            {
                string newFileName = fileName.Replace(searchWord, replaceWord);
                string newPath = Path.Combine(directory, newFileName + extension);

                AssetDatabase.RenameAsset(assetPath, newFileName);
                Debug.Log($"Renamed: {fileName}{extension} -> {newFileName}{extension}");
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
