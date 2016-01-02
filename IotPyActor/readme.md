# IotPyActor

The python version of the IoT (=Internet of Things) actor for the home control project. 
It is meant to run on a Raspberry Pi 2 (or it quite happily simulates it)

## Dependencies
### [gpiocrust](https://github.com/zourtney/gpiocrust)
gpiocrust is used to control the GPIO ports on the raspberry pi. It suppurts running as a mock if
there is no real GPIO available. This allows test/development on non-PI machines.

```
pip install gpiocrust
```

### [pyzmq](https://github.com/zeromq/pyzmq)
PyZMQ is the python binding for zeroMQ, which in turn is the awesome messaging protocol used in HomeControl.

```
pip install pyzmq
```