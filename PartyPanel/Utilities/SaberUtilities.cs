using PartyPanel.Utilities;
using PartyPanelShared;
using PartyPanelShared.Models;
using SongCore;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = PartyPanelShared.Logger;

namespace PartyPanel
{
    class SaberUtilities    
    {
        private static CancellationTokenSource getLevelCancellationTokenSource;
        private static CancellationTokenSource getStatusCancellationTokenSource;
        private static SoloFreePlayFlowCoordinator flow;
        public static PracticeSettings ConvertPractice(PartyPanelShared.Models.PracticeSettings practiceSettings)
        {
            Logger.Debug(practiceSettings.songSpeed.ToString());
            PracticeSettings newSettings = new PracticeSettings(0f, practiceSettings.songSpeed/10000);
            return newSettings;
        }
        public static GameplayModifiers ConvertModifiers(PartyPanelShared.Models.GameplayModifiers mods, GameplayModifiers copy)
        {
            GameplayModifiers newMods = copy;
            foreach(FieldInfo fi in typeof(GameplayModifiers).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                {
                    object val2 = mods.GetField(fi.Name.Substring(fi.Name.IndexOf("_") + 1));
                    if (fi.Name.Contains("Type") || fi.Name.Contains("speed") || fi.Name == "_fastNotes") continue;
                    Logger.Debug(fi.Name);
                    fi.SetValue(newMods, val2);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return newMods;
        }
        public static async void PlaySong(IPreviewBeatmapLevel level, BeatmapCharacteristicSO characteristic, BeatmapDifficulty difficulty, PlaySong packet)
        {
            flow = (SoloFreePlayFlowCoordinator)Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First().GetField("_soloFreePlayFlowCoordinator");
            Action<IBeatmapLevel> SongLoaded = (loadedLevel) =>
            {
                MenuTransitionsHelper _menuSceneSetupData = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().First();
                IDifficultyBeatmap diffbeatmap = loadedLevel.beatmapLevelData.GetDifficultyBeatmap(characteristic, difficulty);
                GameplaySetupViewController gameplaySetupViewController = (GameplaySetupViewController)typeof(SinglePlayerLevelSelectionFlowCoordinator).GetField("_gameplaySetupViewController", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(flow);
                OverrideEnvironmentSettings environmentSettings = gameplaySetupViewController.environmentOverrideSettings;
                ColorScheme scheme = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme();
                PlayerSpecificSettings settings = gameplaySetupViewController.playerSettings;
                //TODO: re add modifier customizability
                
                GameplayModifiers modifiers = gameplaySetupViewController.gameplayModifiers;
                _menuSceneSetupData.StartStandardLevel(
                    "Solo",
                    diffbeatmap,
                    diffbeatmap.level,
                    environmentSettings,
                    scheme,
                    modifiers,
                    settings,
                    null,
                    "Menu",
                    false,
                    null,
                    new Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>((StandardLevelScenesTransitionSetupDataSO q, LevelCompletionResults r) => { })
                );
            };
            if (flow == null || flow.gameObject == null || !flow.gameObject.activeInHierarchy)
            {
                Button button = Resources.FindObjectsOfTypeAll<Button>().Where(x => x != null && x.name == "SoloButton").First();
                button.onClick.Invoke();
            }
            if ((level is PreviewBeatmapLevelSO /* && await HasDLCLevel(level.levelID)*/) || level is CustomPreviewBeatmapLevel)
            {
                Logger.Debug("Loading DLC/Custom level...");
                var result = await GetLevelFromPreview(level);
                if ( !(result?.isError == true))
                {
                    SongLoaded(result?.beatmapLevel);
                    return;
                }
                Logger.Debug("You Suck Idiot");
            }
            else if (level is BeatmapLevelSO)
            {
                Logger.Debug("Reading OST data without songloader...");
                SongLoaded(level as IBeatmapLevel);
            }
            else
            {
                Logger.Debug($"Skipping unowned DLC ({level.songName})");
            }
        }

        public static void ReturnToMenu()
        {
            if (SceneManager.GetActiveScene().name.Contains("Menu")) return;
            Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault().PopScenes(0.35f);
        }

        //Returns the closest difficulty to the one provided, preferring lower difficulties first if any exist
        public static IDifficultyBeatmap GetClosestDifficultyPreferLower(IBeatmapLevel level, BeatmapDifficulty difficulty, BeatmapCharacteristicSO characteristic = null)
        {
            //First, look at the characteristic parameter. If there's something useful in there, we try to use it, but fall back to Standard

            var desiredCharacteristic = level.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == (characteristic?.serializedName ?? "Standard")) ?? level.beatmapLevelData.difficultyBeatmapSets.First();

            IDifficultyBeatmap[] availableMaps =
                level
                .beatmapLevelData
                .difficultyBeatmapSets
                .FirstOrDefault(x => x.beatmapCharacteristic.serializedName == desiredCharacteristic.beatmapCharacteristic.serializedName)
                .difficultyBeatmaps
                .OrderBy(x => x.difficulty)
                .ToArray();

            IDifficultyBeatmap ret = availableMaps.FirstOrDefault(x => x.difficulty == difficulty);

            if (ret is CustomDifficultyBeatmap)
            {
                var extras = Collections.RetrieveExtraSongData(ret.level.levelID);
                var requirements = extras?._difficulties.First(x => x._difficulty == ret.difficulty).additionalDifficultyData._requirements;
                if (
                    (requirements?.Count() > 0) &&
                    (!requirements?.ToList().All(x => Collections.capabilities.Contains(x)) ?? false)
                ) ret = null;
            }

            if (ret == null)
            {
                ret = GetLowerDifficulty(availableMaps, difficulty, desiredCharacteristic.beatmapCharacteristic);
            }
            if (ret == null)
            {
                ret = GetHigherDifficulty(availableMaps, difficulty, desiredCharacteristic.beatmapCharacteristic);
            }

            return ret;
        }

        //Returns the next-lowest difficulty to the one provided
        private static IDifficultyBeatmap GetLowerDifficulty(IDifficultyBeatmap[] availableMaps, BeatmapDifficulty difficulty, BeatmapCharacteristicSO characteristic)
        {
            var ret = availableMaps.TakeWhile(x => x.difficulty < difficulty).LastOrDefault();
            if (ret is CustomDifficultyBeatmap)
            {
                var extras = Collections.RetrieveExtraSongData(ret.level.levelID);
                var requirements = extras?._difficulties.First(x => x._difficulty == ret.difficulty).additionalDifficultyData._requirements;
                if (
                    (requirements?.Count() > 0) &&
                    (!requirements?.ToList().All(x => Collections.capabilities.Contains(x)) ?? false)
                ) ret = null;
            }
            return ret;
        }

        //Returns the next-highest difficulty to the one provided
        private static IDifficultyBeatmap GetHigherDifficulty(IDifficultyBeatmap[] availableMaps, BeatmapDifficulty difficulty, BeatmapCharacteristicSO characteristic)
        {
            var ret = availableMaps.SkipWhile(x => x.difficulty < difficulty).FirstOrDefault();
            if (ret is CustomDifficultyBeatmap)
            {
                var extras = Collections.RetrieveExtraSongData(ret.level.levelID);
                var requirements = extras?._difficulties.First(x => x._difficulty == ret.difficulty).additionalDifficultyData._requirements;
                if (
                    (requirements?.Count() > 0) &&
                    (!requirements?.ToList().All(x => Collections.capabilities.Contains(x)) ?? false)
                ) ret = null;
            }
            return ret;
        }

        public static async Task<bool> HasDLCLevel(string levelId, AdditionalContentModel additionalContentModel = null)
        {
            additionalContentModel = additionalContentModel ?? Resources.FindObjectsOfTypeAll<AdditionalContentModel>().FirstOrDefault();

            if (additionalContentModel != null)
            {
                getStatusCancellationTokenSource?.Cancel();
                getStatusCancellationTokenSource = new CancellationTokenSource();

                var token = getStatusCancellationTokenSource.Token;
                return await additionalContentModel.GetLevelEntitlementStatusAsync(levelId, token) == AdditionalContentModel.EntitlementStatus.Owned;
            }

            return false;
        }

        public static async Task<BeatmapLevelsModel.GetBeatmapLevelResult?> GetLevelFromPreview(IPreviewBeatmapLevel level, BeatmapLevelsModel beatmapLevelsModel = null)
        {
            beatmapLevelsModel = beatmapLevelsModel ?? Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault();

            if (beatmapLevelsModel != null)
            {
                getLevelCancellationTokenSource?.Cancel();
                getLevelCancellationTokenSource = new CancellationTokenSource();

                var token = getLevelCancellationTokenSource.Token;

                BeatmapLevelsModel.GetBeatmapLevelResult? result = null;
                try
                {
                    result = await beatmapLevelsModel.GetBeatmapLevelAsync(level.levelID, token);
                }
                catch (OperationCanceledException) { }
                if (result?.isError == true || result?.beatmapLevel == null) return null; //Null out entirely in case of error
                return result;
            }
            return null;
        }
    }
}
