#region Statements

using System;
using System.Collections.Generic;
using Epic.OnlineServices.P2P;
using EpicChill.EpicCore;
using Mirror;
using UnityEngine;

#endregion

namespace EpicChill.Transport
{
    [DisallowMultipleComponent]
    public class EpicChillTransport : Mirror.Transport
    {
        #region Fields

        [Header("Transport Options")]
        [SerializeField] private EpicManager _epicManager;

#if UNITY_EDITOR
        [Header("Debug Information")]
        [SerializeField] private bool _transportDebug = false;
#endif
        private Client _client;
        private Server _server;

        #endregion

        #region Overrides of Transport

        /// <summary>
        ///     Is this transport available in the current platform?
        ///     <para>Some transports might only be available in mobile</para>
        ///     <para>Many will not work in webgl</para>
        ///     <para>Example usage: return Application.platform == RuntimePlatform.WebGLPlayer</para>
        /// </summary>
        /// <returns>True if this transport works in the current platform</returns>
        public override bool Available()
        {
            return Application.platform == RuntimePlatform.WindowsEditor ||
                   Application.platform == RuntimePlatform.WindowsPlayer ||
                   Application.platform == RuntimePlatform.LinuxEditor ||
                   Application.platform == RuntimePlatform.LinuxPlayer ||
                   Application.platform == RuntimePlatform.OSXEditor ||
                   Application.platform == RuntimePlatform.OSXPlayer;
        }

        #region Client Specific

        /// <summary>
        ///     Determines if we are currently connected to the server
        /// </summary>
        /// <returns>True if a connection has been established to the server</returns>
        public override bool ClientConnected() => !(_client is null);

        /// <summary>
        ///     Establish a connection to a server
        /// </summary>
        /// <param name="address">The IP address or FQDN of the server we are trying to connect to</param>
        public override void ClientConnect(string address)
        {
            if (!_epicManager.Initialized) return;

#if UNITY_EDITOR
            _client = new Client(this, _transportDebug, _epicManager);
#else
            _client = new Client(this, _epicManager);
#endif
            _client?.Connect(address);
        }

        /// <summary>
        ///     Send data to the server
        /// </summary>
        /// <param name="channelId">
        ///     The channel to use.  0 is the default channel,
        ///     but some transports might want to provide unreliable, encrypted, compressed, or any other feature
        ///     as new channels
        /// </param>
        /// <param name="segment">
        ///     The data to send to the server. Will be recycled after returning, so either use it directly or
        ///     copy it internally. This allows for allocation-free sends!
        /// </param>
        /// <returns>true if the send was successful</returns>
        public override bool ClientSend(int channelId, ArraySegment<byte> segment)
        {
            return !(_client is null) && _client.Send(channelId, segment);
        }

        /// <summary>
        ///     Disconnect this client from the server
        /// </summary>
        public override void ClientDisconnect()
        {
            _client?.Disconnect();
        }

        #endregion

        #region Server Specific

        /// <summary>
        ///     Retrieves the address of this server.
        ///     Useful for network discovery
        /// </summary>
        /// <returns>the url at which this server can be reached</returns>
        public override Uri ServerUri()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Determines if the server is up and running
        /// </summary>
        /// <returns>true if the transport is ready for connections from clients</returns>
        public override bool ServerActive() => !(_server is null);

        /// <summary>
        ///     Start listening for clients
        /// </summary>
        public override void ServerStart()
        {
            if (!_epicManager.Initialized) return;

#if UNITY_EDITOR
            _server = new Server(this, _transportDebug, _epicManager);
#else
            _server = new Server(this, _epicManager);
#endif
            _server?.Start();
        }

        /// <summary>
        ///     Send data to one or multiple clients. We provide a list, so that transports can make use
        ///     of multicasting, and avoid allocations where possible.
        ///     We don't provide a single ServerSend function to reduce complexity. Simply overwrite this
        ///     one in your Transport.
        /// </summary>
        /// <param name="connectionIds">The list of client connection ids to send the data to</param>
        /// <param name="channelId">
        ///     The channel to be used.  Transports can use channels to implement
        ///     other features such as unreliable, encryption, compression, etc...
        /// </param>
        /// <param name="data"></param>
        /// <returns>true if the data was sent to all clients</returns>
        public override bool ServerSend(List<int> connectionIds, int channelId, ArraySegment<byte> segment)
        {
            return _server.Send(connectionIds, channelId, segment);
        }

        /// <summary>
        ///     Disconnect a client from this server.  Useful to kick people out.
        /// </summary>
        /// <param name="connectionId">the id of the client to disconnect</param>
        /// <returns>true if the client was kicked</returns>
        public override bool ServerDisconnect(int connectionId)
        {
           return _server.Disconnect(connectionId);
        }

        /// <summary>
        ///     Get the client address
        /// </summary>
        /// <param name="connectionId">id of the client</param>
        /// <returns>address of the client</returns>
        public override string ServerGetClientAddress(int connectionId)
        {
            return _server.ClientAddress(connectionId);
        }

        /// <summary>
        ///     Stop listening for clients and disconnect all existing clients
        /// </summary>
        public override void ServerStop()
        {
            if (!(_server is null))
                Shutdown();
        }

        #endregion

        /// <summary>
        ///     The maximum packet size for a given channel.  Unreliable transports
        ///     usually can only deliver small packets. Reliable fragmented channels
        ///     can usually deliver large ones.
        ///     GetMaxPacketSize needs to return a value at all times. Even if the
        ///     Transport isn't running, or isn't Available(). This is because
        ///     Fallback and Multiplex transports need to find the smallest possible
        ///     packet size at runtime.
        /// </summary>
        /// <param name="channelId">channel id</param>
        /// <returns>the size in bytes that can be sent via the provided channel</returns>
        public override int GetMaxPacketSize(int channelId = Channels.DefaultReliable)
        {
            return P2PInterface.MaxPacketSize;
        }

        /// <summary>
        ///     Shut down the transport, both as client and server
        /// </summary>
        public override void Shutdown()
        {
            _server?.Shutdown();
            _server = null;

            _client?.Shutdown();
            _client = null;
        }

        #endregion
    }
}
