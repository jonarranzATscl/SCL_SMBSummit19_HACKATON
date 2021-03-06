# SCL_SMBSummit19_HACKATON
[SCL Consulting](https://www.scl-consulting.com/) - SAP SMB Summit 2019 - Hackaton Winner!

## Important Update! (2019/10/25)

Darius Heydarian @ SAP made an awesome step by step video implementing this solution. Thank you for your hard work!!

[![Watch the video](https://img.youtube.com/vi/MqdmvYBVAmg/hqdefault.jpg)](https://youtu.be/MqdmvYBVAmg)

(Click on the image for YouTube video link)

## Solution Brief

The present solution was developed for the SAP SMB Summit 2019 Hackaton - Nice ([Hackaton Homepage](https://github.com/B1SA/hackathon)) . It was totally developed in the alloted time, on site, and it won the competition!

We tried to provide a trusted "Cold chain" monitor for our goods; using a CC2650STK sensor to send temperature readings to Leonardo (not really, we'll dive into it later) and store those readings in a blockchain. Also, we provided an alarm system: when temperature went up to a certain threshold, it sent a message to a B1 backend and a particular user at Telegram. Using telegram, one could ask for the last sensor readout in realtime, thus, providing chatbox integration.

## Technologies used

### Hackaton "Building Blocks"

* [B1Backend](https://github.com/B1SA/hackathon/tree/master/B1Backend)
* [Blockchain](https://github.com/B1SA/hackathon/tree/master/Blockchain)
* [CloudFoundry](https://github.com/B1SA/hackathon/tree/master/CloudFoundry)
* [IoTLeonardo](https://github.com/B1SA/hackathon/tree/master/IoTLeonardo)

### Other Tools

* [Node-RED](https://nodered.org/) - Flow based programming for the Internet of Things
* [Mosquitto](https://mosquitto.org/) - Open source MQTT broker
* [Amazon Web Services](https://aws.amazon.com) - For running the mosquitto broker
* [Amazon Elastic Beanstalk](https://aws.amazon.com/elasticbeanstalk) - For hosting node-red

## Solution Architecture

## IoT - Reading from the sensor and acting on received values

For this part, we wanted to use the IoT Leonardo building block which was provided to us in the form of a CC2650STK sensor, an iPad mini with the SAP Gateway to Leonardo and, of course, a SAP Leonardo account. We really couldn't use any of this technologies, because the WiFi connection at the convention hall was working really badly (for the first hour only, it worked fine afterwards) and the iPad couldn't keep a stable connection. So... we invented our own "Leonardo" which we named "Michelangelo".

### Mosquitto - MQTT Server

The first thing we needed for registering sensor readouts was an MQTT Server. Mosquitto is a great open source MQTT server, so we launched an EC2 instance on AWS and ran a mosquitto server there (we used the standard Ubuntu AMI and installed mosquitto via apt: sudo apt-get install mosquitto). It publishes a pure TCP service on port 1883, we used no authentication and our security group was "open for everyone".... do not use this in any production system!.

We had to send sensor data to the mqtt server, so we configured the standard TI SensorLink application to push sensor data to our cloud broker:

![SensorTag App](/Screenshots/IOS_CONF3.jpg?raw=true "SensorTag App")

Then, we were getting fresh sensor data on our cloud server every other second; but had no way to interact with it.

### Node-RED

For acting on sensor data, we chose Node-RED. It's an open source flow based programming tool, specially designed for interacting with IoT devices. It can be deployed as a web app, so we chose to deploy it on AWS Elastic Beanstalk. EB is quite similar to CloudFoundry, you can upload an application and it runs in the cloud, totally managed. [Installing Node-RED on EBS](https://nodered.org/docs/platforms/aws)

We designed a simple flow to act on sensor data:

![MQTT FLOW](/Screenshots/Flow_01_MQTT.JPG?raw=true "MQTT FLOW")

We received the mqtt messages on the CC2650STK topic, and converted them to JSON (the sensor sends data as a stringified JSON, so we converted it back to a JSON object). We kept the last temperature readout locally, as a global variable, which was updated after receiving every message. 

You can "connect" different flows on Node-RED, so after storing the last temperature readout, we diverted paths for each different functionallity:

* Always store readouts on Blockchain
* If the alert condition is met, send a message to both the B1 backend and the predefined Telegram user.

## Blockchain - Storing sensor readouts on a blockchain

For this functionality, we used the [Blockchain](https://github.com/B1SA/hackathon/tree/master/Blockchain) building block directly. Following the step by step [tutorial](https://blogs.sap.com/2018/08/03/step-by-step-process-to-create-your-first-blockchain-project-hello-world/), we managed to have our blockchain up and running in less than 10 minutes. This tutorial creates a really simple chain, a key-value store, but that was all we needed! Fortunately, it has a swagger interface for testing the recently created rest services for interacting with the bloickchain, so we focused our attention on the POST method, for writing new values to the blockchain.

![HyperLedger POST](/Screenshots/SCP_HyperLedger_Swagger.JPG?raw=true "HyperLedger POST")

Consequently, we got a REST service for writing values on our blockchain, so we had to send the message from Node-RED. That's an easy task! We made a little sub-flow for this particular endeavour:

![HyperLedger Flow](/Screenshots/Flow_02__HYPERLEDGER.JPG?raw=true "HyperLedger Flow")

We just compose an HTTP request object and send it to the HyperLedger api gateway URL:

![HyperLedger Request](/Screenshots/Node_HyperLedgerRequest.JPG?raw=true "HyperLedger Request")

The "payload", in this case, refers to the last saved temperature readout; so, every time we received a temperature readout from our sensor via the mqtt server, we stored it in the blockchain directly: no way to miss any transaction, every readout is stored in the blockchain sequentially and the chain can't be broken!

![HyperLedger Graph](/Screenshots/SCP_HyperLedger.JPG?raw=true "HyperLedger Graph")

## Acting on threshold values - IoT decision making

In our design, we wanted to send a message when temperature went up a certain threshold... but that is really hard to simulate! The sensors temperature variance is really slow: even if you put it in a hot place, it takes its time to update the sensor value. So, we "cheated" using the sensor button: if the button present in the sensor was pressed, it triggered the message sending flow. Really easy to do on Node-RED:

![Alert Condition](/Screenshots/Node_AlertCondition.JPG?raw=true "Alert Condition")

Once the alert condition was fulfilled, we continued the flow to two different subflows: sending the message to the B1 backend and sending the message via Telegram.

### B1 Backend - .NET Core App hosted on SAP Cloud Platform on Cloud Foundry

We could have sent the message, via B1 Service Layer, directly from Node-RED; but we wanted to use the [CloudFoundry](https://github.com/B1SA/hackathon/tree/master/CloudFoundry) building block... so we followed @Ralphive 's tutorial on CloudFoundry and used his .NET Core demo app.

We made two simple changes to the example project:

![VS Post Activity](/Screenshots/VS_PostActivity.JPG?raw=true "VS Post Activity")

First, we created a new service for posting B1 Activities. The example app already connects to ServiceLayer, so we used the already estabilished connection to create an activity with the current time-stamp (adjusting the time zome variance between Nice and the server location) and a custom message containing the sensor readout.

After that, we created a new APIContoller, that is, a REST service for receiving a temperature value. We could have saved temperature values on the already provided Postgres database, but for the sake of simplicity, we just used the POST method to send the alert message to the B1 backend using the method shown in the last screenshot.

![VS API Controller](/Screenshots/VS_Controller.JPG?raw=true "VS API Controller")

After compiling the project and uploading it to SAP Cloud Platform via CloudFoundry using the .NET Core buildpack, we had our own webservice bridge for sending messages to the B1 backend from our Node-RED flow!

![SCP App](/Screenshots/SCP_App.JPG?raw=true "SCP App")

Back to Node-RED, we had to compose a new http request for calling our just deployed REST service:

![SCP Flow](/Screenshots/Flow_04_SCP.JPG?raw=true "SCP Flow")

Easy as pie! Just set the request payload to the last sensor readout, select a destination URL and we're done.

![SCP Request](/Screenshots/Node_SCPRequest.JPG?raw=true "SCP Request")
![SCP Destination](/Screenshots/Node_SCPDestination.JPG?raw=true "SCP Destination")


## Chatbox Integration - Send alert messages via Telegram and interact with a Telegram Bot

Same as the B1 Backend, we wanted to send a Telegram message to a certain user after the alert condition was fulfilled. Also, we made the world's simplest AI for answering users interacting with the bot when they asked for sensor temperature.

All of this is implemented in the last Node-RED subflow: Telegram

![Telegram Flow](/Screenshots/Flow_03_TELEGRAM.JPG?raw=true "Telegram Flow")

Telegram interaction from Node-RED is really easy, as it provides some specific nodes for interacting with Telegram Bots. Creating a Telegram bot is really easy, you just have to talk to the @BotFather user, follow his instructions, and you'll have your bot created in less than a minute and will be provided with your API Key.

![Telegram Bot](/Screenshots/NODE_TELEGRAMBOT.JPG?raw=true "Telegram Bot")

Composing a Telegram message is no different from composing an HTTP request: select the destination user, compose a text message and hit the "Telegram sender" node.

For user interaction, we designed a really powerful and ahead of its time AI:

![Telegram AI](/Screenshots/Node_ChatboxAI.JPG?raw=true "Telegram AI")

Simply put: if the user sends any text to the bot containing the "temp" substring, it will automatically initiate the response flow and send the last stored temperature readout from the sensor.

![Telegram Message](/Screenshots/Node_TelegramMessage.JPG?raw=true "Telegram Message")


## Wrap up

So, this is the solution we designed for the Hackaton: a multi-cloud solution (both AWS & SCP!), using IoT sensors to gather real world's data, storing sensor values on a secure blockchain, sending alerts when alert conditions were met and answering user inquiries from our Telegram Bot. 

We had a really great time in the Hackaton and are looking forward to the upcoming years competition, it's the best session to attend in the SMB Summit, doubt no more and join us next year!!

Our most sincere gratitude to the Solution Architect team for making all this possible, organizing and mantaining the event in such a great venue, creating the building blocks and helping everyone out on-site!




