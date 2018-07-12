using Hangfire.Server;
using MessageContracts;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ScheduleMail.Program;

namespace ScheduleMail
{
    public class MTResponse
    {
        public EmailContract Message;
    }

    public class MailSchedule
    {
        public bool IsActive { get; set; } = true;
        private readonly ConnectionFactory _fact;

        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        public MailSchedule()
        {
            _fact = new ConnectionFactory()
            {
                HostName = Configuration["RabbitMq:host"],
                Port = Convert.ToInt32(Configuration["RabbitMq:port"]),
                UserName = Configuration["RabbitMq:user"],
                VirtualHost = Configuration["RabbitMq:vhost"],
                Password = Configuration["RabbitMq:pass"],
                UseBackgroundThreadsForIO = true
            };
        }

        public async Task StartCheckingQueue(PerformContext cont)
        {
            Console.WriteLine("----Started----");

            IsActive = true;
            using (var connection = CreateMqConnection())
            using (var channel = connection.CreateModel())
            {
                channel.BasicQos(0, 1, false);
                
                var res = channel.QueueDeclare(Configuration["RabbitMq:mailQueue"], true, false, false, null);
                var messagesInQueue = res.MessageCount;
                var consumer = new EventingBasicConsumer(channel);
                consumer.ConsumerTag = "mainConsumer";
                if (messagesInQueue == 0)
                {
                    Console.WriteLine("-----Done0-----");
                    IsActive = false;
                    connection.Dispose();
                    return;
                }
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var deserialized = JsonConvert.DeserializeObject<MTResponse>(message).Message;

                    EmailWork(deserialized);

                    if (messagesInQueue > 0)
                    {
                        messagesInQueue--;
                    }
                    if (messagesInQueue == 0)
                    {
                        Console.WriteLine("-----Done-----");
                        IsActive = false;
                        _closing.Set();
                        _closing.Close();
                        connection.Close();
                        connection.Abort();
                        connection.Dispose();
                    }
                };

                channel.BasicConsume(queue: Configuration["RabbitMq:mailQueue"],
                                        autoAck: true,
                                        consumer: consumer);
                _closing.WaitOne();
            }
        }
        public async void EmailWork(EmailContract cont)
        {
            Console.WriteLine($"Sending new email to {cont.Email} : {cont.Content}");
            //await EmailHelper.SendMailToAdmin(cont);
            //Console.WriteLine($"Email sended! {resp}");
        }

        public IConnection CreateMqConnection()
        {    
            return _fact.CreateConnection();
        }
    }
}
