using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class NowPlayingUpdate
    {
        public NowPlayingUpdate(int score, double accuracy, int elapsed, int totalTime)
        {
            this.score = score;
            this.accuracy = accuracy;
            this.elapsed = elapsed;
            this.totalTime = totalTime;
        }

        [ProtoMember(1)]
        public int score { get; set; }
        [ProtoMember(2)]
        public double accuracy { get; set; }
        [ProtoMember(3)]
        public int elapsed { get; set; }
        [ProtoMember(4)]
        public int totalTime { get; set; }
    }
}
