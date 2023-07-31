using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly IEventProcessor eventProcessor;
        private IConnection connection;
        private IModel channel;
        private string queueName;

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            this.configuration = configuration;
            this.eventProcessor = eventProcessor;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQHost"],
                Port = int.Parse(configuration["RabbitMQPort"])
            };

            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare("trigger", ExchangeType.Fanout);
            queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, "trigger", "");

            Console.WriteLine("--> Listening on the Message Bus...");

            connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += (x, ea) =>
            {
                Console.WriteLine("--> Event Received.");

                string body = Encoding.UTF8.GetString(ea.Body.ToArray());

                eventProcessor.ProcessEvent(body);
            };

            channel.BasicConsume(queueName, true, consumer);

            return Task.CompletedTask;
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Connection shutdown");
        }

        public override void Dispose()
        {
            if (channel.IsOpen)
            {
                channel.Close();
                connection.Close();
            }

            base.Dispose();
        }
    }
}