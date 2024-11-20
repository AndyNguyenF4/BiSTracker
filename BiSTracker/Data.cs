using System.Diagnostics.CodeAnalysis;
using BiSTracker.Models;

using Lumina.Excel;
using Lumina.Excel.Sheets;


namespace BiSTracker;

//might need classsheet later we'll see?
internal static partial class Data{

    internal static ExcelSheet<Item>? ItemSheet { get; set; }
    internal static ExcelSheet<ItemLevel>? LevelSheet { get; set; }
    internal static ExcelSheet<Materia>? MateriaSheet { get; set; }
    internal static ExcelSheet<ItemFood>? FoodSheet { get; set; }
    internal static ExcelSheet<TomestonesItem>? TomestonesSheet {get;set;}

[MemberNotNullWhen(true, nameof(ItemSheet), nameof(FoodSheet), nameof(LevelSheet), nameof(MateriaSheet))]
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
        ItemSheet = excel.GetSheet<Item>();
        FoodSheet = excel.GetSheet<ItemFood>();
        LevelSheet = excel.GetSheet<ItemLevel>();
        MateriaSheet = excel.GetSheet<Materia>();
        TomestonesSheet = excel.GetSheet<TomestonesItem>();
    }
}