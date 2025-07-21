public struct AnimationPresetInfo
{
    public AnimationCustomPreset CustomPreset;
    public string Path;
    public AnimationPresetInfo(AnimationCustomPreset customPreset, string path)
    {
        this.CustomPreset = customPreset;
        this.Path = path;
    }
}