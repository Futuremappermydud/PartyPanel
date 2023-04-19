using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PartyPanelShared;
using PartyPanelShared.Models;
using PartyPanelUI.Pages;
using ProtoBuf;
using SongDetailsCache.Structs;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Reflection.Emit;
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
        public static List<SongList> displaySongLists = new List<SongList>();
        public static List<SongList> songLists = new List<SongList>();
        public static Dictionary<string, PreviewBeatmapLevel> allSongs = new Dictionary<string, PreviewBeatmapLevel>(); //Used for Indexing and searching
        public static List<ModifierModel> Modifiers = new List<ModifierModel>();
        public static Network.Server? server;

        public static PreviewBeatmapLevel? currentInGame;

        public static PreviewBeatmapLevel? currentlevel;
        public static GameplayModifiers currentmods = new GameplayModifiers();
        public static int currentpage = 0;
        private static Characteristic? _currentchar;
        private static string _currentdiff = "";

        public static Action<PreviewBeatmapLevel>? SelectLevel;
        public static Action? RefreshList;
        public static Action<ChangeEventArgs>? Search;
        public static Action? Browse;
        public static Action? BeatSaverLoaded;

        public static Song GetSong(PreviewBeatmapLevel? level)
        {
            string id = level != null ? level.LevelId : currentlevel.LevelId;
            if(id.StartsWith("custom_level_"))
            {
                id = id.Substring(13);
            }

			BeatSaverBrowserManager.songDetails.songs.FindByHash(id, out var x);
            return x;
        }

        public static bool Ranked(PreviewBeatmapLevel? level) => GetSong(level).rankedStatus == RankedStatus.Ranked;
        public static float AvgStars(PreviewBeatmapLevel? level) => GetSong(level).difficulties.Where(x => x.ranked == true).Average(x => x.stars);
        public static double Uncertainty(PreviewBeatmapLevel? level)
        {
            var x = GetSong(level);
            var totalVotes = (double)(x.upvotes + x.downvotes);

            var uncertainty = Math.Pow(2.0f, -Math.Log(totalVotes / 2 + 1, 3.0));

            var weightedRange = 25.0f;

            var weighting = 2;

            if ((totalVotes + weighting) < weightedRange)
            {
                uncertainty += (1 - uncertainty) * (1 - (totalVotes + weighting) * (1 / weightedRange));
            }

            return totalVotes < 1 ? 1 : (uncertainty * totalVotes / (1 - uncertainty));
		}

        //Takes into account Uncertainty
        public static double Rating(PreviewBeatmapLevel? level)
        {
            var x = GetSong(level);
            double u = Uncertainty(level);
            double total = x.upvotes + x.downvotes + u;
            return x.upvotes / total;
        }

        public static bool DynamicOwns(PreviewBeatmapLevel level)
        {
            return DynamicOwns(level.LevelId);
        }
        public static bool DynamicOwns(string LevelId)
        {
			string newLevelID = LevelId.StartsWith("custom_level_") ? LevelId : "custom_level_" + LevelId;
			newLevelID = new string(newLevelID.Take(53).ToArray());
			//In case of ost this if statement is needed
			if (LevelId.StartsWith("custom_level_"))
            {
                bool contains = allSongs.ContainsKey(newLevelID);
				if (contains)
				{
					if (allSongs[newLevelID].Owned)
						return true;
					else
					{
						if (!string.IsNullOrWhiteSpace(allSongs[newLevelID].OwnedJustificaton))
						{
							return false;
						}
						else
							return true;
					}
				}
				else
				{
					return false;
				}
            }
            else
			{
				//Beatsaver
				if (!allSongs.Keys.Contains(LevelId))
				{
					return false;
				}
                //ost or dlc
                return allSongs[LevelId].Owned;
            }
        }
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
            level.chars = new Characteristic[2] { new Characteristic("Standard", new string[2] {"Example 1", "Example 2" }), new Characteristic("NoArrows", new string[2] { "Example 3", "Example 4" }) };
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
            level.chars = new Characteristic[1] { new Characteristic("OneSaber", new string[2] { "Example 5", "Example 6" }) };
            level.coverPath = "D:\\SteamLibrary\\steamapps\\common\\Beat Saber\\Beat Saber_Data\\CustomLevels\\7457 (Night Raid with a Dragon - DE125 & Skeelie)\\cover.jpg";
            image = Image.FromFile(level.coverPath);
            bitmap = Server.FixedSize(image, 128);
            level.cover = Server.ToByteArray(bitmap, ImageFormat.Jpeg);
            GlobalData.covers.Add(level.LevelId, Convert.ToBase64String(level.cover));
            level.BPM = 1;
            s.Levels[1] = level;
            allSongs.Add(level.LevelId, level);

            songLists.Add(s);*/

            displaySongLists = songLists;

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
