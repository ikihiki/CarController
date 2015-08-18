using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client;

namespace CarClient
{
    public class HubConnector
    {
        HubConnection connection;
        IHubProxy proxy;
        string carName;

        public bool IsConnecting { get; private set; } = false;
        public string ConnectedClient { get; private set; } = "";

        public event Action<string> ConnectingToClient;
        public event Action DisconnectingFromClient;
        public event Action<Command[]> GetCommand;
        public event Action Shutdown;
        public event Action Reboot;

        public HubConnector(string url, string hubName, string carName)
        {
            connection = new HubConnection(url);
            proxy = connection.CreateHubProxy(hubName);
            this.carName = carName;

            proxy.On<string>("SentConnectToClient", client =>
            {
                IsConnecting = true;
                ConnectedClient = client;
                ConnectingToClient?.Invoke(client);
            });

            proxy.On("SentDisconnectFromClient", () =>
             {
                 IsConnecting = false;
                 ConnectedClient = "";
                 DisconnectingFromClient?.Invoke();
             });

            proxy.On<Command[]>("SentCommands", commands =>
            {
                if (IsConnecting)
                {
                    GetCommand?.Invoke(commands);
                }
            });

            proxy.On("Shutdown", () =>
            {
                Shutdown?.Invoke();
            });

            proxy.On("Reboot", () =>
            {
                Reboot?.Invoke();
            });
        }

        public async Task ConectStart()
        {
            await connection.Start();
            await proxy.Invoke("ConnectAsCar", carName);
        }

        public async Task ConectStop()
        {
            if (!await proxy.Invoke<bool>("DisconnectCar"))
            {
                Console.WriteLine("Can't stop normaly");
            }
            connection.Stop();
        }

        public async Task SentMessage(string message)
        {
            await proxy.Invoke("SentMessageFromClient", message);
        }
    }
}
