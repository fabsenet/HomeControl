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

## Auto-Start on Startup

The intended way on running this is headless on a RaspberryPI2. Therefor it should start itself on startup. The following steps are neccessary to archieve exactly that.

Clone the git repository into a local folder:
```
git clone https://github.com/fabsenet/HomeControl.git ~/HomeControl
```

Edit the rc.local file:
```
sudo nano /etc/rc.local
```
Add the following lines at the end of the file just above `exit 0` to first give the (wireless) network some time to establish a connection and then force an update of the local git repo and finally start the IotPyActor:
```
git -C /home/pi/HomeControl/ fetch
git -C /home/pi/HomeControl/ reset --hard origin/master
python3 /home/pi/HomeControl/IotPyActor/__init__.py tcp://your-servername-or-ip:5556 &
```
