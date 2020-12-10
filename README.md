## This is the test of the "Order does not exist" bug
### This test also measures binance REST latency

## The essence of the "Order does not exist" bug is that actually existing on the exchange order is reported as non-existing for too much time

# This time is actually may be up to 10 minutes ! 

To run the test, fill the Config.txt with your API keys.
The app will execute simple but profitable strategy: it puts some buy orders on USDT markets below current price and waits for 10 minutes.

While waiting, it checks orders state using REST endpoint `Get order`.
If the order's state is unknown due to the bug, this fact will be logged with the order's creation time, ID and the time passed since creation.
### In other words, the time how long we were unable to know the actual order's status.

Also the log will contain timing taken by REST request to the binance API to execute (milliseconds)

## Some results
#### Tokyo AWS

```
9/15/2020 21:07:05: The actually existing order (id: 138283407, clientID: odn_HzxHQEIbUBK9YOoo) 
  created 18:06:43 
  was reported as does not exist since 21959 ms after creation!
```
```
16-Sep-20 00:09:43: The actually existing order (Market:CHZUSDT id: 33380548, clientID: odn_Xun2O0QuQuxiUXVw)             
  created 00:08:41                                                                                                        
  was reported as does not exist since 61918 ms after creation! 
```
```
16-Sep-20 04:48:01: Request order?symbol=FTMUSDT&orderId=47125437&recvWindow=6000&timestamp=1600231621212 
took 60012 ms to excecute !
```

### Real-time statistics from production trading bots https://api.moon-bot.com/latency/

### Updated 10.12.2020

We've created a ticket containing a detailed description of this issue to Binance, the answer was published here: https://dev.binance.vision/t/faq-error-message-order-does-not-exist/46
In essence the issue boils down to an excessive amount of records in the Orders DB.

This topic is closed on dev.binance.vision and thus we're addressing the responsible Binance representatives here: Please review alternative options of temporary orders  storage, e.g. an additional orders in-memory DB keeping 1 hour of history; generally, orders older than 1 hour are available in the main DB.
The issue described above is related only to recently placed orders.

Please mind the fact that the issue presented above, regularly leads to financial losses incurred by API users whose primary trading technique is scalping

