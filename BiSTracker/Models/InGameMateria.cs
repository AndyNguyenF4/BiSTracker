using BiSTracker.Sheets;

namespace BiSTracker.Models;

public class InGameMateria{
	public InGameMateria(ushort materiaGameID, byte grade){
        this.materiaGameID = materiaGameID;
        this.grade = grade;
    }
	public ushort materiaGameID;
	public byte grade;
}

//make a dictionary from InGameMateria() -> materiaID
// SKS - 24
// SPS - 25
// DH - 14
// DET - 16
// CRIT - 15
// PIE - 7
// TEN - 17

// grade = 11 for 12s