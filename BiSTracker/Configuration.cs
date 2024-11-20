using BiSTracker.Models;
using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace BiSTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    public bool buyTwineGlazeOnly { get; set; } = false;
    public HashSet<string>? availableGearsets {get; set;} = new HashSet<string>();
    public string lastSavedSet {get; set;} = "";

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}


