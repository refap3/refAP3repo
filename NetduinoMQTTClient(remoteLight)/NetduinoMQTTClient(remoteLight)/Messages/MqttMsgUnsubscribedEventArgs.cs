#if !MF_FRAMEWORK_VERSION_V4_2
using System;
#else
using Microsoft.SPOT;
#endif

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for unsubscribed topic
    /// </summary>
    public class MqttMsgUnsubscribedEventArgs : EventArgs
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

        #endregion

        // message identifier
        ushort messageId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for unsubscribed topic</param>
        public MqttMsgUnsubscribedEventArgs(ushort messageId)
        {
            this.messageId = messageId;
        }
    }
}
