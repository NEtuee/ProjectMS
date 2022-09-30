using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;


public static class ActionGraphLoader
{
    private static Dictionary<string, string> _globalVariables = new Dictionary<string, string>();
    public static ActionGraphBaseData readFromXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.Load(path);
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml load exception : {0}",ex.Message);
            return null;
        }
        
        if(xmlDoc.HasChildNodes == false)
        {
            DebugUtil.assert(false,"xml is empty");
            return null;
        }

        Dictionary<string, XmlNodeList> branchSetDic = new Dictionary<string, XmlNodeList>();

        XmlNode node = xmlDoc.FirstChild;
        
        if(node.Name.Equals("ActionGraph") == false)
        {
            DebugUtil.assert(false,"wrong xml type. name : {0}",node.Name);
            return null;
        }
        
        float defaultFramePerSecond = 0f;
        string defaultActionName = "";

        ActionGraphBaseData actionBaseData = new ActionGraphBaseData();
        ReadTitle(node,actionBaseData,out defaultFramePerSecond, out defaultActionName);

        List<ActionGraphNodeData> nodeDataList = new List<ActionGraphNodeData>();
        List<ActionGraphBranchData> branchDataList = new List<ActionGraphBranchData>();
        List<ActionGraphConditionCompareData> compareDataList = new List<ActionGraphConditionCompareData>();
        List<AnimationPlayDataInfo> animationDataList = new List<AnimationPlayDataInfo>();

        _globalVariables.Clear();
        Dictionary<ActionGraphBranchData, string> actionCompareDic = new Dictionary<ActionGraphBranchData, string>();
        Dictionary<string, int> actionIndexDic = new Dictionary<string, int>();
        XmlNodeList nodeList = node.ChildNodes;

        int actionIndex = 0;
        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "BranchSet")
            {
                readBranchSet(nodeList[i],ref branchSetDic);
                continue;
            }
            else if(nodeList[i].Name == "GlobalVariable")
            {
                readGlobalVariable(nodeList[i], ref _globalVariables);
                continue;
            }
            
            ActionGraphNodeData nodeData = ReadAction(nodeList[i],defaultFramePerSecond, ref animationDataList, ref actionCompareDic, ref branchDataList,ref compareDataList, in branchSetDic);
            if(nodeData == null)
            {
                DebugUtil.assert(false,"node data is null : {0}",nodeList[i].Name);
                return null;
            }

            nodeDataList.Add(nodeData);
            actionIndexDic.Add(nodeData._nodeName,actionIndex++);
        }

        foreach(var item in actionCompareDic)
        {
            if(actionIndexDic.ContainsKey(item.Value) == false)
            {
                DebugUtil.assert(false,"target action is not exists : {0}",item.Value);
                return null;
            }
            else if(item.Value == defaultActionName)
            {
                actionBaseData._defaultActionIndex = actionIndexDic[item.Value];
            }

            item.Key._branchActionIndex = actionIndexDic[item.Value];
        }

        actionBaseData._actionNodeCount = nodeDataList.Count;
        actionBaseData._branchCount = branchDataList.Count;
        actionBaseData._conditionCompareDataCount = compareDataList.Count;
        actionBaseData._animationPlayDataCount = animationDataList.Count;

        actionBaseData._actionNodeData = nodeDataList.ToArray();
        actionBaseData._branchData = branchDataList.ToArray();
        actionBaseData._conditionCompareData = compareDataList.ToArray();
        actionBaseData._animationPlayData = animationDataList.ToArray();

        return actionBaseData;
    }

    private static void readGlobalVariable(XmlNode node, ref Dictionary<string, string> targetDic)
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
            DebugUtil.assert(false, "invalid globalVariable, name:[{0}] value:[{1}]",name,value);
            return;
        }

        targetDic.Add(name,value);
    }

    public static string getGlobalVariable(string value)
    {
        if(_globalVariables.ContainsKey(value))
            return _globalVariables[value];

        return value;
    }

    private static void readBranchSet(XmlNode branchSetParent, ref Dictionary<string, XmlNodeList> targetDic)
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
            DebugUtil.assert(false, "branchSet is empty : {0}",branchSetName);
            return;
        }

        targetDic.Add(branchSetName,branchSetParent.ChildNodes);
    }

    private static ActionGraphNodeData ReadAction(XmlNode node, float defaultFPS, ref List<AnimationPlayDataInfo> animationDataList,  ref Dictionary<ActionGraphBranchData, string> actionCompareDic,ref List<ActionGraphBranchData> branchDataList, ref List<ActionGraphConditionCompareData> compareDataList, in Dictionary<string, XmlNodeList> branchSetDic)
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

        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value);

            if(targetName == "MovementType")
            {
                nodeData._movementType = (MovementBase.MovementType)System.Enum.Parse(typeof(MovementBase.MovementType), targetValue);
            }
            else if(targetName == "DirectionType")
            {
                nodeData._directionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), targetValue);
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
            else if(targetName == "ApplyBuff")
            {
                string[] buffList = targetValue.Split(' ');
                nodeData._applyBuffList = new int[buffList.Length];

                for(int i = 0; i < buffList.Length; ++i)
                {
                    nodeData._applyBuffList[i] = int.Parse(buffList[i]);
                }
            }
            else
            {
                DebugUtil.assert(false,"invalid attribute type !!! : {0}", targetName);
            }
        }

        XmlNodeList nodeList = node.ChildNodes;
        int branchStartIndex = branchDataList.Count;

        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "Animation")
            {
                AnimationPlayDataInfo animationData = ReadActionAnimation(nodeList[i],defaultFPS);
                nodeData._animationInfoIndex = animationDataList.Count;
                animationData._hasMovementGraph = nodeData._movementType == MovementBase.MovementType.RootMotion;
                animationDataList.Add(animationData);
            }
            else if(nodeList[i].Name == "Branch")
            {
                ActionGraphBranchData branchData = ReadActionBranch(nodeList[i],nodeData,ref actionCompareDic,ref compareDataList);
                if(branchData == null)
                {
                    DebugUtil.assert(false,"invalid branch data");
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
                    DebugUtil.assert(false, "branch set not exists : {0}",branchSetName);
                    return null;
                }

                XmlNodeList branchSetNodeList = branchSetDic[branchSetName];
                for(int branchSetNodeListIndex = 0; branchSetNodeListIndex < branchSetNodeList.Count; ++branchSetNodeListIndex)
                {
                    if(branchSetNodeList[branchSetNodeListIndex].Name != "Branch")
                    {
                        DebugUtil.assert(false, "wrong branch type : {0}",branchSetNodeList[branchSetNodeListIndex].Name);
                        return null;
                    }

                    ActionGraphBranchData branchData = ReadActionBranch(branchSetNodeList[branchSetNodeListIndex],nodeData,ref actionCompareDic,ref compareDataList);
                    if(branchData == null)
                    {
                        DebugUtil.assert(false,"invalid branch data");
                        return null;
                    }

                    branchDataList.Add(branchData);
                }
            }
        }

        if(branchStartIndex == branchDataList.Count)
        {
            DebugUtil.assert(false,"branch data not exists");
            return null;
        }

        nodeData._branchIndexStart = branchStartIndex;
        nodeData._branchCount = branchDataList.Count - branchStartIndex;

        return nodeData;
    }

    public static AnimationPlayDataInfo ReadActionAnimation(XmlNode node, float defaultFPS)
    {
        AnimationPlayDataInfo playData = new AnimationPlayDataInfo();
        playData._framePerSec = defaultFPS;

        XmlAttributeCollection actionAttributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value);

            if(targetName == "Path")
            {
                playData._path = targetValue;
            }
            else if(targetName == "FramePerSecond")
            {
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
            else
            {
                DebugUtil.assert(false, "invalid animation attribute: {0}",targetName);
                return null;
            }
        }

        if(playData._startFrame > playData._endFrame)
        {
            DebugUtil.assert(false, "start frame cannot be greater than the end frame: {0}",playData._path);
            return null;
        }

        if(node.HasChildNodes == true)
        {
            XmlNodeList nodeList = node.ChildNodes;
            List<ActionFrameEventBase> frameEventList = new List<ActionFrameEventBase>();
            for(int i = 0; i < nodeList.Count; ++i)
            {
                if(nodeList[i].Name == "FrameEvent")
                {
                    ActionFrameEventBase frameEvent = FrameEventLoader.readFromXMLNode(nodeList[i]);
                    if(frameEvent == null)
                        continue;
                    
                    frameEventList.Add(frameEvent);
                }
            }

            playData._frameEventDataCount = frameEventList.Count;
            playData._frameEventData = frameEventList.ToArray();
        }

        return playData;
    }

    private static ActionGraphBranchData ReadActionBranch(XmlNode node, ActionGraphNodeData data, ref Dictionary<ActionGraphBranchData, string> actionCompareDic,  ref List<ActionGraphConditionCompareData> compareDataList)
    {
        ActionGraphBranchData branchData = new ActionGraphBranchData();
        XmlAttributeCollection actionAttributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value);

            if(targetName == "Condition")
            {
                ActionGraphConditionCompareData conditionData = ReadConditionCompareData(targetValue);
                if(conditionData == null)
                    return null;

                branchData._conditionCompareDataIndex = compareDataList.Count;
                compareDataList.Add(conditionData);
            }
            else if(targetName == "Action")
            {
                actionCompareDic.Add(branchData, targetValue);
            }
        }


        return branchData;
    }

    public static ActionGraphConditionCompareData ReadConditionCompareData(string formula)
    {
        formula = formula.Replace(" ","");
        int end;
        List<ActionGraphConditionNodeData> symbolList = new List<ActionGraphConditionNodeData>();
        List<ConditionCompareType> compareTypeList = new List<ConditionCompareType>();
        
        int resultIndex = 0;
        DebugUtil.assert(ReadConditionFormula(formula,0, ref resultIndex, out end,symbolList,compareTypeList) == true,"Tlqkfsusdk");

        ActionGraphConditionCompareData compareData = new ActionGraphConditionCompareData();
        compareData._compareTypeArray = compareTypeList.ToArray();
        compareData._compareTypeCount = compareTypeList.Count;
        compareData._conditionNodeDataArray = symbolList.ToArray();
        compareData._conditionNodeDataCount = symbolList.Count;

        return compareData;
    }

    private static bool ReadConditionFormula(string formula, int start, ref int resultIndex, out int end, in List<ActionGraphConditionNodeData> symbolList, in List<ConditionCompareType> compareTypeList )
    {
        end = -1;

        if(formula.Length <= start || formula[start] == ')')
        {
            DebugUtil.assert(false, "condition formular is invalid {0}", formula);
            return false;
        }

        string calcFormula = formula.Substring(start);
        while(calcFormula.Contains("(") == true)
        {
            int brancketIndex = calcFormula.IndexOf("(",0);

            if(calcFormula.Contains(")") && calcFormula.IndexOf(")") < brancketIndex)
                break;

            int brancketEndIndex = -1;
            bool result = ReadConditionFormula(calcFormula, brancketIndex + 1, ref resultIndex, out brancketEndIndex, in symbolList, in compareTypeList);

            if(brancketEndIndex == -1 || result == false)
                return false;
                
            calcFormula = calcFormula.Remove(brancketIndex, brancketEndIndex - brancketIndex + 1);
            calcFormula = calcFormula.Insert(brancketIndex, "RESULT_" + resultIndex++);

        }

        if(calcFormula.Contains(")"))
        {
            end = formula.IndexOf(")");
            calcFormula = calcFormula.Substring(0,calcFormula.IndexOf(")"));
        }

        SplitToMark(calcFormula, ref resultIndex, in symbolList, in compareTypeList);

        return true;
    }

    private static void SplitToMark(string formula, ref int resultIndex, in List<ActionGraphConditionNodeData> symbolList, in List<ConditionCompareType> compareTypeList)
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

            symbolList.Add(getConditionNodeData(symbol));
            compareTypeList.Add(compareType);

            if(++loopCount >= 2)
                symbolList.Add(getConditionNodeData("RESULT_" + resultIndex++));
  
        }
        

        symbolList.Add(getConditionNodeData(calcFormula));

        return;
    }

    private static ActionGraphConditionNodeData getConditionNodeData(string symbol)
    {
        symbol = getGlobalVariable(symbol);
        ActionGraphConditionNodeData nodeData = isLiteral(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isResult(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isStatus(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = new ActionGraphConditionNodeData();
    
        if(ConditionNodeInfoPreset._nodePreset.ContainsKey(symbol) == false)
        {
            DebugUtil.assert(false,"Target Symbol does not exists : {0}",symbol);
            return null;
        }

        nodeData._symbolName = symbol;
        return nodeData;
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

    private static ActionGraphConditionNodeData_ConditionResult isResult(string symbol)
    {
        if(symbol.Contains("RESULT_") == false)
            return null;

        string index = symbol.Substring(7);
        int indexInt = 0;
        if(int.TryParse(index,out indexInt) == false)
        {
            DebugUtil.assert(false,"result index invalid: {0}",symbol);
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

    private static void ReadTitle(XmlNode node, ActionGraphBaseData baseData, out float defaultFPS, out string defaultAction)
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
                    buffKeyList[j] = int.Parse(buffList[j]);
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
