using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace uPLibrary.Networking.M2Mqtt
{
    /// <summary>
    /// MQTT Client
    /// </summary>
    public class MqttClient
    {
        /// <summary>
        /// Delagate that defines event handler for PUBLISH message received from broker
        /// </summary>
        public delegate void MqttMsgPublishEventHandler(object sender, MqttMsgPublishEventArgs e);

        /// <summary>
        /// Delagate that defines event handler for published message
        /// </summary>
        public delegate void MqttMsgPublishedEventHandler(object sender, MqttMsgPublishedEventArgs e);

        /// <summary>
        /// Delagate that defines event handler for subscribed topic
        /// </summary>
        public delegate void MqttMsgSubscribedEventHandler(object sender, MqttMsgSubscribedEventArgs e);

        /// <summary>
        /// Delagate that defines event handler for unsubscribed topic
        /// </summary>
        public delegate void MqttMsgUnsubscribedEventHandler(object sender, MqttMsgUnsubscribedEventArgs e);

        // default port for MQTT protocol
        public const int MQTT_BROKER_DEFAULT_PORT = 1883;
        // default timeout on receiving from broker
        public const int MQTT_DEFAULT_TIMEOUT = 5000;
        // max publish, subscribe and unsubscribe retry for QoS Level 1 or 2
        private const int MQTT_ATTEMPTS_RETRY = 3;
        // delay for retry publish, subscribe and unsubscribe for QoS Level 1 or 2
        private const int MQTT_DELAY_RETRY = 10000;

        // socket for communication with broker
        private Socket socket;

        // broker ip address and port
        private IPAddress brokerIpAddress;
        private int brokerPort;

        // thread for receiving incoming message from broker
        Thread receiveThread;
        bool isRunning;

        // event for signaling receive end from broker
        AutoResetEvent endReceiving;
        // message received from broker
        MqttMsgBase msgReceived;

        // exeption thrown during receiving from broker
        Exception exReceiving;

        // keep alive period (in ms)
        int keepAlivePeriod;
        // thread for sending keep alive message
        Thread keepAliveThread;
        AutoResetEvent keepAliveEvent;
        // last message sent ticks
        long lastSend;

        // event for PUBLISH messahe received from broker
        public event MqttMsgPublishEventHandler MqttMsgPublishReceived;
        // event for published message
        public event MqttMsgPublishedEventHandler MqttMsgPublished;
        // event for subscribed topic
        public event MqttMsgSubscribedEventHandler MqttMsgSubscribed;
        // event for unsubscribed topic
        public event MqttMsgUnsubscribedEventHandler MqttMsgUnsubscribed;

        /// <summary>
        /// Connection state between client and broker
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="brokerIpAddress">Broker IP address</param>
        /// <param name="brokerPort">Broker port</param>
        public MqttClient(IPAddress brokerIpAddress, int brokerPort = MQTT_BROKER_DEFAULT_PORT)
        {
            this.brokerIpAddress = brokerIpAddress;
            this.brokerPort = brokerPort;            

            this.endReceiving = new AutoResetEvent(false);
            this.keepAliveEvent = new AutoResetEvent(false);
        }

        /// <summary>
        /// Connect to broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="willRetain">Will retain flag</param>
        /// <param name="willQosLevel">Will QOS level</param>
        /// <param name="willFlag">Will flag</param>
        /// <param name="willTopic">Will topic</param>
        /// <param name="willMessage">Will message</param>
        /// <param name="cleanSession">Clean sessione flag</param>
        /// <param name="keepAlivePeriod">Keep alive period</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        public byte Connect(string clientId, 
            string username = null,
            string password = null,
            bool willRetain = false,
            byte willQosLevel = MqttMsgConnect.QOS_LEVEL_AT_LEAST_ONCE,
            bool willFlag = false,
            string willTopic = null,
            string willMessage = null,
            bool cleanSession = true,
            ushort keepAlivePeriod = MqttMsgConnect.KEEP_ALIVE_PERIOD_DEFAULT)
        {
            // create CONNECT message
            MqttMsgConnect connect = new MqttMsgConnect(clientId,
                username,
                password,
                willRetain,
                willQosLevel,
                willFlag,
                willTopic,
                willMessage,
                cleanSession,
                keepAlivePeriod);

            try
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // try connection to the broker
                this.socket.Connect(new IPEndPoint(this.brokerIpAddress, this.brokerPort));
            }
            catch
            {
                throw new MqttConnectionException();
            }

            this.lastSend = 0;
            this.isRunning = true;
            // start thread for receiving messages from broker
            this.receiveThread = new Thread(this.ReceiveThread);
            this.receiveThread.Start();
            
            MqttMsgConnack connack = (MqttMsgConnack)this.SendReceive(connect.GetBytes());
            // if connection accepted, start keep alive timer
            if (connack.ReturnCode == MqttMsgConnack.CONN_ACCEPTED)
            {
                this.IsConnected = true;

                this.keepAlivePeriod = keepAlivePeriod * 1000; // convert in ms
                
                // start thread for sending keep alive message to the broker
                this.keepAliveThread = new Thread(this.KeepAliveThread);
                this.keepAliveThread.Start();
            }
            return connack.ReturnCode;
        }

        /// <summary>
        /// Disconnect from broker
        /// </summary>
        public void Disconnect()
        {
            MqttMsgDisconnect disconnect = new MqttMsgDisconnect();
            this.Send(disconnect.GetBytes());

            // close client
            this.Close();
        }

        /// <summary>
        /// Close client
        /// </summary>
        private void Close()
        {
            // stop receiving thread and keep alive thread
            this.isRunning = false;
            this.receiveThread.Join();

            // unlock keep alive thread and wait
            this.keepAliveEvent.Set();
            this.keepAliveThread.Join();

            this.socket.Close();

            this.IsConnected = false;
        }

        /// <summary>
        /// Execute ping to broker for keep alive
        /// </summary>
        /// <returns>PINGRESP message from broker</returns>
        private MqttMsgPingResp Ping()
        {
            MqttMsgPingReq pingreq = new MqttMsgPingReq();
            try
            {
                // broker must send PINGRESP within timeout equal to keep alive period
                return (MqttMsgPingResp)this.SendReceive(pingreq.GetBytes(), this.keepAlivePeriod);
            }
            catch (MqttTimeoutException)
            {
                // client must close connection
                this.Close();
                return null;
            }
        }

        /// <summary>
        /// Subscribe for message topics
        /// </summary>
        /// <param name="topics">List of topics to subscribe</param>
        /// <param name="qosLevels">QOS levels related to topics</param>
        /// <returns>Granted QoS Levels in SUBACK message from broker</returns>
        public byte[] Subscribe(string[] topics, byte[] qosLevels)
        {
            int attempts = 0;
            bool acknowledged = false;

            MqttMsgSubscribe subscribe =
                new MqttMsgSubscribe(topics, qosLevels);

            MqttMsgSuback suback = null;
            do
            {
                try
                {
                    // try subscribe
                    suback = (MqttMsgSuback)this.SendReceive(subscribe.GetBytes());
                    acknowledged = true;
                }
                catch (MqttTimeoutException)
                {
                    // no SUBACK message received in time, retry with duplicate flag
                    attempts++;
                    subscribe.DupFlag = true;
                    // delay before retry
                    if (attempts < MQTT_ATTEMPTS_RETRY)
                        Thread.Sleep(MQTT_DELAY_RETRY);
                }
            } while ((attempts < MQTT_ATTEMPTS_RETRY) && !acknowledged);

            // return granted QoS Levels or null
            return acknowledged ? suback.GrantedQoSLevels : null;
        }

        /// <summary>
        /// Unsubscribe for message topics
        /// </summary>
        /// <param name="topics">List of topics to unsubscribe</param>
        /// <returns>Message Id in UNSUBACK message from broker</returns>
        public ushort Unsubscribe(string[] topics)
        {
            int attempts = 0;
            bool acknowledged = false;

            MqttMsgUnsubscribe unsubscribe =
                new MqttMsgUnsubscribe(topics);

            MqttMsgUnsuback unsuback = null;
            do
            {
                try
                {
                    // try unsubscribe
                    unsuback = (MqttMsgUnsuback)this.SendReceive(unsubscribe.GetBytes());
                    acknowledged = true;
                }
                catch (MqttTimeoutException)
                {
                    // no UNSUBACK message received in time, retry with duplicate flag
                    attempts++;
                    unsubscribe.DupFlag = true;

                    // delay before retry
                    if (attempts < MQTT_ATTEMPTS_RETRY)
                        Thread.Sleep(MQTT_DELAY_RETRY);
                }

            } while ((attempts < MQTT_ATTEMPTS_RETRY) && !acknowledged);

            // return message id from SUBACK or zero (no message id)
            return acknowledged ? unsuback.MessageId : (ushort)0;
        }

        /// <summary>
        /// Publish a message to the broker
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data (payload)</param>
        /// <param name="qosLevel">QoS Level</param>
        /// <param name="retain">Retain flag</param>
        /// <returns>Message Id related to PUBLISH message</returns>
        public ushort Publish(string topic, byte[] message, 
            byte qosLevel = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, 
            bool retain = false)
        {
            ushort messageId = 0;
            int attempts = 0;
            bool acknowledged = false;

            MqttMsgPublish publish = 
                new MqttMsgPublish(topic, message, false, qosLevel, retain);

            // based on QoS level, the messages flow between client and broker changes
            switch (qosLevel)
            {
                // QoS Level 0, no answer from broker
                case MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE:
                                        
                    this.Send(publish.GetBytes());
                    break;

                // QoS Level 1, waiting for PUBACK message from broker
                case MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE:

                    attempts = 0;
                    acknowledged = false;
                    
                    do
                    {
                        MqttMsgPuback puback = null;
                        try
                        {
                            // try publish
                            puback = (MqttMsgPuback)this.SendReceive(publish.GetBytes());
                            acknowledged = true;
                        }
                        catch (MqttTimeoutException)
                        {
                            // no PUBACK message received in time, retry with duplicate flag
                            attempts++;
                            publish.DupFlag = true;

                            // delay before retry
                            if (attempts < MQTT_ATTEMPTS_RETRY)
                                Thread.Sleep(MQTT_DELAY_RETRY);
                        }
                    } while ((attempts < MQTT_ATTEMPTS_RETRY) && !acknowledged);

                    if (acknowledged)
                        messageId = publish.MessageId;

                    break;

                // QoS Level 2, waiting for PUBREC message from broker,
                // send PUBREL message and waiting for PUBCOMP message from broker
                case MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE:

                    attempts = 0;
                    acknowledged = false;

                    do
                    {
                        MqttMsgPubrec pubrec = null;
                        try
                        {
                            // try publish
                            pubrec = (MqttMsgPubrec)this.SendReceive(publish.GetBytes());
                            acknowledged = true;
                        }
                        catch (MqttTimeoutException)
                        {
                            // no PUBREC message received in time, retry with duplicate flag
                            attempts++;
                            publish.DupFlag = true;

                            // delay before retry
                            if (attempts < MQTT_ATTEMPTS_RETRY)
                                Thread.Sleep(MQTT_DELAY_RETRY);
                        }
                    } while ((attempts < MQTT_ATTEMPTS_RETRY) && !acknowledged);

                    // first phase ok
                    if (acknowledged)
                    {
                        attempts = 0;
                        acknowledged = false;

                        do
                        {
                            // set publish message identifier into PUBREL message
                            MqttMsgPubrel pubrel = new MqttMsgPubrel();
                            pubrel.MessageId = publish.MessageId;

                            MqttMsgPubcomp pubcomp = null;
                            try
                            {
                                // try send PUBREL message
                                pubcomp = (MqttMsgPubcomp)this.SendReceive(pubrel.GetBytes());
                                acknowledged = true;
                            }
                            catch (MqttTimeoutException)
                            {
                                // no PUBCOMP message received in time, retry with duplicate flag
                                attempts++;
                                pubrel.DupFlag = true;

                                // delay before retry
                                if (attempts < MQTT_ATTEMPTS_RETRY)
                                    Thread.Sleep(MQTT_DELAY_RETRY);
                            }
                        } while ((attempts < MQTT_ATTEMPTS_RETRY) && !acknowledged);

                        if (acknowledged)
                            messageId = publish.MessageId;
                    }
                    break;

                default:
                    throw new MqttClientException(MqttClientErrorCode.QosNotAllowed);
            }

            return messageId;
        }

        /// <summary>
        /// Wrapper method for raising PUBLISH message received event
        /// </summary>
        /// <param name="publish">PUBLISH message received</param>
        private void OnMqttMsgPublishReceived(MqttMsgPublish publish)
        {
            if (this.MqttMsgPublishReceived != null)
            {
                this.MqttMsgPublishReceived(this, 
                    new MqttMsgPublishEventArgs(publish.Topic, publish.Message, publish.QosLevel, publish.Retain));
            }
        }

        /// <summary>
        /// Wrapper method for raising published message event
        /// </summary>
        /// <param name="messageId">Message identifier for published message</param>
        private void OnMqttMsgPublished(ushort messageId)
        {
            if (this.MqttMsgPublished != null)
            {
                this.MqttMsgPublished(this,
                    new MqttMsgPublishedEventArgs(messageId));
            }
        }

        /// <summary>
        /// Wrapper method for raising subscribed topic event (SUBACK message)
        /// </summary>
        /// <param name="suback">SUBACK message received</param>
        private void OnMqttMsgSubscribed(MqttMsgSuback suback)
        {
            if (this.MqttMsgSubscribed != null)
            {
                this.MqttMsgSubscribed(this,
                    new MqttMsgSubscribedEventArgs(suback.MessageId, suback.GrantedQoSLevels));
            }
        }

        /// <summary>
        /// Wrapper method for raising unsubscribed topic event
        /// </summary>
        /// <param name="messageId">Message identifier for unsubscribed topic</param>
        private void OnMqttMsgUnsubscribed(ushort messageId)
        {
            if (this.MqttMsgUnsubscribed != null)
            {
                this.MqttMsgUnsubscribed(this,
                    new MqttMsgUnsubscribedEventArgs(messageId));
            }
        }

        /// <summary>
        /// Send a message to the broker
        /// </summary>
        /// <param name="msgBytes">Message bytes</param>
        private void Send(byte[] msgBytes)
        {
            try
            {
                // send message
                this.socket.Send(msgBytes);
                // update last message sent ticks
                this.lastSend = DateTime.Now.Ticks;
            }
            catch
            {
                throw new MqttCommunicationException();
            }
        }

        /// <summary>
        /// Send a message to the broker and wait answer
        /// </summary>
        /// <param name="msgBytes">Message bytes</param>
        /// <param name="timeout">Timeout for receiving answer</param>
        /// <returns>MQTT message response</returns>
        private MqttMsgBase SendReceive(byte[] msgBytes, int timeout = MQTT_DEFAULT_TIMEOUT)
        {
            // reset handle before sending
            this.endReceiving.Reset();
            try
            {
                // send message
                this.socket.Send(msgBytes);
                // update last message sent ticks
                this.lastSend = DateTime.Now.Ticks;
            }
            catch (SocketException e)
            {
#if !MF_FRAMEWORK_VERSION_V4_2
                // connection reset by broker
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                    this.IsConnected = false;
#endif

                throw new MqttCommunicationException();
            }

            // wait for answer from broker
            if (this.endReceiving.WaitOne(timeout, false))
            {
                // message received without exception
                if (this.exReceiving == null)
                    return this.msgReceived;
                // receiving thread catched exception
                else
                    throw this.exReceiving;
            }
            else
            {
                // throw timeout exception
                throw new MqttTimeoutException();
            }
        }

        /// <summary>
        /// Thread for receiving messages from broker
        /// </summary>
        private void ReceiveThread()
        {
            int readBytes;
            byte[] fixedHeaderFirstByte = new byte[1];
            byte msgType;
            
            while (this.isRunning)
            {
                try
                {
                    // read first byte (fixed header)
                    readBytes = this.socket.Receive(fixedHeaderFirstByte);

                    if (readBytes > 0)
                    {
                        // extract message type from received byte
                        msgType = (byte)((fixedHeaderFirstByte[0] & MqttMsgBase.MSG_TYPE_MASK) >> MqttMsgBase.MSG_TYPE_OFFSET);

                        switch (msgType)
                        {
                            // impossible, broker can't send CONNECT message
                            case MqttMsgBase.MQTT_MSG_CONNECT_TYPE:

                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);
                                
                            // CONNACK message received from broker
                            case MqttMsgBase.MQTT_MSG_CONNACK_TYPE:

                                this.msgReceived = MqttMsgConnack.Parse(fixedHeaderFirstByte[0], this.socket);
                                this.endReceiving.Set();
                                break;

                            // impossible, broker can't send PINGREQ message
                            case MqttMsgBase.MQTT_MSG_PINGREQ_TYPE:

                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                            // CONNACK message received from broker
                            case MqttMsgBase.MQTT_MSG_PINGRESP_TYPE:

                                this.msgReceived = MqttMsgPingResp.Parse(fixedHeaderFirstByte[0], this.socket);
                                this.endReceiving.Set();
                                break;

                            // impossible, broker can't send SUBSCRIBE message
                            case MqttMsgBase.MQTT_MSG_SUBSCRIBE_TYPE:

                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                            // SUBACK message received from broker
                            case MqttMsgBase.MQTT_MSG_SUBACK_TYPE:

                                this.msgReceived = MqttMsgSuback.Parse(fixedHeaderFirstByte[0], this.socket);
                                this.endReceiving.Set();

                                // raise subscribed topic event (SUBACK message received)
                                this.OnMqttMsgSubscribed((MqttMsgSuback)this.msgReceived);

                                break;

                            // PUBLISH message received from broker
                            case MqttMsgBase.MQTT_MSG_PUBLISH_TYPE:

                                MqttMsgPublish msgReceived = MqttMsgPublish.Parse(fixedHeaderFirstByte[0], this.socket);
                                
                                // for QoS Level 1 and 2, client sends PUBACK message to broker
                                if ((this.msgReceived.QosLevel == MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE) ||
                                    (this.msgReceived.QosLevel == MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE))
                                {
                                    MqttMsgPuback puback = new MqttMsgPuback();
                                    puback.MessageId = (msgReceived).MessageId;
                                    this.Send(puback.GetBytes());
                                }

                                // raise PUBLISH message received event 
                                this.OnMqttMsgPublishReceived(msgReceived);
                                                                
                                break;

                            // PUBACK message received from broker
                            case MqttMsgBase.MQTT_MSG_PUBACK_TYPE:

                                this.msgReceived = MqttMsgPuback.Parse(fixedHeaderFirstByte[0], this.socket);
                                this.endReceiving.Set();

                                // raise published message event
                                // (PUBACK received for QoS Level 1)
                                this.OnMqttMsgPublished(((MqttMsgPuback)this.msgReceived).MessageId);

                                break;

                            // PUBREC message received from broker
                            case MqttMsgBase.MQTT_MSG_PUBREC_TYPE:

                                this.msgReceived = MqttMsgPubrec.Parse(fixedHeaderFirstByte[0], this.socket);
                                this.endReceiving.Set();
                                break;

                            // impossible, broker can't send PUBREL message
                            case MqttMsgBase.MQTT_MSG_PUBREL_TYPE:

                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);
                                
                            // PUBCOMP message received from broker
                            case MqttMsgBase.MQTT_MSG_PUBCOMP_TYPE:

                                this.msgReceived = MqttMsgPubcomp.Parse(fixedHeaderFirstByte[0], this.socket);
                                this.endReceiving.Set();

                                // raise published message event
                                // (PUBCOMP received for QoS Level 2)
                                this.OnMqttMsgPublished(((MqttMsgPuback)this.msgReceived).MessageId);

                                break;

                            // impossible, broker can't send UNSUBSCRIBE message
                            case MqttMsgBase.MQTT_MSG_UNSUBSCRIBE_TYPE:

                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                            // UNSUBACK message received from broker
                            case MqttMsgBase.MQTT_MSG_UNSUBACK_TYPE:

                                this.msgReceived = MqttMsgUnsuback.Parse(fixedHeaderFirstByte[0], this.socket);
                                this.endReceiving.Set();

                                // raise unsubscribed topic event
                                this.OnMqttMsgUnsubscribed(((MqttMsgUnsuback)this.msgReceived).MessageId);

                                break;

                            default:

                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);
                        }

                        this.exReceiving = null;
                    }
                }
                catch (Exception)
                {
                    this.exReceiving = new MqttCommunicationException();
                }
            }
        }

        /// <summary>
        /// Thread for sending keep alive message to broker
        /// </summary>
        private void KeepAliveThread()
        {
            long now = 0;
            int wait = this.keepAlivePeriod;

            while (this.isRunning)
            {
                // waiting...
                this.keepAliveEvent.WaitOne(wait, false);

                if (this.isRunning)
                {
                    now = DateTime.Now.Ticks;
                    
                    // if timeout exceeded ... (keep alive period converted in ticks)
                    if ((now - this.lastSend) >= (this.keepAlivePeriod * TimeSpan.TicksPerMillisecond)) {
						// ... send keep alive
						this.Ping();
						wait = this.keepAlivePeriod;
					} else {
						// update waiting time (convert ticks in milliseconds)
						wait = (int)(this.keepAlivePeriod - (now - this.lastSend) / TimeSpan.TicksPerMillisecond);
					}
                }
            }
        }
    }
}
