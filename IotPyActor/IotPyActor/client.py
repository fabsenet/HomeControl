import logging
import zmq

class Client(object):
    def __init__(self, server_uri):
        logging.info("The client is using the uri {} to connect to the server.".format(server_uri))

        hostname = "asd"

        ctx = zmq.Context()
        socket = ctx.socket(zmq.DEALER)
        socket.setsockopt_string(zmq.IDENTITY, self.getHostname())
        socket.connect(server_uri)
        msg = ConfigurationRequest(self.getHostname())
        #socket.send_string("hallo welt")
        socket.send_pyobj(msg)

        print(socket.recv_string())

    def getHostname(self):
        import platform
        hostname = platform.node()
        logging.info("the hostname of this host is '{}'".format(hostname))
        return hostname

class Message(object):
    __type ="Message"

class PingMessage(Message):
    __type = "PingMessage"

class ConfigurationRequest(Message):
    __type = "ConfigurationRequest"
    __hostname = None

    def __init__(self, hostname):
        __hostname = hostname