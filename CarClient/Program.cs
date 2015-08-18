﻿using System;
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
                var settings = new Settings() { CarName = "carName", HubName = "hubName", Url = "http://url" };
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
            try
            {
                using (var file = File.OpenText(SettingFileName))
                {
                    var setting = JsonConvert.DeserializeObject<Settings>(file.ReadToEnd());
                    connector = new HubConnector(setting.Url, setting.HubName, setting.CarName);
                }
                connector.ConectStart().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }

            CommandExecuter executer = new CommandExecuter();


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
                    executer.Excute(command, source.Token);
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