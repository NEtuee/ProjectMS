
public class GlobalTimer : Singleton<GlobalTimer>
{
    private float _globalTime = 0f;

    public void updateGlobalTime(float deltaTime)
    {
        _globalTime += deltaTime;
    }

    public float getScaledGlobalTime()
    {
        return _globalTime;
    }
}
