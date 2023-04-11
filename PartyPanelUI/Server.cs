using PartyPanelShared;
using PartyPanelShared.Models;
using PartyPanelUI.Network;
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
            ServerMetadata metadata = new ServerMetadata();
            //metadata.runLowCostMode = Program.runLowCostMode;
            server.Send(new Packet(metadata).ToBytes());
        }

        private void Server_PlayerDisconnected(NetworkPlayer obj)
        {
            Logger.Debug("Player Disconnected!");
            //panel.DisableSongList();
        }

        private void Server_PlayerConnected(NetworkPlayer obj)
        {
            Logger.Debug("Player Connected!");
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
                Logger.Debug("SongPreview");
                PreviewSong? song = packet.SpecificPacket as PreviewSong;
                //Add preprocessing logic here i.e converting cover images
                if (song?.level != null)
                {
                    if (!String.IsNullOrEmpty(song.level?.coverPath))
                    {
                        if (song.level.cover == null)
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
                            GlobalData.covers.Add(song.level.LevelId, Convert.ToBase64String(song.level.cover));
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
                        if (song.cover == null)
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
                            GlobalData.covers.Add(song.LevelId, Convert.ToBase64String(song.cover));
                        }
                        else
                        {
                            GlobalData.covers.Add(song.LevelId, ""); ;
                        }
                    }
                }
                GlobalData.songLists.Add(songList);
                if(GlobalData.songLists.IndexOf(songList) == 0)
                {
                    GlobalData.RefreshList();
                }
                foreach(var song in songList.Levels)
                {
                    GlobalData.allSongs.Add(song.LevelId, song);
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
        }
    }
}