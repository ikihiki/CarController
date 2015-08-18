using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarController.Hubs
{
    /// <summary>
    /// 車を表すクラスです。
    /// </summary>
    public class Car
    {
        /// <summary>
        /// 車のIDを取得します。
        /// </summary>
        public string ConnectedID { get; }

        /// <summary>
        /// 車がクライアントと接続されているかどうかを取得または設定します。
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// 車に接続されているクライアントを取得または設定します。
        /// </summary>
        public string ConnectedClient { get; set; }

        /// <summary>
        /// 車のニックネームを取得または設定します。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Carクラスのインスタンスを作成します。
        /// </summary>
        /// <param name="id">車を表すID</param>
        /// <param name="name">車のニックネーム</param>
        public Car(string id,string name)
        {
            ConnectedID = id;
            Name = name;
        }
    }

    partial class CentralHost
    {
        /// <summary>
        /// 車として登録します。
        /// </summary>
        /// <param name="name">登録する車のニックネーム</param>
        public void ConnectAsCar(string name)
        {
            lock(carKey)
            {
                cars.Add(Context.ConnectionId, new Car(Context.ConnectionId, name));
                ChangeCars(cars.Select(kvp => kvp.Value).ToArray());
            }
            SentMessage(Context.ConnectionId + " is Connected as Car");
        }

        /// <summary>
        /// 車としての登録を解除します。
        /// </summary>
        /// <returns>解除が成功したかどうか</returns>
        public bool DisconnectCar()
        {
            return DisconnectCar(Context.ConnectionId);
        }

        /// <summary>
        /// 車としての登録を解除します。
        /// </summary>
        /// <param name="id">解除する車のID</param>
        /// <returns>解除が成功したかどうか</returns>
        public bool DisconnectCar(string id)
        {
            lock(carKey)
            {
                if(!cars.ContainsKey(id))
                {
                    return false;
                }

                if (cars[id].IsConnected)
                {
                    DisconnectFromCar(cars[id].ConnectedClient);
                }

                if (cars.Remove(id))
                {
                    ChangeCars(cars.Select(kvp => kvp.Value).ToArray());
                    SentMessage(id + " is Disconnected as Car");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// すべての車を取得します。
        /// </summary>
        /// <returns>すべての車</returns>
       　public Car[] GetCarList()
        {
            lock(carKey)
            {
                return cars.Select(kvp => kvp.Value).ToArray();
            }
        }

        /// <summary>
        /// 車の状態が変化したことを通知します。
        /// </summary>
        /// <param name="cars">変化後の車のリスト</param>
        private void ChangeCars(Car[] cars)
        {
            Clients.All.ChangeCars(cars);
        }

        /// <summary>
        /// 車にクライアントが接続したことを通知します。
        /// </summary>
        /// <param name="id">通知する車のID</param>
        /// <param name="client">接続したクライアントのID</param>
        private void SentConnectToClient(string id,string client)
        {
            Clients.Client(id).SentConnectToClient(client);
            SentMessage($"{id} is Connected by {client}");
        }

        /// <summary>
        /// 車にクライアントとの接続が解除されたことを通知します。
        /// </summary>
        /// <param name="id">通知する車のID</param>
        public void SentDisconnectFromClient(string id)
        {
            Clients.Client(id).SentDisconnectFromClient();
            SentMessage($"{id} is DisCconnected from client");
        }

        /// <summary>
        /// コマンドリストを送信します。
        /// </summary>
        /// <param name="id">送信先の車のID</param>
        /// <param name="commands">送信するコマンド</param>
        public void SentCommands(string id,Command[] commands)
        {
            Clients.Client(id).SentCommands(commands);
            SentCommandMessage(commands,Context.ConnectionId,id);
        }

        public void ShutdownCar(string id)
        {
            Clients.Client(id).Shutdown();
        }

        public void RebootCar(string id)
        {
            Clients.Client(id).Reboot();
        }
    }
}
