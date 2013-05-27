using System;
using System.Net.Sockets;

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Base class for all MQTT messages
    /// </summary>
    public abstract class MqttMsgBase
    {
        #region Constants...

        // mask, offset and size for fixed header fields
        internal const byte MSG_TYPE_MASK = 0xF0;
        internal const byte MSG_TYPE_OFFSET = 0x04;
        internal const byte MSG_TYPE_SIZE = 0x04;
        internal const byte DUP_FLAG_MASK = 0x08;
        internal const byte DUP_FLAG_OFFSET = 0x03;
        internal const byte DUP_FLAG_SIZE = 0x01;
        internal const byte QOS_LEVEL_MASK = 0x06;
        internal const byte QOS_LEVEL_OFFSET = 0x01;
        internal const byte QOS_LEVEL_SIZE = 0x02;
        internal const byte RETAIN_FLAG_MASK = 0x01;
        internal const byte RETAIN_FLAG_OFFSET = 0x00;
        internal const byte RETAIN_FLAG_SIZE = 0x01;

        // MQTT message types
        internal const byte MQTT_MSG_CONNECT_TYPE = 0x01;
        internal const byte MQTT_MSG_CONNACK_TYPE = 0x02;
        internal const byte MQTT_MSG_PUBLISH_TYPE = 0x03;
        internal const byte MQTT_MSG_PUBACK_TYPE = 0x04;
        internal const byte MQTT_MSG_PUBREC_TYPE = 0x05;
        internal const byte MQTT_MSG_PUBREL_TYPE = 0x06;
        internal const byte MQTT_MSG_PUBCOMP_TYPE = 0x07;
        internal const byte MQTT_MSG_SUBSCRIBE_TYPE = 0x08;
        internal const byte MQTT_MSG_SUBACK_TYPE = 0x09;
        internal const byte MQTT_MSG_UNSUBSCRIBE_TYPE = 0x0A;
        internal const byte MQTT_MSG_UNSUBACK_TYPE = 0x0B;
        internal const byte MQTT_MSG_PINGREQ_TYPE = 0x0C;
        internal const byte MQTT_MSG_PINGRESP_TYPE = 0x0D;
        internal const byte MQTT_MSG_DISCONNECT_TYPE = 0x0E;

        // QOS levels
        public const byte QOS_LEVEL_AT_MOST_ONCE = 0x00;
        public const byte QOS_LEVEL_AT_LEAST_ONCE = 0x01;
        public const byte QOS_LEVEL_EXACTLY_ONCE = 0x02;

        internal const ushort MAX_TOPIC_LENGTH = 65535;
        internal const ushort MIN_TOPIC_LENGTH = 1;
        internal const byte MESSAGE_ID_SIZE = 2;

        #endregion

        #region Properties...

        /// <summary>
        /// Message type
        /// </summary>
        public byte Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        /// <summary>
        /// Duplicate message flag
        /// </summary>
        public bool DupFlag
        {
            get { return this.dupFlag; }
            set { this.dupFlag = value; }
        }

        /// <summary>
        /// Quality of Service level
        /// </summary>
        public byte QosLevel
        {
            get { return this.qosLevel; }
            set { this.qosLevel = value; }
        }

        /// <summary>
        /// Retain message flag
        /// </summary>
        public bool Retain
        {
            get { return this.retain; }
            set { this.retain = value; }
        }

        #endregion

        // message type
        protected byte type;
        // duplicate delivery
        protected bool dupFlag;
        // quality of service level
        protected byte qosLevel;
        // retain flag
        protected bool retain;

        // current message identifier generated
        private ushort messageIdCounter = 0;      

        /// <summary>
        /// Returns message bytes rapresentation
        /// </summary>
        /// <returns>Bytes rapresentation</returns>
        public abstract byte[] GetBytes();
        
        /// <summary>
        /// Encode remaining length and insert it into message buffer
        /// </summary>
        /// <param name="remainingLength">Remaining length value to encode</param>
        /// <param name="buffer">Message buffer for inserting encoded value</param>
        /// <param name="index">Index from which insert encoded value into buffer</param>
        /// <returns>Index updated</returns>
        protected int encodeRemainingLength(int remainingLength, byte[] buffer, int index)
        {
            int digit = 0;
            do
            {
                digit = remainingLength % 128;
                remainingLength /= 128;
                if (remainingLength > 0)
                    digit = digit | 0x80;
                buffer[index++] = (byte)digit;
            } while (remainingLength > 0);
            return index;
        }

        /// <summary>
        /// Decode remaining length reading bytes from socket
        /// </summary>
        /// <param name="socket">Socket from reading bytes</param>
        /// <returns>Decoded remaining length</returns>
        protected static int decodeRemainingLength(Socket socket)
        {
            int multiplier = 1;
            int value = 0;
            int digit = 0;
            byte[] nextByte = new byte[1];
            do
            {
                // next digit from stream
                socket.Receive(nextByte);
                digit = nextByte[0];
                value += ((digit & 127) * multiplier);
                multiplier *= 128;
            } while ((digit & 128) != 0);
            return value;
        }

        /// <summary>
        /// Generate the next message identifier
        /// </summary>
        /// <returns>Message identifier</returns>
        protected ushort GetMessageId()
        {
            this.messageIdCounter = (ushort)(this.messageIdCounter % UInt16.MaxValue);
            return this.messageIdCounter;
            
        }
    }
}
