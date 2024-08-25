using BiSTracker.Sheets;

namespace BiSTracker.Models;

public class MeldedItem{
	public MeldedItem(uint itemID, MeldedMateria[] meldedMateria){	
		this.itemID = itemID;
		this.meldedMateria = meldedMateria;
		this.hasPiece = false;
  	}
	
	public uint itemID;
	public MeldedMateria[] meldedMateria;
	public bool hasPiece;
	// bool HighQuality,

	public ExtendedItem? Row() {
		if (!Data.CheckSheets())
			return null;
		return Data.ItemSheet.GetRow(itemID);
	}
};
// } {

// 	public ExtendedItem? Row() {
// 		if (!Data.CheckSheets())
// 			return null;
// 		return Data.ItemSheet.GetRow(itemID);
// 	}

// };