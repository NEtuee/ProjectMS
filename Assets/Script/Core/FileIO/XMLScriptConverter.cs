using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using System.Text;
using UnityEngine;

public static class XMLScriptConverter
{
    public static MemoryStream convertXMLScriptSymbol(string path)
    {
        string xmlFile = "";
        try
        {
            xmlFile = File.ReadAllText(path);
        }
        catch(Exception exp)
        {
            DebugUtil.assert(false,"failed to load xml file\n{0}\n{1}",path,exp.Message);
            return null;
        }

        xmlFile = xmlFile.Replace("~","\"");

        int startOffset = 0;
        int endOffset = 0;
        while(true)
        {
            if(xmlFile.Length <= startOffset)
                break;

            int offset = xmlFile.IndexOf('"',startOffset);
            if(offset == -1)
            {
                break;
            }

            startOffset = offset + 1;
            endOffset = xmlFile.IndexOf('"',startOffset);
            if(offset == -1)
            {
                DebugUtil.assert(false,"invalid file: {0}",offset);
                return null;
            }

            for(int i = startOffset; i < endOffset; ++i)
            {
                if(xmlFile[i] == '&')
                {
                    xmlFile = xmlFile.Remove(i,1).Insert(i,"&#38;");
                    endOffset += 4;
                    i+=4;
                }
                else if(xmlFile[i] == '>')
                {
                    xmlFile = xmlFile.Remove(i,1).Insert(i,"&gt;");
                    endOffset += 3;
                    i+=3;
                }
                else if(xmlFile[i] == '<')
                {
                    xmlFile = xmlFile.Remove(i,1).Insert(i,"&lt;");
                    endOffset += 3;
                    i+=3;
                }
            }

            startOffset = endOffset + 1;
        }

        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlFile));
    }
}