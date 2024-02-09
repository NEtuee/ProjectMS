using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextTest : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public RectTransform RectTransform;
    public Image Image;

    private void Update()
    {
        RectTransform.sizeDelta = new Vector2(Text.preferredWidth, Text.preferredHeight);
        Image.rectTransform.sizeDelta =  new Vector2(Text.preferredWidth * 1.1f, Text.preferredHeight);
    }
}
