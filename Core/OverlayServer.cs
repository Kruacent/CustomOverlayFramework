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
    public class OverlayServer
    {
        private readonly Config _config;
        private AuthentificationHandler _authentificationHandler;
        private TcpListener _listener;
        private volatile bool _isRunning = false;
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();
        private readonly Dictionary<string, TcpClient> _authentifiedClients = new Dictionary<string, TcpClient>();

        public OverlayServer(Config config)
        {
            _config = config;
            _authentificationHandler = new AuthentificationHandler(config);
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
            _authentificationHandler = null;
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

                string id = string.Empty;
                try
                {
                    NetworkStream stream = client.GetStream();
                    StreamReader reader = new StreamReader(stream, new UTF8Encoding(false));
                    StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

                    IPAddress ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                    string line = await reader.ReadLineAsync();
                    AuthenticationResponse response = _authentificationHandler.Authenticate(line, ip, out string userId);

                    if (_authentifiedClients.ContainsKey(userId))
                    {
                        response = AuthenticationResponse.AlreadyInUse;
                    }

                    writer.WriteLine(response.ToString().ToLower());

                    if(response != AuthenticationResponse.Welcome)
                    {
                        Log.Info($"overlay {response} for {userId} {ip.ToString()}");
                        return;
                    }

                    _authentifiedClients.Add(userId, client);
                    id = userId;
                    Log.Info($"Overlay is authentified for {userId} {ip.ToString()}");

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
                    _authentifiedClients.Remove(id);
                    _connectedClients.Remove(client);
                    Log.Info("A client was disconnected");
                }
            }
        }
    }
}