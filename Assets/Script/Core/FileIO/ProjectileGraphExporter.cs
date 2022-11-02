using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;


public static class ProjectileGraphLoader
{
    public static ProjectileGraphBaseData[] readFromXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(path,readerSettings))
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

        List<ProjectileGraphBaseData> projectileBaseDataList = new List<ProjectileGraphBaseData>();

        XmlNodeList projectileNodes = xmlDoc.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
        {
            ProjectileGraphBaseData baseData = readProjectile(projectileNodes[nodeIndex]);
            if(baseData == null)
                return null;

            projectileBaseDataList.Add(baseData);
        }

        return projectileBaseDataList.ToArray();
    }

    private static ProjectileGraphBaseData readProjectile(XmlNode node)
    {
        if(node.Name.Equals("ProjectileGraph") == false)
        {
            DebugUtil.assert(false,"wrong xml type. name : {0}",node.Name);
            return null;
        }

        float defaultFramePerSecond = 0f;

        ProjectileGraphBaseData projectileBaseData = new ProjectileGraphBaseData();
        readTitle(node,projectileBaseData,out defaultFramePerSecond);

        List<AnimationPlayDataInfo> animationPlayDataInfos = new List<AnimationPlayDataInfo>();

        XmlNodeList nodes = node.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
        {
            string targetName = nodes[nodeIndex].Name;
            string targetValue = nodes[nodeIndex].Value;

            if(targetName == "Animation")
            {
                AnimationPlayDataInfo playData = ActionGraphLoader.ReadActionAnimation(nodes[nodeIndex],defaultFramePerSecond);
                if(playData == null)
                {
                    DebugUtil.assert(false,"invalid animation : {0}",node.Name);
                    return null;
                }

                animationPlayDataInfos.Add(playData);
            }
            else if(targetName == "DefaultShotInfo")
            {
                projectileBaseData._defaultProjectileShotInfoData = readDefaultShotInfo(nodes[nodeIndex]);
            }
            else if(targetName == "Event")
            {
                readChildFrameEvent(nodes[nodeIndex],projectileBaseData);
            }
                
        }

        projectileBaseData._animationPlayData = animationPlayDataInfos.ToArray();

        return projectileBaseData;
    }

    private static ProjectileGraphShotInfoData readDefaultShotInfo(XmlNode node)
    {
        ProjectileGraphShotInfoData shotInfoData = new ProjectileGraphShotInfoData();
        XmlAttributeCollection attributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
        {
            string targetName = attributes[attrIndex].Name;
            string targetValue = attributes[attrIndex].Value;

            if(targetName == "Velocity")
            {
                shotInfoData._deafaultVelocity = float.Parse(targetValue);
            }
            else if(targetName == "Acceleration")
            {
                shotInfoData._acceleration = float.Parse(targetValue);
            }
            else if(targetName == "Friction")
            {
                shotInfoData._friction = float.Parse(targetValue);
            }
            else if(targetName == "Angle")
            {
                shotInfoData._defaultAngle = float.Parse(targetValue);
            }
            else if(targetName == "AngularAcceleration")
            {
                shotInfoData._angularAcceleration = float.Parse(targetValue);
            }
            else if(targetName == "LifeTime")
            {
                shotInfoData._lifeTime = float.Parse(targetValue);
            }
        }

        return shotInfoData;
    }
    private static void readTitle(XmlNode node, ProjectileGraphBaseData baseData, out float defaultFPS)
    {
        defaultFPS = -1f;

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
            else if(targetName == "PenetrateCount")
            {
                baseData._penetrateCount = int.Parse(targetValue);
            }
            else
            {
                DebugUtil.assert(false,"invalid Attribute : {0}", targetName);
            }

        }
    }

    private static void readChildFrameEvent(XmlNode node, ProjectileGraphBaseData baseData)
    {
        XmlNodeList childNodeList = node.ChildNodes;

        if(childNodeList == null || childNodeList.Count == 0)
            return;

        Dictionary<ProjectileChildFrameEventType, ChildFrameEventItem> childFrameEventList = new Dictionary<ProjectileChildFrameEventType, ChildFrameEventItem>();

        for(int i = 0; i < childNodeList.Count; ++i)
        {
            string targetName = childNodeList[i].Name;

            ChildFrameEventItem childItem = new ChildFrameEventItem();
            ProjectileChildFrameEventType eventType = ProjectileChildFrameEventType.Count;

            if(targetName == "OnHit")
                eventType = ProjectileChildFrameEventType.ChildFrameEvent_OnHit;
            else if(targetName == "OnHitEnd")
                eventType = ProjectileChildFrameEventType.ChildFrameEvent_OnHitEnd;
            else if(targetName == "OnEnd")
                eventType = ProjectileChildFrameEventType.ChildFrameEvent_OnEnd;

            List<ActionFrameEventBase> actionFrameEventList = new List<ActionFrameEventBase>();
            XmlNodeList childNodes = childNodeList[i].ChildNodes;
            for(int j = 0; j < childNodes.Count; ++j)
            {
                actionFrameEventList.Add(FrameEventLoader.readFromXMLNode(childNodes[j]));
            }

            childItem._childFrameEventCount = actionFrameEventList.Count;
            childItem._childFrameEvents = actionFrameEventList.ToArray();

            childFrameEventList.Add(eventType, childItem);
        }

        baseData._projectileChildFrameEvent = childFrameEventList;
    }

}
