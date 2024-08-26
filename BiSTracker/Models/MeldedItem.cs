using BiSTracker.Sheets;

namespace BiSTracker.Models;

public class MeldedItem{
	public MeldedItem(uint itemID, MeldedMateria[] meldedMateria){	
		this.itemID = itemID;
		this.meldedMateria = meldedMateria;
		hasPiece = false;
		hasUnaugmented = false;
  	}
	
	public uint itemID;
	public MeldedMateria[] meldedMateria;
	public bool hasPiece;
	public bool hasUnaugmented;

	public ExtendedItem? Row() {
		if (!Data.CheckSheets())
			return null;
		return Data.ItemSheet.GetRow(itemID);
	}
};