using System.Collections.Generic;

namespace BiSTracker.Models;
public class EtroGearsetParse{
    public string id {get; set;}
    public string name {get; set;}
    public uint weapon{get; set;}
    public uint head{get; set;}
    public uint body{get; set;}
    public uint hands{get; set;}
    public uint legs{get; set;}
    public uint feet{get; set;}
    public uint? offHand{get; set;}
    public uint ears{get; set;}
    public uint neck{get; set;}
    public uint wrists{get; set;}
    public uint fingerL{get; set;}
    public uint fingerR{get; set;}
    public Dictionary<string, Dictionary<string, uint>>? materia{get; set;}
}