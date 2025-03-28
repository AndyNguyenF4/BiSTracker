﻿using System;
using System.Reflection;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using BiSTracker.Models;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Colors;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace BiSTracker.Windows;
public class MainWindow : Window, IDisposable
{
    private string GoatImagePath; //delete later
    private string etroImportString = "";
    private string etroID = "";
    private static readonly HttpClient client = new HttpClient();
    private bool clickedDelete;
    private List<InventoryType> itemSpace;
    private DirectoryInfo savedSetsDirectory;
    private Dictionary<uint, InGameMateria> materiaIDMapping = new Dictionary<uint, InGameMateria>{
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
    private Dictionary<uint, string> materiaIDToName = new Dictionary<uint, string>{
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
        Book1 = 46732,
        Book2 = 46733,
        Book3 = 46734,
        Book4 = 46735,
        Glaze = 46731,
        Twine = 46730,
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
        WEAP = 8,
        HEAD_HANDS_FEET = 4,
        CHEST_LEGS = 6,
        ACCESSORY = 3,
        TWINE = 4,
        GLAZE = 3
    };

    public enum tomeCost : int{
        WEAP = 500,
        HEAD_HANDS_FEET = 495,
        CHEST_LEGS = 825,
        ACCESSORY = 375
    }
    //tomeCost tomeCost, bookCost bookCost, int bookType, bookCost bookUpgradeCost, int bookUpgradeType
    private Dictionary<string, GearCost> COST = new Dictionary<string, GearCost>{
        ["weapon"] = new GearCost(tomeCost.WEAP, bookCost.WEAP, 4, bookCost.TWINE, 3),
        ["head"] = new GearCost(tomeCost.HEAD_HANDS_FEET, bookCost.HEAD_HANDS_FEET, 2, bookCost.TWINE, 3),
        ["body"] = new GearCost(tomeCost.CHEST_LEGS, bookCost.CHEST_LEGS, 3, bookCost.TWINE, 3),
        ["hands"] = new GearCost(tomeCost.HEAD_HANDS_FEET, bookCost.HEAD_HANDS_FEET, 2, bookCost.TWINE, 3),
        ["legs"] = new GearCost(tomeCost.CHEST_LEGS, bookCost.CHEST_LEGS, 3, bookCost.TWINE, 3),
        ["feet"] = new GearCost(tomeCost.HEAD_HANDS_FEET, bookCost.HEAD_HANDS_FEET, 2, bookCost.TWINE, 3),
        ["ears"] = new GearCost(tomeCost.ACCESSORY, bookCost.ACCESSORY, 1, bookCost.GLAZE, 2),
        ["neck"] = new GearCost(tomeCost.ACCESSORY, bookCost.ACCESSORY, 1, bookCost.GLAZE, 2),
        ["wrists"] = new GearCost(tomeCost.ACCESSORY, bookCost.ACCESSORY, 1, bookCost.GLAZE, 2),
        ["fingerL"] = new GearCost(tomeCost.ACCESSORY, bookCost.ACCESSORY, 1, bookCost.GLAZE, 2),
        ["fingerR"] = new GearCost(tomeCost.ACCESSORY, bookCost.ACCESSORY, 1, bookCost.GLAZE, 2)
    };
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
            if (set.Contains("XIVGear_")){
                savedGearsets.Add(set, xivGearToGearSet(xivGJSONToObject(set.Split("_")[1])));
            }

            else{
                savedGearsets.Add(set, etroImport(set));
            }
        }

        if (savedGearsets.Count > 0){
            if (Plugin.Configuration.lastSavedSet.Length == 0){
                currentGearset = savedGearsets.First().Value; 
            }
            
            else{
                currentGearset = savedGearsets[Plugin.Configuration.lastSavedSet];
            }
        }

        else{
            currentGearset = null;
        }

        clickedDelete = false;
        Plugin.ClientState.Logout += clearItemFlags;
    }

    public void Dispose() {}
    public override void Draw()
    {
        //etro links are 60 chars long, 61 for C# esc char? 67 for xivgear
        ImGui.BeginGroup();
        ImGui.InputTextWithHint("##EtroImportTextBox", "Insert Etro URL and click \"Import\"", ref etroImportString, 68);
        ImGui.SameLine();
        if(ImGui.Button("Import" + "##EtroImportButton")){
        
            //if import string >= 0 then attempt to read etro link and then reset string
            if(etroImportString.Length > 0 && etroImportString.Contains("etro")){
                etroImport(etroImportString, savedSetsDirectory);
            }

            else if(etroImportString.Length > 0 && etroImportString.Contains("xivgear")){
                xivGearImport(etroImportString);
            }
        }

        ImGui.SameLine();
        if(ImGui.Button("Inventory Sync##SyncButton")){
            if (savedGearsets != null){
                if (savedGearsets.Count > 0){
                    clearItemFlags(0, 0);
                }
            }
        }

        drawDropdown();
        
        if (currentGearset != null){
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            
            //checks all inv spaces for gear
            foreach (var invTypeEnum in itemSpace){
                bisComparison(currentGearset, invTypeEnum);
            }

            DrawItems(currentGearset);
            drawBookAndTomes();
        }

        ImGui.EndGroup();
        
        //sample plugin stuff

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

    protected void drawBookAndTomes(){
        PlayerGearCost playerGearsetCost = new PlayerGearCost(
            new uint[5],
            getInitialCost(currentGearset, Plugin.Configuration.buyTwineGlazeOnly),
            getCurrentTomes(),
            (uint)getTomes(currentGearset)
        );

        getCurrentBooksOrUpgrades(currentGearset, playerGearsetCost, Plugin.Configuration.buyTwineGlazeOnly);
        calc(currentGearset, playerGearsetCost, Plugin.Configuration.buyTwineGlazeOnly);
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Text("Books:");
        ImGui.SameLine(0, 10);

        for (var index = 0; index < playerGearsetCost.bookCount.Length; index++){
            if (index < playerGearsetCost.bookCount.Length-1){
                var currentBooks = playerGearsetCost.bookCount[index];
                var remainingBookCost = playerGearsetCost.remainingBookCost[index];
                var currentBookCost = playerGearsetCost.currentBookCost[index];
                var calculatedBookCost = remainingBookCost-currentBookCost;
                var colorText = playerGearsetCost.bookColors[index];

                if (currentBookCost > remainingBookCost){
                    ImGui.TextColored(colorText, $"{currentBooks}/0");
                }

                else{
                    ImGui.TextColored(colorText, $"{currentBooks}/{calculatedBookCost}");//+ (remainingBookCost-currentBookCost).ToString()
                }

                ImGui.SameLine(0, 15);
            }

            else{
                var currentTomes = playerGearsetCost.currentTomes;
                var totalTomeCost = playerGearsetCost.remainingBookCost[index];
                var remainingTomes = playerGearsetCost.remainingTotalTomes;
                var overallRemainingTomes = totalTomeCost-remainingTomes;
                var colorText = playerGearsetCost.bookColors[index];
                
                ImGui.NewLine();
                ImGui.Text("Tomes: ");
                ImGui.SameLine(0, 5);
                
                ImGui.TextColored(colorText, $"{currentTomes}/{overallRemainingTomes}");
            }
        }

        ImGui.NewLine();

        bool buyTwineGlazeOnly = Plugin.Configuration.buyTwineGlazeOnly;
        if (ImGui.Checkbox("Use books for Twines and Glazes only", ref buyTwineGlazeOnly)){
            Plugin.Configuration.buyTwineGlazeOnly = buyTwineGlazeOnly;
            Plugin.Configuration.Save();
        }
        ImGui.NewLine();
    }

    protected void drawDropdown(){
        if (savedGearsets != null){
            if (savedGearsets.Count > 0){
                var arrayOfGearsets = new string[savedGearsets.Count];
                var currentIndexGearset = 0;
                var dropdownIndexReference = 0;
                foreach(KeyValuePair<string, Gearset> kvp in savedGearsets){
                    arrayOfGearsets[currentIndexGearset] = kvp.Value.name;
                    //this sets the gearset indexing to the currentgearset, so figure out a way to implement saving last set and loading it first
                    if (kvp.Value.Equals(currentGearset)){
                        dropdownIndexReference = currentIndexGearset;
                    }

                    if (currentIndexGearset<arrayOfGearsets.Length-1){
                        currentIndexGearset++;
                    }
                }               
                
                if(ImGui.Combo("##GearsetDropDown", ref dropdownIndexReference, arrayOfGearsets, arrayOfGearsets.Length)){
                    var tempIndex = 0;
                    
                    foreach(KeyValuePair<string, Gearset> kvp in savedGearsets){
                        if (kvp.Value.name == arrayOfGearsets[dropdownIndexReference]){
                            dropdownIndexReference = tempIndex;
                            currentGearset = savedGearsets[kvp.Key];
                            Plugin.Configuration.lastSavedSet = kvp.Key;
                            Plugin.Configuration.Save();
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
    }

    protected void clearItemFlags(int type1, int code){
        if (savedGearsets != null){
            if (savedGearsets.Count > 0){
                foreach (KeyValuePair<string, Gearset> kvp in savedGearsets){
                    var currentSet = kvp.Value;
                    
                    Type type = currentSet.GetType();
                    PropertyInfo[] properties = type.GetProperties();
                    
                    foreach (PropertyInfo property in properties){
                        object value = property.GetValue(currentSet);
                        
                        if (value == null){
                            continue;
                        }

                        if (property.PropertyType == typeof(MeldedItem)){
                            MeldedItem gearsetItem = (MeldedItem)value;
                            
                            gearsetItem.hasPiece = false;
                            gearsetItem.hasUnaugmented = false;

                            foreach (MeldedMateria materia in gearsetItem.meldedMateria){
                                if (materia != null){
                                    materia.hasMateria = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    protected unsafe uint getCurrentTomes(){
        //uint id = Data.TomestonesSheet.Where(tomestone => tomestone.Tomestones.RowId is 3).First().Item.RowId;

        uint id = Data.TomestonesSheet.First(item => item.Tomestones.RowId is 3).Item.RowId;

        var returnVal = (uint)InventoryManager.Instance()->GetInventoryItemCount(id);
        return returnVal;
    }
	protected ISharedImmediateTexture? GetIcon(Item? item, bool hq = false) {
		if (item is not null)
			return GetIcon(item.Value.Icon, hq);
		return null;
	}
	protected ISharedImmediateTexture GetIcon(uint id, bool hq = false) {
		return Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(id, hq));
	}
    protected Gearset xivGearToGearSet(XIVGearsetParse inputGear){
        Gearset returnGearset = new Gearset(inputGear);
        returnGearset.fillMateria(inputGear); 

        return returnGearset;
    }
    protected XIVGearsetParse xivGJSONToObject(string xivGearID){
        string fullPath = savedSetsDirectory + "\\" + "XIVGear_" + xivGearID + ".json";
        string jsonString = File.ReadAllText(fullPath);

        return JsonSerializer.Deserialize<XIVGearsetParse>(jsonString); 
    }
    protected void xivGearImport(string xivGearURL){
        var xivGearID = "";

        if (!xivGearURL.Contains("https://xivgear.app/?page=sl%7C")){
            return;
        }

        xivGearID = xivGearURL.Split("%7C")[1];

        Task.Run(async () =>
        {
            using HttpResponseMessage response = await client.GetAsync("https://api.xivgear.app/shortlink/" + xivGearID);
            string responseBody = await response.Content.ReadAsStringAsync();

            if(response.IsSuccessStatusCode){
                File.WriteAllText(savedSetsDirectory + "\\" + "XIVGear_" + xivGearID + ".json", responseBody);
                Gearset temp = xivGearToGearSet(xivGJSONToObject(xivGearID));
                var uniqueXIVGearID = "XIVGear_" + xivGearID;

                savedGearsets.Add(uniqueXIVGearID, temp);
                foreach (var invTypeEnum in itemSpace){
                    bisComparison(temp, invTypeEnum);
                }
                
                currentGearset = temp;
                etroImportString = "";

                Plugin.Configuration.availableGearsets.Add(uniqueXIVGearID);
                Plugin.Configuration.lastSavedSet = uniqueXIVGearID;
                Plugin.Configuration.Save();
                etroImportString = "";
            }
        });
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

        // var etroImportStringSplit = etroURL.Split("gearsets/");

        Task.Run(async () =>
        {
            using HttpResponseMessage response = await client.GetAsync("https://etro.gg/api/gearsets/" + etroID);
            string responseBody = await response.Content.ReadAsStringAsync();

            if(response.IsSuccessStatusCode){
                File.WriteAllText(directory + "\\" + etroID + ".json", responseBody);
                Gearset temp = etroGearToGearSet(etroJsonToObject(etroID, directory));
                
                savedGearsets.Add(etroID, temp);
                foreach (var invTypeEnum in itemSpace){
                    bisComparison(temp, invTypeEnum);
                }
                
                currentGearset = temp;
                etroImportString = "";

                Plugin.Configuration.availableGearsets.Add(etroID);
                Plugin.Configuration.lastSavedSet = etroID;
                Plugin.Configuration.Save();
            }
            etroID = "";
        });
    }
    protected static EtroGearsetParse etroJsonToObject(string etroID, DirectoryInfo directory){
        string fullPath = directory + "\\" + etroID + ".json";
        string jsonString = File.ReadAllText(fullPath);

        return JsonSerializer.Deserialize<EtroGearsetParse>(jsonString); 
    }
    protected static Gearset etroGearToGearSet(EtroGearsetParse inputGear){
        Gearset returnEtroGearset = new Gearset(inputGear);
        if (inputGear.materia != null){
            returnEtroGearset.fillMateria(inputGear.materia); 
        }

        return returnEtroGearset;
    }
    public void DrawItems(Gearset gearsetTest){
        Type type = gearsetTest.GetType();
        PropertyInfo[] properties = type.GetProperties();

        float scale = ImGui.GetFontSize() / 17;

        foreach (PropertyInfo property in properties){
            object value = property.GetValue(gearsetTest);

            if (value == null){
                continue;
            }

            if (property.PropertyType == typeof(MeldedItem)){
                MeldedItem gearsetItem = (MeldedItem)value;
                var rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID); 
                
                var icon = GetIcon(rawItem)?.GetWrapOrEmpty();
                int height = icon is null ? 0 : Math.Min(icon.Height, (int) (32 * scale));
                
                if (icon != null){
                    ImGui.Image(icon.ImGuiHandle, new Vector2(height, height));
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                }

                if (gearsetItem.hasPiece){
                    ImGui.TextColored(ImGuiColors.ParsedGreen, rawItem.Name.ExtractText().ToString());
                }

                else if (gearsetItem.hasUnaugmented){
                    ImGui.TextColored(ImGuiColors.DalamudYellow, rawItem.Name.ExtractText().ToString());
                }

                else{
                    ImGui.TextColored(ImGuiColors.DalamudWhite, rawItem.Name.ExtractText().ToString());
                }

                if (rawItem.LevelItem.IsValid){
                    var level = rawItem.LevelItem.RowId;
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                    
                    ImGui.TextColored(ImGuiColors.ParsedGrey, $"i{level}");

                    for (var j = 0; j < gearsetItem.meldedMateria.Length; j++){
                        if (gearsetItem.meldedMateria[j] != null){
                            ImGui.SameLine();
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);  

                            try{
                                if (gearsetItem.meldedMateria[j].hasMateria){
                                    ImGui.TextColored(ImGuiColors.ParsedGreen, materiaIDToName[gearsetItem.meldedMateria[j].materiaID].ToString()); 
                                }

                                else{
                                    ImGui.TextColored(ImGuiColors.DalamudWhite, materiaIDToName[gearsetItem.meldedMateria[j].materiaID].ToString()); 
                                }    
                            } 
                            catch{
                                ImGui.Spacing();
                            }                    
                        }
                    }
                }
            }
        }
    }
    public void DrawDeleteButton(){
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
    protected bool DrawDeletionConfirmationWindow(ref bool isVisible){
        if (!isVisible)
            return false;

        var ret = false;

        ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(280f, 120f));
        ImGui.Begin("Gearset Deletion Confirmation", ImGuiWindowFlags.NoResize);
        ImGui.Text("Are you sure you want to delete this?");
        ImGui.Text("This cannot be undone.");
        ImGui.PushStyleColor(ImGuiCol.Text, 0xFC5A5AFF);
        
        if (ImGui.Button("Yes")){
            isVisible = false;
            ret = true;
        }
        
        ImGui.PopStyleColor();
        ImGui.SameLine();

        if (ImGui.Button("No")){
            isVisible = false;
        }

        if (ret){
            var setName = Plugin.Configuration.lastSavedSet;
            
            savedGearsets.Remove(setName);
            File.Delete(savedSetsDirectory + "\\" + setName + ".json");
            Plugin.Configuration.availableGearsets.Remove(setName);

            Plugin.Configuration.lastSavedSet = "";
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
        
        if (inventory == null){
            return;
        }

        for (uint i = 0; i < inventory->Size; i++){
            var item = inventory->Items[i];
            uint id = item.ItemId;
            
            if (id == 0){
                continue;
            }
            
            Type type = givenGearset.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties){
                string name = property.Name;
                object value = property.GetValue(givenGearset);

                if (value == null){
                    continue;
                }

                if (property.PropertyType == typeof(MeldedItem)){
                    MeldedItem gearsetItem = (MeldedItem)value;
                    var rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID);
                    
                    if (gearsetItem.itemID == id){
                        gearsetItem.hasPiece = true;

                        try{
                            for (byte materiaIndex = 0; materiaIndex < 5; materiaIndex++){
                                InGameMateria gameMateria = new InGameMateria(item.GetMateriaId(materiaIndex), item.GetMateriaGrade(materiaIndex));
                                if((gearsetItem.meldedMateria[materiaIndex] != null) && materiaIDMapping[gearsetItem.meldedMateria[materiaIndex].materiaID].grade == gameMateria.grade && materiaIDMapping[gearsetItem.meldedMateria[materiaIndex].materiaID].materiaGameID == gameMateria.materiaGameID){
                                    gearsetItem.meldedMateria[materiaIndex].hasMateria = true;
                                }
                            }
                        }

                        catch{
                           break; 
                        }
                        
                        break;
                    }

                    if (rawItem.Name.ExtractText().ToString().Contains("Augmented")){
                        var currentInvItem = Data.ItemSheet.GetRow(id);
                        var gearsetItemString = rawItem.Name.ExtractText().ToString().Split(" ").ToList();
                        var currentItemString = currentInvItem.Name.ExtractText().ToString().Split(" ").ToList();

                        
                        if (gearsetItemString[1] == currentItemString[0]){
                            gearsetItemString.Remove(gearsetItemString.First());
                            
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
                var rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID);
                GearCost costOfItem = COST[name];

                if (gearsetItem.hasPiece){
                    
                    if (rawItem.Name.ExtractText().ToString().Contains("Augmented")){
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
                var rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID);
                GearCost costOfItem = COST[name];

                costHelperFunction(name, rawItem, costOfItem, buyTwineGlazeOnly, returnCost);
            }
        }
        return returnCost;
    }
    protected void costHelperFunction(string name, Item rawItem, GearCost costOfItem, bool buyTwineGlazeOnly, uint[] returnCost){
        if (name == "weapon" && buyTwineGlazeOnly){
            return;
        }

        if ((!buyTwineGlazeOnly) && !rawItem.Name.ExtractText().ToString().Contains("Augmented")){
            returnCost[costOfItem.bookType-1] += (uint)costOfItem.bookCost;
        }
        
        else if (rawItem.Name.ExtractText().ToString().Contains("Augmented")){
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
                var rawItem = Data.ItemSheet.GetRow(gearsetItem.itemID);
                GearCost costOfItem = COST[name];

                if (gearsetItem.hasPiece){
                    if (!rawItem.Name.ExtractText().ToString().Contains("Augmented") && !buyTwineGlazeOnly){
                        playerGearCost.currentBookCost[costOfItem.bookType-1] += (uint)costOfItem.bookCost;
                    }

                    else if (rawItem.Name.ExtractText().ToString().Contains("Augmented")){
                        playerGearCost.currentBookCost[costOfItem.bookUpgradeType-1] += (uint)costOfItem.bookUpgradeCost;

                    }
                }

                else if (gearsetItem.hasUnaugmented){
                    playerGearCost.remainingTotalTomes += (uint)costOfItem.tomeCost;
                }
            }        
        }
    }
    protected unsafe void getCurrentBooksOrUpgrades(Gearset givenGearset, PlayerGearCost playerGearCost, bool buyTwineGlazeOnly){
        for (int i = 0; i <= 5; i++){
            InventoryContainer* inventory = GetInventoryContainer(itemSpace[i]);
            if (inventory == null){
                continue;
            }

            for (var j = 0; j < inventory->Size; j++){
                var item = inventory->Items[j];
                uint id = item.ItemId;

                if (id == 0){
                    continue;
                }

                foreach (raidDropIDs raidDropID in Enum.GetValues(typeof(raidDropIDs))){
                    if (id == (uint)raidDropID){
                        if (id == (uint)raidDropIDs.Glaze){
                            playerGearCost.remainingBookCost[raidDropIDToIndex[id]] -= (uint)bookCost.GLAZE * item.GetQuantity();
                        }

                        else if (id == (uint)raidDropIDs.Twine){
                            playerGearCost.remainingBookCost[raidDropIDToIndex[id]] -= (uint)bookCost.TWINE * item.GetQuantity();
                        }   

                        else{
                            playerGearCost.bookCount[raidDropIDToIndex[id]] = (uint)item.Quantity;
                        }   
                        
                        break;                 
                    }
                }
            }
        }
    }
}
