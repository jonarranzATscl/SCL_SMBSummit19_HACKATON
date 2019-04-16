# SMBSummit19_HACKATON
SCL Consulting - SAP SMB Summit 2019 - Hackaton Winner!

## Solution Brief

The present solution was developed for the SAP SMB Summit 2019 Hackaton - Nice ([Hackaton Homepage](https://github.com/B1SA/hackathon)) . It was totally developed in the slotted time, on site, and it won the competition!

We tried to provide a trusted "Cold chain" monitor for our goods; using a CC2650STK sensor to send temperature readings to Leonardo (not really, more into that later) and store those readings in a blockchain. We also provided an alarm system: when temperature went down from a certain threshold, it sent a message to a B1 backend and a particular user at Telegram. Using telegram, one could ask for the last sensor readout in realtime, thus, providing chatbox integration.

## Technologies used

### Hackaton "Building Blocks"

* [B1Backend](https://github.com/B1SA/hackathon/tree/master/B1Backend)
* [Blockchain](https://github.com/B1SA/hackathon/tree/master/Blockchain)
* [CloudFoundry](https://github.com/B1SA/hackathon/tree/master/CloudFoundry)
* [IoTLeonardo](https://github.com/B1SA/hackathon/tree/master/IoTLeonardo)

### Other Tools

* [Node-RED](https://nodered.org/) - Flow based programming for the Internet of Things
* [Node-Mosquitto](https://mosquitto.org/) - Open source MQTT broker
* [Amazon Web Services](https://aws.amazon.com) - For running the mosquitto broker
* [Amazon Elastic Beanstalk](https://aws.amazon.com/elasticbeanstalk) - For hosting node-red


### TEST IMAGE
![MQTT FLOW](/Screenshots/Flow_01_MQTT.JPG?raw=true "MQTT FLOW")
