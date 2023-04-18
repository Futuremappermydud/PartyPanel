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
using BeatSaverSharp;
using System;
using System.Runtime.InteropServices;
using static IPA.Logging.Logger;
using PartyPanelShared.Models;
using PartyPanelShared;

namespace PartyPanel
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public string Name => "PartyPanel";
        public string Version => "0.1.1";

        private BeatmapLevelsModel beatmapLevelsModel;

        public static List<IPreviewBeatmapLevel> masterLevelList;
        public static TaskCompletionSource<bool> IsSongsLoading = new TaskCompletionSource<bool>();
        public static IPA.Logging.Logger logger;

        internal static Harmony harmony;

        public static Client client;

        public static BeatSaver Client { get; } = new BeatSaver(new BeatSaverOptions("PartyPanel", new Version(0, 1, 1)));

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

            Loader.SongsLoadedEvent += (Loader _, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> x) =>
            {
                if (beatmapLevelsModel == null) beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().First();
                var values = beatmapLevelsModel.GetField<Dictionary<string, IPreviewBeatmapLevel>, BeatmapLevelsModel>("_loadedPreviewBeatmapLevels").Values.ToArray();
                if (masterLevelList != null)
                {
                    PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
                    string[] names = masterLevelList.Select((x) => x.songName).ToArray();
                    List<CustomPreviewBeatmapLevel> newLevels = x.Where((l) => { return !names.Contains(l.Value.songName); }).Select((l) => l.Value).ToList();
                    //logger.Info(newLevels.Select(x => x.songName).Aggregate((x, y)=>x + ", " + y));
                    List<PreviewBeatmapLevel> convertedLevels = new List<PreviewBeatmapLevel>();

                    List<Task<PreviewBeatmapLevel>> tasks = new List<Task<PreviewBeatmapLevel>>();

                    foreach (var level in newLevels)
                    {
                        tasks.Add(PartyPanel.Client.ConvertToPacketType(level, playerData));
                    }
                    Task.Run(async () =>
                    {
                        convertedLevels.AddRange(await Task.WhenAll(tasks));

                        client.client.Send(new Packet(new SongList(convertedLevels.ToArray())).ToBytes());
                    });

                }

                masterLevelList = new List<IPreviewBeatmapLevel>();
                
                masterLevelList.AddRange(values);

                IsSongsLoading.SetResult(true);
            };
        }

        [OnDisable] public void OnDisable()
        {
            harmony.UnpatchSelf();

        }
    }
}
