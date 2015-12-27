import logging
import sys

from IotPyActor.client import Client

__version__ = "0.1"


def main():
    args = sys.argv[1:]
    logging.basicConfig(level=logging.DEBUG)
    logging.info("calling arguments are {}".format(args))
    Client(args[0])
    return 0


if __name__ == '__main__':
    sys.exit(main())
