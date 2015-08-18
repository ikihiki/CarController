using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace CarClient
{
    class Program
    {
        const string SettingFileName = "settings.json";

        static void Main(string[] args)
        {
            if (!File.Exists(SettingFileName))
            {
                var settings = new Settings() { CarName = "carName", HubName = "hubName", Url = "http://url", ServoPin = "7", ServoLeft = "0", ServoRight = "100", ServoStraight = "50" };
                using (var file = File.Create(SettingFileName))
                {
                    using (var writer = new StreamWriter(file))
                    {
                        writer.Write(JsonConvert.SerializeObject(settings));
                        writer.Flush();
                    }
                }
                return;
            }


            HubConnector connector;
            CommandExecuter executer;
            try
            {
                using (var file = File.OpenText(SettingFileName))
                {
                    var setting = JsonConvert.DeserializeObject<Settings>(file.ReadToEnd());
                    connector = new HubConnector(setting.Url, setting.HubName, setting.CarName);
                    executer = new CommandExecuter(setting);
                }
                connector.ConectStart().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }

            try
            {

                string ipaddress = "";
                IPHostEntry ipentry = Dns.GetHostEntry(Dns.GetHostName());

                foreach (IPAddress ip in ipentry.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipaddress = ip.ToString();
                        break;
                    }
                }

                connector.SentMessage(ipaddress);
                System.Threading.CancellationTokenSource source = new System.Threading.CancellationTokenSource();
                connector.GetCommand += command =>
                {
                    source?.Cancel();
                    source = new System.Threading.CancellationTokenSource();
                    connector.StartExcuting();
                    executer.Excute(command, source.Token).ContinueWith(t => connector.EndExcuting()).Start();
                };

                connector.AbortExcuting += () =>
                {
                    if(!source.IsCancellationRequested)
                    {
                        source.Cancel();
                    }
                };

                connector.Shutdown += () =>
                {
                    Console.WriteLine("Shutdown");
                };

                connector.Reboot += () =>
                {
                    Console.WriteLine("Reboot");
                };

                Console.Read();
            }
            catch (Exception ex)
            {
                connector.SentMessage(ex.ToString());
            }
            finally
            {
                connector.ConectStop().Wait();
                executer?.Dispose();
            }

        }
    }
}
