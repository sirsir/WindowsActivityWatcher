using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace AmivoiceWatcher
{
    class RabbitMQWrapper
    {
        private static ConnectionFactory factory;

        private static ClientNotifyResponse _rabbitMQ;

        private static bool blnThreadAborted;

        public static bool BlnThreadAborted
        {
            get { return blnThreadAborted; }
            set { blnThreadAborted = value; }
        }

        public class ClientNotifyResponse
        {
            public string success { get; set; }
            public string login_name { get; set; }
            public string queue_name { get; set; }
            public string timestamp { get; set; }           
            public RabbitMQ rabbitmq { get; set; }

        }

        public class RabbitMQ
        {
            public string host { get; set; }
            public string port { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string vhost { get; set; }
        }

        private static void RegisterNotification()
        {
            try
            {
                var login_name = Globals.myComputerInfo.UserName;
#if DEBUG
                //login_name = "aohsadmin";
#endif

                string path = Globals.configuration["notification.register_url"] + @"?do_act=register&login_name=" + login_name;

                Globals.log.Debug("Register Notification through: " + path);

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(path);
                httpWebRequest.ContentType = "application/json; charset=utf-8";

                httpWebRequest.Method = "POST";

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var jsonObject = JsonConvert.DeserializeObject<ClientNotifyResponse> (result);

                    if (jsonObject.success == "true")
                    {
                        Globals.log.Debug("Get rabbitMq para from server >>" + result);

                        _rabbitMQ = jsonObject;
                    }else
                    {
                        Globals.log.Debug("Disable Notification function because UNABLE to set rabbitMq para from server:" + path);
                        _rabbitMQ = null;
                    }



                }


            }
            catch (Exception e)
            {
                Globals.log.Error(e.ToString());
            }
        }

        private static void Setup()
        {

            RegisterNotification();

            if (_rabbitMQ == null)
            {
                blnThreadAborted = true;
                return;
            }else if (_rabbitMQ.rabbitmq == null)
            {
                blnThreadAborted = true;
            }else
            {
                factory = new ConnectionFactory() {
                    HostName = _rabbitMQ.rabbitmq.host,
                    UserName = _rabbitMQ.rabbitmq.username,
                    Password = _rabbitMQ.rabbitmq.password,
                    VirtualHost = _rabbitMQ.rabbitmq.vhost
                };
            }
        }


        public static void ThreadMain()
        {
            var bln_connection_ok = false;

            Setup();

            while (!bln_connection_ok &&  ! blnThreadAborted)
            {
                try
                {
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {

                        channel.ExchangeDeclare(exchange: "direct_logs",
                                                type: "direct");

                        channel.QueueDeclare(queue: _rabbitMQ.queue_name,
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null
                                             );

                        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                       
                        string[] routingKeys = new string[] { "all", Globals.myComputerInfo.UserName };
                        if (routingKeys.Length < 1)
                        {
                            Console.Error.WriteLine("Usage: {0} [info] [warning] [error]",
                                                    Environment.GetCommandLineArgs()[0]);
                            Console.WriteLine(" Press [enter] to exit.");
                            Console.ReadLine();
                            Environment.ExitCode = 1;
                            return;
                        }

                        foreach (var severity in routingKeys)
                        {
                            channel.QueueBind(
                                              exchange: "direct_logs",
                                              queue: _rabbitMQ.queue_name,
                                              routingKey: severity);
                        }

                        Console.WriteLine(" [*] Waiting for messages.");
             
                       
                        while (!RabbitMQWrapper.BlnThreadAborted)
                        {
                            try
                            {
                                if (AmivoiceWatcher.formDummy.OwnedForms.Count() > Globals.notification_client_max)
                                {
                                    //if (channel.ConsumerCount(queue: queueName) > 0)
                                    //{
                                    //    channel.BasicCancel(consumerTag);
                                    //}

                                }
                                else
                                {

                                    var e = channel.BasicGet(queue: _rabbitMQ.queue_name, noAck: true);

                                    if (e != null)
                                    {
                                        string data = Encoding.UTF8.GetString(e.Body);

                                        Globals.log.Debug("Get message from queue:> " + data);

                                        var duration = Globals.notification_duration;
                                        //var animationMethod = FormAnimator.AnimationMethod.Slide;
                                        var animationMethod = FormAnimator.AnimationMethod.Fade;
                                        var animationDirection = FormAnimator.AnimationDirection.Up;

                                        
                                        AmivoiceWatcher.Notifications.Popup(data, duration, animationMethod, animationDirection);

                                        // ack the message, ie. confirm that we have processed it
                                        // otherwise it will be requeued a bit later
                                        //channel.BasicAck(data.DeliveryTag, false);

                                    }


                                }

                                Thread.Sleep(1000);
                            }
                            catch (Exception e)
                            {
                                Globals.log.Error(e.ToString());
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }
            }
            
        }
    }
}
