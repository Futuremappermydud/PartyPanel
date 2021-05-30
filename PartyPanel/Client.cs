using IPA.Utilities;
using IPA.Utilities.Async;
using Newtonsoft.Json;
using PartyPanelShared;
using PartyPanelShared.Models;
using Polyglot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using Logger = PartyPanelShared.Logger;
using Timer = System.Timers.Timer;

namespace PartyPanel
{
    class ThreadTester : MonoBehaviour
    {
        Thread mainThread;
        void Start()
        {
            mainThread = Thread.CurrentThread;
        }
        public bool TestThread(Thread thread)
        {
            return mainThread.Equals(thread);
        }
    }

    // Is there a class or better way to replace this?
    internal class AtomicBool
    {
        public volatile bool b;

        public AtomicBool(bool b)
        {
            this.b = b;
        }
    }

    class Client
    {
        private Network.Client client;
        private Timer heartbeatTimer = new Timer();
        private ThreadTester threadTester;
        // The amount of songs loaded until a packet is sent.
        // TODO: make configurable?
        private const int QueueThreshold = 30;

        public void Start()
        {
            heartbeatTimer.Interval = 10000;
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            heartbeatTimer.Start();
            threadTester = new GameObject("ThreadTester").AddComponent<ThreadTester>();
        }

        private void HeartbeatTimer_Elapsed(object _, ElapsedEventArgs __)
        {
            try
            {
                var command = new Command();
                command.commandType = Command.CommandType.Heartbeat;
                client.Send(new Packet(command).ToBytes());
            }
            catch (Exception e)
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
                Logger.Debug("Start");
                //Send the server the master list if we can
                if (Plugin.masterLevelList != null)
                {
                    Logger.Debug("X");
                    HMMainThreadDispatcher.instance.Enqueue(new Action(async () =>
                    {
                        await SendAllSongList(Plugin.masterLevelList);

                    }));
                }
            
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

        // Hash: Level
        private Dictionary<string, PreviewBeatmapLevel> subPacketListCached = new Dictionary<string, PreviewBeatmapLevel>();


        public async Task SendAllSongList(List<IPreviewBeatmapLevel> levels)
        {
            // Send cache
            if (levels.TrueForAll(l => subPacketListCached.ContainsKey(l.levelID)))
            {
                await Task.Run(async () =>
                {
                    // just send all songs since they're cached
                    await SendSongList(subPacketListCached.Values);
                });
                return;
            }

            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault().playerData;

            Dictionary<string, PreviewBeatmapLevel> subpacketList = new Dictionary<string, PreviewBeatmapLevel>();
            var beatmapLevelsQueue = new Queue<PreviewBeatmapLevel>();

            var running = new AtomicBool(true);
            var queueTask = Task.Run(async () =>
            {
                while (running.b)
                {
                    if (beatmapLevelsQueue.Count < QueueThreshold) continue;

                    // Copy queue
                    var queueClone = new Queue<PreviewBeatmapLevel>(beatmapLevelsQueue);
                    // then clear
                    beatmapLevelsQueue.Clear();
                    await SendSongList(queueClone);
                }

                // Make sure that the queue is finally finished
                if (beatmapLevelsQueue.Count != 0)
                {
                    await SendSongList(beatmapLevelsQueue);
                    beatmapLevelsQueue.Clear();
                }
            });

            foreach (var level in levels)
            {
                var levelConverted = await ConvertToPacketType(level, playerData);
                beatmapLevelsQueue.Enqueue(levelConverted);
                subpacketList[levelConverted.LevelId] = levelConverted;
            }


            subPacketListCached = subpacketList;
            running.b = false;
            await queueTask;
        }

        public async Task SendSongList(IEnumerable<PreviewBeatmapLevel> songs)
        {
            Logger.Debug("F");

            //Check if we are connected
            if (client != null && client.Connected)
            {
                var songSendingList = new SongList {Levels = songs.ToArray()};
                client.Send(new Packet(songSendingList).ToBytes());
            }
            else
            {
                var unused = SendSongList(songs).ConfigureAwait(false);
            }
        }

        public Texture2D GetReadableTexForUnreadableTex(Texture2D tex)
        {
            Logger.Debug("Is Main Thread: " + threadTester.TestThread(Thread.CurrentThread).ToString());
            tex.filterMode = FilterMode.Point;

            //Get Temporary RenderTexture
            RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height);
            rt.filterMode = FilterMode.Point;

            //Set RenderTexture as Active
            RenderTexture.active = rt;

            //Blit Texture to RenderTexture
            Graphics.Blit(tex, rt);

            //Make New Texture
            Texture2D img2 = new Texture2D(tex.width, tex.height);

            //Read Pixels from RenderTexture
            img2.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

            //Apply Pixels
            img2.Apply();
            
            //Reset Active RenderTexure
            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = null;

            return img2;
        }
        public Texture2D ClipTexture(Texture2D tex, Rect rect)
        {
            var pixData = tex.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            var newTex = new Texture2D((int)rect.width, (int)rect.height);
            newTex.SetPixels(pixData);
            newTex.Apply();
            return newTex;
        }
        public void LoadSong(PreviewBeatmapLevel packetLevel, IPreviewBeatmapLevel x)
        {
            PreviewBeatmapLevel level = packetLevel;
            LoadedSong loadedSong = new LoadedSong();
            HMMainThreadDispatcher.instance.Enqueue(() => 
            {
                Logger.Debug("X");
                level.chars = x.previewDifficultyBeatmapSets.Select((PreviewDifficultyBeatmapSet set) => 
                    { 
                        Characteristic Char = new Characteristic(); 
                        Char.Name = set.beatmapCharacteristic.serializedName; 
                        Char.diffs = set.beatmapDifficulties.Select((BeatmapDifficulty diff) => { return BeatmapDifficultyMethods.Name(diff); }).ToArray(); 
                        return Char; 
                    }).ToArray();
                loadedSong.level = level;
                client.Send(new Packet(loadedSong).ToBytes()); 
            });
        }
        public async Task<PreviewBeatmapLevel> ConvertToPacketType(IPreviewBeatmapLevel x, PlayerData playerData)   
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
                //level.chars = x.previewDifficultyBeatmapSets.Select((PreviewDifficultyBeatmapSet set) => { Characteristic Char = new Characteristic(); Logger.Debug(set.beatmapCharacteristic.icon.texture.isReadable.ToString()); Char.Name = set.beatmapCharacteristic.serializedName; Char.Icon = GetReadableTexForUnreadableTex(set.beatmapCharacteristic.icon.texture).EncodeToPNG(); Char.diffs = set.beatmapDifficulties.Select((BeatmapDifficulty diff)=> { return BeatmapDifficultyMethods.ShortName(diff); }).ToArray(); return Char; }).ToArray();
                //Get Level Sprite
                Sprite sprite = await x.GetCoverImageAsync(CancellationToken.None);

                //Get Texture
                Texture2D texture = sprite.texture;
                if (!texture.isReadable)
                {
                    texture = GetReadableTexForUnreadableTex(texture);
                }

                //Encode to PNG and set packet cover
                level.cover = texture.EncodeToPNG();
                Logger.Info("Got Cover for" + x.songName);

                PreviewSong songPacket = new PreviewSong();
                songPacket.level = level;
                client.Send(new Packet(songPacket).ToBytes());
            }
            catch(Exception e)
            {
                throw e;
            }
            return level;
        }
        public static ServerMetadata metadata = new ServerMetadata();
        private void Client_PacketRecieved(Packet packet)
        {
            if (packet.Type == PacketType.PlaySong)
            {
                PlaySong playSong = packet.SpecificPacket as PlaySong;

                var desiredLevel = Plugin.masterLevelList.First(x => x.levelID == playSong.levelId);
                var desiredCharacteristic = desiredLevel.previewDifficultyBeatmapSets.GetBeatmapCharacteristics().First(x => x.serializedName == playSong.characteristic.Name);
                BeatmapDifficulty desiredDifficulty;
                playSong.difficulty.BeatmapDifficultyFromSerializedName(out desiredDifficulty);


                SaberUtilities.PlaySong(desiredLevel, desiredCharacteristic, desiredDifficulty, playSong);
            }
            else if (packet.Type == PacketType.LoadSong)
            {
                LoadSong loadSong = packet.SpecificPacket as LoadSong;

                LoadedSong loaded = new LoadedSong();
                loaded.level = subPacketListCached.First(x => x.Value.LevelId == loadSong.levelId).Value;
                LoadSong(loaded.level, Plugin.masterLevelList.First(x => x.levelID == loadSong.levelId));
            }
            else if (packet.Type == PacketType.Command)
            {
                Command command = packet.SpecificPacket as Command;
                if (command.commandType == Command.CommandType.ReturnToMenu)
                {
                    SaberUtilities.ReturnToMenu();
                }
            }
            else if (packet.Type == PacketType.ServerMetadata)
            {
                metadata = packet.SpecificPacket as ServerMetadata;
            }
        }

        private async void LoadSong(string levelId, Action<IBeatmapLevel> loadedCallback)
        {
            IPreviewBeatmapLevel level = Plugin.masterLevelList.Where(x => x.levelID == levelId).First();

            //Load IBeatmapLevel
            if (level is PreviewBeatmapLevelSO || level is CustomPreviewBeatmapLevel)
            {
                if (level is PreviewBeatmapLevelSO)
                {
                    if (!await SaberUtilities.HasDLCLevel(level.levelID)) return; //In the case of unowned DLC, just bail out and do nothing
                }

                var map = ((CustomPreviewBeatmapLevel)level).standardLevelInfoSaveData.difficultyBeatmapSets.First().difficultyBeatmaps.First();

                var result = await SaberUtilities.GetLevelFromPreview(level);
                if (result != null && !(result?.isError == true))
                {
                    loadedCallback(result?.beatmapLevel);
                }
            }
            else if (level is BeatmapLevelSO)
            {
                loadedCallback(level as IBeatmapLevel);
            }
        }
    }
}
