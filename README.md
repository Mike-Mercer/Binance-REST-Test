## This is the test of the diligently ignored by binance "Order does not exist" bug
### This test also measures binance REST latency

## The essence of the "Order does not exist" bug is that actually existing on the exchange order is reported as non-existing for too much time
# This timne is actually may be up to 10 minutes ! 

To run the test, fill the Config.txt with your API keys.
The app will execute simple but profitable strategy: it puts some buy orders on USDT markets below current price and waits for 10 minutes.

While waiting, it checks orders state using REST endpoint `Get order`.
If the order's state is unknown due to the bug, this fact will be logged with the order's creation time, ID and the time passed since creation.
### In another words, the time how long we was unable to know actual order's staus.

Also the log will contain timing taken by REST request to the binance API to excecute (milliseconds)

## Some results

```
9/15/2020 21:07:05: The actually existing order (id: 138283407, clientID: odn_HzxHQEIbUBK9YOoo) 
  created 18:06:43 
  was reported as does not exist since 21959 ms after creation!
```
