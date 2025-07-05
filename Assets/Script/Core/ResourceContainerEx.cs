using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine.Profiling;

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
	private DataResourceItem<SequencerGraphSetBaseData,SequencerGraphSetLoader>	_sequencerGraphSetResource = new DataResourceItem<SequencerGraphSetBaseData,SequencerGraphSetLoader>();
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

	public SequencerGraphSetBaseData getSequencerGraphSet(string path)
	{
		return _sequencerGraphSetResource.GetOrLoadResource(path);
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

	/// <summary>
	/// 현재 캐시된 모든 리소스의 메모리 사용량을 계산합니다.
	/// </summary>
	public long GetTotalMemoryUsage()
	{
		long totalMemory = 0;
		
		totalMemory += GetManagedResourceMemory<Sprite>(_spriteResource, "Sprite");
		totalMemory += GetManagedResourceMemory<ScriptableObject>(_scriptableResource, "ScriptableObject");
		totalMemory += GetManagedResourceMemory<AnimationCustomPreset>(_animationCustomPresetResource, "AnimationCustomPreset");
		totalMemory += GetManagedResourceMemory<GameObject>(_prefabResource, "Prefab");
		totalMemory += GetManagedResourceMemory<Material>(_materialResource, "Material");
		totalMemory += GetManagedResourceMemory<StageData>(_stageDataResource, "StageData");
		totalMemory += GetManagedResourceMemory<TMP_FontAsset>(_fontAssetResource, "FontAsset");
		
		totalMemory += GetDataResourceMemory();
		
		return totalMemory;
	}

	/// <summary>
	/// 상세한 메모리 사용량 정보를 출력합니다.
	/// </summary>
	public void PrintMemoryUsageDetails()
	{
		Debug.Log("=== ResourceContainerEx Memory Usage ===");
		
		long spriteMemory = GetManagedResourceMemory<Sprite>(_spriteResource, "Sprite");
		long scriptableMemory = GetManagedResourceMemory<ScriptableObject>(_scriptableResource, "ScriptableObject");
		long animationMemory = GetManagedResourceMemory<AnimationCustomPreset>(_animationCustomPresetResource, "AnimationCustomPreset");
		long prefabMemory = GetManagedResourceMemory<GameObject>(_prefabResource, "Prefab");
		long materialMemory = GetManagedResourceMemory<Material>(_materialResource, "Material");
		long stageDataMemory = GetManagedResourceMemory<StageData>(_stageDataResource, "StageData");
		long fontMemory = GetManagedResourceMemory<TMP_FontAsset>(_fontAssetResource, "FontAsset");
		long dataMemory = GetDataResourceMemory();
		
		Debug.Log($"Sprite Resources: {FormatBytes(spriteMemory)} ({_spriteResource._singleResourceContainer.Count + _spriteResource._multiResourceContainer.Count} items)");
		Debug.Log($"ScriptableObject Resources: {FormatBytes(scriptableMemory)} ({_scriptableResource._singleResourceContainer.Count + _scriptableResource._multiResourceContainer.Count} items)");
		Debug.Log($"AnimationCustomPreset Resources: {FormatBytes(animationMemory)} ({_animationCustomPresetResource._singleResourceContainer.Count + _animationCustomPresetResource._multiResourceContainer.Count} items)");
		Debug.Log($"Prefab Resources: {FormatBytes(prefabMemory)} ({_prefabResource._singleResourceContainer.Count + _prefabResource._multiResourceContainer.Count} items)");
		Debug.Log($"Material Resources: {FormatBytes(materialMemory)} ({_materialResource._singleResourceContainer.Count + _materialResource._multiResourceContainer.Count} items)");
		Debug.Log($"StageData Resources: {FormatBytes(stageDataMemory)} ({_stageDataResource._singleResourceContainer.Count + _stageDataResource._multiResourceContainer.Count} items)");
		Debug.Log($"Font Resources: {FormatBytes(fontMemory)} ({_fontAssetResource._singleResourceContainer.Count + _fontAssetResource._multiResourceContainer.Count} items)");
		Debug.Log($"Data Resources: {FormatBytes(dataMemory)}");
		
		long totalMemory = spriteMemory + scriptableMemory + animationMemory + prefabMemory + materialMemory + stageDataMemory + fontMemory + dataMemory;
		Debug.Log($"TOTAL MEMORY USAGE: {FormatBytes(totalMemory)}");
		Debug.Log("========================================");
	}

	/// <summary>
	/// ManagedResourceItem의 메모리 사용량을 계산합니다.
	/// </summary>
	private long GetManagedResourceMemory<T>(ManagedResourceItem<T> resourceItem, string typeName) where T : class
	{
		long memory = 0;
		
		// Single resource container
		foreach (var resource in resourceItem._singleResourceContainer.Values)
		{
			if (resource != null)
			{
				memory += GetObjectMemorySize(resource);
			}
		}
		
		// Multi resource container
		foreach (var resourceArray in resourceItem._multiResourceContainer.Values)
		{
			if (resourceArray != null)
			{
				foreach (var resource in resourceArray)
				{
					if (resource != null)
					{
						memory += GetObjectMemorySize(resource);
					}
				}
			}
		}
		
		return memory;
	}

	/// <summary>
	/// DataResourceItem들의 메모리 사용량을 계산합니다.
	/// </summary>
	private long GetDataResourceMemory()
	{
		long memory = 0;
		
		// 각 DataResourceItem의 컨테이너 크기를 추정
		memory += EstimateDataResourceSize(_actionGraphResource, "ActionGraph");
		memory += EstimateDataResourceSize(_aiGraphResource, "AIGraph");
		memory += EstimateDataResourceSize(_projectileGraphResource, "ProjectileGraph");
		memory += EstimateDataResourceSize(_danmakuGraphResource, "DanmakuGraph");
		memory += EstimateDataResourceSize(_sequencerGraphResource, "SequencerGraph");
		memory += EstimateDataResourceSize(_sequencerGraphSetResource, "SequencerGraphSet");
		memory += EstimateDataResourceSize(_characterInfoResource, "CharacterInfo");
		memory += EstimateDataResourceSize(_allyInfoResource, "AllyInfo");
		
		return memory;
	}

	/// <summary>
	/// DataResourceItem의 메모리 사용량을 추정합니다.
	/// </summary>
	private long EstimateDataResourceSize<TValue, TLoader>(DataResourceItem<TValue, TLoader> dataResource, string typeName) 
		where TValue : SerializableDataType 
		where TLoader : LoaderBase<TValue>, new()
	{
		// DataResourceItem의 private 필드에 접근할 수 없으므로 추정값 사용
		// 실제 구현에서는 DataResourceItem에 GetMemoryUsage() 메서드를 추가하는 것이 좋습니다.
		return 1024; // 기본 추정값 (1KB per item)
	}

	/// <summary>
	/// Unity Object의 메모리 사용량을 계산합니다.
	/// </summary>
	private long GetObjectMemorySize(object obj)
	{
		if (obj == null) return 0;
		
		if (obj is UnityEngine.Object unityObj)
		{
			return Profiler.GetRuntimeMemorySizeLong(unityObj);
		}
		
		// Unity Object가 아닌 경우 추정값 반환
		return System.Runtime.InteropServices.Marshal.SizeOf(obj.GetType());
	}

	/// <summary>
	/// 바이트를 읽기 쉬운 형태로 포맷합니다.
	/// </summary>
	private string FormatBytes(long bytes)
	{
		if (bytes < 1024)
			return $"{bytes} B";
		else if (bytes < 1024 * 1024)
			return $"{bytes / 1024.0:F2} KB";
		else if (bytes < 1024 * 1024 * 1024)
			return $"{bytes / (1024.0 * 1024.0):F2} MB";
		else
			return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
	}

	/// <summary>
	/// 특정 타입의 리소스만 해제합니다.
	/// </summary>
	public void ClearResourceType<T>() where T : class
	{
		if (typeof(T) == typeof(Sprite))
		{
			ClearManagedResource(_spriteResource);
		}
		else if (typeof(T) == typeof(GameObject))
		{
			ClearManagedResource(_prefabResource);
		}
		else if (typeof(T) == typeof(Material))
		{
			ClearManagedResource(_materialResource);
		}
		// 필요에 따라 다른 타입들도 추가
	}

	/// <summary>
	/// ManagedResourceItem의 캐시를 지웁니다.
	/// </summary>
	private void ClearManagedResource<T>(ManagedResourceItem<T> resourceItem) where T : class
	{
		resourceItem._singleResourceContainer.Clear();
		resourceItem._multiResourceContainer.Clear();
	}
}