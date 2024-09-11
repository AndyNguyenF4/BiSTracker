using Lumina.Excel.GeneratedSheets;

namespace BiSTracker.Models;

public class MeldedMateria{
	public MeldedMateria(uint materiaID){
        this.materiaID = materiaID;
		hasMateria = false;
    }
	
	public uint materiaID;
	public bool hasMateria;

	public Materia? Row() {
		if (!Data.CheckSheets())
			return null;
		return Data.MateriaSheet.GetRow(materiaID);
	}

}