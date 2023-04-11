using HarmonyLib;
using PartyPanel.Network;
using PartyPanelShared;
using PartyPanelShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PartyPanel.HarmonyPatches
{
    [HarmonyLib.Harmony]
    internal static class StandardLevelScenesTransitionSetupDataSOPatch
    {
        private static Timer heartbeatTimer = new Timer();

        public static void HeartbeatTimer_Elapsed(object _, ElapsedEventArgs __)
        {
            var dpData = DataPuller.Data.LiveData.Instance;
            Plugin.client.client.Send(new Packet(new NowPlayingUpdate(dpData.Score, dpData.Accuracy, dpData.TimeElapsed, DataPuller.Data.MapData.Instance.Duration)).ToBytes());
        }
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "Init")]
        [HarmonyPrefix]
        public static void Prefix(string gameMode, IDifficultyBeatmap difficultyBeatmap, IPreviewBeatmapLevel previewBeatmapLevel, OverrideEnvironmentSettings overrideEnvironmentSettings, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, string backButtonText, bool useTestNoteCutSoundEffects = false, bool startPaused = false, BeatmapDataCache beatmapDataCache = null)
        {
            heartbeatTimer.Interval = 1000;
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            heartbeatTimer.Start();
            Plugin.client.client.Send(new Packet(new NowPlaying(previewBeatmapLevel.levelID, false)).ToBytes());
        }

        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "Finish")]
        [HarmonyPrefix]
        public static void Prefix(LevelCompletionResults levelCompletionResults)
        {
            Plugin.client.client.Send(new Packet(new NowPlaying(null, true)).ToBytes());
            heartbeatTimer.Stop();
        }
    }
}
