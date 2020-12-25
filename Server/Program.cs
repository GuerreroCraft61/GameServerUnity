using System;
using System.Threading;

namespace Server {
    internal class Program {
        private static bool isRunning;

        private static void Main(string[] args) {
            Console.Title = "Game Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(50, 26950);
        }

        private static void MainThread() {
            Console.WriteLine($"Main thread started. Running ad {Constants.TICKS_PER_SEC} ticks per second.");
            var nextLoop = DateTime.Now;
            while (isRunning) {
                while (nextLoop < DateTime.Now) {
                    GameLogic.Update();
                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (nextLoop > DateTime.Now) {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}