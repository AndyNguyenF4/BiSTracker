
using System.Collections;

namespace BiSTracker.Models;

public class Gearset : IEnumerable{
public Gearset(EtroGearsetParse inputGear){
        this.etroID = inputGear.id;
        this.name = inputGear.name;
        this.weapon = new MeldedItem(inputGear.weapon, new MeldedMateria[5]);
        this.head = new MeldedItem(inputGear.head, new MeldedMateria[5]);
        this.body = new MeldedItem(inputGear.body, new MeldedMateria[5]);
        this.hands = new MeldedItem(inputGear.hands, new MeldedMateria[5]);
        this.legs = new MeldedItem(inputGear.legs, new MeldedMateria[5]);
        this.feet = new MeldedItem(inputGear.feet, new MeldedMateria[5]);
        
        if (inputGear.offHand != null){
            this.offHand = new MeldedItem((uint)inputGear.offHand, new MeldedMateria[5]);
        }

        this.ears = new MeldedItem(inputGear.ears, new MeldedMateria[5]);
        this.neck = new MeldedItem(inputGear.neck, new MeldedMateria[5]);
        this.wrists = new MeldedItem(inputGear.wrists, new MeldedMateria[5]);
        this.fingerL = new MeldedItem(inputGear.fingerL, new MeldedMateria[5]);
        this.fingerR = new MeldedItem(inputGear.fingerR, new MeldedMateria[5]);
        this.weapon = new MeldedItem(inputGear.weapon, new MeldedMateria[5]);
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

    public IEnumerator GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    //food here for later?
}