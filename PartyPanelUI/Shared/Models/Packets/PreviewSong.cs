using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class PreviewSong
    {
        [ProtoMember(1)]
        public PreviewBeatmapLevel level { get; set; }

        public PreviewSong()
        {
        }

        public PreviewSong(PreviewBeatmapLevel level)
        {
            this.level = level;
        }
    }
}
