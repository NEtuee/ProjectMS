using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;


public class AIGraphLoader : LoaderBase<AIGraphBaseData>
{

    private static string _aiPackageRoot = "Assets\\Data\\AIPackage\\";

    private static Dictionary<string, string> _aiGraphGlobalVariables = new Dictionary<string, string>();
    private static Dictionary<string, string> _aiPackageGlobalVariables = new Dictionary<string, string>();
    private static Dictionary<string, AIPackageBaseData> _loadedAiPackage = new Dictionary<string, AIPackageBaseData>();
    public override AIGraphBaseData readFromXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(XMLScriptConverter.convertXMLScriptSymbol(path),readerSettings))
            {
                xmlDoc.Load(reader);
            }
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
        
        if(node.Name.Equals("AIGraph") == false)
        {
            DebugUtil.assert(false,"wrong xml type. name : {0}",node.Name);
            return null;
        }
        
        string defaultAiNodeName = "";

        AIGraphBaseData aiBaseData = new AIGraphBaseData();
        readAIGraphTitle(node,aiBaseData,out defaultAiNodeName);

        List<AIGraphNodeData> nodeDataList = new List<AIGraphNodeData>();
        List<ActionGraphBranchData> branchDataList = new List<ActionGraphBranchData>();
        List<ActionGraphConditionCompareData> compareDataList = new List<ActionGraphConditionCompareData>();
        List<AIPackageBaseData> aiPackageList = new List<AIPackageBaseData>();

        _aiGraphGlobalVariables.Clear();
        Dictionary<ActionGraphBranchData, string> actionCompareDic = new Dictionary<ActionGraphBranchData, string>();
        Dictionary<string, int> actionIndexDic = new Dictionary<string, int>();
        Dictionary<string, List<AIEvent_ExecuteState>> aiExecuteStateDic = new Dictionary<string, List<AIEvent_ExecuteState>>();

        Dictionary<string, int> aiPackageIndexDic = new Dictionary<string, int>();

        XmlNodeList nodeList = node.ChildNodes;

        int actionIndex = 0;
        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "BranchSet")
            {
                ActionGraphLoader.readBranchSet(nodeList[i],ref branchSetDic);
                continue;
            }
            else if(nodeList[i].Name == "GlobalVariable")
            {
                readGlobalVariable(nodeList[i], ref _aiGraphGlobalVariables);
                continue;
            }
            else if(nodeList[i].Name == "Include")
            {
                if(nodeList[i].Attributes.Count == 0)
                {
                    DebugUtil.assert(false,"path does not exists");
                    return null;
                }
                else if(nodeList[i].Attributes[0].Name != "Path")
                {
                    DebugUtil.assert(false,"first attribute must path");
                    return null;
                }

                AIPackageBaseData packageData = null;
                string packagePath = _aiPackageRoot + nodeList[i].Attributes[0].Value;
                if(_loadedAiPackage.ContainsKey(packagePath) == true)
                {
                    packageData = _loadedAiPackage[packagePath];
                }
                else
                {
                    packageData = readAIPackageFromXML( packagePath);
                    _loadedAiPackage.Add(packagePath,packageData);
                }

                if(packageData == null)
                    return null;

                aiPackageIndexDic.Add(packageData._name,aiPackageList.Count);
                aiPackageList.Add(packageData);

                continue;
            }
            else if(nodeList[i].Name.Contains("Event_"))
            {
                readAIChildEvent(nodeList[i], ref aiBaseData._aiEvents, ref aiExecuteStateDic);
            }
            
            AIGraphNodeData nodeData = readAIGraphNode(nodeList[i], ref aiPackageIndexDic, ref actionCompareDic, ref branchDataList,ref compareDataList, in branchSetDic);
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
            else if(item.Value == defaultAiNodeName)
            {
                aiBaseData._defaultAIIndex = actionIndexDic[item.Value];
            }

            item.Key._branchActionIndex = actionIndexDic[item.Value];
        }

        if(nodeDataList.Count != 0 && aiBaseData._defaultAIIndex == -1)
        {
            if(actionIndexDic.ContainsKey(defaultAiNodeName) == false)
            {
                DebugUtil.assert(false, "invalid default state name: {0}",defaultAiNodeName);
                return null;
            }

            aiBaseData._defaultAIIndex = actionIndexDic[defaultAiNodeName];
        }

        aiBaseData._aiNodeCount = nodeDataList.Count;
        aiBaseData._aiPackageCount = aiPackageList.Count;
        aiBaseData._branchCount = branchDataList.Count;
        aiBaseData._conditionCompareDataCount = compareDataList.Count;

        aiBaseData._aiGraphNodeData = nodeDataList.ToArray();
        aiBaseData._aiPackageData = aiPackageList.ToArray();
        aiBaseData._branchData = branchDataList.ToArray();
        aiBaseData._conditionCompareData = compareDataList.ToArray();

        return aiBaseData;
    }

    private static AIGraphNodeData readAIGraphNode(XmlNode node, ref Dictionary<string, int> aiPackageIndexDic,ref Dictionary<ActionGraphBranchData, string> actionCompareDic,ref List<ActionGraphBranchData> branchDataList, ref List<ActionGraphConditionCompareData> compareDataList, in Dictionary<string, XmlNodeList> branchSetDic)
    {
        AIGraphNodeData nodeData = new AIGraphNodeData();
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
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value, _aiGraphGlobalVariables);

            if(targetName == "Package")
            {
                if(aiPackageIndexDic.ContainsKey(targetValue) == false)
                {
                    DebugUtil.assert(false, "ai package does not exists: {0}",targetValue);
                    return null;
                }

                nodeData._packageIndex = aiPackageIndexDic[targetValue];
            }
            else
            {
                DebugUtil.assert(false,"invalid attribute type !!! : {0}", targetName);
            }
        }

        XmlNodeList nodeList = node.ChildNodes;
        int branchStartIndex = branchDataList.Count;

        Dictionary<string, List<AIEvent_ExecuteState>> aiExecuteStateDic = new Dictionary<string, List<AIEvent_ExecuteState>>();

        for(int i = 0; i < nodeList.Count; ++i)
        {
            
            if(nodeList[i].Name == "Branch")
            {
                ActionGraphBranchData branchData = ActionGraphLoader.ReadActionBranch(nodeList[i],ref actionCompareDic,ref compareDataList, ref _aiGraphGlobalVariables);
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

                    ActionGraphBranchData branchData = ActionGraphLoader.ReadActionBranch(branchSetNodeList[branchSetNodeListIndex],ref actionCompareDic,ref compareDataList, ref _aiGraphGlobalVariables);
                    if(branchData == null)
                    {
                        DebugUtil.assert(false,"invalid branch data");
                        return null;
                    }

                    branchDataList.Add(branchData);
                }
            }
            else if(nodeList[i].Name.Contains("Event_"))
            {
                readAIChildEvent(nodeList[i], ref nodeData._aiEvents, ref aiExecuteStateDic);
            }
            else
            {
                DebugUtil.assert(false, "invalid attribute: {0}",nodeList[i].Name);
            }
        }
        
        nodeData._branchIndexStart = branchStartIndex;
        nodeData._branchCount = branchDataList.Count - branchStartIndex;

        return nodeData;
    } 

    private static void readAIGraphTitle(XmlNode node, AIGraphBaseData baseData, out string defaultState)
    {
        defaultState = "";

        XmlAttributeCollection attributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
        {
            string targetName = attributes[attrIndex].Name;
            string targetValue = attributes[attrIndex].Value;

            if(targetName == "Name")
            {
                baseData._name = targetValue;
            }
            else if(targetName == "DefaultState")
            {
                defaultState = targetValue;
            }
            else
            {
                DebugUtil.assert(false, "invalid attribute: {0}",targetName);
            }

        }
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

    public static string getGlobalVariable(string value, Dictionary<string, string> globalVariableContainer)
    {
        if(globalVariableContainer.ContainsKey(value))
            return globalVariableContainer[value];

        return value;
    }




    private static AIPackageBaseData readAIPackageFromXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(XMLScriptConverter.convertXMLScriptSymbol(path),readerSettings))
            {
                xmlDoc.Load(reader);
            }
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
        
        if(node.Name.Equals("AIPackage") == false)
        {
            DebugUtil.assert(false,"wrong xml type. name : {0}",node.Name);
            return null;
        }
        
        string defaultAIName = "";

        AIPackageBaseData aiPackageBaseData = new AIPackageBaseData();
        readAIPackageTitle(node,aiPackageBaseData, out defaultAIName);

        List<AIPackageNodeData> nodeDataList = new List<AIPackageNodeData>();
        List<ActionGraphBranchData> branchDataList = new List<ActionGraphBranchData>();
        List<ActionGraphConditionCompareData> compareDataList = new List<ActionGraphConditionCompareData>();

        _aiPackageGlobalVariables.Clear();
        Dictionary<ActionGraphBranchData, string> actionCompareDic = new Dictionary<ActionGraphBranchData, string>();
        Dictionary<string, int> aiIndexDic = new Dictionary<string, int>();
        Dictionary<string, List<AIEvent_ExecuteState>> aiExecuteEventDic = new Dictionary<string, List<AIEvent_ExecuteState>>();
        XmlNodeList nodeList = node.ChildNodes;

        int actionIndex = 0;
        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "BranchSet")
            {
                ActionGraphLoader.readBranchSet(nodeList[i],ref branchSetDic);
                continue;
            }
            else if(nodeList[i].Name == "GlobalVariable")
            {
                readGlobalVariable(nodeList[i], ref _aiPackageGlobalVariables);
                continue;
            }
            else if(nodeList[i].Name == "AIState")
            {
                XmlNodeList aiStateNodeList = nodeList[i].ChildNodes;
                for(int index = 0; index < aiStateNodeList.Count; ++index)
                {
                    AIPackageNodeData nodeData = readAIPackageNode(aiStateNodeList[index], ref actionCompareDic, ref branchDataList,ref compareDataList, in branchSetDic, ref aiExecuteEventDic);

                    if(nodeData == null)
                    {
                        DebugUtil.assert(false,"node data is null : {0}",aiStateNodeList[index].Name);
                        return null;
                    }

                    nodeDataList.Add(nodeData);
                    aiIndexDic.Add(nodeData._nodeName,actionIndex++);
                }
                
            }
            else if(nodeList[i].Name.Contains("PackageEvent_"))
            {
                readAIPackageChildEvent(nodeList[i],ref aiPackageBaseData._aiPackageEvents, ref aiExecuteEventDic);
            }
            else if(nodeList[i].Name.Contains("Event_"))
            {
                readAIChildEvent(nodeList[i], ref aiPackageBaseData._aiEvents, ref aiExecuteEventDic);
            }
            else
            {
                DebugUtil.assert(false, "invalid attribute: {0}",nodeList[i].Name);
                return null;
            }
            
        }

        foreach(var item in actionCompareDic)
        {
            if(aiIndexDic.ContainsKey(item.Value) == false)
            {
                DebugUtil.assert(false,"target state is not exists : {0}",item.Value);
                return null;
            }
            else if(item.Value == defaultAIName)
            {
                aiPackageBaseData._defaultAIIndex = aiIndexDic[item.Value];
            }

            item.Key._branchActionIndex = aiIndexDic[item.Value];
        }

        foreach(var item in aiExecuteEventDic)
        {
            if(aiIndexDic.ContainsKey(item.Key) == false)
            {
                DebugUtil.assert(false,"aiExecuteEvent target is not exists : {0}",item.Key);
                return null;
            }

            for(int i = 0; i < item.Value.Count; ++i)
            {
                item.Value[i].targetStateIndex = aiIndexDic[item.Key];
            }
            
        }

        if(nodeDataList.Count != 0 && aiPackageBaseData._defaultAIIndex == -1)
        {
            if(aiIndexDic.ContainsKey(defaultAIName) == false)
            {
                DebugUtil.assert(false, "invalid default state name: {0}",defaultAIName);
                return null;
            }

            aiPackageBaseData._defaultAIIndex = aiIndexDic[defaultAIName];
        }

        aiPackageBaseData._aiNodeCount = nodeDataList.Count;
        aiPackageBaseData._branchCount = branchDataList.Count;
        aiPackageBaseData._conditionCompareDataCount = compareDataList.Count;

        aiPackageBaseData._aiPackageNodeData = nodeDataList.ToArray();
        aiPackageBaseData._branchData = branchDataList.ToArray();
        aiPackageBaseData._conditionCompareData = compareDataList.ToArray();

        return aiPackageBaseData;
    }

    private static void readAIPackageTitle(XmlNode node, AIPackageBaseData baseData, out string defaultState)
    {
        defaultState = "";

        XmlAttributeCollection attributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
        {
            string targetName = attributes[attrIndex].Name;
            string targetValue = attributes[attrIndex].Value;

            if(targetName == "Name")
            {
                baseData._name = targetValue;
            }
            else if(targetName == "DefaultState")
            {
                defaultState = targetValue;
            }

        }
    }

    private static AIPackageNodeData readAIPackageNode(XmlNode node, ref Dictionary<ActionGraphBranchData, string> actionCompareDic,ref List<ActionGraphBranchData> branchDataList, ref List<ActionGraphConditionCompareData> compareDataList, in Dictionary<string, XmlNodeList> branchSetDic, ref Dictionary<string, List<AIEvent_ExecuteState>> aiExecuteEventDic)
    {
        AIPackageNodeData nodeData = new AIPackageNodeData();
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
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value, _aiPackageGlobalVariables);

            if(targetName == "UpdateTime")
            {
                nodeData._updateTime = float.Parse(targetValue);
            }
            else if(targetName == "TargetSearchType")
            {
                nodeData._targetSearchType = (TargetSearchType)System.Enum.Parse(typeof(TargetSearchType), targetValue);
            }
            else if(targetName == "TargetSearchRange")
            {
                nodeData._targetSearchRange = float.Parse(targetValue);
            }
            else if(targetName == "SearchIdentifier")
            {
                nodeData._searchIdentifier = (SearchIdentifier)System.Enum.Parse(typeof(SearchIdentifier), targetValue);
            }
            else if(targetName == "TargetPosition")
            {
                nodeData._hasTargetPosition = true;

                string[] xy = targetValue.Split(' ');
                nodeData._targetPosition = new Vector3(float.Parse(xy[0]), float.Parse(xy[1]),0f);

                Debug.Log(nodeData._targetPosition);
            }
            else if(targetName == "ArriveThreshold")
            {
                nodeData._arriveThreshold = float.Parse(targetValue);
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
            if(nodeList[i].Name == "Branch")
            {
                ActionGraphBranchData branchData = ActionGraphLoader.ReadActionBranch(nodeList[i],ref actionCompareDic,ref compareDataList, ref _aiPackageGlobalVariables);
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

                    ActionGraphBranchData branchData = ActionGraphLoader.ReadActionBranch(branchSetNodeList[branchSetNodeListIndex],ref actionCompareDic,ref compareDataList, ref _aiPackageGlobalVariables);
                    if(branchData == null)
                    {
                        DebugUtil.assert(false,"invalid branch data");
                        return null;
                    }

                    branchDataList.Add(branchData);
                }
            }
            else if(nodeList[i].Name.Contains("Event_"))
            {
                readAIChildEvent(nodeList[i], ref nodeData._aiEvents, ref aiExecuteEventDic);
            }
            
        }

        // if(branchStartIndex == branchDataList.Count)
        // {
        //     DebugUtil.assert(false,"branch data not exists");
        //     return null;
        // }

        nodeData._branchIndexStart = branchStartIndex;
        nodeData._branchCount = branchDataList.Count - branchStartIndex;

        return nodeData;
    }

    private static void readAIPackageChildEvent(XmlNode node, ref Dictionary<AIPackageEventType,AIChildFrameEventItem> childEventDic, ref Dictionary<string, List<AIEvent_ExecuteState>> aiExecuteEventDic)
    {
        string eventType = node.Name.Replace("PackageEvent_","");

        AIPackageEventType currentEventType = AIPackageEventType.Count;
        AIChildFrameEventItem item = new AIChildFrameEventItem();

        if(eventType == "OnExecute")
            currentEventType = AIPackageEventType.PackageEvent_OnExecute;
        else if(eventType == "OnExit")
            currentEventType = AIPackageEventType.PackageEvent_OnExit;

        DebugUtil.assert((int)AIPackageEventType.Count == 2, "check this");

        if(childEventDic.ContainsKey(currentEventType))
        {
            DebugUtil.assert(false,"eventtype overlap: {0}",currentEventType.ToString());
            return;
        }

        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Consume")
            {
                item._consume = bool.Parse(attrValue);
            }
        }

        List<AIEventBase> aiEventList = new List<AIEventBase>();

        XmlNodeList childNodes = node.ChildNodes;
        for(int i = 0; i < childNodes.Count; ++i)
        {
            aiEventList.Add(readAiEvent(childNodes[i], ref aiExecuteEventDic));
        }

        item._childFrameEventCount = aiEventList.Count;
        item._childFrameEvents = aiEventList.ToArray();

        childEventDic.Add(currentEventType, item);
    }

    private static void readAIChildEvent(XmlNode node, ref Dictionary<AIChildEventType,AIChildFrameEventItem> childEventDic, ref Dictionary<string, List<AIEvent_ExecuteState>> aiExecuteEventDic)
    {
        string eventType = node.Name.Replace("Event_","");

        AIChildEventType currentEventType = AIChildEventType.Count;
        AIChildFrameEventItem item = new AIChildFrameEventItem();

        if(eventType == "OnExecute")
            currentEventType = AIChildEventType.AIChildEvent_OnExecute;
        else if(eventType == "OnAttack")
            currentEventType = AIChildEventType.AIChildEvent_OnAttack;
        else if(eventType == "OnAttacked")
            currentEventType = AIChildEventType.AIChildEvent_OnAttacked;
        else if(eventType == "OnExit")
            currentEventType = AIChildEventType.AIChildEvent_OnExit;
        else if(eventType == "OnFrame")
            currentEventType = AIChildEventType.AIChildEvent_OnFrame;
        else if(eventType == "OnUpdate")
            currentEventType = AIChildEventType.AIChildEvent_OnUpdate;
        else if(eventType == "OnGuard")
            currentEventType = AIChildEventType.AIChildEvent_OnGuard;
        else if(eventType == "OnGuarded")
            currentEventType = AIChildEventType.AIChildEvent_OnGuarded;
        else if(eventType == "OnParry")
            currentEventType = AIChildEventType.AIChildEvent_OnParry;
        else if(eventType == "OnParried")
            currentEventType = AIChildEventType.AIChildEvent_OnParried;
        else if(eventType == "OnEvade")
            currentEventType = AIChildEventType.AIChildEvent_OnEvade;
        else if(eventType == "OnEvaded")
            currentEventType = AIChildEventType.AIChildEvent_OnEvaded;
        else if(eventType == "OnGuardBreak")
            currentEventType = AIChildEventType.AIChildEvent_OnGuardBreak;
        else if(eventType == "OnGuardBroken")
            currentEventType = AIChildEventType.AIChildEvent_OnGuardBroken;

        DebugUtil.assert((int)AIChildEventType.Count == 14, "check this");

        if(childEventDic.ContainsKey(currentEventType))
        {
            DebugUtil.assert(false,"eventtype overlap: {0}, {1}",currentEventType.ToString(), eventType);
            return;
        }

        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Consume")
            {
                item._consume = bool.Parse(attrValue);
            }
        }

        List<AIEventBase> aiEventList = new List<AIEventBase>();

        XmlNodeList childNodes = node.ChildNodes;
        for(int i = 0; i < childNodes.Count; ++i)
        {
            aiEventList.Add(readAiEvent(childNodes[i], ref aiExecuteEventDic));
        }

        item._childFrameEventCount = aiEventList.Count;
        item._childFrameEvents = aiEventList.ToArray();

        
        childEventDic.Add(currentEventType, item);
    }

    public static AIEventBase readAiEvent(XmlNode node, ref Dictionary<string, List<AIEvent_ExecuteState>> aiExecuteEventDic)
    {
        if(node.Name != "AIEvent")
        {
            DebugUtil.assert(false,"target node is not aiEvent: {0}",node.Name);
            return null;
        }

        XmlAttributeCollection attributes = node.Attributes;
        AIEventBase aiEvent = null;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Type")
            {
                if(attrValue == "Test")
                {
                    aiEvent = new AIEvent_Test();
                }
                else if(attrValue == "SetAngleDirection")
                {
                    aiEvent = new AIEvent_SetAngleDirection();
                }
                else if(attrValue == "SetDirectionToTarget")
                {
                    aiEvent = new AIEvent_SetDirectionToTarget();
                }
                else if(attrValue == "SetAction")
                {
                    aiEvent = new AIEvent_SetAction();
                }
                else if(attrValue == "ClearTarget")
                {
                    aiEvent = new AIEvent_ClearTarget();
                }
                else if(attrValue == "ExecuteState")
                {
                    aiEvent = new AIEvent_ExecuteState();
                }
                else if(attrValue == "TerminatePackage")
                {
                    aiEvent = new AIEvent_TerminatePackage();
                }
                else if(attrValue == "KillEntity")
                {
                    aiEvent = new AIEvent_KillEntity();
                }
                else
                {
                    DebugUtil.assert(false,"invalid ai event type: {0}",attrValue);
                    return null;
                }
            }
        }

        if(aiEvent == null)
        {
            return null;
        }

        if(aiEvent.getFrameEventType() == AIEventType.AIEvent_ExecuteState)
        {
            bool find = false;
            for(int i = 0; i < attributes.Count; ++i)
            {
                string attrName = attributes[i].Name;
                string attrValue = attributes[i].Value;

                if(attrName != "Execute")
                    continue;

                if(aiExecuteEventDic.ContainsKey(attrValue) == false)
                    aiExecuteEventDic.Add(attrValue,new List<AIEvent_ExecuteState>());

                aiExecuteEventDic[attrValue].Add((AIEvent_ExecuteState)aiEvent);
                find = true;
                break;
            }

            if(find == false)
            {
                DebugUtil.assert(false, "Execute State not found");
                return null;
            }
        }
        aiEvent.loadFromXML(node);
        return aiEvent;
    }

}
