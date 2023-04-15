using PartyPanelShared.Models;
using System.Globalization;
using System.Text;
using SongDetailsCache;
using System.Drawing.Imaging;
using PartyPanelShared;
using SongDetailsCache.Structs;
using System.Linq;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Reflection.Emit;
using Microsoft.Extensions.ObjectPool;

namespace PartyPanelUI
{
    public struct BSaverSong
    {
        public PreviewBeatmapLevel level;
        public Song BeatsaverSong;
    }
    public static class BeatSaverBrowserManager
    {
        public static List<PreviewBeatmapLevel> convertedBeatSaverLevels = new();
        public static SongDetails songDetails;

        public static async Task Initialize()
        {
            var bSaverSongs = new List<BSaverSong>();
            songDetails = await SongDetails.Init();
            foreach(var level in songDetails.songs) 
            { 
                var previewBeatmapLevel = new PreviewBeatmapLevel();
                previewBeatmapLevel.Name = level.songName;
                previewBeatmapLevel.LevelId = level.hash;
                previewBeatmapLevel.Author = level.songAuthorName;
                previewBeatmapLevel.Mapper = level.levelAuthorName;
                previewBeatmapLevel.Owned = GlobalData.allSongs.ContainsKey("custom_level_" + previewBeatmapLevel.LevelId);
                previewBeatmapLevel.OwnedJustificaton = "Not Downloaded";
                previewBeatmapLevel.BPM = level.bpm;
                previewBeatmapLevel.Duration = level.songDuration.ToString(@"m\:ss");
                bSaverSongs.Add(new BSaverSong { level = previewBeatmapLevel, BeatsaverSong = level });
                GlobalData.covers.Add(previewBeatmapLevel.LevelId, level.coverURL);
            }

            Logger.Info("Done Loading");
            convertedBeatSaverLevels = bSaverSongs.Select(x => x.level).ToList();
        }
    }
}