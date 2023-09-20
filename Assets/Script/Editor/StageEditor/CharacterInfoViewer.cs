using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterInfoViewer : EditorWindow
{
    private static CharacterInfoViewer _window;
    private CharacterInfoData _selectedData = null;

    [MenuItem("Tools/CharacterInfoViewer", priority = 0)]
    private static void ShowWindow()
    {
        _window = (CharacterInfoViewer)EditorWindow.GetWindow(typeof(CharacterInfoViewer));
    }

    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    private Vector2 _scrollPosition;
    private Vector2 _characterInfoScrollPosition;
    private string _addedCharacterKey = "";

    private Texture _characterTexture = null;

    public void OnGUI()
    {
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl("");
            Repaint();
        }

        _searchString = EditorGUILayout.TextField("Search",_searchString);
        if(_searchStringCompare != _searchString)
        {
            if(_searchString == "")
                _searchStringList = null;
            else
                _searchStringList = _searchString.Split(' ');

            _searchStringCompare = _searchString;
        }

        const string kCharacterInfoPath = "Assets\\Data\\StaticData\\CharacterInfo.xml";
        Dictionary<string,CharacterInfoData> characterInfo = ResourceContainerEx.Instance().getCharacterInfo(kCharacterInfoPath);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,"box");
        bool gamePlaying = Application.isPlaying && SceneCharacterManager._managerInstance != null && StageProcessor.Instance() != null && StageProcessor.Instance().getPlayerEntity() != null;
        foreach(var item in characterInfo)
        {
            if(_searchString != "" && (searchStringCompare(item.Key) == false && searchStringCompare(item.Value._displayName) == false))
                continue;

            GUILayout.BeginHorizontal("box");
            
            if(GUILayout.Button("Show",GUILayout.Width(45f)))
            {
                _selectedData = item.Value;

                Sprite characterSprite = getFirstActionSpriteFromCharacter(item.Value);
                _characterTexture = characterSprite?.texture;
            }

            GUI.enabled = gamePlaying;
            if(GUILayout.Button("Add",GUILayout.Width(40f)))
            {
                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                SpawnCharacterOptionDesc spawnDesc = new SpawnCharacterOptionDesc();
                spawnDesc._direction = Vector3.right;
                spawnDesc._position = StageProcessor.Instance().getPlayerEntity().transform.position + Vector3.right * 0.5f;
                spawnDesc._rotation = Quaternion.identity;
                spawnDesc._searchIdentifier = SearchIdentifier.Enemy;
                
                CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(item.Value,spawnDesc);
            }

            if(GUILayout.Button("Random",GUILayout.Width(60f)))
            {
                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                SpawnCharacterOptionDesc spawnDesc = new SpawnCharacterOptionDesc();
                spawnDesc._direction = Vector3.right;
                spawnDesc._position = StageProcessor.Instance().getPlayerEntity().transform.position + CameraControlEx.Instance().getRandomPositionInCamera();
                spawnDesc._rotation = Quaternion.identity;
                spawnDesc._searchIdentifier = SearchIdentifier.Enemy;
                
                CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(item.Value,spawnDesc);
            }


            GUI.enabled = true;

            GUILayout.Label(item.Key + ": " + item.Value._displayName);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        if(_selectedData != null)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.BeginHorizontal("box");

            if(_characterTexture != null)
            {
                Rect rect = GUILayoutUtility.GetRect(_characterTexture.width, _characterTexture.height);

                EditorGUIUtility.ScaleAroundPivot(Vector3.one,rect.center);
                GUI.DrawTexture(rect, _characterTexture,ScaleMode.ScaleToFit);
            }

            GUILayout.BeginVertical();

            _characterInfoScrollPosition = GUILayout.BeginScrollView(_characterInfoScrollPosition);

            EditorGUILayout.LabelField(_selectedData._displayName);
            EditorGUILayout.LabelField("ActionGraph: " + _selectedData._actionGraphPath);
            EditorGUILayout.LabelField("AIGraph: " + _selectedData._aiGraphPath);
            
            EditorGUILayout.LabelField("Status: " + _selectedData._statusName);
            EditorGUILayout.LabelField("Radius: " + _selectedData._characterRadius);
            EditorGUILayout.LabelField("HeadUpOffset: " + _selectedData._headUpOffset);
            EditorGUILayout.LabelField("SearchIdentifier: " + _selectedData._searchIdentifer);

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
    }

    private Sprite getFirstActionSpriteFromCharacter(CharacterInfoData characterInfoData)
    {
        StaticDataLoader.loadStaticData();
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        AnimationPlayDataInfo playDataInfo = baseData._animationPlayData[baseData._actionNodeData[baseData._defaultActionIndex]._animationInfoIndex][0];

        Sprite[] sprites = ResourceContainerEx.Instance().GetSpriteAll(playDataInfo._path);
        if(sprites == null)
            return null;
        
        return sprites[0];
    }

    public string getAddedCharacter()
    {
        string characterKey = _addedCharacterKey;
        _addedCharacterKey = "";

        return characterKey;
    }

    private bool searchStringCompare(string target)
    {
        string lowerTarget = target.ToLower();
        foreach(var stringItem in _searchStringList)
        {
            if(lowerTarget.Contains(stringItem))
                return true;
        }

        return false;
    }

}
