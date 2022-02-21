using System;
using System.Collections.Generic;
using System.Net;
using Mikrocosmos;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.Events;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-discovery
    API Reference: https://mirror-networking.com/docs/api/Mirror.Discovery.NetworkDiscovery.html
*/

public struct DiscoveryRequest : NetworkMessage
{
    // Add properties for whatever information you want sent by clients
    // in their broadcast messages that servers will consume.
}

public struct DiscoveryResponse : NetworkMessage
{
    // Add properties for whatever information you want the server to return to
    // clients for them to display or consume for establishing a connection.
    public long ServerID;
    public int ServerPlayerNum;
    public int ServerMaxPlayerNum;
    public bool IsGaming;
    public Uri Uri;
    public string HostName;
    public IPEndPoint EndPoint { get; set; }

   
}
[Serializable]
public class MenuServerFoundUnityEvent : UnityEvent<DiscoveryResponse> { };
public class MenuNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    #region Server
    public long ServerId { get; private set; }

    [Tooltip("Transport to be advertised during discovery")]
    public Transport transport;

    [Tooltip("Invoked when a server is found")]
    public MenuServerFoundUnityEvent OnServerFound;

    public override void Start()
    {
        ServerId = RandomLong();

        // active transport gets initialized in awake
        // so make sure we set it here in Start()  (after awakes)
        // Or just let the user assign it in the inspector
        if (transport == null)
            transport = Transport.activeTransport;

        base.Start();

        //Debug.Log("Start discovery");
        //StartDiscovery();
    }

    /// <summary>
    /// Process the request from a client
    /// </summary>
    /// <remarks>
    /// Override if you wish to provide more information to the clients
    /// such as the name of the host player
    /// </remarks>
    /// <param name="request">Request coming from client</param>
    /// <param name="endpoint">Address of the client that sent the request</param>
    /// <returns>A message containing information about this server</returns>
    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint) 
    {
        // In this case we don't do anything with the request
        // but other discovery implementations might want to use the data
        // in there,  This way the client can ask for
        // specific game mode or something

        try
        {
            Debug.Log("Processing response");
            // this is an example reply message,  return your own
            // to include whatever is relevant for your game
            return new DiscoveryResponse
            {
                ServerID = ServerId,
                Uri = transport.ServerUri(),
                IsGaming =  ((NetworkedRoomManager)(NetworkedRoomManager.singleton)).IsInGame,
                ServerPlayerNum = NetworkServer.connections.Count,
                HostName = ((NetworkedRoomManager)NetworkManager.singleton).GetHostName(),
                ServerMaxPlayerNum = 8
            };
        }
        catch (NotImplementedException)
        {
            Debug.LogError($"Transport {transport} does not support network discovery");
            throw;
        }
    }

    #endregion

    #region Client

    /// <summary>
    /// Create a message that will be broadcasted on the network to discover servers
    /// </summary>
    /// <remarks>
    /// Override if you wish to include additional data in the discovery message
    /// such as desired game mode, language, difficulty, etc... </remarks>
    /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
    protected override DiscoveryRequest GetRequest()
    {
        return new DiscoveryRequest();
    }

    /// <summary>
    /// Process the answer from a server
    /// </summary>
    /// <remarks>
    /// A client receives a reply from a server, this method processes the
    /// reply and raises an event
    /// </remarks>
    /// <param name="response">Response that came from the server</param>
    /// <param name="endpoint">Address of the server that replied</param>
    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint) {
        // we received a message from the remote endpoint
        response.EndPoint = endpoint;

        // although we got a supposedly valid url, we may not be able to resolve
        // the provided host
        // However we know the real ip address of the server because we just
        // received a packet from it,  so use that as host.
        UriBuilder realUri = new UriBuilder(response.Uri)
        {
            Host = response.EndPoint.Address.ToString()
        };
        response.Uri = realUri.Uri;
      //  Debug.Log("Find server");
        OnServerFound.Invoke(response);
    }

    #endregion
}
