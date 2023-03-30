
public class GlobalTimer : Singleton<GlobalTimer>
{
    private float _globalTime = 0f;
    private float _currentDeltaTime = 0f;

    private bool _updateProcessing = false;

    public void setUpdateProcessing(bool value)
    {
        _updateProcessing = value;
    }

    public bool isUpdateProcessing()
    {
        return _updateProcessing;
    }

    public void updateGlobalTime(float deltaTime)
    {
        _globalTime += deltaTime;
    }

    public float getScaledGlobalTime()
    {
        return _globalTime;
    }

    public void setScaledDeltaTime(float deltaTime)
    {
        _currentDeltaTime = deltaTime;
    }

    public float getSclaedDeltaTime()
    {
        return _currentDeltaTime;
    }
}
