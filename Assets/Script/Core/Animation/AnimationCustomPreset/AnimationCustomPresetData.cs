[System.Serializable]
public class AnimationCustomPresetData
{
    public float[]      _duration = null;
    public int          _playCount = 0;

    public float getTotalDuration()
    {
        float total = 0f;
        for(int index = 0; index < _duration.Length; ++index)
            total += _duration[index];

        return total;
    }
}
