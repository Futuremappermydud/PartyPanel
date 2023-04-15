using PartyPanelShared.Models;
using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PartyPanelShared
{
    public enum PacketType
    {
        SongList,
        Command,
        NowPlaying,
        NowPlayingUpdate,
        PlaySong,
        PreviewSong,
        DownloadSong,
        AllSongs
    }

    public class Packet
    {
        //Size of the header, the info we need to parse the specific packet
        // 4x byte - "moon"
        // int - packet type
        // int - packet size
        public const int packetHeaderSize = 16;

        public int Size { get; set; }
        public PacketType Type { get; set; }

        public object SpecificPacket { get; set; }

        public Packet(object specificPacket)
        {
            //Assign type based on parameter type
            Type = (PacketType)Enum.Parse(typeof(PacketType), specificPacket.GetType().Name);

            SpecificPacket = specificPacket;
        }

        public byte[] ToBytes()
        {
            MemoryStream memory = new MemoryStream();

            switch (Type)
            {
                case PacketType.Command:
                    Serializer.Serialize(memory, SpecificPacket as Command);
                    break;
                case PacketType.PreviewSong:
                    Serializer.Serialize(memory, SpecificPacket as PreviewSong);
                    break;
                case PacketType.SongList:
                    Serializer.Serialize(memory, SpecificPacket as SongList);
                    break;
                case PacketType.NowPlaying:
                    Serializer.Serialize(memory, SpecificPacket as NowPlaying);
                    break;
                case PacketType.NowPlayingUpdate:
                    Serializer.Serialize(memory, SpecificPacket as NowPlayingUpdate);
                    break;
                case PacketType.PlaySong:
                    Serializer.Serialize(memory, SpecificPacket as PlaySong);
                    break;
                case PacketType.DownloadSong:
                    Serializer.Serialize(memory, SpecificPacket as DownloadSong);
                    break;
				case PacketType.AllSongs:
					Serializer.Serialize(memory, SpecificPacket as AllSongs);
					break;
			}

            var magicFlag = Encoding.UTF8.GetBytes("moon");
            var typeBytes = BitConverter.GetBytes((int)Type);
            var sizeBytes = BitConverter.GetBytes(memory.Length);

            return Combine(magicFlag, typeBytes, sizeBytes, memory.ToArray());
        }

        public string ToBase64() => Convert.ToBase64String(ToBytes());

        public static Packet FromBytes(byte[] bytes) => FromStream(new MemoryStream(bytes));
        //TODO: Implement a Packet consumer
        //to make sure packets do not get mixed up when packets are deserialized too fast (Invalid field in source data: 0)
        public static Packet FromStream(MemoryStream stream)
        {
            var typeBytes = new byte[sizeof(int)];
            var sizeBytes = new byte[sizeof(int)];

            //Verify that this is indeed a Packet
            if (!StreamIsAtPacket(stream, false))
            {
                stream.Seek(-(sizeof(byte) * 4), SeekOrigin.Current); //Return to original position in stream
                return null;
            }

            stream.Read(typeBytes, 0, sizeof(int));
            stream.Read(sizeBytes, 0, sizeof(int));

            var specificPacketSize = BitConverter.ToInt32(sizeBytes, 0);
            object specificPacket = null;

            PacketType type = (PacketType)BitConverter.ToInt32(typeBytes, 0);
            byte[] msg = stream.ToArray();
            switch (type)
            {
                case PacketType.Command:
                    using (MemoryStream ms = new MemoryStream(msg, packetHeaderSize, msg.Length - packetHeaderSize))
                    {
                        specificPacket = Serializer.Deserialize<Command>(ms);
                    }
                    break;
                case PacketType.PreviewSong:
                    using (MemoryStream ms = new MemoryStream(msg, packetHeaderSize, msg.Length - packetHeaderSize))
                    {
                        specificPacket = Serializer.Deserialize<PreviewSong>(ms);
                    }
                    break;
                case PacketType.SongList:
                    using (MemoryStream ms = new MemoryStream(msg, packetHeaderSize, msg.Length - packetHeaderSize))
                    {
                        specificPacket = Serializer.Deserialize<SongList>(ms);
                    }
                    break;
                case PacketType.NowPlaying:
                    using (MemoryStream ms = new MemoryStream(msg, packetHeaderSize, msg.Length - packetHeaderSize))
                    {
                        specificPacket = Serializer.Deserialize<NowPlaying>(ms);
                    }
                    break;
                case PacketType.NowPlayingUpdate:
                    using (MemoryStream ms = new MemoryStream(msg, packetHeaderSize, msg.Length - packetHeaderSize))
                    {
                        specificPacket = Serializer.Deserialize<NowPlayingUpdate>(ms);
                    }
                    break;
                case PacketType.PlaySong:
                    using (MemoryStream ms = new MemoryStream(msg, packetHeaderSize, msg.Length - packetHeaderSize))
                    {
                        specificPacket = Serializer.Deserialize<PlaySong>(ms);
                    }
                    break;
                case PacketType.DownloadSong:
                    using (MemoryStream ms = new MemoryStream(msg, packetHeaderSize, msg.Length - packetHeaderSize))
                    {
                        specificPacket = Serializer.Deserialize<DownloadSong>(ms);
                    }
                    break;
				case PacketType.AllSongs:
					using (MemoryStream ms = new MemoryStream(msg, packetHeaderSize, msg.Length - packetHeaderSize))
					{
                        var x = Serializer.Deserialize<AllSongs>(ms);
                        specificPacket = x;
					}
					break;
			}

            return new Packet(specificPacket)
            {
                Size = specificPacketSize,
                Type = (PacketType)BitConverter.ToInt32(typeBytes, 0)
            };
        }

        public static bool StreamIsAtPacket(byte[] bytes, bool resetStreamPos = true) => StreamIsAtPacket(new MemoryStream(bytes), resetStreamPos);
        public static bool StreamIsAtPacket(MemoryStream stream, bool resetStreamPos = true)
        {
            var magicFlagBytes = new byte[sizeof(byte) * 4];

            //Verify that this is indeed a Packet
            stream.Read(magicFlagBytes, 0, sizeof(byte) * 4);

            if (resetStreamPos) stream.Seek(-(sizeof(byte) * 4), SeekOrigin.Current); //Return to original position in stream

            return Encoding.UTF8.GetString(magicFlagBytes) == "moon";
        }

        public static bool PotentiallyValidPacket(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);

            var typeBytes = new byte[sizeof(int)];
            var sizeBytes = new byte[sizeof(int)];

            //Verify that this is indeed a Packet
            if (!StreamIsAtPacket(stream, false))
            {
                stream.Seek(-(sizeof(byte) * 4), SeekOrigin.Current); //Return to original position in stream
                return false;
            }

            stream.Read(typeBytes, 0, sizeof(int));
            stream.Read(sizeBytes, 0, sizeof(int));

            stream.Seek(-(sizeof(byte) * 4 + sizeof(int) * 2), SeekOrigin.Current); //Return to original position in stream

            return (BitConverter.ToInt32(sizeBytes, 0) + packetHeaderSize) <= bytes.Length;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
