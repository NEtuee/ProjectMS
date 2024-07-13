using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class BinaryHelper
{
    public static void writeEnum<T>(ref BinaryWriter binaryWriter, T data) where T : Enum
    {
        binaryWriter.Write((int)(object)data);
    }

    public static T readEnum<T>(ref BinaryReader binaryReader) where T : Enum
    {
        int enumValue = binaryReader.ReadInt32();
        return (T)Enum.ToObject(typeof(T), enumValue);
    }

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

    public static void writeQuaternion(ref BinaryWriter binaryWriter, Quaternion quaternion)
    {
        binaryWriter.Write(quaternion.x);
        binaryWriter.Write(quaternion.y);
        binaryWriter.Write(quaternion.z);
        binaryWriter.Write(quaternion.w);
    }

    public static Quaternion readQuaternion(ref BinaryReader binaryReader)
    {
        float x,y,z,w;
        x = binaryReader.ReadSingle();
        y = binaryReader.ReadSingle();
        z = binaryReader.ReadSingle();
        w = binaryReader.ReadSingle();

        return new Quaternion(x,y,z,w);
    }

    public static Vector2 readVector2(ref BinaryReader binaryReader)
    {
        Vector2 data = new Vector2();
        data.x = binaryReader.ReadSingle();
        data.y = binaryReader.ReadSingle();

        return data;
    }

    public static void writeVector2(ref BinaryWriter binaryWriter, Vector2 vector2)
    {
        binaryWriter.Write(vector2.x);
        binaryWriter.Write(vector2.y);
    }

    

    public static Color readColor(ref BinaryReader binaryReader)
    {
        Color data = new Color();
        data.r = binaryReader.ReadSingle();
        data.g = binaryReader.ReadSingle();
        data.b = binaryReader.ReadSingle();
        data.a = binaryReader.ReadSingle();

        return data;
    }

    public static void writeColor(ref BinaryWriter binaryWriter, Color color)
    {
        binaryWriter.Write(color.r);
        binaryWriter.Write(color.g);
        binaryWriter.Write(color.b);
        binaryWriter.Write(color.a);
    }
#if UNITY_EDITOR
    public static void writeArray<T>(ref BinaryWriter binaryWriter, T[] dataArray) where T : SerializableDataType
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                dataArray[i].serialize(ref binaryWriter);
            }
        }
    }

    public static void writeArrayStructure<T>(ref BinaryWriter binaryWriter, T[] dataArray) where T : struct, SerializableStructure
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                dataArray[i].serialize(ref binaryWriter);
            }
        }
    }
#endif
    public static void writeArray(ref BinaryWriter binaryWriter, int[] dataArray)
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                binaryWriter.Write(dataArray[i]);
            }
        }
    }

    public static void writeArray(ref BinaryWriter binaryWriter, float[] dataArray)
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                binaryWriter.Write(dataArray[i]);
            }
        }
    }

    public static void writeArray(ref BinaryWriter binaryWriter, bool[] dataArray)
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                binaryWriter.Write(dataArray[i]);
            }
        }
    }

    public static void writeArray(ref BinaryWriter binaryWriter, string[] dataArray)
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                if(dataArray[i] == null)
                    binaryWriter.Write("");
                else
                    binaryWriter.Write(dataArray[i]);
            }
        }
    }

    public static void writeArray(ref BinaryWriter binaryWriter, Vector2[] dataArray)
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                writeVector2(ref binaryWriter, dataArray[i]);
            }
        }
    }

    public static void writeArray(ref BinaryWriter binaryWriter, Vector3[] dataArray)
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                writeVector3(ref binaryWriter, dataArray[i]);
            }
        }
    }

    public static void writeEnumArray<T>(ref BinaryWriter binaryWriter, T[] dataArray) where T : Enum
    {
        binaryWriter.Write(dataArray == null ? 0 : dataArray.Length);
        if(dataArray != null)
        {
            for(int i = 0; i < dataArray.Length; ++i)
            {
                writeEnum<T>(ref binaryWriter, dataArray[i]);
            }
        }
    }

    public static T[] readArray<T>(ref BinaryReader binaryReader) where T : SerializableDataType, new()
    {
        int count = binaryReader.ReadInt32();
        T[] dataArray = count == 0 ? null : new T[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = new T();
            dataArray[i].deserialize(ref binaryReader);
        }

        return dataArray;
    }

    public static T[] readArrayStructure<T>(ref BinaryReader binaryReader) where T : struct, SerializableStructure
    {
        int count = binaryReader.ReadInt32();
        T[] dataArray = count == 0 ? null : new T[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = new T();
            dataArray[i].deserialize(ref binaryReader);
        }

        return dataArray;
    }

    public static int[] readArrayInt(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        int[] dataArray = count == 0 ? null : new int[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = binaryReader.ReadInt32();
        }

        return dataArray;
    }

    public static float[] readArrayFloat(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        float[] dataArray = count == 0 ? null : new float[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = binaryReader.ReadSingle();
        }

        return dataArray;
    }

    public static bool[] readArrayBool(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        bool[] dataArray = count == 0 ? null : new bool[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = binaryReader.ReadBoolean();
        }

        return dataArray;
    }

    public static string[] readArrayString(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        string[] dataArray = count == 0 ? null : new string[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = binaryReader.ReadString();
        }

        return dataArray;
    }

    public static Vector2[] readArrayVector2(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        Vector2[] dataArray = count == 0 ? null : new Vector2[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = readVector2(ref binaryReader);
        }

        return dataArray;
    }

    public static Vector3[] readArrayVector3(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        Vector3[] dataArray = count == 0 ? null : new Vector3[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = readVector3(ref binaryReader);
        }

        return dataArray;
    }

    public static T[] readArrayEnum<T>(ref BinaryReader binaryReader) where T : Enum
    {
        int count = binaryReader.ReadInt32();
        T[] dataArray = count == 0 ? null : new T[count];
        for(int i = 0; i < count; ++i)
        {
            dataArray[i] = readEnum<T>(ref binaryReader);
        }

        return dataArray;
    }
}