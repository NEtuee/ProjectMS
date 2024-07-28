using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using TMPro;

public abstract class SerializableDataType
{
#if UNITY_EDITOR
	public abstract void serialize(ref BinaryWriter binaryWriter);

	public void writeData(string binaryOutputPath)
	{
		FileStream fileStream = new FileStream(binaryOutputPath, FileMode.OpenOrCreate, FileAccess.Write);
        BinaryWriter binaryWriter = new BinaryWriter(fileStream);

        serialize(ref binaryWriter);

        binaryWriter.Close();
        fileStream.Close();
	}
#endif

	public abstract void deserialize(ref BinaryReader binaryReader);
	public void readData(string path)
	{
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);

        deserialize(ref binaryReader);

        binaryReader.Close();
        fileStream.Close();
	}

}

public interface SerializableStructure
{
#if UNITY_EDITOR
	public abstract void serialize(ref BinaryWriter binaryWriter);
#endif
	public abstract void deserialize(ref BinaryReader binaryReader);
}

public class ManagedResourceItem<Value> where Value : class
{
	public Dictionary<string, Value> _singleResourceContainer = new Dictionary<string, Value>();
	public Dictionary<string, Value[]> _multiResourceContainer = new Dictionary<string, Value[]>();
	public Type _resourceType = typeof(Value);
	
	public Value GetOrLoadResource(string path)
	{
		if(_singleResourceContainer.ContainsKey(path))
			return _singleResourceContainer[path];
		
		Value obj = Load(path,GetResourceType()) as Value;
		if(obj == null)
		{
            DebugUtil.assert(false, "file does not exist : {0}",path);
			return null;
		}

		_singleResourceContainer.Add(path,obj);
		return obj;
	}

	public Value[] GetOrLoadResources(string path, string additionalMessage)
	{
		if(_multiResourceContainer.ContainsKey(path))
			return _multiResourceContainer[path];

		UnityEngine.Object[] obj = LoadAll(path, GetResourceType());
		if(obj.Length == 0)
		{
			DebugUtil.assert(false, additionalMessage + "file does not exist : {0}",path);
			return null;
		}

		Value[] items = new Value[obj.Length];
		for(int i = 0; i < obj.Length; ++i)
		{
			items[i] = obj[i] as Value;
		}

		_multiResourceContainer.Add(path,items);

		return items;
	}

	public Type GetResourceType()
	{
		return _resourceType;
	}

	public UnityEngine.Object Load(string path, Type type)
	{
		return Resources.Load(path, type);
	}

	public UnityEngine.Object[] LoadAll(string path, Type type)
	{
		return Resources.LoadAll(path,type);
	}
}

public class DataResourceItem<Value, Loader> where Value : SerializableDataType where Loader : LoaderBase<Value>, new()
{
	class ValueWithTimeStamp
	{
		public Value 		_value;
		public DateTime 	_timeStamp;
	}

#if UNITY_EDITOR
	private Dictionary<string, ValueWithTimeStamp> _resourceContainer = new Dictionary<string, ValueWithTimeStamp>();
#else
	private Dictionary<string, Value> _resourceContainer = new Dictionary<string, Value>();
#endif

	private Loader loader = new Loader();

	public Value GetOrLoadResource(string path)
	{
#if UNITY_EDITOR
		DateTime timeStamp = getTimeStamp(path);
		if(_resourceContainer.ContainsKey(path))
		{
			if(_resourceContainer[path]._timeStamp == timeStamp)
				return _resourceContainer[path]._value;
		}
#else
		if(_resourceContainer.ContainsKey(path))
			return _resourceContainer[path];
#endif
		//try
		{
			string binaryPath = ResourceMap.Instance().findResourcePath(path);

#if UNITY_EDITOR
			loader.readFromXMLAndExportToBinary(path, binaryPath);
			Value obj = loader.readFromBinary(binaryPath);

			if(obj == null)
				return null;
			if(_resourceContainer.ContainsKey(path))
			{
				ValueWithTimeStamp item = _resourceContainer[path];
				item._value = obj;
				item._timeStamp = timeStamp;
			}
			else
			{
				_resourceContainer.Add(path,new ValueWithTimeStamp(){_value = obj, _timeStamp = timeStamp});
			}
#else
			Value obj = loader.readFromBinary(binaryPath);
			_resourceContainer.Add(path,obj);
#endif
			return obj;
		}
	}

	private DateTime getTimeStamp(string path)
	{
        if (File.Exists(path) == false)
            return DateTime.MinValue;

        return File.GetLastWriteTime(path);
	}
}

public class ResourceContainerEx : Singleton<ResourceContainerEx>
{
    private ManagedResourceItem<Sprite> 				_spriteResource = new ManagedResourceItem<Sprite>();
	private ManagedResourceItem<ScriptableObject> 		_scriptableResource = new ManagedResourceItem<ScriptableObject>();
	private ManagedResourceItem<AnimationCustomPreset> 	_animationCustomPresetResource = new ManagedResourceItem<AnimationCustomPreset>();
	private ManagedResourceItem<GameObject>		 		_prefabResource = new ManagedResourceItem<GameObject>();
	private ManagedResourceItem<Material>		 		_materialResource = new ManagedResourceItem<Material>();
	private ManagedResourceItem<StageData>		 		_stageDataResource = new ManagedResourceItem<StageData>();
	private ManagedResourceItem<TMP_FontAsset>		 	_fontAssetResource = new ManagedResourceItem<TMP_FontAsset>();

	private DataResourceItem<ActionGraphBaseData,ActionGraphLoader>				_actionGraphResource = new DataResourceItem<ActionGraphBaseData,ActionGraphLoader>();
	private DataResourceItem<AIGraphBaseData,AIGraphLoader>						_aiGraphResource = new DataResourceItem<AIGraphBaseData,AIGraphLoader>();
	private DataResourceItem<ProjectileGraphBaseDataList,ProjectileGraphLoader>	_projectileGraphResource = new DataResourceItem<ProjectileGraphBaseDataList,ProjectileGraphLoader>();
	private DataResourceItem<DanmakuGraphBaseData,DanmakuGraphLoader>			_danmakuGraphResource = new DataResourceItem<DanmakuGraphBaseData,DanmakuGraphLoader>();
	private DataResourceItem<SequencerGraphBaseData,SequencrGraphLoader>		_sequencerGraphResource = new DataResourceItem<SequencerGraphBaseData,SequencrGraphLoader>();
	private DataResourceItem<CharacterInfoDataList,CharacterInfoLoader>			_characterInfoResource = new DataResourceItem<CharacterInfoDataList,CharacterInfoLoader>();
	private DataResourceItem<AllyInfoDataList,AllyInfoLoader>					_allyInfoResource = new DataResourceItem<AllyInfoDataList,AllyInfoLoader>();


	public AnimationCustomPreset GetAnimationCustomPreset(string filePath)
	{
		AnimationCustomPreset[] customPresets = _animationCustomPresetResource.GetOrLoadResources(filePath,"해당 폴더가 없거나, 폴더 안에 Custom Preset이 없습니다. 경로를 잘못 쓰지는 않았나요? ");
		return customPresets == null ? null : customPresets[0];
	}

	public ScriptableObject GetScriptableObject(string fileName)
	{
		return _scriptableResource.GetOrLoadResource(fileName);
	}

	public ScriptableObject[] GetScriptableObjects(string fileName)
	{
		return _scriptableResource.GetOrLoadResources(fileName, "해당 폴더가 없거나, 폴더 안에 ScriptableObject가 없습니다. 경로를 잘못 쓰지는 않았나요? ");
	}

	public Sprite GetSprite(string fileName)
	{
		return _spriteResource.GetOrLoadResource(fileName);
	}

	public Sprite[] GetSpriteAll(string folderName)
	{
		string cut = folderName;
		if(cut.Contains("Resources"))
			cut = cut.Substring(folderName.IndexOf("Resources") + 10);
		
		return _spriteResource.GetOrLoadResources(cut, "해당 폴더가 없거나, 폴더 안에 Sprite가 없습니다. 경로를 잘못 쓰지는 않았나요? ");
	}

	public GameObject GetPrefab(string fileName)
	{
		return _prefabResource.GetOrLoadResource(fileName);
	}
	
	public Material GetMaterial(string fileName)
	{
		return _materialResource.GetOrLoadResource(fileName);
	}

	public StageData GetStageData(string fileName)
	{
		return _stageDataResource.GetOrLoadResource(fileName);
	}

	public StageData[] GetStageDataAll(string filePath)
	{
		return _stageDataResource.GetOrLoadResources(filePath,"StageData not found");
	}

	public TMP_FontAsset getFont(string fileName)
	{
		return _fontAssetResource.GetOrLoadResource(fileName);
	}

	public ActionGraphBaseData GetActionGraph(string path)
	{
		return _actionGraphResource.GetOrLoadResource(path);
	}

	public AIGraphBaseData GetAIGraph(string path)
	{
		return _aiGraphResource.GetOrLoadResource(path);
	}

	public SequencerGraphBaseData GetSequencerGraph(string path)
	{
		return _sequencerGraphResource.GetOrLoadResource(path);
	}

	public DanmakuGraphBaseData GetDanmakuGraph(string path)
	{
		return _danmakuGraphResource.GetOrLoadResource(path);
	}

	public ProjectileGraphBaseData[] GetProjectileGraphBaseData(string path)
	{
		return _projectileGraphResource.GetOrLoadResource(path)._projectileGraphBaseDataList;
	}

	public ProjectileGraphBaseDataList GetProjectileGraphBaseDataList(string path)
	{
		return _projectileGraphResource.GetOrLoadResource(path);
	}

	public Dictionary<string, CharacterInfoData> getCharacterInfo(string path)
	{
		return _characterInfoResource.GetOrLoadResource(path)._characterInfoDataDic;
	}

	public CharacterInfoDataList getCharacterInfoList(string path)
	{
		return _characterInfoResource.GetOrLoadResource(path);
	}

	public Dictionary<string,AllyInfoData> getAllyInfo(string path)
	{
		return _allyInfoResource.GetOrLoadResource(path)._allyInfoDataDic;
	}

	public AllyInfoDataList GetAllyInfoDataList(string path)
	{
		return _allyInfoResource.GetOrLoadResource(path);
	}

	public static TextAsset readTextAsset(string path)
	{
		return Resources.Load(path) as TextAsset;
	}

	public void UnLoadUnused()
	{
		Resources.UnloadUnusedAssets();
	}

	public void UnLoad(UnityEngine.Object obj)
	{
		Resources.UnloadAsset(obj);
	}
}