using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR

public class UnityWebRequestHelper : MonoBehaviour
{
    public static string _webHookDEV = "https://discord.com/api/webhooks/1093591505328943278/cRZ5tPmJA0QkrKSVz1jhurtpWBoSETt8WtYkNNqIgZM5ko3yAJ0148qkpk6JYwamNnK7";
    public static string _webHookCHAT = "https://discord.com/api/webhooks/1172906772785279177/O-JvdNhqtg-c8-McaDrIA4wizlJ7gwBe-5zb-XvAagW0AiIuJ-GL5zlPtQ_QDURASro8";

    public void sendWebHook(string webHook, string message)
    {
        StartCoroutine(sendWebHookInner(webHook, message));
    }

    private IEnumerator sendWebHookInner(string link, string message)
    {
        WWWForm form = new WWWForm();
        form.AddField("content", message);
        using (UnityWebRequest request = UnityWebRequest.Post(link,form))
        {
            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.DataProcessingError ||
                request.result == UnityWebRequest.Result.ProtocolError )
            {
                UnityEngine.Debug.Log("Web Requset Error : " + request.error);
            }
        }

        DestroyImmediate(gameObject);
    }


}
#endif
