using System.Text;

namespace HoneyPot.SSH.Messages.Connection
{
    public class CommandRequestMessage : ChannelRequestMessage
    {
        public string Command { get; private set; }

        protected override void OnLoad(SshDataReader reader)
        {
            base.OnLoad(reader);

            Command = reader.ReadString(Encoding.ASCII);
        }
    }
}
