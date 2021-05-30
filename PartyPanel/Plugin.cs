using IPA;
using SongCore;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;  
using Logger = PartyPanelShared.Logger;
using System.Reflection;
using IPA.Utilities;

/*
 * Created by Moon on 11/12/2018
 * 
 * This plugin is designed to provide a user interface to launch songs
 * without being in the game
 */

namespace PartyPanel
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public string Name => "PartyPanel";
        public string Version => "0.0.1";

        private BeatmapLevelsModel beatmapLevelsModel;

        public static List<IPreviewBeatmapLevel> masterLevelList;

        private Client client;
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
            };
        }
    }
}
