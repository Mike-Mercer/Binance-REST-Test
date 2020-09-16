using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace RestTest
{
    class Program
    {
        static MakeMoneyAlgo algo = new MakeMoneyAlgo();
        static bool Terminated = false;

        static void Main(string[] args)
        {
            // Establish an event handler to process CTRL-C
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

            ServicePointManager.DefaultConnectionLimit = 200;
            Console.WriteLine("DefaultConnectionLimit:" + ServicePointManager.DefaultConnectionLimit);

            BinanceClient bClient = new BinanceClient();
            bClient.UpdateExchangeInfo();
            if (!BinanceSpot.settings.KeysSet)
            {
                Logger.ConsoleOut("API keys are not set! Please set the keys in the Config.txt");
            }

            while (!Terminated)
            {
                Thread.Sleep(1000);
                bClient.UpdatePrices();
                if (BinanceSpot.CheckUsedWeight())
                    bClient.TestSequence();
                Console.Write("\r Working ... Orders: {0} UsedWeight: {1}" + "         ", algo.workersCount, BinanceSpot.used_weight);
                algo.checkState();
            }
            Console.WriteLine("\nWaiting workers....");
            algo.waitWorkers();
        }
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            Terminated = true;
            algo.Terminated = true;
        }
    }
}
