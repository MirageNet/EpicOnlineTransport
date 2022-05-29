using System;
using System.Linq;
using Epic.OnlineServices.P2P;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Sockets.EpicSocket
{
    internal sealed class EpicSocket : ISocket
    {
        bool _isClosed;
        RelayHandle _relayHandle;

        SendPacketOptions _sendOptions;
        ReceivePacketOptions _receiveOptions;

        int _lastTickedFrame;
        ReceivedPacket _nextPacket;
        EpicEndPoint _receiveEndPoint;

        public EpicSocket(RelayHandle relayHandle)
        {
            _relayHandle = relayHandle ?? throw new ArgumentNullException(nameof(relayHandle));
        }

        void ThrowIfRelayNotActive()
        {
            if (!_relayHandle.IsOpen)
                throw new InvalidOperationException("Relay not open, can not start socket");
        }

        public void Bind(IEndPoint endPoint)
        {
            ThrowIfRelayNotActive();
            _receiveEndPoint = (EpicEndPoint)endPoint;
        }

        public void Connect(IEndPoint endPoint)
        {
            ThrowIfRelayNotActive();
            _receiveEndPoint = (EpicEndPoint)endPoint;
        }

        public void Close()
        {
            _relayHandle.CloseRelay();
            _relayHandle = default;
            _isClosed = true;
        }

        bool IsOpenAndLoaded()
        {
            if (_isClosed)
                return false;

            if (_relayHandle.CheckOpen())
            {
                return true;
            }
            else
            {
                EpicLogger.logger.LogError("Calling when when EOS is not loaded, Closing socket");
                Close();
                return false;
            }
        }

        public bool Poll()
        {
            if (!IsOpenAndLoaded()) return false;

            // first time this tick?
            if (_lastTickedFrame != Time.frameCount)
            {
                _relayHandle.Manager.Tick();
                _lastTickedFrame = Time.frameCount;
            }

            Debug.Assert(_nextPacket.data == null);
            return _relayHandle.ReceiveGameData(out _nextPacket);
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            Debug.Assert(_nextPacket.data != null);

            Buffer.BlockCopy(_nextPacket.data, 0, buffer, 0, _nextPacket.data.Length);

            _receiveEndPoint.UserId = _nextPacket.userId;
            endPoint = _receiveEndPoint;
            int length = _nextPacket.data.Length;

            EpicLogger.Verbose($"Receive {length} bytes from {_nextPacket.userId}");

            // clear refs
            _nextPacket = default;
            return length;
        }

        public void Send(IEndPoint iEndPoint, byte[] packet, int length)
        {
            if (!IsOpenAndLoaded()) return;

            var endPoint = (EpicEndPoint)iEndPoint;

            // send option has no length field, we have to copy to new array
            // todo avoid allocation
            byte[] data = packet.Take(length).ToArray();
            _relayHandle.SendGameData(endPoint.UserId, data);

            EpicLogger.Verbose($"Send {length} bytes to {endPoint.UserId}");
        }
    }
}

