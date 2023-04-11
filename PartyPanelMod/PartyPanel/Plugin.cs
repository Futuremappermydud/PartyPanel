using IPA;
using SongCore;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IPA.Utilities;
using System.Threading.Tasks;
using HarmonyLib;
using PartyPanel.HarmonyPatches;
using System.Reflection;

namespace PartyPanel
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public string Name => "PartyPanel";
        public string Version => "0.0.1";

        private BeatmapLevelsModel beatmapLevelsModel;

        public static List<IPreviewBeatmapLevel> masterLevelList;
        public static TaskCompletionSource<bool> IsSongsLoading = new TaskCompletionSource<bool>();
        public static IPA.Logging.Logger logger;

        private static Harmony harmony;

        public static Client client;

        [Init]
        public void Init(IPA.Logging.Logger _logger)
        { 
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            logger = _logger;
        }

        [OnStart]
        public void OnApplicationStart()
        {
            client = new Client();
            client.Start();

            Loader.SongsLoadedEvent += (Loader _, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> __) =>
            {
                if (beatmapLevelsModel == null) beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().First();

                masterLevelList = new List<IPreviewBeatmapLevel>();
                var values = beatmapLevelsModel.GetField<Dictionary<string, IPreviewBeatmapLevel>, BeatmapLevelsModel>("_loadedPreviewBeatmapLevels").Values.ToArray();
                
                masterLevelList.AddRange(values);

                IsSongsLoading.SetResult(true);
            };
        }
    }
}
