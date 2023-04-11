using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class SongList
    {
        public SongList(PreviewBeatmapLevel[] levels)
        {
            Levels = levels;
        }

        [ProtoMember(1)]
        public PreviewBeatmapLevel[] Levels { get; set; }
    }
}
