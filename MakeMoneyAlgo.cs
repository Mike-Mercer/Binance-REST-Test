using System;
using System.Threading;
using System.Collections.Generic;


public class MakeMoneyAlgo
{
	public bool Terminated = false;
	public int workersCount = 0;

	public MakeMoneyAlgo()
	{
	}

	public void checkState() 
	{	
		if ((workersCount == 0) & BinanceSpot.settings.KeysSet & BinanceSpot.CheckUsedWeight()) { newIteration(); }
	}

	public void waitWorkers()
	{
		while (workersCount > 0) { Thread.Sleep(500); }
	}

	private void newIteration()
	{
		for (int i = 0; i < BinanceSpot.settings.OrdersCount; i++)
		{
			Thread thread = new Thread(() => Run());
			thread.Start();
		}
	}

	private void DoSleep(int sleepTime) 
	{
		int actualSleepTime = 0;
		while ((!Terminated) & (actualSleepTime < sleepTime))
		{
			actualSleepTime += 500;
			Thread.Sleep(500);
		}
	}

	private void Run()
	{
		Random rnd = new Random();
		Thread.Sleep(rnd.Next(100, 1500));  // dont push all orders at once

		BinanceClient bClient = new BinanceClient();
		Spot.BinanceOrder order = bClient.PlaceMinBuyOrder(BinanceSpot.RandomSymbol());
		if (order == null) { return; }	

		Interlocked.Increment(ref workersCount);

		for (int i = 0; i < 50; i++)
		{
			if (!Terminated)
			{
				DoSleep(6000);
				bClient.CheckOrder(order);
			}
		}

		bClient.CancelOrder(order);
		Thread.Sleep(500);

		Interlocked.Decrement(ref workersCount);
	}
}


