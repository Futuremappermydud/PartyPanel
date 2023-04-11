using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PartyPanelShared;
using PartyPanelShared.Models;
using PartyPanelUI.Pages;
using ProtoBuf;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PartyPanelUI
{
    public class ModifierModel
    {
        public ModifierModel(string path, string name, string subName)
        {
            Path = path;
            Name = name;
            SubName = subName;
        }

        public ModifierModel(string path, string name, string subName, bool isNegative)
        {
            Path = path;
            Name = name;
            SubName = subName;
            IsNegative = isNegative;
        }

        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string SubName { get; set; } = "";
        public bool IsNegative { get; set; } = false;
    }
    public static class GlobalData
    {
        public static Dictionary<string, string> covers = new Dictionary<string, string>();
        public static Dictionary<string, string> characteristics = new Dictionary<string, string>();
        public static List<SongList> songLists = new List<SongList>();
        public static Dictionary<string, PreviewBeatmapLevel> allSongs = new Dictionary<string, PreviewBeatmapLevel>(); //Used for Indexing and searching
        public static List<ModifierModel> Modifiers = new List<ModifierModel>();
        public static Network.Server server;

        public static PreviewBeatmapLevel currentInGame;

        public static PreviewBeatmapLevel currentlevel;
        public static GameplayModifiers currentmods = new GameplayModifiers();
        public static int currentpage = 0;
        private static Characteristic _currentchar;
        private static string _currentdiff;

        public static Action<PreviewBeatmapLevel> SelectLevel;
        public static Action RefreshList;

        public static Characteristic currentchar
        {
            get
            {
                if (_currentchar == null || !currentlevel.chars.Contains(_currentchar))
                {
                    _currentchar = currentlevel?.chars[0];
                }
                return _currentchar;
            }
            set { _currentchar = value; }
        }
        public static string currentdiff
        {
            get
            {
                if (_currentdiff == null || !_currentchar.diffs.Contains(_currentdiff))
                {
                    _currentdiff = _currentchar?.diffs[0];
                }
                return _currentdiff;
            }
            set { _currentdiff = value; }
        }


        public static void StartServer()
        {
            server = new Network.Server(10155);
            server.Start();
            Logger.Info("Test");

            characteristics.Add("Standard", "standard.png");
            characteristics.Add("OneSaber", "onesaber.png");
            characteristics.Add("NoArrows", "dotnotes.png");
            characteristics.Add("90Degree", "90.png");
            characteristics.Add("360Degree", "360.png");
            characteristics.Add("Lawless", "ExtraDiffsIcon.png");
            characteristics.Add("Lightshow", "Lightshow.png");
            
            /*var s = new SongList();
            s.Levels = new PreviewBeatmapLevel[2];

            var level = new PreviewBeatmapLevel();
            level.Name = "Red Balloon";
            level.LevelId = "Bruh";
            level.SubName = "";
            level.Author = "Charli XcX";
            level.Mapper = "RedNewth";
            level.Owned = true;
            level.chars = new Characteristic[2] { new Characteristic("Standard", new string[2] {"sheesh", "bruh" }), new Characteristic("NoArrows", new string[2] { "Guru", "Stacky" }) };
            level.coverPath = "D:\\SteamLibrary\\steamapps\\common\\Beat Saber\\Beat Saber_Data\\CustomLevels\\25f3f (Red Balloon - RedNewth)\\ballon.png";
            var image = Image.FromFile(level.coverPath);
            var bitmap = Server.FixedSize(image, 128);
            level.cover = Server.ToByteArray(bitmap, ImageFormat.Png);
            GlobalData.covers.Add(level.LevelId, Convert.ToBase64String(level.cover));
            level.BPM = 1;
            s.Levels[0] = level;
            currentlevel = level;
            allSongs.Add(level.LevelId, level);

            level = new PreviewBeatmapLevel();
            level.Name = "Night Raid with a Dragon";
            level.LevelId = "Bruh2";
            level.SubName = "";
            level.Author = "Camellia";
            level.Mapper = "DE125 & Skeelie";
            level.Owned = true;
            level.chars = new Characteristic[1] { new Characteristic("OneSaber", new string[2] { "balls", "queef" }) };
            level.coverPath = "D:\\SteamLibrary\\steamapps\\common\\Beat Saber\\Beat Saber_Data\\CustomLevels\\7457 (Night Raid with a Dragon - DE125 & Skeelie)\\cover.jpg";
            image = Image.FromFile(level.coverPath);
            bitmap = Server.FixedSize(image, 128);
            level.cover = Server.ToByteArray(bitmap, ImageFormat.Jpeg);
            GlobalData.covers.Add(level.LevelId, Convert.ToBase64String(level.cover));
            level.BPM = 1;
            s.Levels[1] = level;
            allSongs.Add(level.LevelId, level);

            songLists.Add(s);*/

            Modifiers.Add(new ModifierModel("images/Modifiers/NoFail.png", "No Fail", "+0% / -50%", true));
            Modifiers.Add(new ModifierModel("images/Modifiers/OneLife.png", "1 Life", ""));
            Modifiers.Add(new ModifierModel("images/Modifiers/FourLives.png", "4 Lives", ""));
            Modifiers.Add(new ModifierModel("images/Modifiers/NoBombs.png", "No Bombs", "-10%", true));
            Modifiers.Add(new ModifierModel("images/Modifiers/NoObstacles.png", "No Walls", "-5%", true));
            Modifiers.Add(new ModifierModel("images/Modifiers/NoArrows.png", "No Arrows", "-30%", true));
            Modifiers.Add(new ModifierModel("images/Modifiers/GhostNotes.png", "Ghost Notes", "+11%"));
            Modifiers.Add(new ModifierModel("images/Modifiers/DisappearingArrows.png", "Disappearing Arrows", "+7%"));
            Modifiers.Add(new ModifierModel("images/Modifiers/SmallNotes.png", "Small Notes(beta)", ""));
            Modifiers.Add(new ModifierModel("images/Modifiers/ProMode.png", "Pro Mode(beta)", ""));
            Modifiers.Add(new ModifierModel("images/Modifiers/PreciseAngles.png", "Strict Angles(beta)", ""));
            Modifiers.Add(new ModifierModel("images/Modifiers/Zen.png", "Zen Mode", "-100%", true));
            Modifiers.Add(new ModifierModel("images/Modifiers/SlowerSong.png", "Slower Song", "-30%", true));
            Modifiers.Add(new ModifierModel("images/Modifiers/FasterSong.png", "Faster Song", "+8%"));
            Modifiers.Add(new ModifierModel("images/Modifiers/SuperFastSong.png", "Super Fast Song", "+10%"));
        }
    }
}
