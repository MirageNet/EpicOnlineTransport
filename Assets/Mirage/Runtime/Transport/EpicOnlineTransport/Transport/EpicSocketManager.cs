using System;
using Cysharp.Threading.Tasks;
using Epic.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using EpicChill.Transport;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace EpicTransport
{
    internal sealed class EpicSocketManager : IDisposable
    {
        private PacketReliability[] Channels => new PacketReliability[3];

        /// <summary>
        ///     Check to see if we have received any data from epic users.
        /// </summary>
        /// <param name="clientProductUserId">Returns back the epic id of users who sent message.</param>
        /// <param name="receiveBuffer">The data that was sent to use.</param>
        /// <param name="channel">The channel the data was sent on.</param>
        /// <returns></returns>
        private bool DataAvailable(out ProductUserId clientProductUserId, out byte[] receiveBuffer, byte channel, out SocketId socket)
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
                    for (int chNum = 0; chNum <= Channels.Length; chNum++)
                    {
                        while (DataAvailable(out ProductUserId clientUserID, out byte[] receiveBuffer, (byte)chNum,
                                   out SocketId socket))
                        {

                            switch (chNum == Channels.Length)
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
}
