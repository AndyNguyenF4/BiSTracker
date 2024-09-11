using System.Collections.Generic;

namespace BiSTracker.Models;
public class XIVGearsetParse{
  public string XIVGearsetID {get; set;} //we need to manually set this when we're initializing 
  public string name {get; set;} //set name
  public Dictionary<string, XIVGearsetItem> items{get; set;} //items stores a string corresponding to gear type mapped to the item itself

}

public class XIVGearsetItem{
  public uint id {get; set;} //item has the item id
  public XIVGearsetMateria[] materia {get; set;}
  // public Dictionary<string, int>[] materia {get; set;} //has an array of dictionaries where string id points to materia id
}

public class XIVGearsetMateria{
  public uint id {get; set;}
}
/*
 "items": {
    "Wrist": {
      "materia": [
        {
          "id": 33942
        },
        {
          "id": 33942
        }
      ],
      "id": 40233
    },
    "RingRight": {
      "materia": [
        {
          "id": 33931
        },
        {
          "id": 33931
        }
      ],
      "id": 40088
    },
*/