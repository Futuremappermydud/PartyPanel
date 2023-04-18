using IPA.Utilities;
using IPA.Utilities.Async;
using Newtonsoft.Json;
using PartyPanelShared;
using PartyPanelShared.Models;
using Polyglot;
using SongCore.Utilities;
using SongCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using static IPA.Logging.Logger;
using Logger = PartyPanelShared.Logger;
using Timer = System.Timers.Timer;
using static AlphabetScrollInfo;
using static BeatmapLevelSO.GetBeatmapLevelDataResult;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using UnityEngine.UIElements;
using System.Reflection.Metadata.Ecma335;

namespace PartyPanel
{
    public class Client
    {
        public Network.Client client;
        private Timer heartbeatTimer = new Timer();

        public void Start()
        {
            heartbeatTimer.Interval = 5000;
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            heartbeatTimer.Start();
        }

        private void HeartbeatTimer_Elapsed(object _, ElapsedEventArgs __)
        {
            try
            {
                var command = new Command();
                command.commandType = Command.CommandType.Heartbeat;
                client.Send(new Packet(command).ToBytes());
            }
            catch
            {
                Logger.Debug("HEARTBEAT FAILED");

                ConnectToServer();
            }
        }

        private void ConnectToServer()
        {
            
            try
            {
                client = new Network.Client(10155);
                client.PacketRecieved += Client_PacketRecieved;
                client.ServerDisconnected += Client_ServerDisconnected;
                client.Start();
                HMMainThreadDispatcher.instance.Enqueue(new Action(async () =>
                {
                    await Plugin.IsSongsLoading.Task;
                    await GetSongList(Plugin.masterLevelList);
                }));
            }
            catch (Exception e)
            {
                Logger.Debug(e.ToString());
            }
        }

        private void Client_ServerDisconnected()
        {
            Logger.Debug("Server disconnected!");
        }
        #nullable enable
        public class OverrideLabels
        {
            internal string? EasyOverride = null;
            internal string? NormalOverride = null;
            internal string? HardOverride = null;
            internal string? ExpertOverride = null;
            internal string? ExpertPlusOverride = null;
        }
#nullable disable
        public async Task GetSongList(List<IPreviewBeatmapLevel> levels)
        {
            List<PreviewBeatmapLevel> subpacketList = new List<PreviewBeatmapLevel>();
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;
            List<Task<PreviewBeatmapLevel>> tasks = new List<Task<PreviewBeatmapLevel>>();

            foreach (var level in levels)
            {
                tasks.Add(ConvertToPacketType(level, playerData));
            }

            subpacketList.AddRange(await Task.WhenAll(tasks));

            PreviewBeatmapLevel[] buffer;
            List<SongList> songLists = new List<SongList>();

            PreviewBeatmapLevel[] subpacketListArr = subpacketList.ToArray();

            for (int i = 0; i < subpacketListArr.Length; i += 20)
            {
                try {
                    buffer = new PreviewBeatmapLevel[20];
                    Array.Copy(subpacketListArr, i, buffer, 0, 20);
                    songLists.Add(new SongList(buffer));
                }
                catch
                {
                    continue;
                }
            }
            Logger.Info(subpacketListArr.Length.ToString());
            Logger.Info(songLists.Count.ToString());
            await Task.Run(() =>
            {
                client.Send(new Packet(new AllSongs(songLists)).ToBytes());
            });
        }
        private static Texture2D GetFromUnreadable(Texture2D tex, Rect rect)
        {
            var tmp = RenderTexture.GetTemporary(
                                    tex.width,
                                    tex.height,
                                    0,
                                    RenderTextureFormat.Default,
                                    RenderTextureReadWrite.Linear);

            UnityEngine.Graphics.Blit(tex, tmp);
            var previous = RenderTexture.active;
            RenderTexture.active = tmp;
            var myTexture2D = new Texture2D((int)rect.width, (int)rect.height);
            myTexture2D.ReadPixels(rect, 0, 0);
            myTexture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            return myTexture2D;
        }
        private static Rect InvertAtlas(Rect i)
        {
            Rect o = new Rect(i.x, i.y, i.width, i.height);
            o.y = 2048 - o.y - 160;
            return o;
        }

        private static string Name(OverrideLabels labels, BeatmapDifficulty difficulty)
        {
            string test = (difficulty switch
            {
                BeatmapDifficulty.Easy when labels?.EasyOverride != null => labels?.EasyOverride,
                BeatmapDifficulty.Normal when labels?.NormalOverride != null => labels?.NormalOverride,
                BeatmapDifficulty.Hard when labels?.HardOverride != null => labels?.HardOverride,
                BeatmapDifficulty.Expert when labels?.ExpertOverride != null => labels?.ExpertOverride,
                BeatmapDifficulty.ExpertPlus when labels?.ExpertPlusOverride != null => labels?.ExpertPlusOverride,
                _ => BeatmapDifficultyMethods.Name(difficulty),
            });
            return test;
        }
        public static async Task<PreviewBeatmapLevel> ConvertToPacketType(IPreviewBeatmapLevel x, PlayerData playerData)   
        {
            //Make packet level
            var level = new PreviewBeatmapLevel();
            try
            {
                //Set Parameters;
                level.LevelId = x.levelID;
                level.Name = x.songName;
                level.SubName = x.songSubName;
                level.Author = x.songAuthorName;
                level.Mapper = x.levelAuthorName;
                level.BPM = x.beatsPerMinute;
                level.Duration = TimeExtensions.MinSecDurationText(x.songDuration);
                level.Favorited = playerData.favoritesLevelIds.Contains(x.levelID);
                level.Owned = await SaberUtilities.HasDLCLevel(x.levelID);
                if(!level.Owned)
                {
                    level.OwnedJustificaton = "Unowned DLC Level";
                }
                if(x is CustomPreviewBeatmapLevel)
                {
                    var extras = Collections.RetrieveExtraSongData(new string(x.levelID.Skip(13).ToArray()));
                    var requirements = extras?._difficulties.SelectMany((x) => { return x.additionalDifficultyData._requirements; });
                    List<string> missingReqs = new List<string>();
                    if (
                        (requirements?.Count() > 0) &&
                        (!requirements?.ToList().All(x => { 
                            if(Collections.capabilities.Contains(x))
                            {
                                return true;
                            }
                            else
                            {
                                missingReqs.Add(x);
                                return false;
                            }
                        }) ?? false)
                    )
                    {
                        level.Owned = false;
                        level.OwnedJustificaton = "Missing " + missingReqs.Aggregate((x, x2) => { return x + x2; });
                    }
                }

                OverrideLabels labels = new OverrideLabels();
                var songData = Collections.RetrieveExtraSongData(new string(x.levelID.Skip(13).ToArray()));
                
                Dictionary<string, OverrideLabels> LevelLabels = new Dictionary<string, OverrideLabels>();
                LevelLabels.Clear();
                if (songData != null)
                {
                    foreach (SongCore.Data.ExtraSongData.DifficultyData diffLevel in songData._difficulties)
                    {

                        var difficulty = diffLevel._difficulty;
                        string characteristic = diffLevel._beatmapCharacteristicName;

                        if (!LevelLabels.ContainsKey(characteristic))
                        {
                            LevelLabels.Add(characteristic, new OverrideLabels());
                        }

                        var charLabels = LevelLabels[characteristic];
                        if (!string.IsNullOrWhiteSpace(diffLevel._difficultyLabel))
                        {

                            switch (difficulty)
                            {
                                case BeatmapDifficulty.Easy:
                                    charLabels.EasyOverride = diffLevel._difficultyLabel;
                                    break;
                                case BeatmapDifficulty.Normal:
                                    charLabels.NormalOverride = diffLevel._difficultyLabel;
                                    break;
                                case BeatmapDifficulty.Hard:
                                    charLabels.HardOverride = diffLevel._difficultyLabel;
                                    break;
                                case BeatmapDifficulty.Expert:
                                    charLabels.ExpertOverride = diffLevel._difficultyLabel;
                                    break;
                                case BeatmapDifficulty.ExpertPlus:
                                    charLabels.ExpertPlusOverride = diffLevel._difficultyLabel;
                                    break;
                            }
                        }
                    }
                }
                level.chars = x.previewDifficultyBeatmapSets.Select((PreviewDifficultyBeatmapSet set) => { Characteristic Char = new Characteristic(); Char.Name = set.beatmapCharacteristic.serializedName;    Char.diffs = set.beatmapDifficulties.Select((BeatmapDifficulty diff)=> { return Name(LevelLabels.ContainsKey(Char.Name) ? LevelLabels[Char.Name]: null, diff); }).ToArray(); return Char; }).ToArray();
                if (x.GetType().Name.Contains("BeatmapLevelSO"))
                {
                    Texture2D tex;
                    Sprite sprite = (await x.GetCoverImageAsync(System.Threading.CancellationToken.None));
                    try
                    {
                        tex = sprite.texture;
                    }
                    catch
                    {
                        tex = GetFromUnreadable((x as CustomPreviewBeatmapLevel)?.defaultCoverImage.texture, sprite.textureRect);
                    }
                    if (!(x is CustomPreviewBeatmapLevel) || tex == null || !tex.isReadable)
                    {
                        tex = GetFromUnreadable(tex, InvertAtlas(sprite.textureRect));
                    }
                    level.cover = tex.EncodeToJPG();
                }
                else
                {
                    if(x is CustomPreviewBeatmapLevel)
                    {
                        string path = Path.Combine(((CustomPreviewBeatmapLevel)x).customLevelPath, ((CustomPreviewBeatmapLevel)x).standardLevelInfoSaveData.coverImageFilename);
                        if (File.Exists(path))
                        {
                            level.coverPath = path;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error(e.ToString());
            }
            return level;
        }
        private void Client_PacketRecieved(Packet packet)
        {
            if (packet.Type == PacketType.PlaySong)
            {
                PlaySong playSong = packet.SpecificPacket as PlaySong;

                var desiredLevel = Plugin.masterLevelList.First(x => x.levelID == playSong.levelId);
                var desiredCharacteristic = desiredLevel.previewDifficultyBeatmapSets.Select(level => level.beatmapCharacteristic).First(x => x.serializedName == playSong.characteristic.Name);
                BeatmapDifficulty desiredDifficulty;
                playSong.difficulty.BeatmapDifficultyFromSerializedName(out desiredDifficulty);

                SaberUtilities.PlaySong(desiredLevel, desiredCharacteristic, desiredDifficulty, playSong);
            }
            else if (packet.Type == PacketType.Command)
            {
                Command command = packet.SpecificPacket as Command;
                if (command.commandType == Command.CommandType.ReturnToMenu)
                {
                    SaberUtilities.ReturnToMenu();
                }
            } 
            else if (packet.Type == PacketType.DownloadSong)
            {
                DownloadSong download = packet.SpecificPacket as DownloadSong;

                Task.Run(async () => { await BeatSaverDownloader.Misc.SongDownloader.Instance.DownloadSong(Plugin.Client.Beatmap(download.songKey).Result, CancellationToken.None); SongCore.Loader.Instance.RefreshSongs(); });
            }
        }
    }
}
