using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class PlayerSpecificSettings
    {
        [ProtoMember(1)]
        public bool leftHanded;
        [ProtoMember(2)]
        public bool noTextsAndHuds;
        [ProtoMember(3)]
        public bool advancedHud;
        [ProtoMember(4)]
        public bool reduceDebris;
    }
}
