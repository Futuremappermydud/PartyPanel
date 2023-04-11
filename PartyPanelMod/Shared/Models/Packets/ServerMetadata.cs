using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class ServerMetadata
    {
        [ProtoMember(1)]
        public bool runLowCostMode = false;
    }
}
