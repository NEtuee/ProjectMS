using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class BuffStatusViewer : EditorWindow
{
    private static BuffStatusViewer _window;
    private BuffData _selectedData = null;

    [MenuItem("Tools/BuffStatusViewer", priority = 1)]
    private static void ShowWindow()
    {
        _window = (BuffStatusViewer)EditorWindow.GetWindow(typeof(BuffStatusViewer));
        _window.titleContent = new GUIContent("Buff Status Viewer");
        _window.Show();
    }

    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    private Vector2 _scrollPosition;
    private Vector2 _buffInfoScrollPosition;
    private Vector2 _entityStatusScrollPosition;
    private bool _entityInfoFoldout = true;
    private bool _buffInfoFoldout = true;

    void Update()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    public void OnGUI()
    {
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl("");
            Repaint();
        }

        _searchString = EditorGUILayout.TextField("Search", _searchString);
        if(_searchStringCompare != _searchString)
        {
            if(_searchString == "")
                _searchStringList = null;
            else
                _searchStringList = _searchString.Split(' ');

            _searchStringCompare = _searchString;
        }

        // 버프 데이터 가져오기
        Dictionary<int, BuffData> buffDataList = getBuffDataList();
        if(buffDataList == null)
        {
            EditorGUILayout.HelpBox("Buff 데이터가 로드되지 않았습니다. 게임을 실행하거나 데이터를 로드해주세요.", MessageType.Warning);
            return;
        }

        // 현재 선택된 Entity 정보 표시
        bool gamePlaying = Application.isPlaying;
        if(gamePlaying && GameEditorMaster._instance != null && GameEditorMaster._instance._currentlySelectedEntity != null)
        {
            DrawSelectedEntityInfo();
            EditorGUILayout.Space(10);
        }

        EditorGUILayout.LabelField("All Buff List", EditorStyles.boldLabel);
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "box");
        
        foreach(var item in buffDataList)
        {
            if(_searchString != "" && (searchStringCompare(item.Value._buffName) == false && searchStringCompare(item.Key.ToString()) == false))
                continue;

            GUILayout.BeginHorizontal("box");
            
            if(GUILayout.Button("Show", GUILayout.Width(45f)))
            {
                _selectedData = item.Value;
            }

            GUI.enabled = gamePlaying && GameEditorMaster._instance != null && GameEditorMaster._instance._currentlySelectedEntity != null && GameEditorMaster._instance._currentlySelectedEntity.isDead() == false;
            if(GUILayout.Button("Apply", GUILayout.Width(50f)))
            {
                // 선택된 캐릭터에게 버프 적용
                GameEditorMaster._instance._currentlySelectedEntity.getStatusInfo().applyBuff(item.Key);
            }
            GUI.enabled = true;

            GUILayout.Label($"[{item.Key}] {item.Value._buffName}");
            
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        if(_selectedData != null)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            _buffInfoFoldout = EditorGUILayout.Foldout(_buffInfoFoldout, "Buff Information", true, EditorStyles.foldoutHeader);
            if (_buffInfoFoldout)
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginVertical("box");

                DrawBuffInformation(_selectedData);

                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }
        }
    }

    private void DrawBuffInformation(BuffData data)
    {
        if (data == null) return;

        EditorGUILayout.LabelField("버프 이름:", data._buffName);
        EditorGUILayout.LabelField("버프 키:", data._buffKey.ToString());
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("동작 상세:", EditorStyles.boldLabel);

        // 버프 타입 설명
        string buffTypeDesc = "알 수 없음";
        switch (data._buffType)
        {
            case BuffType.Status:
                buffTypeDesc = "스탯 변경 버프";
                break;
            case BuffType.Defence:
                buffTypeDesc = "방어 타입 변경 버프";
                break;
        }
        EditorGUILayout.LabelField("버프 타입:", buffTypeDesc);

        // 업데이트 타입 설명
        string updateTypeDesc = "";
        switch (data._buffUpdateType)
        {
            case BuffUpdateType.OneShot:
                updateTypeDesc = "즉시 1회 적용";
                break;
            case BuffUpdateType.Time:
                updateTypeDesc = $"{data._buffCustomValue0}초 동안 지속";
                break;
            case BuffUpdateType.Continuous:
                updateTypeDesc = "지속적으로 적용";
                break;
            case BuffUpdateType.DelayedContinuous:
                updateTypeDesc = $"{data._buffCustomValue0}초 대기 후, {data._buffCustomValue1}초 간격으로 지속 적용";
                break;
            case BuffUpdateType.GreaterThenSet:
                updateTypeDesc = $"'{data._targetStatusName}' 스탯이 {data._buffCustomValue0}보다 크면 {data._buffCustomValue1}로 설정";
                break;
            case BuffUpdateType.StatSection:
                updateTypeDesc = $"'{data._buffCustomStatusName}' 스탯이 {data._buffCustomValue0} ~ {data._buffCustomValue1} 사이일 때 적용";
                break;
            case BuffUpdateType.ButtonHit:
                updateTypeDesc = "버튼 입력 시 적용";
                break;
        }
        EditorGUILayout.LabelField("업데이트 방식:", updateTypeDesc);

        // 적용 방식 설명
        if (data._buffType == BuffType.Status)
        {
            string applyTypeDesc = "";
            string valueDesc = data._buffVaryStatFactor != 0 ? $"{data._buffVaryStatFactor} 만큼" : $"{data._buffCustomValue0} 만큼";

            switch (data._buffApplyType)
            {
                case BuffApplyType.Direct:
                    applyTypeDesc = $"'{data._targetStatusName}' 스탯에 {valueDesc} 직접 더함";
                    break;
                case BuffApplyType.DirectDelta:
                    applyTypeDesc = $"'{data._targetStatusName}' 스탯에 {valueDesc} (델타값) 직접 더함";
                    break;
                case BuffApplyType.Additional:
                    applyTypeDesc = $"'{data._targetStatusName}' 스탯에 {valueDesc} 추가값으로 더함";
                    break;
                case BuffApplyType.DirectSet:
                    applyTypeDesc = $"'{data._targetStatusName}' 스탯을 {valueDesc}으로 설정";
                    break;
                case BuffApplyType.Empty:
                    applyTypeDesc = "값 변경 없음 (특수 로직)";
                    break;
            }
            EditorGUILayout.LabelField("적용 방식:", applyTypeDesc);
        }
        else if (data._buffType == BuffType.Defence)
        {
            EditorGUILayout.LabelField("적용 방식:", $"방어 타입을 '{data._defenceType}'(으)로 변경");
        }
        
        EditorGUILayout.LabelField("중첩 허용:", data._allowOverlap ? "예" : "아니오");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("이펙트 정보:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("시작 이펙트:", string.IsNullOrEmpty(data._buffStartEffectPreset) ? "없음" : data._buffStartEffectPreset);
        EditorGUILayout.LabelField("종료 이펙트:", string.IsNullOrEmpty(data._buffEndEffectPreset) ? "없음" : data._buffEndEffectPreset);
        EditorGUILayout.LabelField("파티클 이펙트:", string.IsNullOrEmpty(data._particleEffect) ? "없음" : data._particleEffect);
        EditorGUILayout.LabelField("타임라인 이펙트:", string.IsNullOrEmpty(data._timelineEffect) ? "없음" : data._timelineEffect);
        EditorGUILayout.LabelField("프리셋 이펙트:", string.IsNullOrEmpty(data._effectPreset) ? "없음" : data._effectPreset);
        EditorGUILayout.LabelField("오디오 ID:", data._audioID == -1 ? "없음" : data._audioID.ToString());
    }

    private Dictionary<int, BuffData> getBuffDataList()
    {
        // StatusInfo.cs의 private static _buffDataList에 리플렉션으로 접근
        FieldInfo buffDataListField = typeof(StatusInfo).GetField("_buffDataList", BindingFlags.NonPublic | BindingFlags.Static);
        
        if(buffDataListField != null)
        {
            BuffDataList buffDataList = buffDataListField.GetValue(null) as BuffDataList;
            return buffDataList?._buffDataList;
        }
        
        return null;
    }

    private bool searchStringCompare(string target)
    {
        if(_searchStringList == null || string.IsNullOrEmpty(target))
            return true;
            
        string lowerTarget = target.ToLower();
        foreach(var stringItem in _searchStringList)
        {
            if(lowerTarget.Contains(stringItem.ToLower()))
                return true;
        }

        return false;
    }

    private void DrawSelectedEntityInfo()
    {
        var selectedEntity = GameEditorMaster._instance._currentlySelectedEntity;
        if (selectedEntity == null) return;

        _entityInfoFoldout = EditorGUILayout.Foldout(_entityInfoFoldout, "Selected Entity Information", true, EditorStyles.foldoutHeader);
        if (!_entityInfoFoldout)
        {
            return;
        }

        GUILayout.BeginVertical("box");

        // Entity 기본 정보
        EditorGUILayout.LabelField("Entity Name: " + selectedEntity.name);
        EditorGUILayout.LabelField("Is Dead: " + selectedEntity.isDead());

        // Status 정보
        var statusInfo = selectedEntity.getStatusInfo();
        if (statusInfo != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Status Information", EditorStyles.boldLabel);

            // Status 값들 표시
            var statusValues = statusInfo.getStatusValues();
            if (statusValues != null)
            {
                foreach (var status in statusValues)
                {
                    EditorGUILayout.LabelField($"{status.Key}: {status.Value._value} (Real: {status.Value._realValue}, Add: {status.Value._additionalValue})");
                }
            }

            // 적용된 버프 목록
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Applied Buffs", EditorStyles.boldLabel);

            var appliedBuffs = statusInfo.getCurrentlyAppliedBuffList();
            if (appliedBuffs != null)
            {
                _entityStatusScrollPosition = GUILayout.BeginScrollView(_entityStatusScrollPosition, GUILayout.Height(150));

                if (appliedBuffs.Count > 0)
                {
                    for (int i = 0; i < appliedBuffs.Count; i++)
                    {
                        var buffItem = appliedBuffs[i];
                        if (buffItem?._buffData == null) continue;

                        GUILayout.BeginHorizontal("box");

                        var buffData = buffItem._buffData;
                        double remainingTime = (buffItem._startedTime + buffData._buffCustomValue0) - GlobalTimer.Instance().getScaledGlobalTime();
                        string durationText = buffData._buffCustomValue0 > 0 ? $"({remainingTime:F2}s)" : "(Infinite)";
                        
                        string displayText = $"[{buffData._buffKey}] {buffData._buffName} {durationText}";
                        
                        EditorGUILayout.LabelField(displayText);

                        // Remove 버튼
                        if (GUILayout.Button("Remove", GUILayout.Width(60f)))
                        {
                            statusInfo.deleteBuffIndex(i);
                            GUIUtility.ExitGUI(); // 리스트 변경 후 즉시 GUI 종료 및 재시작
                        }

                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No buffs applied");
                }

                GUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No applied buffs data");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No Status Info available");
        }

        GUILayout.EndVertical();
    }
}

