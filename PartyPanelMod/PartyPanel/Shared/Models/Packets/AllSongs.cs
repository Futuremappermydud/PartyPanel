using ProtoBuf;
using System;
using System.Collections.Generic;

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
