using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CarController.Hubs
{
    public partial class CentralHost:Hub
    {
        static Dictionary<string, Client> clients = new Dictionary<string, Client>();
        static object clientKey = new object();
        static Dictionary<string, Car> cars = new Dictionary<string, Car>();
        static object carKey = new object();
        static Dictionary<string, Manager> managers = new Dictionary<string, Manager>();
        static object managerKey = new object();

        public override Task OnConnected()
        {
            Clients.GetType();
            SentMessage(Context.ConnectionId + " is Connected");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (DisconnectClient(Context.ConnectionId)) { }
            else if (DisconnectManager(Context.ConnectionId)) { }
            else if (DisconnectCar(Context.ConnectionId)) { }
                
            return base.OnDisconnected(stopCalled);
        }
    }
}
