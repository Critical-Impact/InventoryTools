using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using AllaganLib.Interface.FormFields;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using Newtonsoft.Json;

namespace InventoryTools.Compendium.Models;

public class SectionState : BaseConfiguration
{
    [JsonIgnore]
    public ICompendiumType CompendiumType { get; set; }
}