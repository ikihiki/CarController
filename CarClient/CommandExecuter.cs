
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Raspberry.IO.GeneralPurpose;

namespace CarClient
{
    public class CommandExecuter:IDisposable
    {
        StreamWriter survoWriter;
        GpioConnection connection;
        OutputPinConfiguration pin1, pin2;
        Settings setting;

        public CommandExecuter(Settings setting)
        {
            this.setting = setting;
            pin1 = ConnectorPin.P1Pin38.Output();
            pin2 = ConnectorPin.P1Pin40.Output();
            connection = new GpioConnection(pin1, pin2);
            survoWriter = new StreamWriter("/dev/servoblaster");
            survoWriter.AutoFlush = true;
        }

        public async Task Excute(Command[] commands,System.Threading.CancellationToken token)
        {
            try {
                foreach (var command in commands)
                {
                    token.ThrowIfCancellationRequested();
                    switch(command.Verb)
                    {
                        case Verb.Back:
                            Back();
                            Straight();
                            break;
                        case Verb.BackLeft:
                            Back();
                            Left();
                            break;
                        case Verb.BackRight:
                            Back();
                            Right();
                            break;
                        case Verb.Fowerd:
                            Fowerd();
                            Straight();
                            break;
                        case Verb.FowerdLeft:
                            Fowerd();
                            Left();
                            break;
                        case Verb.FowerdRight:
                            Fowerd();
                            Right();
                            break;
                        case Verb.Stop:
                            Stop();
                            Straight();
                            break;
                    }
                    await Task.Delay(command.Time * 1000, token);
                }
            }catch(OperationCanceledException)
            {
                Console.WriteLine("Cancel");
            }
            finally
            {
                Stop();
                Straight();
            }
        }

        public void Dispose()
        {
            survoWriter.Dispose();
            connection.Close();
        }

        private void Fowerd()
        {
            Console.WriteLine("Fowerd");
            connection[pin1] = true;
            connection[pin2] = false;
        }

        private void Back()
        {
            Console.WriteLine("Back");
            connection[pin1] = false;
            connection[pin2] = true;
        }

        private void Stop()
        {
            Console.WriteLine("Stop");
            connection[pin1] = false;
            connection[pin2] = false;
        }

        private void Right()
        {
            Console.WriteLine("Right");
            survoWriter.WriteLine($"{setting.ServoPin}={setting.ServoRight}");
        }

        private void Left()
        {
            Console.WriteLine("Left");
            survoWriter.WriteLine($"{setting.ServoPin}={setting.ServoLeft}");
        }

        private void Straight()
        {
            Console.WriteLine("Straight");
            survoWriter.WriteLine($"{setting.ServoPin}={setting.ServoStraight}");
        }
    }
}
