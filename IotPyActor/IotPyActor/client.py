import datetime
import logging
import zmq
import json
from gpiocrust import PWMOutputPin,InputPin,OutputPin, Header
from subprocess import call

class Client(object):
    ctx = None
    socket = None
    hasConfiguration = False

    def __init__(self, server_uri):

        with Header() as header:
            self.pins = {}
            logging.info("The client is using the uri {} to connect to the server.".format(server_uri))

            # Setup ZeroMQ
            self.ctx = zmq.Context()
            self.socket = self.ctx.socket(zmq.DEALER)
            self.socket.connect(server_uri)

            # before we can handle any other messages, we need to get the configuration of this client done
            while not self.hasConfiguration:
                 self.send_configuration_request()
                 while not self.hasConfiguration and self.socket.poll(timeout=15000):
                    msgType = self.socket.recv_string(zmq.RCVMORE)
                    msg = self.socket.recv_json()
                    # ignore any other message
                    # TODO: maybe queue them and handle them after the configuration is done?
                    if msgType != "ConfigurationResponse":
                        logging.warning("Ignoring message of type '{}' while waiting for a ConfigurationResponse".format(msgType))
                        continue
                    self.handle_configuration_response(msg)

            handlers = {
                "ConfigurationResponse": lambda msg: logging.info("Ignored another ConfigurationResponse"),
                "LedOnOffSetStateCommand": lambda msg: self.handle_set_pin_value(msg),
                "TransitionPowerStateCommand": lambda msg: self.handle_transition_power_state_command(msg),
            }

            # this is the main messaging loop which should never exit
            while True:
                while self.socket.poll(timeout=15000):
                    msgType = self.socket.recv_string(zmq.RCVMORE)
                    msg = self.socket.recv_json()
                    handler = handlers.get(msgType, lambda msg: self.handle_unknown_message(msgType, msg))
                    handler(msg)

            logging.info("closing socket")
            self.socket.close(1000)
            logging.info("destroying context")
            self.ctx.destroy(1000)
            logging.info("done, bye")

    def handle_configuration_response(self, msg):
        logging.info("Received a ConfigurationResponse msg. Complete msg: {}".format(msg))
        if msg["GpioPins"] is not None:
            self.setup_gpio_pins(msg["GpioPins"])
        self.hasConfiguration = True

    def handle_set_pin_value(self, msg):
        logging.info("Received a SetPin msg. Complete msg: {}".format(msg))
        pinNumber = msg["PinNumber"]
        value = msg["Value"]
        self.pins[pinNumber].value = value

    def handle_transition_power_state_command(self, msg):
        logging.info("received a command to change the power state. msg: {}".format(msg))
        desired_state = msg["DesiredPowerState"];
        if desired_state == "Restart":
            logging.info('calling call(["shutdown", "-r now"])')
            #call(["shutdown", "-r now"])
        elif desired_state == "Shutdown":
            logging.info('calling call(["shutdown", "-h now"])')
            #call(["shutdown", "-h now"])
        else:
            logging.error("the desired power state '{}' is not supported")

    def handle_unknown_message(self,msgType, msg):
        logging.error("Received an unexpected msg type '{}'. Complete msg: {}".format(msgType, msg))

    def get_hostname(self):
        # https://stackoverflow.com/questions/4271740/how-can-i-use-python-to-get-the-system-hostname
        import platform
        hostname = platform.node()
        logging.info("the hostname of this host is '{}'".format(hostname))
        return hostname

    def send_configuration_request(self):
        logging.info("Sending a ConfigurationRequest")
        self.socket.send_string("ConfigurationRequest", zmq.SNDMORE)
        self.socket.send_json({
            "Hostname": self.get_hostname(),
            "CreateDate": datetime.datetime.now().isoformat()
        })

    def setup_gpio_pins(self, gpiopins):

        for gpioPin in gpiopins:
            typ = gpioPin["Type"]
            pin = None;
            pinNumber = gpioPin["PinNumber"]
            if(typ == "PwmOut"):
                pin = PWMOutputPin(pinNumber , gpioPin.get("Frequency", 200) , gpioPin.get("InitialValue", 0))

            self.pins[pinNumber] = pin
        logging.info("SetupGpioPins done. Pins are {}".format(self.pins))
