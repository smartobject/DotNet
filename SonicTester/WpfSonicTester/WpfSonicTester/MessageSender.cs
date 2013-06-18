using Sonic.Jms;
using Sonic.Jms.Ext;
using System;
using System.Xml;
using Session = Sonic.Jms.Ext.Session;

namespace WpfSonicTester
{
        public class MessageSender
        {
            private static SonicCommunicator sonicSender = null;
            private static Sonic.Jms.Cf.Impl.ConnectionFactory cf = null;
            private static Sonic.Jms.Connection conn = null;
            private static Session session = null;
            private static MessageProducer publisher = null;
            private static Topic topic = null;
            private static TemporaryTopic tempTopic = null;
            private static Sonic.Jms.MessageConsumer subscriber = null;

            public MessageSender(String broker, String topicName)
            {
                //Create connection
                try
                {
                    sonicSender = new SonicCommunicator(topicName, broker);
                    cf = sonicSender.GetConnectionFactory();
                    conn = cf.createConnection();
                    session = (Session)conn.createSession(false, Sonic.Jms.SessionMode.AUTO_ACKNOWLEDGE);
                }
                catch (JMSException jmse)
                {
                    throw new Exception("Unable to establish connection to MQ." + jmse.Message + " / " + jmse.InnerException);
                }

                Console.WriteLine("Create Session: " + session.ToString());

                //create the topic
                try
                {
                    topic = session.createTopic(topicName);
                    publisher = session.createProducer(topic);
                    tempTopic = session.createTemporaryTopic();
                    subscriber = session.createConsumer(tempTopic);
                    conn.start();
                }
                catch (JMSException jmse)
                {
                    throw new Exception("Unable to create topic." + jmse.Message + " / " + jmse.InnerException);
                }
            }

            public static XmlDocument createDocument()
            {
                try
                {
                    XmlDocument doc = new XmlDocument(); //session.createXMLMessage();
                    return doc;
                }
                catch (XmlException e)
                {
                    throw new Exception("Unable to create XML Doc: " + e.Message + " / " + e.InnerException);
                }
            }

            /**
             * Sends an XML message and expects one back.
             * @param request - the XML request document.
             * @return the XML response document.
             * @throws Exception - used to indicate problems with the sonic broker or Juris backend.
             */
            public XmlDocument send(XmlDocument request)
            {
                XmlDocument response = null;

                try
                {
                    XMLMessage xMsg = ((Session)session).createXMLMessage(request);
                    xMsg.setJMSReplyTo(tempTopic);
                    publisher.send(xMsg);
                    Sonic.Jms.Message xResponse = subscriber.receive(30000);

                    if (xResponse == null)
                    {
                        throw new Exception("No response: Unable to communicate with backend.");
                    }

                    XMLMessage xmlMessage = (XMLMessage)xResponse;
                    response = xmlMessage.getDocument();
                }
                catch (JMSException jmse)
                {
                    response = request;
                }

                return response;
            }
        }

}
