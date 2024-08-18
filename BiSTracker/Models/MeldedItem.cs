using BiSTracker.Sheets;

namespace BiSTracker.Models;

internal record struct MeldedItem(
	uint ID,
	bool HighQuality,
	MeldedMateria[] Melds
) {

	public ExtendedItem? Row() {
		if (!Data.CheckSheets())
			return null;
		return Data.ItemSheet.GetRow(ID);
	}

};