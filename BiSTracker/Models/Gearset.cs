
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BiSTracker.Models;

public class Gearset{
public Gearset(EtroGearsetParse inputGear){
        etroID = inputGear.id;
        name = inputGear.name;
        weapon = new MeldedItem(inputGear.weapon, new MeldedMateria[5]);
        head = new MeldedItem(inputGear.head, new MeldedMateria[5]);
        body = new MeldedItem(inputGear.body, new MeldedMateria[5]);
        hands = new MeldedItem(inputGear.hands, new MeldedMateria[5]);
        legs = new MeldedItem(inputGear.legs, new MeldedMateria[5]);
        feet = new MeldedItem(inputGear.feet, new MeldedMateria[5]);
        
        if (inputGear.offHand != null){
            offHand = new MeldedItem((uint)inputGear.offHand, new MeldedMateria[5]);
        }

        ears = new MeldedItem(inputGear.ears, new MeldedMateria[5]);
        neck = new MeldedItem(inputGear.neck, new MeldedMateria[5]);
        wrists = new MeldedItem(inputGear.wrists, new MeldedMateria[5]);
        fingerL = new MeldedItem(inputGear.fingerL, new MeldedMateria[5]);
        fingerR = new MeldedItem(inputGear.fingerR, new MeldedMateria[5]);
    }
    
    public string etroID{get; set;}
    public string name{get; set;}
    public MeldedItem weapon{get; set;}
    public MeldedItem head{get; set;}
    public MeldedItem body{get; set;}
    public MeldedItem hands{get; set;}
    public MeldedItem legs{get; set;}
    public MeldedItem feet{get; set;}
    public MeldedItem? offHand{get; set;}
    public MeldedItem ears{get; set;}
    public MeldedItem neck{get; set;}
    public MeldedItem wrists{get; set;}
    public MeldedItem fingerL{get; set;}
    public MeldedItem fingerR{get; set;}

    public void fillMateria(Dictionary<string, Dictionary<string, ushort>> materiaDictionary){ 
        Type type = GetType();
        PropertyInfo[] properties = type.GetProperties();
        
        foreach (PropertyInfo property in properties){
            string name = property.Name;
            object value = property.GetValue(this);

            if (value == null || property.PropertyType != typeof(MeldedItem)){
                continue;
            }
            //find a way to remove L/R from ring string values
            MeldedItem gearsetItem = (MeldedItem)value;

            Dictionary<string, ushort> gearPieceMateria;
            if (property.Name.Contains("finger")){
                gearPieceMateria = materiaDictionary[gearsetItem.itemID.ToString() + name.Last()];
            }

            else{
                gearPieceMateria = materiaDictionary[gearsetItem.itemID.ToString()];
            }

            foreach(KeyValuePair<string, ushort> kvp in gearPieceMateria){
                gearsetItem.meldedMateria[uint.Parse(kvp.Key)-1] = new MeldedMateria(kvp.Value);
            }  
        }
    }
    //food here for later?
}