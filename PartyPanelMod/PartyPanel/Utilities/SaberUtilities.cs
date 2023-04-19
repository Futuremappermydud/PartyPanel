using HMUI;
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
        private static CancellationTokenSource? getLevelCancellationTokenSource;
        private static CancellationTokenSource? getStatusCancellationTokenSource;
        private static SoloFreePlayFlowCoordinator? flow;
        public static PracticeSettings ConvertPractice(PartyPanelShared.Models.PracticeSettings practiceSettings)
        {
            Logger.Debug(practiceSettings.songSpeed.ToString());
            PracticeSettings newSettings = new PracticeSettings(0f, practiceSettings.songSpeed/10000);
            return newSettings;
        }
        public static GameplayModifiers ConvertModifiers(PartyPanelShared.Models.GameplayModifiers mods)
        {
            GameplayModifiers newMods = new GameplayModifiers((GameplayModifiers.EnergyType)(int)mods.energyType, mods.noFailOn0Energy, mods.instaFail, mods.failOnSaberClash, (GameplayModifiers.EnabledObstacleType)(int)mods.enabledObstacleType, mods.noBombs, false, mods.strictAngles, mods.disappearingArrows, (GameplayModifiers.SongSpeed)(int)mods.songSpeed, mods.noBombs, mods.ghostNotes, mods.proMode, mods.zenMode, mods.smallCubes);
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

                GameplayModifiers modifiers = ConvertModifiers(packet.gameplayModifiers);
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
                    false,
                    null,
                    new Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>((StandardLevelScenesTransitionSetupDataSO q, LevelCompletionResults r) => { }),
                    new Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>((LevelScenesTransitionSetupDataSO q, LevelCompletionResults r) => { })
                );
            };
            HMMainThreadDispatcher.instance.Enqueue(() =>
            {
                NoTransitionsButton button = Resources.FindObjectsOfTypeAll<NoTransitionsButton>().Where(x => x != null && x.gameObject.name == "SoloButton").FirstOrDefault();
                button.onClick.Invoke();
            });
            if (true)
            {
                var result = await GetLevelFromPreview(level);
                if ( !(result?.isError == true))
                {
                    SongLoaded(result?.beatmapLevel);
                    return;
                }
            }
        }

        public static void ReturnToMenu()
        {
            HMMainThreadDispatcher.instance.Enqueue(() =>
            {
                if (!SceneManager.GetActiveScene().name.Contains("Game")) return;
                Resources.FindObjectsOfTypeAll<StandardLevelReturnToMenuController>()?.FirstOrDefault()?.ReturnToMenu();
            });
        }

        public static async Task<bool> HasDLCLevel(string levelId, AdditionalContentModel additionalContentModel = null)
        {
            if(!levelId.StartsWith("custom_level_"))
            {
                Logger.Info(levelId);
            }
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
                if (result?.isError == true || result?.beatmapLevel == null)
                {
                    Logger.Error("Failed to load Level");
                    return null; //Null out entirely in case of error
                }
                return result;
            }
            return null;
        }
    }
}
