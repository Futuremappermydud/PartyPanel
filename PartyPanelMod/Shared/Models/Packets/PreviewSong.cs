using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class PreviewSong
    {
        [ProtoMember(1)]
        public PreviewBeatmapLevel level;
    }
}
