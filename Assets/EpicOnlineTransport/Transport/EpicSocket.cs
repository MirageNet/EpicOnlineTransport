using System;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;
using UnityEngine.Assertions;

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
        readonly EpicEndPoint _receiveEndPoint = new EpicEndPoint();

        public EpicSocket(RelayHandle relayHandle)
        {
            _relayHandle = relayHandle ?? throw new ArgumentNullException(nameof(relayHandle));
        }

        void ThrowIfRelayNotActive()
        {
            if (_relayHandle.IsOpen)
                throw new InvalidOperationException("Relay not open, can not start socket");
        }

        public void Bind(IEndPoint endPoint)
        {
            ThrowIfRelayNotActive();
        }

        public void Connect(IEndPoint _endPoint)
        {
            ThrowIfRelayNotActive();
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

            // todo do we need to do anything with socketid or channel?
            Result result = _relayHandle.P2P.ReceivePacket(_receiveOptions, out _nextPacket.userId, out SocketId _, out byte _, out _nextPacket.data);

            if (result != Result.Success && result != Result.NotFound) // log for results other than Success/NotFound
                EpicLogger.WarnResult("Receive Packet", result);

            return result == Result.Success;
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            Assert.IsNotNull(_nextPacket.data);

            Buffer.BlockCopy(_nextPacket.data, 0, buffer, 0, _nextPacket.data.Length);

            _receiveEndPoint.UserId = _nextPacket.userId;
            endPoint = _receiveEndPoint;
            int length = _nextPacket.data.Length;

            // clear refs
            _nextPacket = default;
            EpicLogger.Verbose($"Receive {length} bytes from {_nextPacket.userId}");
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

            EpicLogger.Verbose($"Send {length} bytes to {_sendOptions.RemoteUserId}");
        }
    }
}

