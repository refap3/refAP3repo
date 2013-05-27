#if !MF_FRAMEWORK_VERSION_V4_2
using System;
#else
using Microsoft.SPOT;
#endif

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for subscribed topics
    /// </summary>
    public class MqttMsgSubscribedEventArgs : EventArgs
    {
        #region Properties...

        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId
        {
            get { return this.messageId; }
            set { this.messageId = value; }
        }

        /// <summary>
        /// List of granted QOS Levels
        /// </summary>
        public byte[] GrantedQoSLevels
        {
            get { return this.grantedQosLevels; }
            set { this.grantedQosLevels = value; }
        }

        #endregion

        // message identifier
        ushort messageId;
        // granted QOS levels
        byte[] grantedQosLevels;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribed topics</param>
        /// <param name="grantedQosLevels">List of granted QOS Levels</param>
        public MqttMsgSubscribedEventArgs(ushort messageId, byte[] grantedQosLevels)
        {
            this.messageId = messageId;
            this.grantedQosLevels = grantedQosLevels;
        }
    }
}
