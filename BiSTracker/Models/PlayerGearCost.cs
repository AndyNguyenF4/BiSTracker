using System.Numerics;
using Dalamud.Interface.Colors;
using static BiSTracker.Windows.MainWindow;
namespace BiSTracker.Models;

public struct PlayerGearCost{
    public int[] bookCount;
    public int[] remainingBookCost;

    public int currentTomes;
    public int remainingTotalTomes;
    public Vector4[] bookColors = [ImGuiColors.DalamudRed, ImGuiColors.DalamudOrange, ImGuiColors.ParsedGreen, ImGuiColors.DalamudGrey, ImGuiColors.ParsedGold];

    public PlayerGearCost(int[] bookCount, int[] remainingBookCost, int currentTomes, int remainingTotalTomes){
        this.bookCount = bookCount;
        this.remainingBookCost = remainingBookCost;
        this.currentTomes = currentTomes;
        this.remainingTotalTomes = remainingTotalTomes;
    }
}
