#region Statements

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using EpicChill.EpicCore;
using Mirror;
using UnityEngine;

#endregion

namespace EpicChill.Transport
{
    public abstract class Common
    {
        private readonly bool _enableDebugMode;
        protected readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        protected readonly ConcurrentQueue<EpicMessage> QueuedMessages = new ConcurrentQueue<EpicMessage>();
        private GetNextReceivedPacketSizeOptions _packetSizeOptions;
        private ReceivePacketOptions _receivePacketOptions;
        protected EpicManager EpicManager;
        protected SocketId SocketListener;

        #region Class Specific

        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private PacketReliability ConvertChannel(int channel)
        {
            switch (channel)
            {
                // This is a hack atm due to not being able to know mirror's channel count.
                // Future maybe mirror change this to enum values so we can determine better.
                case 3:
                case Channels.DefaultReliable:
                    return PacketReliability.ReliableOrdered;
                case Channels.DefaultUnreliable:
                    return PacketReliability.UnreliableUnordered;
                default:
                    throw new Exception("Unexpected channel: " + channel);
            }
        }

        /// <summary>
        ///     Base class constructor.
        /// </summary>
        /// <param name="epicTransport"></param>
        /// <param name="debug">Whether or not we want to debug info.</param>
#if UNITY_EDITOR
        protected Common(EpicChillTransport epicTransport, bool debug, EpicManager epic)
#else
        protected Common(EpicChillTransport epicTransport, EpicManager epic)
#endif
        {
#if UNITY_EDITOR
            _enableDebugMode = debug;
#endif
            EpicManager = epic;

            // Create all needed structs at start
            _packetSizeOptions = CreateReceivedPacketSizeOptions();
            _receivePacketOptions = CreateReceivePacketOptions();

            _ = Task.Run(ProcessEpicMessages);
            _ = Task.Run(ProcessQueuedMessages);
        }

        /// <summary>
        ///     Read incoming packets from epic services queue system.
        /// </summary>
        private void ProcessEpicMessages()
        {
            while (!CancellationToken.IsCancellationRequested && EpicManager.Initialized)
            {
                if(SocketListener == null) continue;

                Result packetSizeResults =
                    EpicManager.P2PInterface.GetNextReceivedPacketSize(_packetSizeOptions, out uint packetSize);

                if (packetSizeResults != Result.Success)
                {
                    if (_enableDebugMode)
                        DebugLogger.RegularDebugLog(
                            $"[Common] Reading packet size something went wrong. Results {packetSizeResults}",
                            LogType.Error);

                    continue;
                }

                byte[] message = new byte[packetSize];

                Result packetReceivedResults = EpicManager.P2PInterface.ReceivePacket(_receivePacketOptions,
                    out ProductUserId productUserId,
                    out SocketId socketId, out byte channel, ref message, out uint bytesWritten);

                switch (packetReceivedResults)
                {
                    case Result.Success:
                        if (_enableDebugMode)
                            DebugLogger.RegularDebugLog("[Common] No packets found yet.");
                        break;
                    case Result.NotFound:
                        if (_enableDebugMode)
                            DebugLogger.RegularDebugLog("[Common] No packets found yet.");
                        continue;
                    default:
                        if (_enableDebugMode)
                            DebugLogger.RegularDebugLog(
                            $"[Common] Something went wrong reading the packet. Results: {packetReceivedResults}",
                            LogType.Error);
                        continue;
                }

                // This is hard code for now because mirror has no way for us
                // to determine how many channels they have. In future will need find better way.

                InternalMessage internalMessage;

                switch (channel)
                {
                    case 3:
                        ProcessInternalMessages((InternalMessage)message[0], productUserId);
                        continue;
                    default:
                        internalMessage = InternalMessage.Data;
                        break;
                }

                if (_enableDebugMode)
                    DebugLogger.RegularDebugLog(
                    $"[Common] Received: {internalMessage} with data {BitConverter.ToString(message)}. Processing it into queue.");

                EpicMessage epicMessage = new EpicMessage(productUserId.InnerHandle.ToInt32(), channel, internalMessage,
                    message);

                QueuedMessages.Enqueue(epicMessage);
            }
        }

        /// <summary>
        ///     Process queue up messages.
        /// </summary>
        protected abstract void ProcessQueuedMessages();

        /// <summary>
        ///     Process internal messages that have been queued up from us.
        /// </summary>
        protected abstract void ProcessInternalMessages(InternalMessage message, ProductUserId userId);

        /// <summary>
        ///     Send an internal message outside of mirror control.
        /// </summary>
        /// <param name="message">The internal message we want to send <see cref="InternalMessage" /></param>
        /// <param name="user">The user we want to send the <see cref="InternalMessage" /> to.</param>
        internal Result SendInternalMessage(InternalMessage message, ProductUserId user)
        {
            // We hard code to channel 3 because mirror only has 2 channels.
            // If in future this changed we will need find a better way to handle this.
            return EpicManager.P2PInterface.SendPacket(CreatePacket(3, new[] {(byte)message}, user));
        }

        /// <summary>
        ///     Internal way of creating packets so we don't have to write boiler plate
        ///     code all over the place every time we want to create a new packet for epic services.
        /// </summary>
        /// <param name="channel">The channel we will be sending the data on.</param>
        /// <param name="message">The message we want to send to epic user.</param>
        /// <param name="user">The user we want to send message to.</param>
        /// <returns></returns>
        internal SendPacketOptions CreatePacket(int channel, byte[] message, ProductUserId user)
        {
            var test = new SocketId {SocketName = "Testing"};

            Debug.Log($"Valid : {EpicManager.AccountId.ProductUserId.IsValid()}");

            return new SendPacketOptions
            {
                LocalUserId = EpicManager.AccountId.ProductUserId,
                Channel = (byte)channel,
                Reliability = ConvertChannel(channel),
                RemoteUserId = user,
                AllowDelayedDelivery = true,
                Data = message,
                SocketId = test
            };
        }

        /// <summary>
        ///     Internal struct method for boiler plate saving for
        ///     closing connections down in epic services.
        /// </summary>
        /// <param name="productId">The account id to which we want to close connection to.</param>
        /// <returns>
        ///     Creates a new <see cref="CloseConnectionOptions" /> struct to be used for closing connections down from epic
        ///     services.
        /// </returns>
        internal CloseConnectionOptions CreateCloseConnectionOptions(ProductUserId productId)
        {
            return new CloseConnectionOptions
            {
                LocalUserId = EpicManager.AccountId.ProductUserId,
                RemoteUserId = productId,
                SocketId = SocketListener
            };
        }

        /// <summary>
        ///     Internal struct method for boiler plate saving for
        ///     receiving a packet from epic services.
        /// </summary>
        /// <returns>Creates a new <see cref="ReceivePacketOptions" /> struct to be used for receiving a packet from epic services.</returns>
        private ReceivePacketOptions CreateReceivePacketOptions()
        {
            return new ReceivePacketOptions
            {
                LocalUserId = EpicManager.AccountId.ProductUserId, MaxDataSizeBytes = P2PInterface.MaxPacketSize
            };
        }

        /// <summary>
        ///     Internal struct method for boiler plate saving for
        ///     receiving next packet size in epic services.
        /// </summary>
        /// <returns>
        ///     Creates a new <see cref="GetNextReceivedPacketSizeOptions" /> struct to be used for receiving new packets from
        ///     epic services.
        /// </returns>
        private GetNextReceivedPacketSizeOptions CreateReceivedPacketSizeOptions()
        {
            return new GetNextReceivedPacketSizeOptions {LocalUserId = EpicManager.AccountId.ProductUserId};
        }

        /// <summary>
        ///     Shutdown and cleanup resources.
        /// </summary>
        public virtual void Shutdown()
        {
            _receivePacketOptions = null;
            _packetSizeOptions = null;
        }

        #endregion
    }
}
