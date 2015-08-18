using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarController.Hubs
{
    /// <summary>
    /// クライアントを表すクラスです。
    /// </summary>
    public class Client
    {
        /// <summary>
        /// クライアントのIDを取得します。
        /// </summary>
        public string ConnectedID { get; }

        /// <summary>
        /// クライアントが車と接続されているかどうかを取得または設定します。
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// クライアントに接続されている車を取得または設定します。
        /// </summary>
        public string ConnectedCar { get; set; }

        /// <summary>
        /// Clientクラスのインスタンスを作成します。
        /// </summary>
        /// <param name="id">クライアントを表すID</param>
        public Client(string id)
        {
            ConnectedID = id;
        }
    }

    partial class CentralHost
    {
        /// <summary>
        /// クライアントとして登録します。
        /// </summary>
        /// <returns>登録が成功したかどうか</returns>
        public void ConectAsClient()
        {
            lock (clientKey)
            {
                clients.Add(Context.ConnectionId, new Client(Context.ConnectionId));
                ChangeClients(clients.Select(kvp => kvp.Value).ToArray());
                SentMessage(Context.ConnectionId + " is Connected as Client");
            }
        }

        /// <summary>
        /// クライアントとしての登録を解除します。
        /// </summary>
        /// <returns>解除が成功したかどうか</returns>
        public bool DisconnectClient()
        {
            return DisconnectClient(Context.ConnectionId);
        }


        /// <summary>
        /// クライアントとしての登録を解除します。
        /// </summary>
        /// <param name="id">解除するクライアントのID</param>
        /// <returns>解除が成功したかどうか</returns>
        private bool DisconnectClient(string id)
        {
            Client client;
            lock (clientKey)
            {
                if(!clients.ContainsKey(id))
                {
                    return false;
                }
                client = clients[id];
            }
            if (client.IsConnected)
            {
                DisconnectFromCar(id);
            }

            lock (clientKey)
            {
                if (clients.Remove(id))
                {
                    ChangeClients(clients.Select(kvp => kvp.Value).ToArray());
                    SentMessage(id + " is Disconnected as client");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// すべてのクライアントを取得します。
        /// </summary>
        /// <returns>すべてのクライアント</returns>
        public Client[] GetClientList()
        {
            lock (clientKey)
            {
                return clients.Select(kvp => kvp.Value).ToArray();
            }
        }

        /// <summary>
        /// クライアントの状態が変化したことを通知します。
        /// </summary>
        /// <param name="clients">変化後のクライアントのリスト</param>
        private void ChangeClients(Client[] clients)
        {
            Clients.All.ChangeClients(clients);
        }


        /// <summary>
        /// 車と接続します。
        /// </summary>
        /// <param name="id">接続する車のID</param>
        /// <returns>接続が成功したかどうか</returns>
        public bool ConectToCar(string id)
        {
            lock(carKey)
            {
                if (!cars.ContainsKey(id))
                {
                    return false;
                }
                if (cars[id].IsConnected)
                {
                    return false;
                }

                lock(clientKey)
                {
                    if (clients[Context.ConnectionId].IsConnected)
                    {
                        DisconnectFromCar(Context.ConnectionId);
                    }
                    clients[Context.ConnectionId].IsConnected = true;
                    clients[Context.ConnectionId].ConnectedCar = id;
                    cars[id].IsConnected = true;
                    cars[id].ConnectedClient = Context.ConnectionId;
                    ChangeClients(clients.Select(kvp => kvp.Value).ToArray());
                    ChangeCars(cars.Select(kvp => kvp.Value).ToArray());
                }
            }
            SentMessage($"{Context.ConnectionId} Conect to {id}");
            SentConnectToClient(id, Context.ConnectionId);
            return true;
        }

        /// <summary>
        /// 車との接続を切断します。
        /// </summary>
        public void DisconnectFromCar()
        {
            DisconnectFromCar(Context.ConnectionId);
            
        }

        /// <summary>
        /// 車との接続を切断します。
        /// </summary>
        /// <param name="connectionId">切断するクライアントのID</param>
        public void DisconnectFromCar(string connectionId)
        {
            lock(carKey)
            {
                lock(clientKey)
                {
                    var car = cars[clients[connectionId].ConnectedCar];
                    car.ConnectedClient = string.Empty;
                    car.IsConnected = false;
                    SentDisconnectFromClient(clients[connectionId].ConnectedCar);
                    clients[connectionId].ConnectedCar = string.Empty;
                    clients[connectionId].IsConnected = false;
                    ChangeClients(clients.Select(kvp => kvp.Value).ToArray());
                    ChangeCars(cars.Select(kvp => kvp.Value).ToArray());
                    SentDisconnectFromCar(connectionId);
                }
            }
        }

        /// <summary>
        /// 車との接続が切断されたことを通知します。
        /// </summary>
        /// <param name="id">通知するクライアントのID</param>
        private void SentDisconnectFromCar(string id)
        {
            Clients.Client(id).SentDisconnectFromCar();
        }

        /// <summary>
        /// コマンドを車に送信します。
        /// </summary>
        /// <param name="commands">送信するコマンド</param>
        /// <returns>送信できたかどうか</returns>
        public bool SentCommandsToCar(Command[] commands)
        {
            string id;
            lock(clientKey)
            {
                if(!clients[Context.ConnectionId].IsConnected)
                {
                    return false;
                }
                id = clients[Context.ConnectionId].ConnectedCar;
            }
            SentCommands(id, commands);
            return true;
        }
    }
}
