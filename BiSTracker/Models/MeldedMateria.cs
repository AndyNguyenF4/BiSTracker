using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace BiSTracker.Models;

public class MeldedMateria{
	public MeldedMateria(ushort materiaID){
        this.materiaID = materiaID;
		hasMateria = false;
    }
	
	public ushort materiaID;
	public bool hasMateria;

	// public ushort tempMateriaID;
	// public byte grade;

	public Materia? Row() {
		if (!Data.CheckSheets())
			return null;
		return Data.MateriaSheet.GetRow(materiaID);
	}

}