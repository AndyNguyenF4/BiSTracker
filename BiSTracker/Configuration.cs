using BiSTracker.Models;
using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace BiSTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    //instead make this be a string 
    // public Gearset? lastGearset {get; set;} = null;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}


