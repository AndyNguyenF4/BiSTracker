// using Lumina.Excel.GeneratedSheets;
using Lumina.Excel.Sheets;

namespace BiSTracker.Models;

public class MeldedMateria{
	public MeldedMateria(uint materiaID){
        this.materiaID = materiaID;
		hasMateria = false;
    }
	
	public uint materiaID;
	public bool hasMateria;

	public Materia? Row() {
		if (!Data.CheckSheets() || !Data.MateriaSheet.TryGetRow(materiaID, out var row))
			return null;
		return row;
	}

}