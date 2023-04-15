using PartyPanelShared;
using PartyPanelShared.Models;
using PartyPanelUI.Network;
using SongDetailsCache.Structs;
using System.Drawing;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;

namespace PartyPanelUI
{
    public class Server
    {
        public static Action<NowPlayingUpdate> OnUpdate;
        public static Action<NowPlaying> OnNowPlaying;
        public Network.Server server;
		public static bool isConnected = false;

        public Server()
        {
        }

        public void Start()
        {
            server = new Network.Server(10155);
            //panel.SetServer(server);
            server.PacketRecieved = new Action<NetworkPlayer, Packet>( Server_PacketRecieved);
            server.PlayerConnected += Server_PlayerConnected;
            server.PlayerDisconnected += Server_PlayerDisconnected;
            server.Start();
        }

        private void Server_PlayerDisconnected(NetworkPlayer obj)
        {
            Logger.Debug("Player Disconnected!");
			//panel.DisableSongList();
			isConnected = false;
		}

        private void Server_PlayerConnected(NetworkPlayer obj)
        {
            Logger.Debug("Player Connected!");
			isConnected = true;
		}
#pragma warning disable CA1416 // Validate platform compatibility
        public static Image FixedSize(Image image, int height)
        {
            double ratio = (double)height / image.Height;
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);
            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            image.Dispose();
            return newImage;
        }

        public static byte[] ToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
        public static void Server_PacketRecieved(NetworkPlayer player, Packet packet)
        {
            if (packet.Type == PacketType.PreviewSong)
            {
                PreviewSong? song = packet.SpecificPacket as PreviewSong;
                //Add preprocessing logic here i.e converting cover images
                if (song?.level != null)
                {
                    if (!String.IsNullOrEmpty(song.level?.coverPath))
                    {
                        if (!(song.level.cover.Length > 1))
                        {
                            var image = Image.FromFile(song.level.coverPath);
                            var bitmap = FixedSize(image, 128);
                            song.level.cover = ToByteArray(bitmap, ImageFormat.Jpeg);
                        }
                    }
                    if (!GlobalData.covers.ContainsKey(song.level.LevelId))
                    {
                        if (song.level?.cover != null)
                        {
                            GlobalData.covers.Add(song.level.LevelId, "data:image/jpg;base64," + Convert.ToBase64String(song.level.cover));
                        }
                        else
                        {
                            GlobalData.covers.Add(song.level.LevelId, ""); ;
                        }
                    }
                }
            
            }
            if(packet.Type == PacketType.SongList)
            {
                SongList songList = packet.SpecificPacket as SongList;
                foreach (var song in songList.Levels)
                {
                    if (!string.IsNullOrEmpty(song?.coverPath))
                    {
                        if (!(song.cover.Length > 1))
                        {
                            var image = Image.FromFile(song.coverPath);
                            var bitmap = FixedSize(image, 128);
                            song.cover = ToByteArray(bitmap, ImageFormat.Jpeg);
                        }
                    }
                    if (!GlobalData.covers.ContainsKey(song.LevelId))
                    {
                        if (song?.cover != null)
                        {
                            GlobalData.covers.Add(song.LevelId, "data:image/jpg;base64," + Convert.ToBase64String(song.cover));
                        }
                        else
                        {
                            GlobalData.covers.Add(song.LevelId, ""); ;
                        }
                    }
                }
                GlobalData.songLists.Add(songList);
                foreach(var song in songList.Levels)
                {
                    if (BeatSaverBrowserManager.convertedBeatSaverLevels.Any((x) => "custom_level_" + x.LevelId == song.LevelId))
                    {
                        var x = BeatSaverBrowserManager.convertedBeatSaverLevels.Find((x) => "custom_level_" + x.LevelId == song.LevelId);
						GlobalData.covers.TryAdd("custom_level_" + x.LevelId, GlobalData.covers[x.LevelId]);
						GlobalData.covers.Remove(x.LevelId); //Cover Migration

						x.LevelId = "custom_level_" + x.LevelId;
                        x.chars = song.chars ;
                        GlobalData.allSongs.Add(x.LevelId, x);
                    }
                    else
                    {
                        GlobalData.allSongs.Add(song.LevelId, song);
                    }
                }
                if (GlobalData.songLists.IndexOf(songList) == 0)
                {
                    GlobalData.RefreshList();
                }
            }
            if(packet.Type == PacketType.NowPlaying)
            {
                NowPlaying nowPlaying = packet.SpecificPacket as NowPlaying;
                OnNowPlaying.Invoke(nowPlaying);
            }
            if (packet.Type == PacketType.NowPlayingUpdate)
            {
                NowPlayingUpdate nowPlayingUpdate = packet.SpecificPacket as NowPlayingUpdate;
                OnUpdate.Invoke(nowPlayingUpdate);
            }
            if(packet.Type == PacketType.AllSongs)
            {
                var x = (packet.SpecificPacket as AllSongs);
                Logger.Info(x.Lists.Count.ToString());
                foreach (var list in x.Lists)
                {
                    Logger.Info("Bruh");
                    Server_PacketRecieved(null, new Packet(list));
                }
            }
        }
    }
}