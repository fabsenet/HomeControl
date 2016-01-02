import datetime
import logging
import zmq
import json
from gpiocrust import PWMOutputPin,InputPin,OutputPin, Header

class Client(object):
    ctx = None
    socket = None

    def __init__(self, server_uri):

        with Header() as header:
            self.pins = {}
            logging.info("The client is using the uri {} to connect to the server.".format(server_uri))

            self.ctx = zmq.Context()

            self.socket = self.ctx.socket(zmq.DEALER)

            self.socket.setsockopt_string(zmq.IDENTITY, self.get_hostname())
            self.socket.connect(server_uri)

            self.sendConfigurationRequest()

            handlers = {
                "ConfigurationResponse": lambda msg: self.handle_configuration_response(msg),
                "SetPinValue": lambda msg: self.handle_set_pin_value(msg),
                "Unknown": lambda msg: self.handle_unknown_message(msg),
            }

            while self.socket.poll(timeout=5000):
                msgType = self.socket.recv_string(zmq.RCVMORE)
                msg = self.socket.recv_json()
                handler = handlers.get(msgType, handlers["Unknown"])
                handler(msg)

            print("done")

    def handle_configuration_response(self, msg):
        logging.info("Received a ConfigurationResponse msg. Complete msg: {}".format(msg))
        if msg["GpioPins"] is not None:
            self.SetupGpioPins(msg["GpioPins"])

    def handle_set_pin_value(self, msg):
        logging.info("Received a SetPin msg. Complete msg: {}".format(msg))
        pinNumber = msg["PinNumber"]
        value = msg["Value"]
        self.pins[pinNumber].value = value

    def handle_unknown_message(self, msg):
        logging.error("Received an unexpected msg type. Complete msg: {}".format(msg))

    def get_hostname(self):
        # https://stackoverflow.com/questions/4271740/how-can-i-use-python-to-get-the-system-hostname
        import platform
        hostname = platform.node()
        logging.info("the hostname of this host is '{}'".format(hostname))
        return hostname

    def sendConfigurationRequest(self):
        self.socket.send_string("ConfigurationRequest", zmq.SNDMORE)
        self.socket.send_json({
            "Hostname":self.get_hostname(),
            "CreateDate": datetime.datetime.now().isoformat()
        })

    def SetupGpioPins(self, gpiopins):

        for gpioPin in gpiopins:
            typ = gpioPin["Type"]
            pin = None;
            pinNumber = gpioPin["PinNumber"]
            if(typ == "PwmOut"):
                pin = PWMOutputPin(pinNumber , gpioPin.get("Frequency", 200) , gpioPin.get("InitialValue", 0))

            self.pins[pinNumber] = pin
        logging.info("SetupGpioPins done. Pins are {}".format(self.pins))
