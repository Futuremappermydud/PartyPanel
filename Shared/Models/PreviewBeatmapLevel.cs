using System;

namespace PartyPanelShared.Models
{
    [Serializable]
    public class Characteristic
    {
        public string Name { get; set; }
        public string[] diffs { get; set; }

    }
    [Serializable]
    public class PreviewBeatmapLevel
    {
        // -- Unloaded levels have the following:
        public string LevelId { get; set; }
        public string Name { get; set; }
        public string SubName { get; set; }
        public string Author { get; set; }
        public string Mapper { get; set; }
        public string Duration { get; set; }
        public float BPM { get; set; }
        public byte[] cover { get; set; } //Cover Imaage stored as bytes
        public bool Favorited { get; set; }
        public bool Owned { get; set; }
        // -- Only Loaded levels will have the following:
        public Characteristic[] chars { get; set; }
    }
}
