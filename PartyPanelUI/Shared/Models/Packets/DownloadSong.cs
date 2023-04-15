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

		public DownloadSong(string levelId, string songKey)
		{
			this.levelId = levelId;
			this.songKey = songKey;
		}

		[ProtoMember(1)]
		public string levelId { get; set; } = "";

		[ProtoMember(2)]
        public string songKey { get; set; } = "";
    }
}
