using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AkaneSequencerGraph
{
    public static class GraphFile
    {
        public static void GraphToXmlFile(AkaneSequencerGraphView graphView, string fileName) 
        {
            if (graphView == null)
            {
                return;
            }

            var filePath = Path.Combine(Application.dataPath, "Script/Editor/AkaneSequencerEditor");
            if (Directory.Exists(filePath) == false)
            {
                Debug.LogError($"{filePath} : 해당 경로 없음");
                return;
            }

            var targetPath = Path.Combine(filePath, $"{fileName}.xml");

            var sb = new StringBuilder();

            var phaseContextSpace = "    ";
            var eventContextSpace = "          ";
            
            sb.AppendLine($"<SequencerGraph Name = \"{fileName}\">");
            sb.AppendLine();

            var (initPhaseNode, initPhaseChildNodeList) = graphView.GetInitializePhase();
            AddPhaseContext(initPhaseNode, initPhaseChildNodeList);

            var (updatePhaseNode, updatePhaseChildNodeList) = graphView.GetUpdatePhase();
            AddPhaseContext(updatePhaseNode, updatePhaseChildNodeList);

            var (endPhaseNode, endPhaseChildNodeList) = graphView.GetEndPhase();
            AddPhaseContext(endPhaseNode, endPhaseChildNodeList);

            sb.AppendLine($"</SequencerGraph>");

            if (File.Exists(targetPath) == false)
            {
                var file = File.CreateText(targetPath);
                file.Close();
            }

            var streamWriter = new StreamWriter(targetPath);
            streamWriter.Write(sb.ToString());
            streamWriter.Flush();
            streamWriter.Close();
            
            void AddPhaseContext(ReservedPhaseNode phaseNode, List<EventNode> childNodeList)
            {
                if (phaseNode == null)
                {
                    return;
                }

                sb.Append(phaseContextSpace);
                sb.AppendLine(phaseNode.GetOpenContext());
                if (childNodeList != null)
                {
                    foreach (var eventNode in childNodeList)
                    {
                        if (eventNode.title == "TaskCall")
                        {
                            if (eventNode is TaskCallNode taskCallNode)
                            {
                                var taskNode = graphView.GetTaskEventNode(taskCallNode.TaskKey);
                                if (taskNode != null)
                                {
                                    sb.AppendLine(taskNode.GetContext());
                                }
                            }
                        }
                        else
                        {
                            sb.Append(eventContextSpace);
                            sb.AppendLine(eventNode.GetResultContext());   
                        }
                    }
                }

                sb.Append(phaseContextSpace);
                sb.AppendLine(phaseNode.GetCloseContext());
                sb.AppendLine();
            }
        }
    }
}
