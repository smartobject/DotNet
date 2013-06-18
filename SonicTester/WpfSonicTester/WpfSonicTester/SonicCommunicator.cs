using Sonic.Jms;
using System;
using System.Xml;

namespace WpfSonicTester
{

        public class SonicCommunicator : ExceptionListener
        {
            private readonly string _topicName;
            private readonly string _brokerUrl;
            //private readonly ILogger _logger;
            private Sonic.Jms.Connection _connection;
            private MessageProducer _replier;
            private Topic _topic;
            private Sonic.Jms.Ext.Session _session;

            public SonicCommunicator(string topicName, string brokerUrl)
            {
                _topicName = topicName;
                _brokerUrl = brokerUrl;
                //_logger = logger;
            }

            public virtual Sonic.Jms.Cf.Impl.ConnectionFactory GetConnectionFactory()
            {
                return new Sonic.Jms.Cf.Impl.ConnectionFactory(_brokerUrl);
            }

            public virtual void SendMessage(string message)
            {
                try
                {
                    //_logger.Log(loggingCategory.ToString(), LogSeverity.Information, "Raw outbound message: " + message);

                    EnsureOpenConnection();

                    var document = new XmlDocument();
                    document.LoadXml(message);

                    var reply = _session.createXMLMessage();

                    reply.setDocument(document);

                    _replier.send(_topic, reply);

                    //_logger.Log(loggingCategory.ToString(), LogSeverity.Information, "Received JMSMessageID: " + reply.getJMSMessageID());

                    _session.commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendMessage ERROR:" + ex.InnerException);
                }
            }

            public virtual void EnsureOpenConnection()
            {
                if (_connection == null)
                {
                    _connection = GetConnectionFactory().createConnection();
                    _session = (Sonic.Jms.Ext.Session)_connection.createSession(true, Sonic.Jms.SessionMode.AUTO_ACKNOWLEDGE);
                    _topic = _session.createTopic(_topicName);
                    _replier = _session.createProducer(null);

                    _connection.setExceptionListener(this);
                    _connection.start();
                }
            }

            public void onException(Sonic.Jms.JMSException jmse)
            {
                Console.WriteLine("SonicCommunicator Exception ERROR:" + jmse.InnerException);
            }

        }

 }