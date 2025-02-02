using HoneyPot.SSH.Messages.Connection;
using System.Text;

namespace HoneyPot.SSH.Messages
{
    public class SubsystemRequestMessage : ChannelRequestMessage
    {
        public string Name { get; private set; }

        protected override void OnLoad(SshDataReader reader)
        {
            base.OnLoad(reader);

            Name = reader.ReadString(Encoding.ASCII);
        }
    }
}
