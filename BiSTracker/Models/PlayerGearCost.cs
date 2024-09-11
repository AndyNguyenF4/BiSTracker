using System.Numerics;
using Dalamud.Interface.Colors;
namespace BiSTracker.Models;
//remove tome vars unless implementing in game reading of tomes
public struct PlayerGearCost{
    public uint[] bookCount;
    public uint[] remainingBookCost;

    public uint[] currentBookCost = new uint[4];

    public uint currentTomes;
    public uint remainingTotalTomes;
    public Vector4[] bookColors = [ImGuiColors.DalamudRed, ImGuiColors.DalamudOrange, ImGuiColors.ParsedGreen, ImGuiColors.DalamudGrey, ImGuiColors.ParsedGold];

    public PlayerGearCost(uint[] bookCount, uint[] remainingBookCost, uint currentTomes, uint remainingTotalTomes){
        this.bookCount = bookCount;
        this.remainingBookCost = remainingBookCost;
        this.currentTomes = currentTomes;
        this.remainingTotalTomes = remainingTotalTomes;
    }
}
