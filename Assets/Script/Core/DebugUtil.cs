#if UNITY_EDITOR
using UnityEditor;
using System.Diagnostics;
#endif

public static class DebugUtil
{
    public static bool _ignoreAssert = false;

    public static void log(string errorLog, params System.Object[] errorArgs)
    {
        string resultString = string.Format("{0}",string.Format(errorLog,errorArgs));
        UnityEngine.Debug.Log(resultString);
    }

    public static bool assert(bool condition, string errorLog, params System.Object[] errorArgs)
    {
#if UNITY_EDITOR
        if(condition)
            return true;

        if(_ignoreAssert == false)
        {
            StackFrame stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);
            string resultString = string.Format("{0}\n\n{1} ({2})",string.Format(errorLog,errorArgs),stackFrame.GetFileName(), stackFrame.GetFileLineNumber());
                
            bool result = EditorUtility.DisplayDialog("Assert",resultString,"Throw Exception","Ignore");
    
            if(result == true)
            {
                UnityEngine.Debug.Break();
                throw new System.Exception(resultString);
            }
        }
        
#else
        return true;

#endif       
        return false;     
    }

}

