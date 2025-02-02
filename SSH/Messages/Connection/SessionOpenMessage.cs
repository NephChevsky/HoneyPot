﻿namespace HoneyPot.SSH.Messages.Connection
{
	public class SessionOpenMessage : ChannelOpenMessage
    {
        protected override void OnLoad(SshDataReader reader)
        {
            base.OnLoad(reader);

            if (ChannelType != "session")
                throw new ArgumentException(string.Format("Channel type {0} is not valid.", ChannelType));
        }
    }
}
