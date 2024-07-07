using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class BinaryHelper
{
    public static Vector3 readVector3(ref BinaryReader binaryReader)
    {
        Vector3 data = new Vector3();
        data.x = binaryReader.ReadSingle();
        data.y = binaryReader.ReadSingle();
        data.z = binaryReader.ReadSingle();

        return data;
    }

    public static void writeVector3(ref BinaryWriter binaryWriter, Vector3 vector3)
    {
        binaryWriter.Write(vector3.x);
        binaryWriter.Write(vector3.y);
        binaryWriter.Write(vector3.z);
    }
}