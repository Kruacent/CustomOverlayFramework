using CustomOverlayFramework.Authentification;
using CustomOverlayFramework.Enums;
using CustomOverlayFramework.Utils;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomOverlayFramework.Core
{
    public sealed class OverlayServer
    {
        private readonly Config _config;
        private readonly ClientSender _clientSender;
        private readonly AuthentificationHandler _authentificationHandler;
        private TcpListener _listener;
        private volatile bool _isRunning = false;
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();
        private readonly Dictionary<string, TcpClient> _authentifiedClients = new Dictionary<string, TcpClient>();

        public OverlayServer(Config config, ClientSender clientSender)
        {
            _config = config;
            _authentificationHandler = new AuthentificationHandler(config);
            _clientSender = clientSender;
        }

        public void Start()
        {
            IPAddress adress = IPUtils.TryValid(_config.ListenAddress);
            _listener = new TcpListener(adress, _config.Port);
            _listener.Start();
            _isRunning = true;

            _ = AcceptClients();
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            FlushConnectedClient();
        }

        private void FlushConnectedClient()
        {
            foreach (TcpClient client in _connectedClients.ToList())
            {
                client.Close();
            }
        }

        private async Task AcceptClients()
        {
            while(_isRunning)
            {
                TcpClient client;

                try
                {
                    client = await _listener.AcceptTcpClientAsync();
                } 
                catch (Exception e)
                {
                    if (_isRunning == false)
                    {
                        return;
                    }

                    Log.Error(e.Message);
                    continue;
                }

                _ = HandleClient(client);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            _connectedClients.Add(client);
            using(client)
            {

                string userId = null;
                try
                {
                    NetworkStream stream = client.GetStream();
                    StreamReader reader = new StreamReader(stream, new UTF8Encoding(false));
                    StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

                    IPAddress userIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                    string line = await reader.ReadLineAsync();
                    AuthenticationResponse response = _authentificationHandler.Authenticate(line, userIp, out string requesterUserId);

                    if (response == AuthenticationResponse.Welcome && _authentifiedClients.ContainsKey(requesterUserId))
                    {
                        response = AuthenticationResponse.AlreadyInUse;
                    }

                    writer.WriteLine(response.ToString().ToLower());

                    if(response != AuthenticationResponse.Welcome)
                    {
                        Log.Info($"overlay {response} for {userId} {userIp.ToString()}");
                        userId = null;
                        return;
                    }

                    userId = requesterUserId;
                    _authentifiedClients.Add(userId, client);
                    _clientSender.Register(userId, writer);

                    Log.Info($"Overlay is authentified for {userId} {userIp.ToString()}");

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        Log.Debug($"{userId} : {line}");
                    }
                }
                catch (Exception e)
                {
                    if (!_isRunning)
                        return;

                    Log.Error(e.Message);
                }
                finally
                {
                    _connectedClients.Remove(client);
                    
                    if(userId != null)
                    {
                        _authentifiedClients.Remove(userId);
                        _clientSender.Unregister(userId);
                        Log.Info($"{userId} was disconnected");
                    }
                }
            }
        }
    }
}