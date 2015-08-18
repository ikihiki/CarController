using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarController.Hubs
{
    /// <summary>
    /// マネージャーを表すクラスです。
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// マネージャーのIDを取得します。
        /// </summary>
        public string ConnectedID { get; }

        /// <summary>
        /// Managerクラスのインスタンスを作成します。
        /// </summary>
        /// <param name="id">マネージャーを表すID</param>
        public Manager(string id)
        {
            ConnectedID = id;
        }

    }

    partial class CentralHost
    {
        /// <summary>
        /// マネージャーとして登録します。
        /// </summary>
        /// <returns>登録が成功したかどうか</returns>
        public void ConnectAsManager()
        {
            lock (managerKey)
            {
                managers.Add(Context.ConnectionId, new Manager(Context.ConnectionId));
                ChangeManagers(managers.Select(kvp => kvp.Value).ToArray());
                SentMessage(Context.ConnectionId + " is Connected as Manager");
            }
        }

        /// <summary>
        /// マネージャーとしての登録を解除します。
        /// </summary>
        /// <returns>解除が成功したかどうか</returns>
        public bool DisconnectManager()
        {
            return DisconnectManager(Context.ConnectionId);
        }

        /// <summary>
        /// マネージャーとしての登録を解除します。
        /// </summary>
        /// <param name="id">解除するマネージャーのID</param>
        /// <returns>解除が成功したかどうか</returns>
        private bool DisconnectManager(string id)
        {
            lock(managerKey)
            {
                if(!managers.ContainsKey(id))
                {
                    return false;
                }
                if (managers.Remove(id))
                {
                    ChangeManagers(managers.Select(kvp => kvp.Value).ToArray());
                    SentMessage(id + " is Disconnected as Manager");
                    return true;
                }
            }
            return false;

        }

        /// <summary>
        /// すべてのマネージャーを取得します。
        /// </summary>
        /// <returns>すべてのマネージャー</returns>
        public Manager[] GetManagerList()
        {
            lock(managerKey)
            {
                return managers.Select(kvp => kvp.Value).ToArray();
            }
        }

        /// <summary>
        /// マネージャーが変化したことを伝えます。
        /// </summary>
        /// <param name="managers">変化後のマネージャーリスト</param>
        private void ChangeManagers(Manager[] managers)
        {
            Clients.All.ChangeManagers(managers);
        }

        /// <summary>
        /// マネージャーすべてにメッセージを送信します。
        /// </summary>
        /// <param name="message">送信するメッセージ</param>
        private void SentMessage(string message)
        {
            var date = DateTime.Now.ToString() ;
            
            lock(managerKey)
            {
                foreach (var manager in managers)
                {
                    Clients.Client(manager.Key).Sent($"[{date}] {message}");
                }
            }
        }

        /// <summary>
        /// マネージャーすべてにメッセージを送信します。
        /// </summary>
        /// <param name="message">送信するメッセージ</param>
        public void SentMessageFromClient(string message)
        {
            SentMessage($"{Context.ConnectionId} is sent message \"{message}\"");
        }


        /// <summary>
        /// マネージャーすべてにコマンドリストを送信します。
        /// </summary>
        /// <param name="commands">送信するコマンド</param>
        /// <param name="client">送信元のクライアント</param>
        /// <param name="car">送信先の車</param>
        public void SentCommandMessage(Command[] commands,string client,string car)
        {
            var date = DateTime.Now.ToString();
            lock(managerKey)
            {
                foreach (var manager in managers)
                {
                    Clients.Client(manager.Key).SentCommand(commands,client,car,date);
                }
            }
        }

    }
}
