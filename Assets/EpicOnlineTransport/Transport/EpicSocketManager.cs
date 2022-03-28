using System;
using Cysharp.Threading.Tasks;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Mirage.SocketLayer;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace EpicServicesPeer
{
    internal sealed class EpicSocketManager : IDisposable
    {
        private EpicOptions _epicOptions;

        public EpicSocketManager(EpicOptions options)
        {
            _epicOptions = options;
        }

            /// <summary>
        ///     Check to see if we have received any data from epic users.
        /// </summary>
        /// <param name="clientProductUserId">Returns back the epic id of users who sent message.</param>
        /// <param name="receiveBuffer">The data that was sent to use.</param>
        /// <param name="channel">The channel the data was sent on.</param>
        /// <returns></returns>
        internal bool DataAvailable(out ProductUserId clientProductUserId, out byte[] receiveBuffer, byte channel, out SocketId socket)
        {
            Result result = EOSManager.Instance.GetEOSP2PInterface().ReceivePacket(new ReceivePacketOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                MaxDataSizeBytes = P2PInterface.MaxPacketSize,
                RequestedChannel = channel
            }, out clientProductUserId, out socket, out _, out receiveBuffer);

            if (result == Result.Success)
            {
                return true;
            }

            receiveBuffer = null;
            clientProductUserId = null;

            return false;
        }

        /// <summary>
        ///     Process our internal messages away from mirage.
        /// </summary>
        /// <param name="type">The <see cref="InternalMessage"/> type message we received.</param>
        /// <param name="clientEpicId">The client id which the internal message came from.</param>
        /// <param name="socket">The socket we want to process internal data on.</param>
        private void OnReceiveInternalData(InternalMessage type, ProductUserId clientEpicId, SocketId socket)
        {

        }

        /// <summary>
        ///     Process data incoming from epic backend.
        /// </summary>
        /// <param name="data">The data that has come in.</param>
        /// <param name="clientEpicId">The client the data came from.</param>
        /// <param name="channel">The channel the data was received on.</param>
        private void OnReceiveData(byte[] data, ProductUserId clientEpicId, int channel)
        {

        }

        /// <summary>
        ///     Epic runs much like steam and process's messages on different channels. We will
        ///     process internal messages on channel.length which will equal to 1 higher then
        ///     what end user's will have for channels due to how we use the for loop.
        /// </summary>
        async void ProcessIncomingMessages()
        {
            try
            {
                while (true)
                {
                    for (int chNum = 0; chNum <= _epicOptions.Channels.Length; chNum++)
                    {
                        while (DataAvailable(out ProductUserId clientUserID, out byte[] receiveBuffer, (byte)chNum,
                                   out SocketId socket))
                        {

                            switch (chNum == _epicOptions.Channels.Length)
                            {
                                case true:
                                    OnReceiveInternalData((InternalMessage)receiveBuffer[0], clientUserID, socket);
                                    break;
                                case false:
                                    OnReceiveData(receiveBuffer, clientUserID, chNum);
                                    break;
                            }
                        }
                    }

                    await UniTask.Delay(1);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.RegularDebugLog(ex.Message, LogType.Exception);
            }
        }


        #region Implementation of IDisposable

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal sealed class EpicSocket : ISocket
    {
        #region Fields

        private readonly EpicOptions _epicOptions;
        private readonly EpicSocketManager _epicSocketManager;

        #endregion

        public EpicSocket(EpicOptions options)
        {
            DebugLogger.RegularDebugLog("[EpicSocket] - Staring up socket.");

            _epicOptions = options;
            _epicSocketManager = new EpicSocketManager(options);

        }

        #region Implementation of ISocket

        /// <summary>
        /// Starts listens for data on an endpoint
        /// <para>Used by Server to allow clients to connect</para>
        /// </summary>
        /// <param name="endPoint">the endpoint to listen on</param>
        public void Bind(IEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets up Socket ready to send data to endpoint as a client
        /// </summary>
        /// <param name="endPoint"></param>
        public void Connect(IEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes the socket, stops receiving messages from other peers
        /// </summary>
        public void Close()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if a packet is available 
        /// </summary>
        /// <returns>true if there is atleast 1 packet to read</returns>
        public bool Poll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets next packet
        /// <para>Should be called after Poll</para>
        /// <para>
        ///     Implementation should check that incoming packet is within the size of <paramref name="buffer"/>,
        ///     and make sure not to return <paramref name="bytesReceived"/> above that size
        /// </para>
        /// </summary>
        /// <param name="buffer">buffer to write recevived packet into</param>
        /// <param name="endPoint">where packet came from</param>
        /// <returns>length of packet, should not be above <paramref name="buffer"/> length</returns>
        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a packet to an endpoint
        /// <para>Implementation should use <paramref name="length"/> because <paramref name="packet"/> is a buffer than may contain data from previous packets</para>
        /// </summary>
        /// <param name="endPoint">where packet is being sent to</param>
        /// <param name="packet">buffer that contains the packet, starting at index 0</param>
        /// <param name="length">length of the packet</param>
        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
