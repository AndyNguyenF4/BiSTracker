using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace BiSTracker.Models;

internal record struct MeldedMateria(
	ushort ID,
	byte Grade
){

	public Materia? Row() {
		if (!Data.CheckSheets())
			return null;
		return Data.MateriaSheet.GetRow(ID);
	}

};