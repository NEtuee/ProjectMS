
public class GlobalTimer : Singleton<GlobalTimer>
{
    private double _globalTime = 0.0;
    private float _currentDeltaTime = 0f;

    private bool _updateProcessing = false;

    private float _debugTimeRatio = 1f;

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
        _globalTime += (double)(deltaTime * _debugTimeRatio);
    }

    public double getScaledGlobalTime()
    {
        return _globalTime;
    }

    public void setScaledDeltaTime(float deltaTime)
    {
        _currentDeltaTime = deltaTime * _debugTimeRatio;
    }

    public float getSclaedDeltaTime()
    {
        return _currentDeltaTime;
    }

    public void setDebugTimeRatio(float ratio)
    {
        _debugTimeRatio = ratio;
    }

    public float getDebugTimeRatio()
    {
        return _debugTimeRatio;
    }
}
