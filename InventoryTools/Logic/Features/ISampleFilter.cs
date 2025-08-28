namespace InventoryTools.Logic.Features;

public interface ISampleFilter
{
    public bool ShouldAdd { get; }
    public FilterConfiguration AddFilter();

    public string Name { get; }

    public string SampleDefaultName { get; }

    public string SampleDescription { get; }

    public SampleFilterType SampleFilterType { get; }
}

public enum SampleFilterType
{
    Default,
    Sample
}