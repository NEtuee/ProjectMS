using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using System;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace XMLUtil.ToBin
{
    [CustomEditor(typeof(TextAsset))]
    public class XML2BinEditor : Editor
    {
        private Editor _editor = null;
        private string _filePath = string.Empty;
        private bool _isXml = false;

        public override void OnInspectorGUI()
        {
            GUI.enabled = true;
            
            GUILayout.BeginVertical();
            if (_isXml == true)
            {
                if (GUILayout.Button("Convert Bin"))
                {
                    if (_filePath == string.Empty)
                    {
                        Debug.LogWarning($"Is Not Have xml File Path");
                        return;
                    }

                    CreateBinFile();
                }
            }
            GUILayout.Space(20);
            GUILayout.EndVertical();

            DrawDefaultGUI();
        }

        private void CreateBinFile()
        {
            string savePath = Path.GetDirectoryName(_filePath);
            string fileName = Path.GetFileNameWithoutExtension(_filePath);
            FileStream fileStream = new FileStream(savePath + $"/{fileName}.bin", FileMode.OpenOrCreate);
            
            using (BinaryWriter wr = new BinaryWriter(fileStream))
            {
                var textAsset = target as TextAsset;
                wr.Write(textAsset.bytes);
            }

            AssetDatabase.Refresh();
        }

        private void DrawDefaultGUI()
        {
            if (_editor == null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var a in assemblies)
                {
                    var type = a.GetType("UnityEditor.TextAssetInspector");
                    if (type != null)
                    {
                        _editor = Editor.CreateEditor(target, type);
                        break;
                    }
                }
            }

            if (_editor != null)
                _editor.OnInspectorGUI();
        }

        private void SetUpXml()
        {
            _filePath = AssetDatabase.GetAssetPath(target);
            _isXml = _filePath.Contains(".xml");
        }

        private void OnEnable()
        {
            SetUpXml();
        }
    }
}