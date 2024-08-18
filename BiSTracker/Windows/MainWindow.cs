using System;
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

namespace BiSTracker.Windows;


public class MainWindow : Window, IDisposable
{
    private string GoatImagePath;
    private string etroImportString = "";

    private string etroID = "";
    private string[] etroImportStringSplit = new string[2];

    static readonly HttpClient client = new HttpClient();

    private string placeholder ="";

    private DirectoryInfo savedSetsDirectory;
    // protected PluginUI Ui { get;}

    //increment to 13 for food later
    private uint[] gearID = {43107, 43076, 43154, 43155, 43079, 43157, 0, 43162, 43090, 43172, 43100, 43177};
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
        // client = new HttpClient();
    }

    public void Dispose() { }

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
                placeholder = etroImportString;
                etroImportString = "";
            }
        }

        ImGui.Text(placeholder);
        ImGui.Text(savedSetsDirectory.ToString());

        ImGui.EndGroup();
        
        if(ImGui.CollapsingHeader("Items", ImGuiTreeNodeFlags.DefaultOpen)){

            float scale = ImGui.GetFontSize() / 17;
            for (int i = 0; i < gearID.Length; i++){
                if (gearID[i] == 0){
                    //should skip over null gear update later
                    continue;
                }
                
                ExtendedItem rawItem = Data.ItemSheet.GetRow(gearID[i]);
                var icon = GetIcon(rawItem)?.GetWrapOrEmpty();
                int height = icon is null ? 0 : Math.Min(icon.Height, (int) (32 * scale));
                if (icon != null){
                    ImGui.Image(icon.ImGuiHandle, new Vector2(height, height));
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                }

                ImGui.Text(rawItem.Name);

                if (rawItem.ExtendedItemLevel.Value is ExtendedItemLevel level){
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (height - ImGui.GetFontSize()) / 2);
                    
                    ImGui.TextColored(ImGuiColors.ParsedGrey, $"i{level.RowId}");
                }
            }



        }

        
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

    }

	protected ISharedImmediateTexture? GetIcon(Item? item, bool hq = false) {
		if (item is not null)
			return GetIcon(item.Icon, hq);
		return null;
	}

	protected ISharedImmediateTexture GetIcon(uint id, bool hq = false) {
		return Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(id, hq));
	}

    protected void etroImport(string etroURL, DirectoryInfo directory){

        for (int i = etroURL.Length-1; i >= 0; i--){
            if (etroURL[i] == '/'){
                break;
            }

            etroID = etroURL[i] + etroID;
        }

    
        
        etroImportStringSplit = etroURL.Split("gearsets/");


        //FIGURE OUT WHY THIS DOESNT WORK
        Task.Run(async () =>
        {
            using HttpResponseMessage response = await client.GetAsync("https://etro.gg/api/gearsets/" + etroID); //etroImportStringSplit[^1]
            // using HttpResponseMessage response = await client.GetAsync("https://etro.gg/api/gearsets/903fafde-f0bf-4e99-9d80-4aceab2d36f2");
            // response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            File.WriteAllText(directory + "\\" + etroID + ".json", responseBody);
            etroID = "";
            // Task<string> response = client.GetStringAsync("https://etro.gg/api/gearsets/903fafde-f0bf-4e99-9d80-4aceab2d36f2");
            // // response.EnsureSuccessStatusCode();
            // var asStringTask = response.Content.ReadAsStringAsync();
            // asStringTask.wait();
            // return responseBody;
            // placeholder = responseBody;
        

        });
    }

        // return "failed";

        // try{
        //     HttpResponseMessage response = await client.GetAsync("https://etro.gg/api/gearsets/903fafde-f0bf-4e99-9d80-4aceab2d36f2");
        //     response.EnsureSuccessStatusCode();
        //     string responseBody = await response.Content.ReadAsStringAsync();
        //     return responseBody;


        //     // using HttpResponseMessage etroResult = client.GetStringAsync("https://etro.gg/api/gearsets/903fafde-f0bf-4e99-9d80-4aceab2d36f2");
        //     // string etroJson = etroResult.Content.ReadAsStringAsync();
        //     // return etroJson;
        //     // Console.WriteLine(etroResult);

        //     // string[] etroURLSplit = etroResult.Split("/");

        //     // string folder = @directory.ToString();
            
            
        //     // string fileName = "903fafde-f0bf-4e99-9d80-4aceab2d36f2.txt";
        //     // string fullPath = folder+fileName;
        // }

        // catch (HttpRequestException e){
        //     Console.WriteLine("\nException Caught!");
        //     Console.WriteLine("Message :{0} ", e.Message);
        //     return "failed connection";
        // }

}
