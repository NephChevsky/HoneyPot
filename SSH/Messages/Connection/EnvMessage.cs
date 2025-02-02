﻿using HoneyPot.SSH.Messages.Connection;
using System.Text;

namespace HoneyPot.SSH.Messages
{
    public class EnvMessage : ChannelRequestMessage
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        protected override void OnLoad(SshDataReader reader)
        {
            base.OnLoad(reader);

            Name = reader.ReadString(Encoding.ASCII);
            Value = reader.ReadString(Encoding.ASCII);
        }
    }
}
