using System;
using System.Reflection;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using BiSTracker.Models;
using BiSTracker.Sheets;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Colors;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Plugin.Services;
using Dalamud.IoC;

namespace BiSTracker.Windows;
public class MainWindow : Window, IDisposable
{
    private string GoatImagePath; //delete later
    private string etroImportString = "";
    private string etroID = "";
    private string[] etroImportStringSplit = new string[2];
    private static readonly HttpClient client = new HttpClient();
    private string placeholder =""; //delete later
    private bool clickedDelete;
    private List<InventoryType> itemSpace;
    private DirectoryInfo savedSetsDirectory;
    private Dictionary<ushort, InGameMateria> materiaIDMapping = new Dictionary<ushort, InGameMateria>{
        [41781] = new InGameMateria(24, 11),
        [41782] = new InGameMateria(25, 11),
        [41771] = new InGameMateria(14, 11),
        [41773] = new InGameMateria(16, 11),
        [41772] = new InGameMateria(15, 11),
        [41770] = new InGameMateria(7, 11),
        [41774] = new InGameMateria(17, 11),

        [41768] = new InGameMateria(24, 10),
        [41769] = new InGameMateria(25, 10),
        [41758] = new InGameMateria(14, 10),
        [41760] = new InGameMateria(16, 10),
        [41759] = new InGameMateria(15, 10),
        [41757] = new InGameMateria(7, 10),
        [41761] = new InGameMateria(17, 10)

    };
    
    //change ushort to uint later for materiaid
    private Dictionary<ushort, string> materiaIDToName = new Dictionary<ushort, string>{
        [41772] = "CRT +54",
        [41771] = "DH +54",
        [41773] = "DET +54",
        [41782] = "SPS +54",
        [41781] = "SKS +54",
        [41774] = "TEN +54",
        [41770] = "PIE +54",
        
        [41759] = "CRT +18",
        [41758] = "DH +18",
        [41760] = "DET +18",
        [41769] = "SPS +18",
        [41768] = "SKS +18",
        [41761] = "TEN +18",
        [41757] = "PIE +18"
    };
    private enum raidDropIDs : int{
        Book1 = 43549,
        Book2 = 43550,
        Book3 = 43551,
        Book4 = 43552,
        Glaze = 43555,
        Twine = 43554,
    };

    private Dictionary<uint, uint> raidDropIDToIndex = new Dictionary<uint, uint>{
        [(uint)raidDropIDs.Book1] = 0,
        [(uint)raidDropIDs.Book2] = 1,
        [(uint)raidDropIDs.Book3] = 2,
        [(uint)raidDropIDs.Book4] = 3,
        [(uint)raidDropIDs.Glaze] = 1,
        [(uint)raidDropIDs.Twine] = 2,
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
    private Gearset? currentGearset;
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

        itemSpace = [InventoryType.SaddleBag1, InventoryType.SaddleBag2, InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4, InventoryType.EquippedItems, InventoryType.ArmoryOffHand, InventoryType.ArmoryMainHand, InventoryType.ArmoryHead, InventoryType.ArmoryBody, InventoryType.ArmoryHands, InventoryType.ArmoryLegs, InventoryType.ArmoryFeets, InventoryType.ArmoryEar, InventoryType.ArmoryNeck, InventoryType.ArmoryWrist, InventoryType.ArmoryRings];

        foreach (var set in plugin.Configuration.availableGearsets){
            savedGearsets.Add(set, etroImport(set));
        }

        if (savedGearsets.Count > 0){
            currentGearset = savedGearsets.First().Value; 
            // currentGearset = savedGearsets[Plugin.Configuration.lastSavedSet];
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
                var arrayOfGearsets = new string[savedGearsets.Count];
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
                if(ImGui.Combo("##GearsetDropDown", ref dropdownIndexReference, arrayOfGearsets, arrayOfGearsets.Length)){
                    var tempIndex = 0;
                    
                    foreach(KeyValuePair<string, Gearset> kvp in savedGearsets){
                        if (kvp.Value.name == arrayOfGearsets[dropdownIndexReference]){
                            dropdownIndexReference = tempIndex;
                            currentGearset = savedGearsets[kvp.Key];
                            // Plugin.Configuration.lastSavedSet = kvp.Key;
                            // Plugin.Configuration.Save();
                        }
                        if(tempIndex<arrayOfGearsets.Length-1){
                            tempIndex++;
                        }
                    }
                }
                ImGui.SameLine();
                DrawDeleteButton();
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
            
            //checks all inv spaces for gear
            foreach (var invTypeEnum in itemSpace){
                bisComparison(currentGearset, invTypeEnum);
            }

            // bisComparison(currentGearset);
            DrawItems(currentGearset);
            // foreach (var value in currentGearsetCost){
            //     ImGui.Text(value.ToString());
            // }
            PlayerGearCost playerGearsetCost = new PlayerGearCost(
                new uint[5],
                getInitialCost(currentGearset, Plugin.Configuration.buyTwineGlazeOnly),
                0,
                0
            );

            //1st array/playerGearsetCost.bookCount[] has the inventory count of books
            //2nd array playerGearsetCost.remainingBookCost[] SHOULD HAVE the cost of missing pieces
            // playerGearsetCost.
            playerGearsetCost.bookCount[4] = (uint)getTomes(currentGearset);
            getCurrentBooksOrUpgrades(currentGearset, playerGearsetCost, Plugin.Configuration.buyTwineGlazeOnly);
            calc(currentGearset, playerGearsetCost, Plugin.Configuration.buyTwineGlazeOnly);
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Text("Books:");
            ImGui.SameLine(0, 10);
            for (var index = 0; index < playerGearsetCost.bookCount.Length; index++){
                if (index < playerGearsetCost.bookCount.Length-1){
                    // ImGui.TextColored(playerGearsetCost.bookColors[index], playerGearsetCost.bookCount[index] + "/" + playerGearsetCost.remainingBookCost[index] + " currentBookCost: " + playerGearsetCost.currentBookCost[index]);
                    ImGui.TextColored(playerGearsetCost.bookColors[index], playerGearsetCost.bookCount[index] + "/" + (playerGearsetCost.remainingBookCost[index] - playerGearsetCost.currentBookCost[index]).ToString());
                    // ImGui.TextColored(playerGearsetCost.bookColors[index], (playerGearsetCost.remainingBookCost[index]-playerGearsetCost.bookCount[index]).ToString());
                    ImGui.SameLine(0, 15);
                }

                else{
                    ImGui.NewLine();
                    ImGui.Text("Tomes: ");
                    ImGui.SameLine(0, 5);
                    var currentTome = getCurrentTomes();
                    // ImGui.TextColored(playerGearsetCost.bookColors[index], playerGearsetCost.bookCount[index] + "/" + playerGearsetCost.remainingBookCost[index]);
                    // ImGui.TextColored(playerGearsetCost.bookColors[4], currentTome.ToString() + "/" + getTomes(currentGearset).ToString() + " TotalTomeCost: " + playerGearsetCost.remainingBookCost[4]);
                    ImGui.TextColored(playerGearsetCost.bookColors[4], currentTome.ToString() + "/" + (playerGearsetCost.remainingBookCost[4] - getTomes(currentGearset)).ToString());
                    // if (playerGearsetCost.remainingBookCost[index]<playerGearsetCost.bookCount[index]){
                    //     ImGui.TextColored(playerGearsetCost.bookColors[index], $"{currentTome}/0");
                    // }

                    // else{
                    //     ImGui.TextColored(playerGearsetCost.bookColors[index], $"{currentTome}/" + (playerGearsetCost.remainingBookCost[index]-playerGearsetCost.bookCount[index]).ToString());
                    // }
                }
            }
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

    protected unsafe uint getCurrentTomes(){
        uint id = Data.TomestonesSheet.Where(tomestone => tomestone.Tomestones.Row is 3).First().Item.Row;
        var returnVal = InventoryManager.Instance()->GetTomestoneCount(id);
        return returnVal;
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
                currentGearset = temp;
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
        //probably do materia adding here
        Gearset returnEtroGearset = new Gearset(inputGear);
        if (inputGear.materia != null){
            returnEtroGearset.fillMateria(returnEtroGearset, inputGear.materia);
        }

        return returnEtroGearset;
        // return new Gearset(inputGear);
    }
    public void DrawItems(Gearset gearsetTest){
        Type type = gearsetTest.GetType();
        PropertyInfo[] properties = type.GetProperties();

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
                    ImGui.Image(icon.ImGuiHandle, new Vector2(height, height));
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                }

                // ImGui.Text(rawItem.Name);
                if (gearsetItem.hasPiece){
                    ImGui.TextColored(ImGuiColors.ParsedGreen, rawItem.Name);
                }

                else if (gearsetItem.hasUnaugmented){
                    ImGui.TextColored(ImGuiColors.DalamudYellow, rawItem.Name);
                }

                else{
                    ImGui.TextColored(ImGuiColors.DalamudWhite, rawItem.Name);
                }

                if (rawItem.ExtendedItemLevel.Value is ExtendedItemLevel level){
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                    
                    ImGui.TextColored(ImGuiColors.ParsedGrey, $"i{level.RowId}");

                    for (var j = 0; j < gearsetItem.meldedMateria.Length; j++){
                        if (gearsetItem.meldedMateria[j] != null){
                            ImGui.SameLine();
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                            // ImGui.Text(materiaIDToName[gearsetItem.meldedMateria[j].materiaID].ToString());  

                            if (gearsetItem.meldedMateria[j].hasMateria){
                                ImGui.TextColored(ImGuiColors.ParsedGreen, materiaIDToName[gearsetItem.meldedMateria[j].materiaID].ToString()); 
                            }

                            else{
                                ImGui.TextColored(ImGuiColors.DalamudWhite, materiaIDToName[gearsetItem.meldedMateria[j].materiaID].ToString()); 
                            }

                            // ImGui.SameLine();
                            // ImGui.Text(gearsetItem.hasUnaugmented.ToString());
                            // if (j == 0){
                            //     ImGui.Text(gearsetItem.meldedMateria[0].tempMateriaID.ToString());
                            //     ImGui.SameLine();
                            //     ImGui.Text(gearsetItem.meldedMateria[0].grade.ToString());
                            //     ImGui.SameLine();
                            // }
                            // ImGui.Text(gearsetItem.meldedMateria[j].hasMateria.ToString());
                            // ImGui.Text(gearsetItem.meldedMateria[j].materiaID.ToString());                            
                        }
                    }
                }
                    
                //test code for checkbox

                // ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - 32);

                // ImGui.Checkbox("", ref gearsetItem.hasPiece);
            }
        }
    }
    
    public void DrawDeleteButton(){
        
        // var color = System.Drawing.Color.FromArgb(217, 39, 39, 204);
        ImGui.PushStyleColor(ImGuiCol.Text, 0xFC5A5AFF); // EE6244FF
        
        if (ImGui.Button("Delete")){
            clickedDelete = true;
        }
        ImGui.PopStyleColor();

        if(clickedDelete){
            DrawDeletionConfirmationWindow(ref clickedDelete);
        }
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
                    ExtendedItem rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID);
                    
                    if (gearsetItem.itemID == id){
                        gearsetItem.hasPiece = true;
                        // var materiaArray = item.Materia.ToArray();

                        for (byte materiaIndex = 0; materiaIndex < 5; materiaIndex++){
                            InGameMateria gameMateria = new InGameMateria(item.GetMateriaId(materiaIndex), item.GetMateriaGrade(materiaIndex));
                            if((gearsetItem.meldedMateria[materiaIndex] != null) && materiaIDMapping[gearsetItem.meldedMateria[materiaIndex].materiaID].grade == gameMateria.grade && materiaIDMapping[gearsetItem.meldedMateria[materiaIndex].materiaID].materiaGameID == gameMateria.materiaGameID){
                                gearsetItem.meldedMateria[materiaIndex].hasMateria = true;
                            }
                        }
                        
                        // var materiaID = item.GetMateriaId(0);
                        // var materiaGrade = item.GetMateriaGrade(0);
                        
                        // gearsetItem.meldedMateria[0].tempMateriaID = materiaID;
                        // gearsetItem.meldedMateria[0].grade = materiaGrade;

                        break;

                    }

                    if (rawItem.Name.RawString.Contains("Augmented")){
                        ExtendedItem currentInvItem = Data.ItemSheet.GetRow(id);
                        var gearsetItemString = rawItem.Name.RawString.Split(" ").ToList();
                        var currentItemString = currentInvItem.Name.RawString.Split(" ").ToList();
                        
                        if (gearsetItemString[1] == currentItemString[0]){
                            // gearsetItemString.Remove("Augmented");
                            gearsetItemString.Remove(gearsetItemString.First());
                            // gearsetItemString.Remove();
                            
                            var sameItemType = true;
                            for(var j = 0; j < gearsetItemString.Count; j++){
                                if (gearsetItemString[j] != currentItemString[j]){
                                    sameItemType = false;
                                    break;
                                }
                            }

                            if (sameItemType){
                                gearsetItem.hasUnaugmented = true;
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }

    protected int getTomes(Gearset givenGearset){
        int updatedCost = 0;
        
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
                GearCost costOfItem = COST[name];

                if (gearsetItem.hasPiece){
                    
                    if (rawItem.Name.RawString.Contains("Augmented")){
                        updatedCost += (int)costOfItem.tomeCost;
                    }
                }

                else if (gearsetItem.hasUnaugmented){
                    updatedCost += (int)costOfItem.tomeCost; 
                }
            }

        }

        return updatedCost;
    }

    //different ways of calculating book costs
    //buying everything with books, including gear pieces + glaze/twine (gear sheet default)
    //buying only glaze/twine with books
    protected uint[] getInitialCost(Gearset givenGearset, bool buyTwineGlazeOnly){
        uint[] returnCost = new uint[5];

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

                GearCost costOfItem = COST[name];

                // if (!gearsetItem.hasPiece){
                    costHelperFunction(name, rawItem, costOfItem, buyTwineGlazeOnly, returnCost);
                // }

                // if (!gearsetItem.hasPiece){

                // }

            }
        }

        return returnCost;
    }

    protected void costHelperFunction(string name, ExtendedItem rawItem, GearCost costOfItem, bool buyTwineGlazeOnly, uint[] returnCost){
        //the overall function should grab the cost of missing pieces

        //if you don't have a gearset piece, add the cost of the piece with books, and the cost of its upgrade if its a tome piece.
        // if it is a raid piece && we !buyTwineGlazeOnly, add cost of books
        // if it is a tome piece && we !buyTwineGlazeOnly, add cost of books for upgrade + tome cost

        //if it is a tome piece && we buyTwineGlazeOnly, add cost of books for upgrade + tome cost
        // 
        //


        if (name == "weapon" && buyTwineGlazeOnly){
            return;
        }

        if ((!buyTwineGlazeOnly) && !rawItem.Name.RawString.Contains("Augmented")){
            returnCost[costOfItem.bookType-1] += (uint)costOfItem.bookCost;
        }
        
        else if (rawItem.Name.RawString.Contains("Augmented")){
            returnCost[costOfItem.bookUpgradeType-1] += (uint)costOfItem.bookUpgradeCost;
            returnCost[4] += (uint)costOfItem.tomeCost;
        }
    }

    protected void calc(Gearset givenGearset, PlayerGearCost playerGearCost, bool buyTwineGlazeOnly){
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
                GearCost costOfItem = COST[name];

                //ways of knowing what item is what
                //if hasPiece then you have a raid or augmented tome piece, you can figure out which is which by a contains(Augmented) function.
                if (gearsetItem.hasPiece){
                    if (!rawItem.Name.RawString.Contains("Augmented") && !buyTwineGlazeOnly){
                        playerGearCost.currentBookCost[costOfItem.bookType-1] += (uint)costOfItem.bookCost;
                    }

                    else if (rawItem.Name.RawString.Contains("Augmented")){
                        playerGearCost.currentBookCost[costOfItem.bookUpgradeType-1] += (uint)costOfItem.bookUpgradeCost;

                    }
                }

                else if (gearsetItem.hasUnaugmented){
                    playerGearCost.remainingTotalTomes += (uint)costOfItem.tomeCost;
                }

                //if its a raid piece, subtract the books IF and ONLY if !buyTwineGlaze only.
                //if its a tome piece, subtract the books of its corresponding upgrade type  

                //if !hasPiece && hasUnaugmented, subtract tomes only

            }        

        }
    }

    //probably new functions to look for books/upgrade augs here
    protected unsafe void getCurrentBooksOrUpgrades(Gearset givenGearset, PlayerGearCost playerGearCost, bool buyTwineGlazeOnly){
        for (int i = 0; i <= 5; i++){
            InventoryContainer* inventory = GetInventoryContainer(itemSpace[i]);

            for (var j = 0; j < inventory->Size; j++){
                var item = inventory->Items[j];
                uint id = item.ItemId;

                if (id == 0){
                    continue;
                }

                foreach (raidDropIDs raidDropID in Enum.GetValues(typeof(raidDropIDs))){
                    if (id == (uint)raidDropID){
                        if (id == (uint)raidDropIDs.Glaze){
                            playerGearCost.remainingBookCost[raidDropIDToIndex[id]] -= (uint)bookCost.GLAZE;
                        }

                        else if (id == (uint)raidDropIDs.Twine){
                            playerGearCost.remainingBookCost[raidDropIDToIndex[id]] -= (uint)bookCost.TWINE_SOLVENT;
                        }   

                        else{
                            playerGearCost.bookCount[raidDropIDToIndex[id]] = item.Quantity;
                        }                    
                    }
                }

                // foreach (raidDropIDs raidDropID in Enum.GetValues(typeof(raidDropIDs))){
                //     if ((uint)raidDropID == id){
                //         if (id != (uint)raidDropIDs.Glaze && id != (uint)raidDropIDs.Twine){
                //             playerGearCost.bookCount[raidDropIDToIndex[id]] = item.Quantity;
                            
                //         }

                //         else{
                //             if (id == (uint)raidDropIDs.Glaze){
                //                 playerGearCost.remainingBookCost[raidDropIDToIndex[id]] -= (uint)bookCost.GLAZE;
                //             }

                //             else if (id == (uint)raidDropIDs.Twine){
                //                 playerGearCost.remainingBookCost[raidDropIDToIndex[id]] -= (uint)bookCost.TWINE_SOLVENT;
                //             }
                            
                //         }
                //         break;
                //     }
                // }
            }
        }
        
        // Type type = givenGearset.GetType();
        // PropertyInfo[] properties = type.GetProperties();
        // foreach (PropertyInfo property in properties){
        //     string name = property.Name;
        //     object value = property.GetValue(givenGearset);
        //     if (value == null || name == "offHand"){
        //         continue;
        //     }

        //     if (property.PropertyType == typeof(MeldedItem)){
                
        //         MeldedItem gearsetItem = (MeldedItem)value;
        //         ExtendedItem rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID);

        //         GearCost costOfItem = COST[name];

        //         if (gearsetItem.hasPiece){
        //             if (name == "weapon" && buyTwineGlazeOnly){
        //                 return;
        //             }

        //             if ((!buyTwineGlazeOnly) && !rawItem.Name.RawString.Contains("Augmented")){
        //                 playerGearCost.remainingBookCost[costOfItem.bookType-1] -= (uint)costOfItem.bookCost;
        //             }
                    
        //             else if (rawItem.Name.RawString.Contains("Augmented")){
        //                 playerGearCost.remainingBookCost[costOfItem.bookUpgradeType-1] -= (uint)costOfItem.bookUpgradeCost;
        //                 playerGearCost.remainingBookCost[4] -= (uint)costOfItem.tomeCost;
        //             }
        //         }
        //     }
        // }
    }
}
