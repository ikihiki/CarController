
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Raspberry.IO.GeneralPurpose;
using System.Diagnostics;

namespace CarClient
{
    public class CommandExecuter:IDisposable
    {
        ProcessStartInfo servo;
        GpioConnection connection;
        OutputPinConfiguration pin1, pin2;
        Settings setting;

        public CommandExecuter(Settings setting)
        {
            this.setting = setting;

            servo = new ProcessStartInfo("/bin/bash");
            servo.UseShellExecute = false;
            servo.CreateNoWindow = true;
            servo.RedirectStandardOutput = true;
            servo.RedirectStandardError = true;
         
#if !DEBUG
            pin1 = ConnectorPin.P1Pin38.Output();
            pin2 = ConnectorPin.P1Pin40.Output();
            connection = new GpioConnection(pin1, pin2);
#endif
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
#if !DEBUG
            connection.Close();
#endif
        }

        private void Fowerd()
        {
            Console.WriteLine("Fowerd");
#if !DEBUG
            connection[pin1] = true;
            connection[pin2] = false;
#endif
        }

        private void Back()
        {
            Console.WriteLine("Back");
#if !DEBUG
            connection[pin1] = false;
            connection[pin2] = true;
#endif
        }

        private void Stop()
        {
            Console.WriteLine("Stop");
#if !DEBUG
            connection[pin1] = false;
            connection[pin2] = false;
#endif
        }

        private void Right()
        {
            Console.WriteLine("Right");
#if !DEBUG
            servo.Arguments = $"-c 'echo {setting.ServoPin}={setting.ServoRight} > /dev/servoblaster'";
            Console.WriteLine(servo.Arguments);
            var proc = Process.Start(servo);
            proc.WaitForExit();
            Console.WriteLine(proc.StandardOutput.ReadToEnd());
            Console.WriteLine(proc.StandardError.ReadToEnd());

            proc.Close();
#endif
        }

        private void Left()
        {
            Console.WriteLine("Left");
#if !DEBUG
            servo.Arguments = $"-c 'echo {setting.ServoPin}={setting.ServoLeft} > /dev/servoblaster'";
            Console.WriteLine(servo.Arguments);
            var proc = Process.Start(servo);
            proc.WaitForExit();
            Console.WriteLine(proc.StandardOutput.ReadToEnd());
            Console.WriteLine(proc.StandardError.ReadToEnd());

            proc.Close();
#endif
        }

        private void Straight()
        {
            Console.WriteLine("Straight");
#if !DEBUG
            servo.Arguments = $"-c 'echo {setting.ServoPin}={setting.ServoStraight} > /dev/servoblaster'";
            Console.WriteLine(servo.Arguments);
            var proc = Process.Start(servo);
            
            proc.WaitForExit();
            Console.WriteLine(proc.StandardOutput.ReadToEnd());
            Console.WriteLine(proc.StandardError.ReadToEnd());
            proc.Close();
#endif
        }
    }
}
