using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;
using System;

public abstract class LoaderBase<T> where T : SerializableDataType
{
    public abstract T readFromXML(string path);
#if UNITY_EDITOR
    public virtual void readFromXMLAndExportToBinary(string xmlPath, string binaryOutputPath)
    {
        if(binaryOutputPath == null || binaryOutputPath == "")
        {
            DebugUtil.assert(false,"Binary Output Path is null [{0}]", xmlPath);
            return;
        }

        T data = readFromXML(xmlPath);
        if(data == null)
            return;

        data.writeData(binaryOutputPath);
    }
#endif
    public abstract T createNewDataInstance();

    public T readFromBinary(string path)
    {
        if(File.Exists(path) == false)
        {
            DebugUtil.assert(false,"file does not exists : {0}", path);
            return null;
        }

        T data = createNewDataInstance();

        data.readData(path);
        return data;
    }
}
