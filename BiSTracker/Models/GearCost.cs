using static BiSTracker.Windows.MainWindow;
namespace BiSTracker.Models;

public struct GearCost{
    public tomeCost tomeCost;
    public bookCost bookCost;
    public int bookType;
    public int bookUpgradeType;
    public bookCost bookUpgradeCost;


    public GearCost(tomeCost tomeCost, bookCost bookCost, int bookType, bookCost bookUpgradeCost, int bookUpgradeType){
        this.tomeCost = tomeCost;
        this.bookCost = bookCost;
        this.bookType = bookType;
        this.bookUpgradeCost = bookUpgradeCost;
        this.bookUpgradeType = bookUpgradeType;
    }
}
