using ProtoBuf;
using System;

namespace PartyPanelShared.Models
{
    [ProtoContract]
    public class Command
    {
        public enum CommandType
        {
            Heartbeat,
            ReturnToMenu
        }
        [ProtoMember(1)]
        public CommandType commandType;

        public Command(CommandType commandType)
        {
            this.commandType = commandType;
        }

        public Command()
        {
        }
    }
}
