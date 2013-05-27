using System;
using System.Net.Sockets;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Class for PUBLISH message from client to broker
    /// </summary>
    public class MqttMsgPublish : MqttMsgBase
    {
        #region Properties...

        /// <summary>
        /// Message topic
        /// </summary>
        public string Topic
        {
            get { return this.topic; }
            set { this.topic = value; }
        }

        /// <summary>
        /// Message data
        /// </summary>
        public byte[] Message
        {
            get { return this.message; }
            set { this.message = value; }
        }

        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId
        {
            get { return this.messageId; }
            set { this.messageId = value; }
        }

        #endregion

        // message topic
        private string topic;
        // message data
        private byte[] message;
        // message identifier
        ushort messageId;

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgPublish()
        {
            this.type = MQTT_MSG_PUBLISH_TYPE;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data</param>
        /// <param name="dupFlag">Duplicate flag</param>
        /// <param name="qosLevel">Quality of Service level</param>
        /// <param name="retain">Retain flag</param>
        public MqttMsgPublish(string topic,
            byte[] message,
            bool dupFlag = false,
            byte qosLevel = QOS_LEVEL_AT_MOST_ONCE,
            bool retain = false) : base()
        {
            this.topic = topic;
            this.message = message;
            this.dupFlag = dupFlag;
            this.qosLevel = qosLevel;
            this.retain = retain;
            this.messageId = 0;
        }

        public override byte[] GetBytes()
        {
            int fixedHeaderSize = 0;
            int varHeaderSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;

            // topic can't contain wildcards
            if ((this.topic.IndexOf('#') != -1) || (this.topic.IndexOf('+') != -1))
                throw new MqttClientException(MqttClientErrorCode.TopicWildcard);

            // check topic length
            if ((this.topic.Length < MIN_TOPIC_LENGTH) || (this.topic.Length > MAX_TOPIC_LENGTH))
                throw new MqttClientException(MqttClientErrorCode.TopicLength);

            byte[] topicUtf8 = Encoding.UTF8.GetBytes(this.topic);

            // topic name
            varHeaderSize += topicUtf8.Length + 2;

            // message id is valid only with QOS level 1 or QOS level 2
            if ((this.qosLevel == QOS_LEVEL_AT_LEAST_ONCE) || 
                (this.qosLevel == QOS_LEVEL_EXACTLY_ONCE))
            {
                varHeaderSize += MESSAGE_ID_SIZE;
            }
            
            // check on message with zero length
            if (this.message != null)
                // message data
                payloadSize += this.message.Length;

            remainingLength += (varHeaderSize + payloadSize);

            // first byte of fixed header
            fixedHeaderSize = 1;

            int temp = remainingLength;
            // increase fixed header size based on remaining length
            // (each remaining length byte can encode until 128)
            do
            {
                fixedHeaderSize++;
                temp = temp / 128;
            } while (temp > 0);

            // allocate buffer for message
            buffer = new byte[fixedHeaderSize + varHeaderSize + payloadSize];

            // first fixed header byte
            buffer[index] = (byte)((MQTT_MSG_PUBLISH_TYPE << MSG_TYPE_OFFSET) |
                                   (this.qosLevel << QOS_LEVEL_OFFSET));
            buffer[index] |= this.dupFlag ? (byte)(1 << DUP_FLAG_OFFSET) : (byte)0x00;
            buffer[index] |= this.retain ? (byte)(1 << RETAIN_FLAG_OFFSET) : (byte)0x00;
            index++;

            // encode remaining length
            index = this.encodeRemainingLength(remainingLength, buffer, index);

            // topic name
            buffer[index++] = (byte)((topicUtf8.Length >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(topicUtf8.Length & 0x00FF); // LSB
            Array.Copy(topicUtf8, 0, buffer, index, topicUtf8.Length);
            index += topicUtf8.Length;

            // message id is valid only with QOS level 1 or QOS level 2
            if ((this.qosLevel == QOS_LEVEL_AT_LEAST_ONCE) ||
                (this.qosLevel == QOS_LEVEL_EXACTLY_ONCE))
            {
                this.messageId = this.GetMessageId();
                buffer[index++] = (byte)((this.messageId >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(this.messageId & 0x00FF); // LSB
            }

            // check on message with zero length
            if (this.message != null)
            {
                // message data
                Array.Copy(this.message, 0, buffer, index, this.message.Length);
                index += this.message.Length;
            }

            return buffer;
        }

        public static MqttMsgPublish Parse(byte fixedHeaderFirstByte, Socket socket)
        {
            byte[] buffer;
            int index = 0;
            byte[] topicUtf8;
            int topicUtf8Length;
            MqttMsgPublish msg = new MqttMsgPublish();

            // get remaining length and allocate buffer
            int remainingLength = MqttMsgBase.decodeRemainingLength(socket);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            socket.Receive(buffer);

            // topic name
            topicUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            topicUtf8Length |= buffer[index++];
            topicUtf8 = new byte[topicUtf8Length];
            Array.Copy(buffer, index, topicUtf8, 0, topicUtf8Length);
            index += topicUtf8Length;
            msg.topic = new String(Encoding.UTF8.GetChars(topicUtf8));

            // read QoS level from fixed header
            msg.qosLevel = (byte)((fixedHeaderFirstByte & QOS_LEVEL_MASK) >> QOS_LEVEL_OFFSET);
            // read DUP flag from fixed header
            msg.dupFlag = (((fixedHeaderFirstByte & DUP_FLAG_MASK) >> DUP_FLAG_OFFSET) == 0x01);
            // read retain flag from fixed header
            msg.retain = (((fixedHeaderFirstByte & RETAIN_FLAG_MASK) >> RETAIN_FLAG_OFFSET) == 0x01);
            
            // message id is valid only with QOS level 1 or QOS level 2
            if ((msg.qosLevel == QOS_LEVEL_AT_LEAST_ONCE) ||
                (msg.qosLevel == QOS_LEVEL_EXACTLY_ONCE))
            {
                // message id
                msg.messageId = (ushort)((buffer[index++] << 8) & 0xFF00);
                msg.messageId |= (buffer[index++]);
            }

            // get payload with message data
            msg.message = new byte[remainingLength - index];
            Array.Copy(buffer, index, msg.message, 0, msg.message.Length);

            return msg;
        }
    }
}
