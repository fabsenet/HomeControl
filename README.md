# HomeControl
A client-server system to remotely collect information and to send 
commands to devices with the target to measure and control 
(some aspects of) a smart home.

# Message Patterns
`HomeControl` uses multiple messaging patterns and they are named and 
defined as follows:

## Request/Response
A `Request` is a message which is sent from a client to a server 
and the server answers with exactly one `Response` in return.

Sample is the request for the configuration data.

## Telemetry
`Telemetry` is a message which is sent from a client to a server. 
This is an unicast message without any response at all.

Samples are sensor data and the (client) heartbeat.

## Command/Result
A `command` is a message which is sent from the server to a single
client. The client processes it and answers with exactly one `Result` 
message.

An example is the command to enable an LED or to shutdown a PC 
(remotely).

## Notification
A `notification` is a message which is sent from the server to the 
client.

Example is the (server) heartbeat.