using System;
using System.Reflection;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using BiSTracker.Models;
using BiSTracker.Sheets;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Interface.Textures;
using Dalamud;
using Dalamud.Interface.Colors;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Dalamud.Game.Inventory;

namespace BiSTracker.Windows;

public class MainWindow : Window, IDisposable
{
    private string GoatImagePath;
    private string etroImportString = "";

    private string etroID = "";
    private string[] etroImportStringSplit = new string[2];

    // private bool buttonPressed;

    private static readonly HttpClient client = new HttpClient();

    private string placeholder ="";

    private bool clickedDelete;
    // private enum[] searchItemSpace = { (uint)InventoryType.ArmoryHead, InventoryType. };
    // const itemSpace = InventoryType.EquippedItems;
    List<InventoryType> itemSpace;
    private DirectoryInfo savedSetsDirectory;
    // protected PluginUI Ui { get;}

    //increment to 13 for food later
    // private uint[] gearID = {43107, 43076, 43154, 43155, 43079, 43157, 0, 43162, 43090, 43172, 43100, 43177};
    private enum raidDropIDs : int{
        Book1 = 43549,
        Book2 = 43550,
        Book3 = 43551,
        Book4 = 43552,
        Glaze = 43554,
        Twine = 43555,
    };

    //skip raidOffHand
    public enum bookCost : int{
        BOOK_WEAP = 8,
        BOOK_HEAD_HANDS_FEET = 4,
        BOOK_CHEST_LEGS = 6,
        BOOK_ACCESSORY = 3,
        TWINE_SOLVENT = 4,
        GLAZE = 3
    };

    public enum tomeCost : int{
        TOME_WEAP = 500,
        TOME_HEAD_HANDS_FEET = 495,
        TOME_CHEST_LEGS = 825,
        TOME_ACCESSORY = 375
    }
    //tomeCost tomeCost, bookCost bookCost, int bookType, bookCost bookUpgradeCost, int bookUpgradeType
    private Dictionary<string, GearCost> COST = new Dictionary<string, GearCost>{
        ["weapon"] = new GearCost(tomeCost.TOME_WEAP, bookCost.BOOK_WEAP, 4, bookCost.TWINE_SOLVENT, 3),
        ["head"] = new GearCost(tomeCost.TOME_HEAD_HANDS_FEET, bookCost.BOOK_HEAD_HANDS_FEET, 2, bookCost.TWINE_SOLVENT, 3),
        ["body"] = new GearCost(tomeCost.TOME_CHEST_LEGS, bookCost.BOOK_CHEST_LEGS, 3, bookCost.TWINE_SOLVENT, 3),
        ["hands"] = new GearCost(tomeCost.TOME_HEAD_HANDS_FEET, bookCost.BOOK_HEAD_HANDS_FEET, 2, bookCost.TWINE_SOLVENT, 3),
        ["legs"] = new GearCost(tomeCost.TOME_CHEST_LEGS, bookCost.BOOK_CHEST_LEGS, 3, bookCost.TWINE_SOLVENT, 3),
        ["feet"] = new GearCost(tomeCost.TOME_HEAD_HANDS_FEET, bookCost.BOOK_HEAD_HANDS_FEET, 2, bookCost.TWINE_SOLVENT, 3),
        ["ears"] = new GearCost(tomeCost.TOME_ACCESSORY, bookCost.BOOK_ACCESSORY, 1, bookCost.GLAZE, 2),
        ["neck"] = new GearCost(tomeCost.TOME_ACCESSORY, bookCost.BOOK_ACCESSORY, 1, bookCost.GLAZE, 2),
        ["wrists"] = new GearCost(tomeCost.TOME_ACCESSORY, bookCost.BOOK_ACCESSORY, 1, bookCost.GLAZE, 2),
        ["fingerL"] = new GearCost(tomeCost.TOME_ACCESSORY, bookCost.BOOK_ACCESSORY, 1, bookCost.GLAZE, 2),
        ["fingerR"] = new GearCost(tomeCost.TOME_ACCESSORY, bookCost.BOOK_ACCESSORY, 1, bookCost.GLAZE, 2)
    };
    private int[] remainingBooks = [0, 0, 0, 0];
    private int[] remainingAugments = [0, 0];

    private Dictionary<string, Gearset>? savedGearsets;
    public Gearset? currentGearset;
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath, DirectoryInfo directory)
        : base("BiS Tracker##BiSTrackerDisplay", ImGuiWindowFlags.NoScrollbar)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        GoatImagePath = goatImagePath;
        Plugin = plugin;
        savedSetsDirectory = directory;
        savedGearsets = new Dictionary<string, Gearset>();

        itemSpace = [InventoryType.EquippedItems, InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4, InventoryType.ArmoryOffHand, InventoryType.ArmoryMainHand, InventoryType.ArmoryHead, InventoryType.ArmoryBody, InventoryType.ArmoryHands, InventoryType.ArmoryLegs, InventoryType.ArmoryFeets, InventoryType.ArmoryEar, InventoryType.ArmoryNeck, InventoryType.ArmoryWrist, InventoryType.ArmoryRings, InventoryType.SaddleBag1, InventoryType.SaddleBag2];

        foreach (var set in plugin.Configuration.availableGearsets){
            savedGearsets.Add(set, etroImport(set));
        }

        if (savedGearsets.Count > 0){
            currentGearset = savedGearsets.First().Value; 
        }

        else{
            currentGearset = null;
        }
        clickedDelete = false;
    }

    public void Dispose() {}
    public override void Draw()
    {
        //etro links are 60 chars long, 61 for C# esc char?

        ImGui.BeginGroup();
        ImGui.InputTextWithHint("##EtroImportTextBox", "Insert Etro URL and click \"Import\"", ref etroImportString, 61);
        ImGui.SameLine();
        if(ImGui.Button("Import" + "###EtroImportButton")){
        
            //if import string >= 0 then attempt to read etro link and then reset string
            if(etroImportString.Length > 0){
                etroImport(etroImportString, savedSetsDirectory);
            }
        }
        

        // ImGui.Combo(); //string label, ref int current_item, string[] items, int items_count (items.length)

        if (savedGearsets != null){
            if (savedGearsets.Count > 0){
                var arrayOfGearsets = new String[savedGearsets.Count];
                var currentIndexGearset = 0;
                var dropdownIndexReference = 0;
                foreach(KeyValuePair<string, Gearset> kvp in savedGearsets){
                    arrayOfGearsets[currentIndexGearset] = kvp.Value.name;
                    if (kvp.Value.Equals(currentGearset)){
                        dropdownIndexReference = currentIndexGearset;
                    }

                    if (currentIndexGearset<arrayOfGearsets.Length-1){
                        currentIndexGearset++;
                    }
                }               
                
                //should probably add a lastSavedSet config to load the last one for users on startup
                if(ImGui.Combo("Gearsets##GearsetDropDown", ref dropdownIndexReference, arrayOfGearsets, arrayOfGearsets.Length)){
                    var tempIndex = 0;
                    
                    foreach(KeyValuePair<string, Gearset> kvp in savedGearsets){
                        if (kvp.Value.name == arrayOfGearsets[dropdownIndexReference]){
                            dropdownIndexReference = tempIndex;
                            currentGearset = savedGearsets[kvp.Key];
                        }
                        if(tempIndex<arrayOfGearsets.Length-1){
                            tempIndex++;
                        }
                    }
                }

                // if(ImGui.CollapsingHeader("Gearsets##GearsetsCollapsingHeader")){
                //     foreach(KeyValuePair<string, Gearset> kvp in savedGearsets){
                //         if(ImGui.Selectable(kvp.Value.name)){
                //             currentGearset = kvp.Value;                            
                //         }
                //     }
                // }
            }
        }
        

        // if (currentGearset != null && ImGui.CollapsingHeader("Items" + $"({currentGearset.name})##GearsetItemsCollapsingHeader", ImGuiTreeNodeFlags.DefaultOpen)){
        //     bisComparison(currentGearset);
        //     DrawItems(currentGearset);
        // }
        if (currentGearset != null){
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            var currentGearsetCost = getInitialBookCost(currentGearset);
            foreach (var invTypeEnum in itemSpace){
                bisComparison(currentGearset, invTypeEnum);
            }

            // bisComparison(currentGearset);
            DrawItems(currentGearset);
            foreach (var value in currentGearsetCost){
                ImGui.Text(value.ToString());
            }
            DrawDeleteButton();
        }

        ImGui.Text(placeholder);

        ImGui.Text(savedSetsDirectory.ToString());

        ImGui.EndGroup();
        
        //sample plugin stuff
        ImGui.Text($"The random config bool is {Plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }

        ImGui.Spacing();
     

        ImGui.Text("Have a goat:");
        var goatImage = Plugin.TextureProvider.GetFromFile(GoatImagePath).GetWrapOrDefault();
        if (goatImage != null)
        {
            ImGuiHelpers.ScaledIndent(55f);
            ImGui.Image(goatImage.ImGuiHandle, new Vector2(goatImage.Width, goatImage.Height));
            ImGuiHelpers.ScaledIndent(-55f);
        }
        else
        {
            ImGui.Text("Image not found.");
        }

        //clears gearsets saved in config
        // Plugin.Configuration.availableGearsets.Clear();
        // Plugin.Configuration.Save();

    }

	protected ISharedImmediateTexture? GetIcon(Item? item, bool hq = false) {
		if (item is not null)
			return GetIcon(item.Icon, hq);
		return null;
	}

	protected ISharedImmediateTexture GetIcon(uint id, bool hq = false) {
		return Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(id, hq));
	}


    protected Gearset etroImport(string etroID){
        return etroGearToGearSet(etroJsonToObject(etroID, savedSetsDirectory));
    }
    protected void etroImport(string etroURL, DirectoryInfo directory){
        for (int i = etroURL.Length-1; i >= 0; i--){
            if (etroURL[i] == '/'){
                break;
            }

            etroID = etroURL[i] + etroID;
        }

        etroImportStringSplit = etroURL.Split("gearsets/");

        Task.Run(async () =>
        {
            using HttpResponseMessage response = await client.GetAsync("https://etro.gg/api/gearsets/" + etroID);
            string responseBody = await response.Content.ReadAsStringAsync();

            if(response.IsSuccessStatusCode){
                File.WriteAllText(directory + "\\" + etroID + ".json", responseBody);
                Gearset temp = etroGearToGearSet(etroJsonToObject(etroID, directory));
                // this.currentGearset = etroGearToGearSet(etroJsonToObject(etroID, directory));
                savedGearsets.Add(etroID, temp);
                foreach (var invTypeEnum in itemSpace){
                    bisComparison(temp, invTypeEnum);
                }
                
                // bisComparison(temp);
                this.currentGearset = temp;
                etroImportString = "";

                Plugin.Configuration.availableGearsets.Add(etroID);
                Plugin.Configuration.Save();
            }
            etroID = "";
        });

        // this.currentGearset = etroGearToGearSet(etroJsonToObject(etroID, directory));
    }

    protected static EtroGearsetParse etroJsonToObject(string etroID, DirectoryInfo directory){
        string fullPath = directory + "\\" + etroID + ".json";
        string jsonString = File.ReadAllText(fullPath);

        return JsonSerializer.Deserialize<EtroGearsetParse>(jsonString); 
    }

    protected static Gearset etroGearToGearSet(EtroGearsetParse inputGear){
        return new Gearset(inputGear);
    }

    //button should update a variable for a currentSet and drawItems should utilize
    //this to render the currentSet instead of rendering all sets
    //waymark library has imgui.selectable, good reference
    public void DrawItems(Gearset gearsetTest){
        Type type = gearsetTest.GetType();
        PropertyInfo[] properties = type.GetProperties();
        
        // if(ImGui.CollapsingHeader("Items", ImGuiTreeNodeFlags.DefaultOpen)){

            float scale = ImGui.GetFontSize() / 17;

            foreach (PropertyInfo property in properties){
                // string name = property.Name;
                object value = property.GetValue(gearsetTest);

                if (value == null){
                    continue;
                }

                if (property.PropertyType == typeof(MeldedItem)){
                    MeldedItem gearsetItem = (MeldedItem)value;

                    ExtendedItem rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID); //gearID[i] originally in here
                    
                    var icon = GetIcon(rawItem)?.GetWrapOrEmpty();
                    int height = icon is null ? 0 : Math.Min(icon.Height, (int) (32 * scale));
                    if (icon != null){
                        // ImGui.Text(rawItem.Name + " hasPiece:" + gearsetItem.hasPiece.ToString()); //test code
                        ImGui.Image(icon.ImGuiHandle, new Vector2(height, height));
                        ImGui.SameLine();
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                    }

                    // ImGui.Text(rawItem.Name);
                    if (gearsetItem.hasPiece){
                        ImGui.TextColored(ImGuiColors.ParsedGreen, rawItem.Name);
                    }

                    else{
                        ImGui.TextColored(ImGuiColors.DalamudWhite, rawItem.Name);
                    }

                    if (rawItem.ExtendedItemLevel.Value is ExtendedItemLevel level){
                        ImGui.SameLine();
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                        
                        ImGui.TextColored(ImGuiColors.ParsedGrey, $"i{level.RowId}");
                    }

                    


                    //test code for checkbox

                    // ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - 32);

                    // ImGui.Checkbox("", ref gearsetItem.hasPiece);
                }
            }
        // }
    }
    
    public void DrawDeleteButton(){
        
        var color = System.Drawing.Color.FromArgb(217, 39, 39, 204);
        ImGui.PushStyleColor(ImGuiCol.Text, 0xFC5A5AFF); // EE6244FF
        
        if (ImGui.Button("Delete Gearset?")){
            clickedDelete = true;
        }
        ImGui.PopStyleColor();

        if(clickedDelete){
            DrawDeletionConfirmationWindow(ref clickedDelete);
        }
    }

    protected unsafe InventoryContainer* GetInventoryContainer(){
        return InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
    }

    protected unsafe InventoryContainer* GetInventoryContainer(InventoryType invTypeEnum){
        return InventoryManager.Instance()->GetInventoryContainer(invTypeEnum);
    }



    protected bool DrawDeletionConfirmationWindow(ref bool isVisible)
    {
        if (!isVisible)
            return false;

        var ret = false;

        ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(280f, 120f));
        ImGui.Begin("Gearset Deletion Confirmation", ImGuiWindowFlags.NoResize);

        ImGui.Text("Are you sure you want to delete this?");
        ImGui.Text("This cannot be undone.");
        ImGui.PushStyleColor(ImGuiCol.Text, 0xFC5A5AFF);
        if (ImGui.Button("Yes"))
        {
            isVisible = false;
            ret = true;
        }
        ImGui.PopStyleColor();

        ImGui.SameLine();
        if (ImGui.Button("No"))
        {
            isVisible = false;
        }

        if (ret){
            savedGearsets.Remove(currentGearset.etroID);
            File.Delete(savedSetsDirectory + "\\" + currentGearset.etroID + ".json");
            Plugin.Configuration.availableGearsets.Remove(currentGearset.etroID);
            Plugin.Configuration.Save();
            if (savedGearsets.Count != 0){
                currentGearset = savedGearsets.First().Value;
            }

            else{
                currentGearset = null;
            }
        }

        ImGui.End();

        return ret;
    }


// maybe use item level to narrow down search
    protected unsafe void bisComparison(Gearset givenGearset){
        InventoryContainer* inventory = GetInventoryContainer();

        for (uint i = 0; i < inventory->Size; i++){
            var item = inventory->Items[i];
            uint id = item.ItemId;

            if (id == 0){
                continue;
            }

            Type type = givenGearset.GetType();
            PropertyInfo[] properties = type.GetProperties();
            // ExtendedItem rawItem = Data.ItemSheet.GetRow(id);
            foreach (PropertyInfo property in properties){
                string name = property.Name;
                object value = property.GetValue(givenGearset);

                if (value == null){
                    continue;
                }

                if (property.PropertyType == typeof(MeldedItem)){
                    MeldedItem gearsetItem = (MeldedItem)value;

                    if (gearsetItem.itemID == id){
                        gearsetItem.hasPiece = true;
                        break;
                        //add a break here since it no longer needs to compare current gear piece to the rest of inv?
                    }
                }
            
            // ImGui.Text(rawItem.Name);
            }
        }
    }

    protected unsafe void bisComparison(Gearset givenGearset, InventoryType invTypeEnum){
        InventoryContainer* inventory = GetInventoryContainer(invTypeEnum);

        for (uint i = 0; i < inventory->Size; i++){
            var item = inventory->Items[i];
            uint id = item.ItemId;

            if (id == 0){
                continue;
            }

            Type type = givenGearset.GetType();
            PropertyInfo[] properties = type.GetProperties();
            // ExtendedItem rawItem = Data.ItemSheet.GetRow(id);
            foreach (PropertyInfo property in properties){
                string name = property.Name;
                object value = property.GetValue(givenGearset);

                if (value == null){
                    continue;
                }

                if (property.PropertyType == typeof(MeldedItem)){
                    MeldedItem gearsetItem = (MeldedItem)value;

                    if (gearsetItem.itemID == id){
                        gearsetItem.hasPiece = true;
                        break;

                    }
                }
            }
        }
    }

    //different ways of calculating book costs
    //buying everything with books, including gear pieces + glaze/twine (gear sheet default)
    //buying only glaze/twine with books
    protected int[] getInitialBookCost(Gearset givenGearset){
        int[] returnCost = new int[5];

        Type type = givenGearset.GetType();
        PropertyInfo[] properties = type.GetProperties();
        foreach (PropertyInfo property in properties){
            string name = property.Name;
            object value = property.GetValue(givenGearset);
            if (value == null || name == "offHand"){
                continue;
            }

            if (property.PropertyType == typeof(MeldedItem)){
                
                MeldedItem gearsetItem = (MeldedItem)value;
                ExtendedItem rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID);
                rawItem.


                GearCost costOfItem = COST[name];


                if (!rawItem.Name.RawString.Contains("Augmented")){
                    returnCost[costOfItem.bookType-1] += (int)costOfItem.bookCost;
                }
                
                else if (rawItem.Name.RawString.Contains("Augmented")){
                    returnCost[costOfItem.bookUpgradeType-1] += (int)costOfItem.bookUpgradeCost;
                    returnCost[4] += (int)costOfItem.tomeCost;
                }
            }
        }

        return returnCost;
    }
}
