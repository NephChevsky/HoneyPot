﻿using HoneyPot.SSH.Messages;
using HoneyPot.SSH.Messages.UserAuth;
using System.Diagnostics.Contracts;

namespace HoneyPot.SSH.Services
{
	public class UserAuthService : SshService
    {
        public UserAuthService(Session session)
            : base(session)
        {
        }

        public event EventHandler<UserAuthArgs> UserAuth;

        public event EventHandler<string> Succeed;

        protected internal override void CloseService()
        {
        }

        internal void HandleMessageCore(UserAuthServiceMessage message)
        {
            Contract.Requires(message != null);

            this.HandleMessage((dynamic)message);
        }

        private void HandleMessage(RequestMessage message)
        {
            switch (message.MethodName)
            {
                case "publickey":
                    var keyMsg = Message.LoadFrom<PublicKeyRequestMessage>(message);
                    HandleMessage(keyMsg);
                    break;
                case "password":
                    var pswdMsg = Message.LoadFrom<PasswordRequestMessage>(message);
                    HandleMessage(pswdMsg);
                    break;
                case "hostbased":
                case "none":
                default:
                    _session.SendMessage(new FailureMessage());
                    break;
            }
        }

        private void HandleMessage(PasswordRequestMessage message)
        {
            var verified = false;

            var args = new UserAuthArgs(_session, message.Username, message.Password);
            if (UserAuth != null)
            {
                UserAuth(this, args);
                verified = args.Result;
            }

            if (verified)
            {
                _session.RegisterService(message.ServiceName, args);

                Succeed?.Invoke(this, message.ServiceName);

                _session.SendMessage(new SuccessMessage());
                return;
            }
            else
            {
                _session.SendMessage(new FailureMessage());
            }
        }

        private void HandleMessage(PublicKeyRequestMessage message)
        {
            if (Session._publicKeyAlgorithms.ContainsKey(message.KeyAlgorithmName))
            {
                var verified = false;

                var keyAlg = Session._publicKeyAlgorithms[message.KeyAlgorithmName](null);
                keyAlg.LoadKeyAndCertificatesData(message.PublicKey);

                var args = new UserAuthArgs(base._session, message.Username, message.KeyAlgorithmName, keyAlg.GetFingerprint(), message.PublicKey);
                UserAuth?.Invoke(this, args);
                verified = args.Result;

                if (!verified)
                {
                    _session.SendMessage(new FailureMessage());
                    return;
                }

                if (!message.HasSignature)
                {
                    _session.SendMessage(new PublicKeyOkMessage { KeyAlgorithmName = message.KeyAlgorithmName, PublicKey = message.PublicKey });
                    return;
                }

                var sig = keyAlg.GetSignature(message.Signature);

                var bytes = new SshDataWriter(4 + _session.SessionId.Length + message.PayloadWithoutSignature.Length)
                    .WriteBinary(_session.SessionId)
                    .WriteBytes(message.PayloadWithoutSignature)
                    .ToByteArray();

                verified = keyAlg.VerifyData(bytes, sig);

                if (!verified)
                {
                    _session.SendMessage(new FailureMessage());
                    return;
                }

                _session.RegisterService(message.ServiceName, args);
                Succeed?.Invoke(this, message.ServiceName);
                _session.SendMessage(new SuccessMessage());
            }
            else
            {
                _session.SendMessage(new FailureMessage());
            }
        }
    }
}
