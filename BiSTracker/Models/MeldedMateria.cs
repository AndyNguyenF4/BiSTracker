using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace BiSTracker.Models;

public class MeldedMateria{
	    public MeldedMateria(uint materiaID){
        this.materiaID = materiaID;
    }
	
	public uint materiaID;

	public Materia? Row() {
		if (!Data.CheckSheets())
			return null;
		return Data.MateriaSheet.GetRow(materiaID);
	}

}
// {

// 	public Materia? Row() {
// 		if (!Data.CheckSheets())
// 			return null;
// 		return Data.MateriaSheet.GetRow(materiaID);
// 	}

// };