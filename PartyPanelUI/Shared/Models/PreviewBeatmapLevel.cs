using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class Characteristic
    {
        public Characteristic()
        {
        }

        public Characteristic(string name, string[] diffs)
        {
            Name = name;
            this.diffs = diffs;
        }

        [ProtoMember(1)]
        public string Name { get; set; } = "";
        [ProtoMember(2)]
        public string[] diffs { get; set; } = new string[0];

    }
    [ProtoContract]
    public class PreviewBeatmapLevel
    {
        public PreviewBeatmapLevel()
        {
        }

        [ProtoMember(1)]
        public string LevelId { get; set; } = "";
        [ProtoMember(2)]
        public string Name { get; set; } = "";
        [ProtoMember(3)]
        public string SubName { get; set; } = "";
        [ProtoMember(4)]
        public string Author { get; set; } = "";
        [ProtoMember(5)]
        public string Mapper { get; set; } = "";
        [ProtoMember(6)]
        public string Duration { get; set; } = "";
        [ProtoMember(7)]
        public float BPM { get; set; } = 0.0f;
        [ProtoMember(8)]
        public byte[] cover { get; set; } = new byte[0]; //Cover Imaage stored as bytes
        [ProtoMember(9)]
        public string coverPath { get; set; } = "";
        [ProtoMember(10)]
        public bool Favorited { get; set; } = false;
        [ProtoMember(12)]
        public bool Owned { get; set; } = false;
        [ProtoMember(13)]
        public string OwnedJustificaton { get; set; } = "";
        [ProtoMember(14)]
        public Characteristic[] chars { get; set; } = new Characteristic[0];
    }
}
