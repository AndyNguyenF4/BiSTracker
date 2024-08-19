
namespace BiSTracker.Models;

internal record struct GearSet{
    string gearSetName;
    int etroID;
    MeldedItem weapon;
    MeldedItem head;
    MeldedItem body;
    MeldedItem hands;
    MeldedItem legs;
    MeldedItem feet;
    MeldedItem offhand;
    MeldedItem earrings;
    MeldedItem necklace;
    MeldedItem bracelet;
    MeldedItem leftRing;
    MeldedItem rightRing;

    //food here for later?
}