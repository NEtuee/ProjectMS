using UnityEngine;

[System.Serializable]
public class ExpressionData
{
    public ExpressionType _type;
    public Sprite _baseSprite;
    public Sprite[] _eyeAnimation;
    public Sprite[] _talkAnimation;

    public float _eyeFPS;
    public float _mouthFPS;
    public int[] _eyeAnimationOrder;

    public Vector2 _eyeOffset;
    public Vector2 _mouthOffset;

    public bool _eyeRandomPause;
    public Vector2 _eyeRandomPauseMinMax;
}

[CreateAssetMenu(fileName = "PortraitData", menuName = "Scriptable Object/PortraitData")]
public class PortraitData : ScriptableObject
{
    public ExpressionData[] _expressionData;

    public ExpressionData getExpressionData(ExpressionType type)
    {
        foreach(var item in _expressionData)
        {
            if(item._type == type)
                return item;
        }

        return null;
    }
}
