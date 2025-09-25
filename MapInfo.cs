using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Translations;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

    public class MapInfoPlugin : BasePlugin 
    {
    public override string ModuleName => "MapInfo";
    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "alnoise";

    public class MapData {
        public int Tier { get; set; }
        public int Stages { get; set; }
        public string Type { get; set; }
    }

    private Dictionary<string, MapData> mapInfo;

    private LangConfig lang;

    public override void Load(bool hotReload) 
    {
        Console.WriteLine("[MapInfo] Plugin Loaded!");

        LoadLang();
        LoadMapData();
        AddCommand("m", "Shows map info", Command_MapInfo);
    }

    public class LangConfig
    {
        public string Prefix { get; set; } = "[Map Info]";
        public string CurrentMap { get; set; } = "{PREFIX} {MAP} - Tier: {TIER}, Type: {TYPE}{STAGES}";
        public string OtherMap { get; set; } = "{PREFIX} {MAP} - Tier: {TIER}, Type: {TYPE}{STAGES}";
        public string NoData { get; set; } = "{PREFIX} No data found for {MAP}";
        public string StagesFormat { get; set; } = ", Stages: {STAGES}";
    }

    private void LoadLang()
    {
        string path = Path.Combine(ModuleDirectory, "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            lang = JsonConvert.DeserializeObject<LangConfig>(json) ?? new LangConfig();
        }
        else
        {
            lang = new LangConfig();
            File.WriteAllText(path, JsonConvert.SerializeObject(lang, Formatting.Indented));
            Console.WriteLine("[MapInfo] Created default configuration file.");
        }
    }

    private void LoadMapData() 
    {
        string path = Path.Combine(ModuleDirectory, "mapinfo.json");
        if (File.Exists(path)) {
            string json = File.ReadAllText(path);
            mapInfo = JsonConvert.DeserializeObject<Dictionary<string, MapData>>(json);
        }
        else
        {
            mapInfo = new Dictionary<string, MapData>();
            Console.WriteLine($"[MapInfoPlugin] No mapinfo.json found at {path}");
        }
    }

    private void Command_MapInfo(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        string mapName;
        if (info.ArgCount > 1)
        {
            mapName = info.GetArg(1).ToLower();
        }
        else
        {
            mapName = Server.MapName.ToLower();
        }

        if (mapInfo.TryGetValue(mapName, out var data))
        {
            string stagesText = "";
            if (!string.Equals(data.Type, "linear", StringComparison.OrdinalIgnoreCase))
            {
                stagesText = lang.StagesFormat.Replace("{STAGES}", data.Stages.ToString());
            }

            string template = info.ArgCount > 1 ? lang.OtherMap : lang.CurrentMap;

            string msg = template
                .Replace("{PREFIX}", lang.Prefix)
                .Replace("{MAP}", mapName)
                .Replace("{TIER}", data.Tier.ToString())
                .Replace("{TYPE}", data.Type)
                .Replace("{STAGES}", stagesText);

            msg = msg.ReplaceColorTags();

            player.PrintToChat(msg);

        }
        else
        {
            string msg = lang.NoData
                .Replace("{PREFIX}", lang.Prefix)
                .Replace("{MAP}", mapName);

            msg = msg.ReplaceColorTags();

            player.PrintToChat(msg);
        }
    }
}
