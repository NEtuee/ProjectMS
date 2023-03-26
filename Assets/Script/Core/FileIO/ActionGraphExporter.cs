using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class ActionGraphLoader : LoaderBase<ActionGraphBaseData>
{
    private static Dictionary<string, string> _globalVariables = new Dictionary<string, string>();

    private static string _currentFileName = "";
    PositionXmlDocument _xmlDoc = null;
    public override ActionGraphBaseData readFromXML(string path)
    {
        _currentFileName = path;
        _xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(XMLScriptConverter.convertXMLScriptSymbol(path),readerSettings))
            {
                _xmlDoc.Load(reader);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml load exception : {0}",ex.Message);
            return null;
        }
        
        if(_xmlDoc.HasChildNodes == false)
        {
            DebugUtil.assert(false,"xml is empty");
            return null;
        }

        Dictionary<string, XmlNodeList> branchSetDic = new Dictionary<string, XmlNodeList>();

        XmlNode node = _xmlDoc.FirstChild;
        
        if(node.Name.Equals("ActionGraph") == false)
        {
            DebugUtil.assert(false,"wrong xml type. name : {0} [FileName {1}]",node.Name, _currentFileName);
            return null;
        }
        
        float defaultFramePerSecond = 0f;
        string defaultActionName = "";

        ActionGraphBaseData actionBaseData = new ActionGraphBaseData();
        ReadTitle(node,actionBaseData,out defaultFramePerSecond, out defaultActionName, path);

        List<ActionGraphNodeData> nodeDataList = new List<ActionGraphNodeData>();
        List<ActionGraphBranchData> branchDataList = new List<ActionGraphBranchData>();
        List<ActionGraphConditionCompareData> compareDataList = new List<ActionGraphConditionCompareData>();
        List<AnimationPlayDataInfo[]> animationDataList = new List<AnimationPlayDataInfo[]>();

        _globalVariables.Clear();
        Dictionary<ActionGraphBranchData, string> actionCompareDic = new Dictionary<ActionGraphBranchData, string>();
        Dictionary<string, int> actionIndexDic = new Dictionary<string, int>();
        XmlNodeList nodeList = node.ChildNodes;

        int actionIndex = 0;
        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "BranchSet")
            {
                readBranchSet(nodeList[i],ref branchSetDic, path);
                continue;
            }
            else if(nodeList[i].Name == "GlobalVariable")
            {
                readGlobalVariable(nodeList[i], ref _globalVariables, path);
                continue;
            }
            
            ActionGraphNodeData nodeData = ReadAction(nodeList[i],defaultFramePerSecond, ref animationDataList, ref actionCompareDic, ref branchDataList,ref compareDataList, in branchSetDic, path);
            if(nodeData == null)
            {
                DebugUtil.assert(false,"node data is null : [NodeName: {0}] [Line: {1}] [FileName: {2}]",nodeList[i].Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                return null;
            }

            nodeDataList.Add(nodeData);
            actionIndexDic.Add(nodeData._nodeName,actionIndex++);
        }

        foreach(var item in actionCompareDic)
        {
            if(actionIndexDic.ContainsKey(item.Value) == false)
            {
                DebugUtil.assert(false,"target action is not exists : {0} [FileName: {1}]",item.Value, _currentFileName);
                return null;
            }
            else if(item.Value == defaultActionName)
            {
                actionBaseData._defaultActionIndex = actionIndexDic[item.Value];
            }

            item.Key._branchActionIndex = actionIndexDic[item.Value];
        }

        if(actionBaseData._defaultActionIndex == -1)
        {
            if(actionIndexDic.ContainsKey(defaultActionName) == false)
            {
                DebugUtil.assert(false, "invalid default action name: {0} [FileName: {1}]",defaultActionName, _currentFileName);
                return null;
            }

            actionBaseData._defaultActionIndex = actionIndexDic[defaultActionName];
        }

        actionBaseData._actionNodeCount = nodeDataList.Count;
        actionBaseData._branchCount = branchDataList.Count;
        actionBaseData._conditionCompareDataCount = compareDataList.Count;
        actionBaseData._animationPlayDataCount = animationDataList.Count;

        actionBaseData._actionNodeData = nodeDataList.ToArray();
        actionBaseData._branchData = branchDataList.ToArray();
        actionBaseData._conditionCompareData = compareDataList.ToArray();
        actionBaseData._animationPlayData = animationDataList.ToArray();

        actionBaseData._actionIndexMap = actionIndexDic;

        return actionBaseData;
    }

    private static void readGlobalVariable(XmlNode node, ref Dictionary<string, string> targetDic, string filePath)
    {
        string name = "";
        string value = "";
        for(int i = 0; i < node.Attributes.Count; ++i)
        {
            if(node.Attributes[i].Name == "Name")
                name = node.Attributes[i].Value;
            else if(node.Attributes[i].Name == "Value")
                value = node.Attributes[i].Value;
        }

        if(name == "" || value == "" || name.Contains("gv_") == false )
        {
            DebugUtil.assert(false, "invalid globalVariable, [name: {0}] [value: {1}] [Line: {2}] [FileName: {3}]",name,value, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return;
        }

        targetDic.Add(name,value);
    }

    public static Dictionary<string, string> getGlobalVariableContainer()
    {
        return _globalVariables;
    }

    public static string getGlobalVariable(string value)
    {
        if(_globalVariables.ContainsKey(value))
            return _globalVariables[value];

        return value;
    }


    public static string getGlobalVariable(string value, Dictionary<string,string> globalVariableContainer)
    {
        if(globalVariableContainer.ContainsKey(value))
            return globalVariableContainer[value];

        return value;
    }

    public static void readBranchSet(XmlNode branchSetParent, ref Dictionary<string, XmlNodeList> targetDic, string filePath)
    {
        string branchSetName = "";
        XmlAttributeCollection branchSetAttr = branchSetParent.Attributes;
        for(int attrIndex = 0; attrIndex < branchSetAttr.Count; ++attrIndex)
        {
            string targetName = branchSetAttr[attrIndex].Name;
            string targetValue = branchSetAttr[attrIndex].Value;

            if(targetName == "Name")
            {
                branchSetName = targetValue;
            }
        }

        if(branchSetName == "")
        {
            DebugUtil.assert(false, "branchSet name can not be Empty");
            return;
        }

        if(branchSetParent.ChildNodes.Count == 0)
        {
            DebugUtil.assert(false, "branchSet is empty : [Name: {0}] [Line: {1}] [FileName: {2}]",branchSetName, XMLScriptConverter.getLineFromXMLNode(branchSetParent), filePath);
            return;
        }

        targetDic.Add(branchSetName,branchSetParent.ChildNodes);
    }

    private static ActionGraphNodeData ReadAction(XmlNode node, float defaultFPS, ref List<AnimationPlayDataInfo[]> animationDataList,  ref Dictionary<ActionGraphBranchData, string> actionCompareDic,ref List<ActionGraphBranchData> branchDataList, ref List<ActionGraphConditionCompareData> compareDataList, in Dictionary<string, XmlNodeList> branchSetDic, string filePath)
    {
        ActionGraphNodeData nodeData = new ActionGraphNodeData();
        nodeData._nodeName = node.Name;

        //action attribute
        XmlAttributeCollection actionAttributes = node.Attributes;
        if(actionAttributes == null)
        {
            Debug.Log(node.Name);
            return null;
        }

        float actionTime = -1f;

        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value,_globalVariables);

            if(targetName == "MovementType")
            {
                nodeData._movementType = (MovementBase.MovementType)System.Enum.Parse(typeof(MovementBase.MovementType), targetValue);
            }
            else if(targetName == "DirectionType")
            {
                nodeData._directionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), targetValue);
            }
            else if(targetName == "DefenceDirectionType")
            {
                nodeData._defenceDirectionType = (DefenceDirectionType)System.Enum.Parse(typeof(DefenceDirectionType), targetValue);
            }
            else if(targetName == "MovementGraphPreset")
            {
                MovementGraphPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\MovementGraphPreset") as MovementGraphPreset;
                nodeData._movementGraphPresetData = preset.getPresetData(targetValue);
            }
            else if(targetName == "FlipType")
            {
                nodeData._flipType = (FlipType)System.Enum.Parse(typeof(FlipType), targetValue);
            }
            else if(targetName == "MoveScale")
            {
                nodeData._moveScale = float.Parse(targetValue);
            }
            else if(targetName == "IsActionSelection")
            {
                nodeData._isActionSelection = bool.Parse(targetValue);
            }
            else if(targetName == "RotationType")
            {
                nodeData._rotationType = (RotationType)System.Enum.Parse(typeof(RotationType), targetValue);
            }
            else if(targetName == "DefenceType")
            {
                nodeData._defenceType = (DefenceType)System.Enum.Parse(typeof(DefenceType), targetValue);
            }
            else if(targetName == "DefenceAngle")
            {
                nodeData._defenceAngle = float.Parse(targetValue);
            }
            else if(targetName == "DirectionAngle")
            {
                nodeData._additionalDirectionAngle = float.Parse(targetValue);
            }
            else if(targetName == "ApplyBuff")
            {
                string[] buffList = targetValue.Split(' ');
                nodeData._applyBuffList = new int[buffList.Length];

                for(int i = 0; i < buffList.Length; ++i)
                {
                    bool parse = int.TryParse(buffList[i],out int buffKey);
                    if(parse == false)
                        buffKey = StatusInfo.getBuffKeyFromName(buffList[i]);

                    if(buffKey == -1)
                    {
                        DebugUtil.assert(false, "invalidBuff : {0} [Line: {1}] [FileName: {2}]", buffList[i], XMLScriptConverter.getLineFromXMLNode(node), filePath);
                        continue;
                    }

                    nodeData._applyBuffList[i] = buffKey;
                }
            }
            else if(targetName == "NormalizedSpeed")
            {
                nodeData._normalizedSpeed = bool.Parse(targetValue);
            }
            else if(targetName == "RotateBySpeed")
            {
                nodeData._rotateBySpeed = bool.Parse(targetValue);
            }
            else if(targetName == "RotateSpeed")
            {
                nodeData._rotateSpeed = float.Parse(targetValue);
            }
            else if(targetName == "DirectionUpdateOnce")
            {
                nodeData._directionUpdateOnce = bool.Parse(targetValue);
            }
            else if(targetName == "FlipTypeUpdateOnce")
            {
                nodeData._flipTypeUpdateOnce = bool.Parse(targetValue);
            }
            else if(targetName == "HasAngleSector")
            {
                nodeData._hasAngleSector = bool.Parse(targetValue);
            }
            else if(targetName == "AngleSectorCount")
            {
                nodeData._angleSectorCount = int.Parse(targetValue);
            }
            else if(targetName == "Time")
            {
                actionTime = float.Parse(targetValue);
            }
            else
            {
                DebugUtil.assert(false,"invalid attribute type !!! : {0} [Line: {1}] [FileName: {2}]", targetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            }
        }

        XmlNodeList nodeList = node.ChildNodes;
        int branchStartIndex = branchDataList.Count;

        List<AnimationPlayDataInfo> animationPlayDataInfoList = new List<AnimationPlayDataInfo>();

        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "Animation")
            {
                AnimationPlayDataInfo animationData = ReadActionAnimation(nodeList[i],defaultFPS, filePath, actionTime);
                nodeData._animationInfoIndex = animationDataList.Count;
                animationData._hasMovementGraph = nodeData._movementType == MovementBase.MovementType.RootMotion;
                animationPlayDataInfoList.Add(animationData);
            }
            else if(nodeList[i].Name == "Branch")
            {
                ActionGraphBranchData branchData = ReadActionBranch(nodeList[i],ref actionCompareDic,ref compareDataList, ref _globalVariables, filePath);
                if(branchData == null)
                {
                    DebugUtil.assert(false,"invalid branch data [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    return null;
                }
                    
                    
                branchDataList.Add(branchData);
            }
            else if(nodeList[i].Name == "UseBranchSet")
            {
                string branchSetName = "";
                XmlAttributeCollection branchSetAttr = nodeList[i].Attributes;
                for(int branchSetAttrIndex = 0; branchSetAttrIndex < branchSetAttr.Count; ++branchSetAttrIndex)
                {
                    if(branchSetAttr[branchSetAttrIndex].Name == "Name")
                    {
                        branchSetName = branchSetAttr[branchSetAttrIndex].Value;
                    }
                }

                if(branchSetDic.ContainsKey(branchSetName) == false)
                {
                    DebugUtil.assert(false, "branch set not exists : {0} [Line: {1}] [FileName: {2}]",branchSetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    return null;
                }

                XmlNodeList branchSetNodeList = branchSetDic[branchSetName];
                for(int branchSetNodeListIndex = 0; branchSetNodeListIndex < branchSetNodeList.Count; ++branchSetNodeListIndex)
                {
                    if(branchSetNodeList[branchSetNodeListIndex].Name != "Branch")
                    {
                        DebugUtil.assert(false, "wrong branch type : {0} [Line: {1}] [FileName: {2}]",branchSetNodeList[branchSetNodeListIndex].Name, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                        return null;
                    }

                    ActionGraphBranchData branchData = ReadActionBranch(branchSetNodeList[branchSetNodeListIndex],ref actionCompareDic,ref compareDataList, ref _globalVariables, filePath);
                    if(branchData == null)
                    {
                        DebugUtil.assert(false,"invalid branch data [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), filePath);
                        return null;
                    }

                    branchDataList.Add(branchData);
                }
            }
        }

        if(branchStartIndex == branchDataList.Count)
        {
            DebugUtil.assert(false,"branch data not exists [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        animationDataList.Add(animationPlayDataInfoList.ToArray());

        nodeData._animationInfoCount = animationPlayDataInfoList.Count;
        nodeData._branchIndexStart = branchStartIndex;
        nodeData._branchCount = branchDataList.Count - branchStartIndex;

        return nodeData;
    }

    public static AnimationPlayDataInfo ReadActionAnimation(XmlNode node, float defaultFPS, string filePath, float actionTime = -1f)
    {
        AnimationPlayDataInfo playData = new AnimationPlayDataInfo();
        playData._framePerSec = actionTime == -1f ? defaultFPS : -1f;
        playData._actionTime = actionTime;

        XmlAttributeCollection actionAttributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value, _globalVariables);

            if(targetName == "Path")
            {
                playData._path = targetValue;
            }
            else if(targetName == "FramePerSecond")
            {
                if(actionTime != -1f)
                {
                    DebugUtil.assert(false, "ActionTime이 존재하면 FramePerSecond를 쓰면 안됩니다 [Line: {1}] [FileName: {2}]",XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    continue;
                }

                playData._framePerSec = float.Parse(targetValue);
            }
            else if(targetName == "StartFrame")
            {
                playData._startFrame = float.Parse(targetValue);
            }
            else if(targetName == "EndFrame")
            {
                playData._endFrame = float.Parse(targetValue);
            }
            else if(targetName == "XFlip")
            {
                playData._flipState.xFlip = bool.Parse(targetValue);
            }
            else if(targetName == "YFlip")
            {
                playData._flipState.yFlip = bool.Parse(targetValue);
            }
            else if(targetName == "Loop")
            {
                playData._isLoop = bool.Parse(targetValue);
            }
            else if(targetName == "RotationPreset")
            {
                AnimationRotationPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationRotationPreset") as AnimationRotationPreset;
                playData._rotationPresetData = preset.getPresetData(targetValue);
            }
            else if(targetName == "ScalePreset")
            {
                AnimationScalePreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationScalePreset") as AnimationScalePreset;
                playData._scalePresetData = preset.getPresetData(targetValue);
            }
            else if(targetName == "AngleBaseAnimation")
            {
                playData._isAngleBaseAnimation = bool.Parse(targetValue);
            }
            else if(targetName == "AngleBaseAnimationCount")
            {
                playData._angleBaseAnimationSpriteCount = int.Parse(targetValue);
            }
            else if(targetName == "LoopCount")
            {
                playData._animationLoopCount = int.Parse(targetValue);
            }
            else
            {
                DebugUtil.assert(false, "invalid animation attribute: {0} [Line: {1}] [FileName: {2}]",targetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                return null;
            }
        }

        if((playData._startFrame > playData._endFrame) && playData._endFrame != -1)
        {
            DebugUtil.assert(false, "start frame cannot be greater than the end frame: [Path: {0}] [Line: {1}] [FileName: {2}]",playData._path, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        if(node.HasChildNodes == true)
        {
            XmlNodeList nodeList = node.ChildNodes;
            List<ActionFrameEventBase> frameEventList = new List<ActionFrameEventBase>();
            List<ActionFrameEventBase> timeEventList = new List<ActionFrameEventBase>();
            List<MultiSelectAnimationData> multiSelectAnimationList = new List<MultiSelectAnimationData>();
            for(int i = 0; i < nodeList.Count; ++i)
            {
                if(nodeList[i].Name == "FrameEvent")
                {
                    ActionFrameEventBase frameEvent = FrameEventLoader.readFromXMLNode(nodeList[i], filePath);
                    if(frameEvent == null)
                        continue;
                    
                    if(frameEvent._isTimeBase)
                        timeEventList.Add(frameEvent);
                    else
                        frameEventList.Add(frameEvent);
                }
                else if(nodeList[i].Name == "MultiSelectAnimation")
                {
                    MultiSelectAnimationData animationData = readMultiSelectAnimationData(nodeList[i], filePath);
                    if(animationData == null)
                        continue;
                    
                    multiSelectAnimationList.Add(animationData);
                }
                else
                {
                    DebugUtil.assert(false,"invalid animation child: {0} [Line: {1}] [FileName: {2}]",nodeList[i].Name, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    return null;
                }
            }

            frameEventList.Sort((x,y)=>{
                return x._startFrame.CompareTo(y._startFrame);
            });

            playData._frameEventDataCount = frameEventList.Count;
            playData._frameEventData = frameEventList.ToArray();
            playData._timeEventDataCount = timeEventList.Count;
            playData._timeEventData = timeEventList.ToArray();

            playData._multiSelectAnimationDataCount = multiSelectAnimationList.Count;
            playData._multiSelectAnimationData = multiSelectAnimationList.ToArray();
        }

        return playData;
    }

    private static MultiSelectAnimationData readMultiSelectAnimationData(XmlNode node, string filePath)
    {
        MultiSelectAnimationData animationData = new MultiSelectAnimationData();

        XmlAttributeCollection actionAttributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value, _globalVariables);

            if(targetName == "Path")
            {
                animationData._path = targetValue;
            }
            else if(targetName == "Condition")
            {
                ActionGraphConditionCompareData compareData = ReadConditionCompareData(targetValue, _globalVariables, node, filePath);
                if(compareData == null)
                    return null;

                animationData._actionConditionData = compareData;
            }
            else
            {
                DebugUtil.assert(false,"invalid multiSelect Animation Data: {0} [Line: {1}] [FileName: {2}]", targetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                return null;
            }
        }

        if(animationData._path == "")
        {
            DebugUtil.assert(false,"animation path not exists [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }
        else if(animationData._actionConditionData == null)
        {
            DebugUtil.assert(false,"multi select animation must have condition data [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        return animationData;
    }

    public static ActionGraphBranchData ReadActionBranch(XmlNode node, ref Dictionary<ActionGraphBranchData, string> actionCompareDic,  ref List<ActionGraphConditionCompareData> compareDataList, ref Dictionary<string, string> globalVariableContainer, string filePath)
    {
        ActionGraphBranchData branchData = new ActionGraphBranchData();
        XmlAttributeCollection actionAttributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value, globalVariableContainer);

            if(targetName == "Condition")
            {
                if(targetValue == "")
                    continue;

                ActionGraphConditionCompareData conditionData = ReadConditionCompareData(targetValue, globalVariableContainer, node, filePath);
                if(conditionData == null)
                    return null;

                branchData._conditionCompareDataIndex = compareDataList.Count;
                compareDataList.Add(conditionData);
            }
            else if(targetName == "Key")
            {
                if(targetValue == "")
                    continue;
                    
                ActionGraphConditionCompareData keyConditionData = ReadConditionCompareData(targetValue, globalVariableContainer, node, filePath);
                if(keyConditionData == null)
                    return null;

                branchData._keyConditionCompareDataIndex = compareDataList.Count;
                compareDataList.Add(keyConditionData);
            }
            else if(targetName == "Weight")
            {
                if(targetValue == "")
                    continue;
                
                targetValue = "getWeight_" + targetValue;
                ActionGraphConditionCompareData keyConditionData = ReadConditionCompareData(targetValue, globalVariableContainer, node, filePath);
                if(keyConditionData == null)
                    return null;

                branchData._weightConditionCompareDataIndex = compareDataList.Count;
                compareDataList.Add(keyConditionData);
            }
            else if(targetName == "Execute")
            {
                actionCompareDic.Add(branchData, targetValue);
            }
            else
            {
                DebugUtil.assert(false, "invalid branch attribute: {0} [Line: {1}] [FileName: {2}]",targetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                return null;
            }
        }


        return branchData;
    }

    public static ActionGraphConditionCompareData ReadConditionCompareData(string formula, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath)
    {
        formula = formula.Replace(" ","");
        List<ActionGraphConditionNodeData> symbolList = new List<ActionGraphConditionNodeData>();
        List<ConditionCompareType> compareTypeList = new List<ConditionCompareType>();
        
        //int end;
        //int resultIndex = 0;
        //DebugUtil.assert(ReadConditionFormula(formula,0, ref resultIndex, out end,symbolList,compareTypeList) == true,"Tlqkfsusdk");
        DebugUtil.assert(readConditionFormula(formula,ref symbolList,ref compareTypeList, globalVariableContainer, node, filePath) == true,"[Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), filePath);

        ActionGraphConditionCompareData compareData = new ActionGraphConditionCompareData();
        compareData._compareTypeArray = compareTypeList.ToArray();
        compareData._compareTypeCount = compareTypeList.Count;
        compareData._conditionNodeDataArray = symbolList.ToArray();
        compareData._conditionNodeDataCount = symbolList.Count;

        return compareData;
    }

    private static bool readConditionFormula(string formula, ref List<ActionGraphConditionNodeData> symbolList, ref List<ConditionCompareType> compareTypeList, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath)
    {
        string calcFormula = formula;
        calcFormula = calcFormula.Insert(0,"(");
        calcFormula += ")";

        int result = 0;
        int finalIndex = readFormulaBracket(ref calcFormula,ref result,0,ref symbolList,ref compareTypeList, globalVariableContainer, node, filePath);

        return finalIndex != -1;
    }

    private static int readFormulaBracket(ref string formula, ref int resultIndex, int startOffset, ref List<ActionGraphConditionNodeData> symbolList, ref List<ConditionCompareType> compareTypeList, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath )
    {
        if(formula.Length <= startOffset || formula[startOffset] == ')')
        {
            DebugUtil.assert(false, "condition formular is invalid {0} [Line: {1}] [FileName: {2}]", formula, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return -1;
        }

        int stringLength = formula.Length;
        int endOffset = -1;
        for(int i = startOffset + 1; i < stringLength; ++i)
        {
            if(formula[i] == '(')
            {
                int length = readFormulaBracket(ref formula, ref resultIndex, i, ref symbolList, ref compareTypeList, globalVariableContainer, node, filePath);
                if(length == -1)
                    return -1;

                formula = formula.Remove(i, length + 1);
                formula = formula.Insert(i, "RESULT_" + resultIndex++);
            }
            
            if(formula[i] == ')')
            {
                endOffset = i;
                break;
            }
        }

        if(endOffset == -1)
        {
            DebugUtil.assert(false, "condition formular is invalid {0} [Line: {1}] [FileName: {2}]", formula, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return -1;
        }

        int finalLength = endOffset - startOffset;
        string calcFormula = formula.Substring(startOffset + 1,finalLength - 1);

        SplitToMark(calcFormula, ref resultIndex, in symbolList, in compareTypeList, globalVariableContainer, node, filePath);
        return finalLength;
    }

    private static void SplitToMark(string formula, ref int resultIndex, in List<ActionGraphConditionNodeData> symbolList, in List<ConditionCompareType> compareTypeList, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath)
    {
        string calcFormula = formula;
        int symbolEndIndex = 0;
        int markLength = 0;
        ConditionCompareType compareType = ConditionCompareType.Count;

        int loopCount = 0;
        while(ReadNearestMark(calcFormula,out symbolEndIndex,out markLength, out compareType) == true)
        {
            string symbol = calcFormula.Substring(0,symbolEndIndex);
            calcFormula = calcFormula.Remove(0,symbolEndIndex + markLength);

            symbolList.Add(getConditionNodeData(symbol, globalVariableContainer, node, filePath));
            compareTypeList.Add(compareType);

            if(++loopCount >= 2)
                symbolList.Add(getConditionNodeData("RESULT_" + resultIndex++, globalVariableContainer, node, filePath));
  
        }
        

        symbolList.Add(getConditionNodeData(calcFormula,globalVariableContainer, node, filePath));

        return;
    }

    private static ActionGraphConditionNodeData getConditionNodeData(string symbol, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath)
    {
        symbol = getGlobalVariable(symbol, globalVariableContainer);
        ActionGraphConditionNodeData nodeData = isLiteral(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isResult(symbol, node, filePath);
        if(nodeData != null)
            return nodeData;

        nodeData = isStatus(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isKey(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isTargetFrameTag(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isFrameTag(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isWeight(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = new ActionGraphConditionNodeData();
    
        if(ConditionNodeInfoPreset._nodePreset.ContainsKey(symbol) == false)
        {
            DebugUtil.assert(false,"Target Symbol does not exists : {0} [Line: {1}] [FileName: {2}]",symbol, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        nodeData._symbolName = symbol;
        return nodeData;
    }

    private static ActionGraphConditionNodeData_FrameTag isTargetFrameTag(string symbol)
    {
        if(symbol.Contains("getTargetFrameTag_") == false)
            return null;
        
        ActionGraphConditionNodeData_FrameTag item = new ActionGraphConditionNodeData_FrameTag();
        item._symbolName = "TargetFrameTag";
        item._targetFrameTag = symbol.Replace("getTargetFrameTag_","");
        return item;
    }

    private static ActionGraphConditionNodeData_FrameTag isFrameTag(string symbol)
    {
        if(symbol.Contains("getFrameTag_") == false)
            return null;

        ActionGraphConditionNodeData_FrameTag item = new ActionGraphConditionNodeData_FrameTag();
        item._symbolName = "FrameTag";
        item._targetFrameTag = symbol.Replace("getFrameTag_","");
        return item;
    }

    private static ActionGraphConditionNodeData_Weight isWeight(string symbol)
    {
        if(symbol.Contains("getWeight_") == false)
            return null;

        symbol = symbol.Replace("getWeight_","");
        string[] groupData = symbol.Split('^');

        if(groupData == null || groupData.Length > 2 || groupData.Length < 2)
        {
            DebugUtil.assert(false, "invalid weight data: {0}",symbol);
            return null;
        }

        ActionGraphConditionNodeData_Weight item = new ActionGraphConditionNodeData_Weight();
        item._symbolName = "Weight";
        item._weightGroupKey = groupData[0];
        item._weightName = groupData[1];
        return item;
    }

    private static ActionGraphConditionNodeData_Status isStatus(string symbol)
    {
        if(symbol.Contains("getStat_") == false)
            return null;

        ActionGraphConditionNodeData_Status item = new ActionGraphConditionNodeData_Status();
        item._symbolName = "Status";
        item._targetStatus = symbol.Replace("getStat_","");
        return item;
    }

    private static ActionGraphConditionNodeData_Key isKey(string symbol)
    {
        if(symbol.Contains("getKey_") == false)
            return null;

        ActionGraphConditionNodeData_Key item = new ActionGraphConditionNodeData_Key();
        item._symbolName = "Key";
        item._targetKeyName = symbol.Replace("getKey_","");
        return item;
    }

    private static ActionGraphConditionNodeData_ConditionResult isResult(string symbol, XmlNode node, string filePath)
    {
        if(symbol.Contains("RESULT_") == false)
            return null;

        string index = symbol.Substring(7);
        int indexInt = 0;
        if(int.TryParse(index,out indexInt) == false)
        {
            DebugUtil.assert(false,"result index invalid: {0} [Line: {1}] [FileName: {2}]",symbol, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        ActionGraphConditionNodeData_ConditionResult result = new ActionGraphConditionNodeData_ConditionResult();
        result._resultIndex = indexInt;
        result._symbolName = "RESULT";

        return result;
    }

    private static ActionGraphConditionNodeData_Literal isLiteral(string symbol)
    {
        if(int.TryParse(symbol,out int intRresult))
        {
            ActionGraphConditionNodeData_Literal literal = new ActionGraphConditionNodeData_Literal();
            literal._symbolName = "Literal_Int";
            literal.setLiteral(System.BitConverter.GetBytes(intRresult));

            return literal;
        }
        else if(float.TryParse(symbol,out float floatResult))
        {
            ActionGraphConditionNodeData_Literal literal = new ActionGraphConditionNodeData_Literal();
            literal._symbolName = "Literal_Float";
            literal.setLiteral(System.BitConverter.GetBytes(floatResult));

            return literal;
        }
        else if(bool.TryParse(symbol,out bool boolResult))
        {
            ActionGraphConditionNodeData_Literal literal = new ActionGraphConditionNodeData_Literal();
            literal._symbolName = "Literal_Bool";
            literal.setLiteral(System.BitConverter.GetBytes(boolResult));

            return literal;
        }

        return null;
    }

    private static bool ReadNearestMark(string formula, out int symbolEndIndex, out int markLength, out ConditionCompareType compareType)
    {
        compareType = ConditionCompareType.Count;
        symbolEndIndex = int.MaxValue;
        markLength = 0;
        if(formula.Contains("&&") == true && formula.IndexOf("&&") < symbolEndIndex)
        {
            compareType = ConditionCompareType.And;
            symbolEndIndex = formula.IndexOf("&&");
            markLength = 2;
        }
        if(formula.Contains("||") == true && formula.IndexOf("||") < symbolEndIndex)
        {
            compareType = ConditionCompareType.Or;
            symbolEndIndex = formula.IndexOf("||");
            markLength = 2;
        }
        if(formula.Contains("==") == true && formula.IndexOf("==") < symbolEndIndex)
        {
            compareType = ConditionCompareType.Equals;
            symbolEndIndex = formula.IndexOf("==");
            markLength = 2;
        }

        if(formula.Contains(">") == true && formula.IndexOf(">") < symbolEndIndex)
        {
            compareType = ConditionCompareType.Greater;
            symbolEndIndex = formula.IndexOf(">");
            markLength = 1;
        }
        if(formula.Contains(">=") == true && formula.IndexOf(">=") <= symbolEndIndex)
        {
            compareType = ConditionCompareType.GreaterEqual;
            symbolEndIndex = formula.IndexOf(">=");
            markLength = 2;
        }

        if(formula.Contains("<") == true && formula.IndexOf("<") < symbolEndIndex)
        {
            compareType = ConditionCompareType.Smaller;
            symbolEndIndex = formula.IndexOf("<");
            markLength = 1;
        }
        if(formula.Contains("<=") == true && formula.IndexOf("<=") <= symbolEndIndex)
        {
            compareType = ConditionCompareType.SmallerEqual;
            symbolEndIndex = formula.IndexOf("<=");
            markLength = 2;
        }
        if(formula.Contains("!=") == true && formula.IndexOf("!=") < symbolEndIndex)
        {
            compareType = ConditionCompareType.NotEquals;
            symbolEndIndex = formula.IndexOf("!=");
            markLength = 2;
        }

        if(markLength == 0 || symbolEndIndex == int.MaxValue || compareType == ConditionCompareType.Count)
            return false;

        return true;
    }

    private static void ReadTitle(XmlNode node, ActionGraphBaseData baseData, out float defaultFPS, out string defaultAction, string filePath)
    {
        defaultFPS = -1f;
        defaultAction = "";

        XmlAttributeCollection attributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
        {
            string targetName = attributes[attrIndex].Name;
            string targetValue = attributes[attrIndex].Value;

            if(targetName == "Name")
            {
                baseData._name = targetValue;
            }
            else if(targetName == "DefaultFramePerSecond")
            {
                defaultFPS = float.Parse(targetValue);
            }
            else if(targetName == "DefaultAction")
            {
                defaultAction = targetValue;
            }
            else if(targetName == "DefaultBuff")
            {
                string[] buffList = targetValue.Split(' ');

                int[] buffKeyList = new int[buffList.Length];
                for(int j = 0; j < buffList.Length; ++j)
                {
                    bool parse = int.TryParse(buffList[j],out int buffKey);
                    if(parse == false)
                        buffKey = StatusInfo.getBuffKeyFromName(buffList[j]);

                    if(buffKey == -1)
                    {
                        DebugUtil.assert(false, "invalidBuff : {0} [Line: {1}] [FileName: {2}]", buffList[j], XMLScriptConverter.getLineFromXMLNode(node), filePath);
                        continue;
                    }

                    buffKeyList[j] = buffKey;
                }

                baseData._defaultBuffList = buffKeyList;
            }

        }
    }


    // public static MovementGraph readFromBinary(string path)
    // {
    //     if(File.Exists(path) == false)
    //     {
    //         DebugUtil.assert(false,"file does not exists : {0}", path);
    //         return null;
    //     }

    //     var fileStream = File.Open(path, FileMode.Create);
    //     var reader = new BinaryReader(fileStream,Encoding.UTF8,false);
    //     MovementGraph graph = ScriptableObject.CreateInstance<MovementGraph>();

    //     graph.deserialize(reader);

    //     reader.Close();
    //     fileStream.Close();
    //     return graph;
    // }

    // public static MovementGraph readFromXMLAndExportToBinary(string xmlPath, string binaryPath)
    // {
    //     MovementGraph graph = readFromXML(xmlPath);
    //     if(graph == null)
    //     {
    //         return null;
    //     }

    //     exportToBinary(graph, binaryPath);
    //     return graph;
    // }

    // public static void exportToBinary(MovementGraph graph, string path)
    // {
    //     var fileStream = File.Open(path, FileMode.Create);
    //     var writer = new BinaryWriter(fileStream,Encoding.UTF8,false);

    //     graph.serialize(writer);

    //     writer.Close();
    //     fileStream.Close();
    // }
}
