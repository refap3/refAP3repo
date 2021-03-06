using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Class for SUBSCRIBE message from client to broker
    /// </summary>
    public class MqttMsgSubscribe : MqttMsgBase
    {
        #region Properties...

        /// <summary>
        /// List of topics to subscribe
        /// </summary>
        public string[] Topics
        {
            get { return this.topics; }
            set { this.topics = value; }
        }

        /// <summary>
        /// List of QOS Levels related to topics
        /// </summary>
        public byte[] QoSLevels
        {
            get { return this.qosLevels; }
            set { this.qosLevels = value; }
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

        // topics to subscribe
        string[] topics;
        // QOS levels related to topics
        byte[] qosLevels;
        // message identifier
        ushort messageId;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topics">List of topics to subscribe</param>
        /// <param name="qosLevels">List of QOS Levels related to topics</param>
        public MqttMsgSubscribe(string[] topics, byte[] qosLevels)
        {
            this.type = MQTT_MSG_SUBSCRIBE_TYPE;

            this.topics = topics;
            this.qosLevels = qosLevels;

            // SUBSCRIBE message uses QoS Level 1
            this.qosLevel = QOS_LEVEL_AT_LEAST_ONCE;
        }

        public override byte[] GetBytes()
        {
            int fixedHeaderSize = 0;
            int varHeaderSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;

            // topics list empty
            if ((this.topics == null) || (this.topics.Length == 0))
                throw new MqttClientException(MqttClientErrorCode.TopicsEmpty);

            // qos levels list empty
            if ((this.qosLevels == null) || (this.qosLevels.Length == 0))
                throw new MqttClientException(MqttClientErrorCode.QosLevelsEmpty);

            // topics and qos levels lists length don't match
            if (this.topics.Length != this.qosLevels.Length)
                throw new MqttClientException(MqttClientErrorCode.TopicsQosLevelsNotMatch);

            // message identifier
            varHeaderSize += MESSAGE_ID_SIZE;

            int topicIdx = 0;
            byte[][] topicsUtf8 = new byte[this.topics.Length][];

            for (topicIdx = 0; topicIdx < this.topics.Length; topicIdx++)
            {
                // check topic length
                if ((this.topics[topicIdx].Length < MIN_TOPIC_LENGTH) || (this.topics[topicIdx].Length > MAX_TOPIC_LENGTH))
                    throw new MqttClientException(MqttClientErrorCode.TopicLength);

                topicsUtf8[topicIdx] = Encoding.UTF8.GetBytes(this.topics[topicIdx]);
                payloadSize += 2; // topic size (MSB, LSB)
                payloadSize += topicsUtf8[topicIdx].Length;
                payloadSize++; // byte for QoS
            }

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
            buffer[index] = (byte)((MQTT_MSG_SUBSCRIBE_TYPE << MSG_TYPE_OFFSET) |
                                   (this.qosLevel << QOS_LEVEL_OFFSET));
            buffer[index] |= this.dupFlag ? (byte)(1 << DUP_FLAG_OFFSET) : (byte)0x00;
            index++;

            // encode remaining length
            index = this.encodeRemainingLength(remainingLength, buffer, index);

            // get next message identifier (SUBSCRIBE uses QoS Level 1, so message id is mandatory)
            this.messageId = this.GetMessageId();
            buffer[index++] = (byte)((messageId >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(messageId & 0x00FF); // LSB 

            topicIdx = 0;
            for (topicIdx = 0; topicIdx < this.topics.Length; topicIdx++)
            {
                // topic name
                buffer[index++] = (byte)((topicsUtf8[topicIdx].Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(topicsUtf8[topicIdx].Length & 0x00FF); // LSB
                Array.Copy(topicsUtf8[topicIdx], 0, buffer, index, topicsUtf8[topicIdx].Length);
                index += topicsUtf8[topicIdx].Length;

                // requested QoS
                buffer[index++] = this.qosLevels[topicIdx];
            }
            
            return buffer;
        }
    }
}
