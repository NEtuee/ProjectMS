using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class Console
{
    private static ConsoleCommand _commandInstance = null;
    private static Action<string> _logCallBack = null;

    public static void Execute(string commandName)
    {
        string[] split = commandName.Split(' ');
        if (split.Length <= 0)
        {
            return;
        }
        
        var instance = GetCommandInstance();
        
        MethodInfo methodInfo = typeof(ConsoleCommand).GetMethod(split[0]);

        if (methodInfo == null)
        {
            return;
        }

        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        if (parameterInfos.Length <= 0)
        {
            methodInfo.Invoke(instance, null);
            return;
        }

        List<object> parameterObjectList = new List<object>(split.Length - 1);
        
        for (int i = 0; i < parameterInfos.Length; i++)
        {
            if (i + 1 >= split.Length)
            {
                return;
            }
            
            parameterObjectList.Add(Convert.ChangeType(split[i + 1], parameterInfos[i].ParameterType));
        }

        methodInfo.Invoke(instance, parameterObjectList.ToArray());
    }

    public static void SetLogCallBack(Action<string> logCallBack)
    {
        _logCallBack = logCallBack;
    }

    public static void ConsoleLog(string logText)
    {
        _logCallBack?.Invoke(logText);
    }

    private static ConsoleCommand GetCommandInstance()
    {
        if (_commandInstance != null)
        {
            return _commandInstance;
        }

        _commandInstance = new ConsoleCommand();
        return _commandInstance;
    }
}

