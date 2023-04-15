using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class NowPlaying
    {
        public NowPlaying()
        {
        }

        public NowPlaying(string levelID, bool isFinished)
        {
            this.levelID = levelID;
            this.isFinished = isFinished;
        }

        [ProtoMember(1)]
        public string levelID { get; set; }
        [ProtoMember(2)]
        public bool isFinished { get; set; } = false;
    }
}
