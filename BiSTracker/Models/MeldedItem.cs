// using BiSTracker.Sheets;
// using Dalamud.Storage.Assets;
using Lumina.Excel.Sheets;


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

	public Item? Row() {
		if (!Data.CheckSheets() || !Data.ItemSheet.TryGetRow(itemID, out var row))
			return null;
		return row;
	}
};