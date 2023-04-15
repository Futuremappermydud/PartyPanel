using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class PlaySong
    {
        [ProtoMember(1)]
        public string levelId { get; set; } = "";
        [ProtoMember(2)]
        public string difficulty { get; set; } = "";
        [ProtoMember(3)]
        public Characteristic characteristic { get; set; } = null;
        [ProtoMember(4)]
        public GameplayModifiers gameplayModifiers { get; set; } = null;
    }
}
