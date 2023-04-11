using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class SongList
    {
        [ProtoMember(1)]
        public PreviewBeatmapLevel[] Levels { get; set; }
    }
}
