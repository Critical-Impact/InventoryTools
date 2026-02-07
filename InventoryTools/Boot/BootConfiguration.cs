using System.ComponentModel;

namespace InventoryTools.Boot;

public sealed class BootConfiguration
{
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsDirty { get; private set; }

    [DefaultValue(true)]
    private bool _persistLuminaCache = true;

    public bool PersistLuminaCache
    {
        get => _persistLuminaCache;
        set
        {
            if (_persistLuminaCache == value)
                return;

            _persistLuminaCache = value;
            IsDirty = true;
        }
    }

    public void ClearDirty()
    {
        IsDirty = false;
    }
}