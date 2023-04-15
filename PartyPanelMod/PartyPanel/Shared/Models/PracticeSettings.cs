using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class PracticeSettings
    {
        [ProtoMember(1)]
        public float songSpeed;
    }
}
