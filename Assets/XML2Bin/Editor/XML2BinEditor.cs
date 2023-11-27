using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using System;

namespace XMLUtil.ToBin
{
    [CustomEditor(typeof(TextAsset))]
    public class XML2BinEditor : Editor
    {
        private Editor _editor;
        private bool _isXml = false;

        public override void OnInspectorGUI()
        {
            GUI.enabled = true;
            
            GUILayout.BeginVertical();
            if (_isXml == true)
            {
                if (GUILayout.Button("Convert Bin"))
                {
                    Debug.Log("Test");
                }
            }
            GUILayout.Space(20);
            GUILayout.EndVertical();

            DrawDefaultGUI();
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

        private void CheckIsXml()
        {
            string path = AssetDatabase.GetAssetPath(target);
            _isXml = path.Contains(".xml");
        }

        private void OnEnable()
        {
            CheckIsXml();
        }
    }
}