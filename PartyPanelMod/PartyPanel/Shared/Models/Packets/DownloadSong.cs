using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class DownloadSong
    {
		public DownloadSong()
		{
		}

		[ProtoMember(1)]
        public string levelId { get; set; } = "";
        [ProtoMember(2)]
        public string songKey { get; set; } = "";
    }
}
