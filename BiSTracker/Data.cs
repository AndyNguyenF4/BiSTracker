using BiSTracker.Models;
using BiSTracker.Sheets;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;


namespace BiSTracker;

//might need classsheet later we'll see?
internal static partial class Data{

    internal static ExcelSheet<ExtendedItem>? ItemSheet { get; set; }
    internal static ExcelSheet<ExtendedItemLevel>? LevelSheet { get; set; }
    internal static ExcelSheet<Materia>? MateriaSheet { get; set; }
    internal static ExcelSheet<ItemFood>? FoodSheet { get; set; }
    // internal static ExcelSheet<TomestonesItem>? TomestonesSheet {get;set;}

//[MemberNotNullWhen(true, nameof(ItemSheet), nameof(FoodSheet), nameof(LevelSheet), nameof(MateriaSheet))]
    internal static bool CheckSheets(ExcelModule? excel = null){
        if (ItemSheet == null || LevelSheet == null || MateriaSheet == null || FoodSheet == null){
            if (excel is not null){
                LoadSheets(excel);
                return CheckSheets(null);
            }

            else{
                return false;
            }
        }
        return true;
    }

    internal static void LoadSheets(ExcelModule excel){
        ItemSheet = excel.GetSheet<ExtendedItem>();
        FoodSheet = excel.GetSheet<ItemFood>();
        LevelSheet = excel.GetSheet<ExtendedItemLevel>();
        MateriaSheet = excel.GetSheet<Materia>();
        // TomestonesSheet = excel.GetSheet<TomestonesItem>();
    }
}