using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



public static class Logger
{
    public static long maxElapsedTime = 100; // log requests with timing higher then maxElapsedTime (milliseconds)
    public static long maxODN_time = 2000; // time while actually existing order was reported as does not exit 

    private static object locky = new object();
    private static object PingChecklock = new object();

    static Logger()
    {
        //            File.Create("Log.txt").Close(); // clear log on startup
    }
    public static void Log(string message)
    {
        string msg = DateTime.UtcNow.ToLongTimeString() + " " + message;
        lock (locky)
        {
            using (StreamWriter streamWriter = new StreamWriter("Log.txt", true, System.Text.Encoding.Default))
            {
                streamWriter.WriteLine(message);
            }

        }
    }

    public static void ConsoleOut(string message)
    {
        message = DateTime.Now.ToString() + ": " + message;
        Console.WriteLine("\n" + message);
        Log(message);

    }

    public static void CheckRequestTiming(long ElapsedTime, string Request)
    {
        if (ElapsedTime > maxElapsedTime + 10)
        {
            maxElapsedTime = ElapsedTime;
            ConsoleOut($"Request {Request} took {ElapsedTime} ms to excecute !");

        }

    }

    public static void ReportODN(long ODN_Time, string orderData)
    {
        if (ODN_Time > maxODN_time) 
        {
            maxODN_time = ODN_Time;
            ConsoleOut(orderData);
        }
    }



}

