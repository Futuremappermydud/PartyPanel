using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
	[ProtoContract]
	public class AllSongs
	{
		public AllSongs()
		{
		}

		public AllSongs(List<SongList> lists)
		{
			Lists = lists;
		}

		[ProtoMember(1)]
		public List<SongList> Lists { get; set; }
	}
}
