using System;
using System.Text;
using System.Xml;
using Sonic.Jms;
using Connection = Sonic.Jms.Connection;
using ConnectionFactory = Sonic.Jms.ConnectionFactory;
using MessageConsumer = Sonic.Jms.MessageConsumer;
using SessionMode = Sonic.Jms.SessionMode;
using TextMessage = Sonic.Jms.TextMessage;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace IVR.SonicCommunication
{
    public class IVRSonicCommunicator : ExceptionListener, IDisposable
    {
        private readonly string _brokerUrl;
        private readonly string _topicName;
        private Connection _connection;
        private MessageConsumer _messageConsumer;
        private MessageProducer _messageProducer;
        private Sonic.Jms.Ext.Session _session;
        private Topic _topic;

        
        public IVRSonicCommunicator(string brokerUrl, string topicName)
        {			
            _topicName = topicName;
            _brokerUrl = brokerUrl;
        }

        public void Close()
        {
            if (_connection == null)
                return;
            _connection.stop();
            _connection.close();
            if(_session!=null)_session.close();
            if(_messageProducer!=null)_messageProducer.close();
            if(_messageConsumer!=null)_messageConsumer.close();
            _connection = null;
        }

        public void onException(JMSException exception)
        {
            //var log = SCMS.Logging.Log.Create(GetType());
            //log.Error(exception);
        }

        public virtual string SendMessage(string messageText)
        {
            //var log = SCMS.Logging.Log.Create(GetType());
            string line = "";
            try
            {
                line = string.Format("Start IVRSonicCommunicator::SendMessage()\t {0}\t Raw outbound async message : {1} ", FormatLogMessage("Tams topic Name"), messageText);
              //  log.Info(line);

                EnsureOpenConnection();

                var document = new XmlDocument();
                document.LoadXml(messageText);

                var message = _session.createXMLMessage();
                var replyTo = _session.createTemporaryTopic();
                _messageConsumer = _session.createConsumer(replyTo);
                                
                message.setJMSReplyTo(replyTo);                
                message.setDocument(document);

                _messageProducer.send(_topic, message);
                _session.commit();

                line = string.Format("End IVRSonicCommunicator::SendMessage()\t JMSMessageID: {0}", message.getJMSMessageID());
                //log.Info(line);
                
                return (message.getJMSMessageID());
            }
            catch (Exception ex)
            {                
               // log.Error(ex);

                //Context.Logger.Log(
                //    GetType().Name, 
                //    LogSeverity.Error, 
                //    "Error Sending with the Sonic Communicator: Exception writing to sonic." + FormatLogMessage(), 
                //    ex);
            }
            return (null);
        }

        public virtual string SendSynchronousMessage(string message, int timeout = 30000)
        {
            //var log = SCMS.Logging.Log.Create(GetType());
            string line;

            if (SendMessage(message) == null) return (null);

            try
            {
                line = string.Format("Start IVRSonicCommunicator::SendSynchronousMessage()\t {0}\t Raw outbound sync message: {1} ", FormatLogMessage("Tams topic Name"), message);
              //  log.Info(line);

                var response = _messageConsumer.receive(timeout);
                if (response != null)
                {
                    try
                    {
                        response.acknowledge();
                    }
                    catch(Exception e)
                    {
                //        log.Error(e);
                    }

                    var toReturn = ((TextMessage)response).getText();

                    line = string.Format("End IVRSonicCommunicator::SendSynchronousMessage(), Received response Message: {0} ", toReturn);
                  //  log.Info(line);

                    _session.commit();  
                    return (toReturn);
                }
                line = string.Format("IVRSonicCommunicator::SendSynchronousMessage(), Failed to receive message due to timeout: {0} ", FormatLogMessage("Tams topic Name"));
               // log.Info(line);
            }
            catch (Exception ex)
            {                
              //  log.Error(ex);
            }
            return null;
        }

        private void EnsureOpenConnection()
        {
            if (_connection != null)
                return;
            _connection = GetNewConnectionFactory().createConnection();
            _session = (Sonic.Jms.Ext.Session)_connection.createSession(true, SessionMode.AUTO_ACKNOWLEDGE);            
            _topic = _session.createTopic(_topicName);
            _messageProducer = _session.createProducer(null);
            _messageConsumer = _session.createConsumer(_topic);

            _connection.setExceptionListener(this);
            _connection.start();
        }

        private ConnectionFactory GetNewConnectionFactory()
        {
            return new Sonic.Jms.Cf.Impl.ConnectionFactory(_brokerUrl);
        }

        protected string FormatLogMessage(string topicName)
        {
            //var logMessage = new StringBuilder(Environment.NewLine);
            var logMessage = new StringBuilder();
            logMessage.Append("Broker URL: " + _brokerUrl + "\t");
            //logMessage.Append("Topic Name: " + _topicName);
            logMessage.Append(topicName+ ": " + _topicName);
            return (logMessage.ToString());
        }
        
        //Sending message to Juris
        public virtual string SendMessageToJuris(string messageText,string jurisReplyToTopicName)
        {
            //var log = SCMS.Logging.Log.Create(GetType());
            string line = "";
            try
            {
                line = string.Format("Start IVRSonicCommunicator::SendMessageToJuris()\t {0}\t Message : {1} ", FormatLogMessage("Juris Topic Name") , messageText);
              //  log.Info(line);

                EnsureOpenConnection();

                var document = new XmlDocument();
                document.LoadXml(messageText);

                var message = _session.createXMLMessage();
                var topicReplyTo = _session.createTopic(jurisReplyToTopicName);
                _messageConsumer = _session.createConsumer(topicReplyTo);

                //Create durable subscriber
                _messageConsumer = _session.createDurableSubscriber(topicReplyTo, "jurisSubscriber");
                //end

                message.setJMSReplyTo(topicReplyTo);
                message.setDocument(document);

                _messageProducer.send(_topic, message);
                _session.commit();

                line = string.Format("End IVRSonicCommunicator::SendMessageToJuris(), Received JMSMessageID: {0} ", message.getJMSMessageID());
              //  log.Info(line);

                return (message.getJMSMessageID());
            }
            catch (Exception ex)
            {
              //  log.Error(ex);
            }
            return (null);
        }

        public string GetTopicName()
        {
            return _topicName;
        }
        public void Dispose()
        {
            Close();
        }
    }
}
