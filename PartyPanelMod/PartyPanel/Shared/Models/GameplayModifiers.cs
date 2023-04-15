using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class GameplayModifiers
    {
		public enum EnabledObstacleType
		{
			All,
			FullHeightOnly,
			NoObstacles
		}
		public enum SongSpeed
        {
            Normal,
            Faster,
            Slower,
            SuperFast
        }
		public enum EnergyType
		{
			Bar,
			Battery
		}
        [ProtoMember(1)]
        public bool disappearingArrows;
        [ProtoMember(2)]
        public EnabledObstacleType enabledObstacleType;
        [ProtoMember(3)]
        public EnergyType energyType;
        [ProtoMember(4)]
        public bool failOnSaberClash;
        [ProtoMember(5)]
        public bool instaFail;
        [ProtoMember(6)]
        public bool noFailOn0Energy;
        [ProtoMember(7)]
        public bool demoNoObstacles;
        [ProtoMember(8)]
        public bool strictAngles;
        [ProtoMember(9)]
        public bool demoNoFail;
        [ProtoMember(10)]
        public bool ghostNotes;
        [ProtoMember(11)]
        public bool noBombs;
        [ProtoMember(12)]
        public SongSpeed songSpeed;
        [ProtoMember(13)]
        public bool noArrows;
        [ProtoMember(14)]
        public bool proMode;
        [ProtoMember(15)]
        public bool zenMode;
        [ProtoMember(16)]
        public bool smallCubes;
    }
}
