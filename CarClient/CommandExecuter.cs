
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CarClient
{
    public class CommandExecuter:IDisposable
    {
        Stream survoStream;
        StreamWriter survoWriter;

        public CommandExecuter()
        {

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
            //survoWriter.Dispose();
            //survoStream.Dispose();
        }

        private void Fowerd()
        {
            Console.WriteLine("Fowerd");
        }

        private void Back()
        {
            Console.WriteLine("Back");
        }

        private void Stop()
        {
            Console.WriteLine("Stop");
        }

        private void Right()
        {
            Console.WriteLine("Right");
        }

        private void Left()
        {
            Console.WriteLine("Left");
        }

        private void Straight()
        {
            Console.WriteLine("Straight");
        }
    }
}
