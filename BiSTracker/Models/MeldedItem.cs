using BiSTracker.Sheets;

namespace BiSTracker.Models;

internal record struct MeldedItem(
	uint itemID,
	MeldedMateria[] Melds
	// bool HighQuality,
) {

	public ExtendedItem? Row() {
		if (!Data.CheckSheets())
			return null;
		return Data.ItemSheet.GetRow(itemID);
	}

};