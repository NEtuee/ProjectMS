using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class AIGraphLoader : LoaderBase<AIGraphBaseData>
{

    private static string _aiPackageRoot = "Assets\\Data\\AIPackage\\";

    private static Dictionary<string, string> _aiGraphGlobalVariables = new Dictionary<string, string>();
    private static Dictionary<string, string> _aiPackageGlobalVariables = new Dictionary<string, string>();
    private static Dictionary<string, AIPackageBaseData> _loadedAiPackage = new Dictionary<string, AIPackageBaseData>();


    private static string _currentFileName = "";
    private static string _currentPackageFileName = "";
    public override AIGraphBaseData readFromXML(string path)
    {
        _currentFileName = path;
        PositionXmlDocument xmlDoc = new PositionXmlDocument();
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
            DebugUtil.assert(false,"xml 로드 예외 : {0}",ex.Message);
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
            DebugUtil.assert(false,"잘못된 xml 타입. name : [FileName: {0}] [NodeName: {0}]",_currentFileName, node.Name);
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
                ActionGraphLoader.readBranchSet(nodeList[i],ref branchSetDic,path);
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
                    DebugUtil.assert(false,"Include할 Path가 없습니다. 왜씀?? [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(nodeList[i]), _currentFileName);
                    return null;
                }
                else if(nodeList[i].Attributes[0].Name != "Path")
                {
                    DebugUtil.assert(false,"첫번쨰 어트리뷰트는 무조건 Path여야 합니다. [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(nodeList[i]), _currentFileName);
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
                DebugUtil.assert(false,"NodeData가 null입니다. 로드 실패! : [NodeName: {0}] [Line: {1}] [FileName: {2}]", nodeList[i].Name, XMLScriptConverter.getLineFromXMLNode(nodeList[i]), _currentFileName);
                return null;
            }

            nodeDataList.Add(nodeData);
            actionIndexDic.Add(nodeData._nodeName,actionIndex++);
        }

        foreach(var item in actionCompareDic)
        {
            if(actionIndexDic.ContainsKey(item.Value) == false)
            {
                DebugUtil.assert(false,"대상 액션이 존재하지 않습니다. : [Action: {0}] [FileName: {1}]",item.Value, _currentFileName);
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
                DebugUtil.assert(false, "디폴트 스테이트가 존재하지 않습니다. 이름에 오타는 없나요? : [StateName: {0}] [FileName: {1}]",defaultAiNodeName, _currentFileName);
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
                    DebugUtil.assert(false, "해당 AI Package가 존재하지 않습니다. 오타는 없나요? : [Package: {0}] [Line: {1}] [FileName: {2}]",targetValue, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                    return null;
                }

                nodeData._packageIndex = aiPackageIndexDic[targetValue];
            }
            else
            {
                DebugUtil.assert(false,"유효하지 않은 어트리뷰트입니다. !!! : [Type: {0}] [Line: {1}] [FileName: {2}]", targetName, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
            }
        }

        XmlNodeList nodeList = node.ChildNodes;
        int branchStartIndex = branchDataList.Count;

        Dictionary<string, List<AIEvent_ExecuteState>> aiExecuteStateDic = new Dictionary<string, List<AIEvent_ExecuteState>>();

        for(int i = 0; i < nodeList.Count; ++i)
        {
            
            if(nodeList[i].Name == "Branch")
            {
                ActionGraphBranchData branchData = ActionGraphLoader.ReadActionBranch(nodeList[i],ref actionCompareDic,ref compareDataList, ref _aiGraphGlobalVariables, _currentFileName);
                if(branchData == null)
                {
                    DebugUtil.assert(false,"유효하지 않은 브랜치 데이터입니다. [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
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
                    DebugUtil.assert(false, "해당 브랜치 셋이 존재하지 않습니다. 오타가 있지는 않나요? : [BranchSetName: {0}] [Line: {1}] [FileName: {2}]",branchSetName, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                    return null;
                }

                XmlNodeList branchSetNodeList = branchSetDic[branchSetName];
                for(int branchSetNodeListIndex = 0; branchSetNodeListIndex < branchSetNodeList.Count; ++branchSetNodeListIndex)
                {
                    if(branchSetNodeList[branchSetNodeListIndex].Name != "Branch")
                    {
                        DebugUtil.assert(false, "잘못된 브랜치 타입입니다. 오타가 있지는 않나요? : [BranchType: {0}] [Line: {1}] [FileName: {2}]",branchSetNodeList[branchSetNodeListIndex].Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                        return null;
                    }

                    ActionGraphBranchData branchData = ActionGraphLoader.ReadActionBranch(branchSetNodeList[branchSetNodeListIndex],ref actionCompareDic,ref compareDataList, ref _aiGraphGlobalVariables,_currentFileName);
                    if(branchData == null)
                    {
                        DebugUtil.assert(false,"유효하지 않은 브랜치 데이터 입니다. [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
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
                DebugUtil.assert(false, "유효하지 않은 어트리뷰트 입니다. 오타는 아닌가요? : [Name: {0}] [Line: {1}] [FileName: {2}]",nodeList[i].Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
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
                DebugUtil.assert(false, "유효하지 않은 어트리뷰트 입니다. 오타는 아닌가요? : [Attribute: {0}] [Line: {1}] [FileName: {2}]",targetName, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
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
            DebugUtil.assert(false, "잘못된 글로벌 변수명 입니다. 오타는 아닌가요?, name:[{0}] value:[{1}] [Line: {2}] [FileName: {3}]",name,value,XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
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
        _currentPackageFileName = path;
        PositionXmlDocument xmlDoc = new PositionXmlDocument();
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
            DebugUtil.assert(false,"잘못된 xml 타입. name : {0}",node.Name);
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
                ActionGraphLoader.readBranchSet(nodeList[i],ref branchSetDic,_currentPackageFileName);
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
                        DebugUtil.assert(false,"AIPackage 스테이트가 비정상입니다. : [Name: {0}] [Line: {1}] [FileName: {2}]",aiStateNodeList[index].Name,XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
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
                DebugUtil.assert(false, "유효하지 않은 어트리뷰트 입니다. 오타는 아닌가요?: [Attribute: {0}] [Line: {1}] [FileName: {2}]",nodeList[i].Name,XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
                return null;
            }
            
        }

        foreach(var item in actionCompareDic)
        {
            if(aiIndexDic.ContainsKey(item.Value) == false)
            {
                DebugUtil.assert(false,"대상 스테이트가 존재하지 않습니다. 오타는 아닌가요? : {0} [FileName: {1}]",item.Value, _currentPackageFileName);
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
                DebugUtil.assert(false,"aiExecuteEvent의 대상이 존재하지 않습니다. 오타는 아닌가요? : {0} [FileName: {1}]",item.Key, _currentPackageFileName);
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
                DebugUtil.assert(false, "디폴트 스테이트가 존재하지 않습니다. 오타는 아닌가요? : {0} [FileName: {1}]",defaultAIName, _currentPackageFileName);
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
            else if(targetName == "TargetSearchStartRange")
            {
                nodeData._targetSearchStartRange = float.Parse(targetValue);
            }
            else if(targetName == "TargetSearchSphereRadius")
            {
                nodeData._targetSearchSphereRadius = float.Parse(targetValue);
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
            }
            else if(targetName == "ArriveThreshold")
            {
                nodeData._arriveThreshold = float.Parse(targetValue);
            }
            else
            {
                DebugUtil.assert(false,"유효하지 않은 어트리뷰트 입니다. 오타는 아닌가요? : [Attribute: {0}] [Line: {1}] [FileName: {2}]", targetName, XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
            }
        }

        XmlNodeList nodeList = node.ChildNodes;
        int branchStartIndex = branchDataList.Count;

        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "Branch")
            {
                ActionGraphBranchData branchData = ActionGraphLoader.ReadActionBranch(nodeList[i],ref actionCompareDic,ref compareDataList, ref _aiPackageGlobalVariables,_currentPackageFileName);
                if(branchData == null)
                {
                    DebugUtil.assert(false,"유효하지 않은 브랜치 데이터 입니다. [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
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
                    DebugUtil.assert(false, "해당 브랜치 셋은 존재하지 않습니다. : [BranchSetName: {0}] [Line: {1}] [FileName: {2}]",branchSetName, XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
                    return null;
                }

                XmlNodeList branchSetNodeList = branchSetDic[branchSetName];
                for(int branchSetNodeListIndex = 0; branchSetNodeListIndex < branchSetNodeList.Count; ++branchSetNodeListIndex)
                {
                    if(branchSetNodeList[branchSetNodeListIndex].Name != "Branch")
                    {
                        DebugUtil.assert(false, "잘못된 브랜치 타입입니다. 오타는 아닌가요? : [Type: {0}] [Line: {1}] [FileName: {2}]",branchSetNodeList[branchSetNodeListIndex].Name, XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
                        return null;
                    }

                    ActionGraphBranchData branchData = ActionGraphLoader.ReadActionBranch(branchSetNodeList[branchSetNodeListIndex],ref actionCompareDic,ref compareDataList, ref _aiPackageGlobalVariables,_currentPackageFileName);
                    if(branchData == null)
                    {
                        DebugUtil.assert(false,"유효하지 않은 브랜치 데이터 입니다. [Line: {0}] [FileName: {1}]", XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
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
            DebugUtil.assert(false,"중복된 이벤트 타입 입니다. 하나만 남겨 주세요 : {0} [Line: {1}] [FileName: {2}]",currentEventType.ToString(), XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
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
        else if(eventType == "OnGuardBreakFail")
            currentEventType = AIChildEventType.AIChildEvent_OnGuardBreakFail;
        else if(eventType == "OnAttackGuardBreakFail")
            currentEventType = AIChildEventType.AIChildEvent_OnAttackGuardBreakFail;
        else if(eventType == "OnCatchTarget")
            currentEventType = AIChildEventType.AIChildEvent_OnCatchTarget;
        else if(eventType == "OnCatched")
            currentEventType = AIChildEventType.AIChildEvent_OnCatched;

        DebugUtil.assert((int)AIChildEventType.Count == 18, "check this");

        if(childEventDic.ContainsKey(currentEventType))
        {
            DebugUtil.assert(false,"중복된 이벤트 타입 입니다. 하나만 남겨 주세요: {0}, {1} [Line: {2}] [FileName: {3}]",currentEventType.ToString(), eventType, XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
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
            DebugUtil.assert(false,"target node is not aiEvent: {0} [Line: {1}] [FileName: {2}]",node.Name, XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
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
                else if(attrValue == "RotateDirection")
                {
                    aiEvent = new AIEvent_RotateDirection();
                }
                else
                {
                    DebugUtil.assert(false,"유효하지 않은 AI 이벤트 타입 입니다. 오타는 아닌가요?: [{0}] [Line: {1}] [FileName: {2}]",attrValue, XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
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
            string targetAction = "";
            for(int i = 0; i < attributes.Count; ++i)
            {
                string attrName = attributes[i].Name;
                string attrValue = attributes[i].Value;

                Debug.Log(attrName);

                if(attrName != "Execute")
                    continue;

                targetAction = attrValue;

                if(aiExecuteEventDic.ContainsKey(attrValue) == false)
                    aiExecuteEventDic.Add(attrValue,new List<AIEvent_ExecuteState>());

                aiExecuteEventDic[attrValue].Add((AIEvent_ExecuteState)aiEvent);
                find = true;
                break;
            }

            if(find == false)
            {
                DebugUtil.assert(false, "Execute할 State가 존재하지 않습니다. 오타는 아닌가요? : [{0}] [Line: {1}] [FileName: {2}]",targetAction, XMLScriptConverter.getLineFromXMLNode(node), _currentPackageFileName);
                return null;
            }
        }
        aiEvent.loadFromXML(node);
        return aiEvent;
    }

}
