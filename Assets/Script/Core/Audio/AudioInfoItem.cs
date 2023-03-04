using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AudioInfoItem", order = 1)]
public class AudioInfoItem : ScriptableObject
{
    [System.Serializable]
    public class AudioParameter
    {
        public string name;
        public int id;
        public float min;
        public float max;
    }
    [System.Serializable]
    public class AudioInfo
    {
        public string path;
        public string name;
        public int id;
        public float defaultVolume = 1f;
        public bool overrideAttenuation = false;
        public Vector3 overrideDistance;
        public string type;
        public List<AudioParameter> parameters;

        public AudioParameter FindParameter(int id)
        {
            return parameters.Find(x=> x.id == id);
        }

        public AudioInfo()
        {
            parameters = new List<AudioParameter>();
        }

    };

    public TextAsset csvData;

    [SerializeField]
    public List<AudioInfo> audioData;

    public AudioInfo FindAudio(int id)
    {
        return audioData.Find(x => x.id == id);
    }

#if UNITY_EDITOR
    public void CreateInfoFromCSV()
    {
        CreateInfoFromCSV(csvData);
    }


    public void CreateInfoFromCSV(TextAsset csv)
    {
        var saveData = new List<AudioInfo>();
        //audioData = new List<AudioInfo>();
        IOControl.ReadRangeFromCSV(csv.text,1,-1,0,2,out var data);
        IOControl.ReadRangeFromCSV(csv.text,1,-1,4,8,out var param);

        var globalData = new AudioInfo();
        globalData.id = 0;
        globalData.path = "global data";
        globalData.type = "Global";

        saveData.Add(globalData);
        
        if(data != null && param != null)
        {
            foreach(var d in data)
            {
                if(d[0] == "")
                    continue;

                var item = new AudioInfo();
                item.id = int.Parse(d[0]);
                item.path = d[1];
                var split = item.path.Split('/');
                item.name = split[split.Length - 1];
                item.type = d[2];


                if(audioData != null)
                {
                    var audio = FindAudio(item.id);
                    if(audio != null)
                    {
                        item.defaultVolume = audio.defaultVolume;
                        item.overrideAttenuation = audio.overrideAttenuation;
                        item.overrideDistance = audio.overrideDistance;
                    }
                }


                saveData.Add(item);
            }

            foreach(var p in param)
            {
                if(p[0] == "")
                    continue;

                int group = int.Parse(p[0]);
                int id = int.Parse(p[1]);
                float min = float.Parse(p[3]);
                float max = float.Parse(p[4]);

                var find = saveData.Find(x=> x.id == group);

                if(find == null)
                {
                    Debug.Log("parameter group id not found");
                    Debug.Log("group id : " + group + " name : " + p[2]);
                    break;
                }

                var item = new AudioParameter();
                item.id = id;
                item.name = p[2];
                item.min = min;
                item.max = max;

                find.parameters.Add(item);
            }
        }
        else
        {
            Debug.Log("file error");
        }

        saveData.Sort((x,y)=>{return x.id > y.id ? 1 : (x.id < y.id ? -1 : 0);});

        audioData = new List<AudioInfo>(saveData);
        EditorUtility.SetDirty(this);
    }
#endif

}
