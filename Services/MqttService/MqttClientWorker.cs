using Microsoft.Extensions.Hosting;
using MQTTnet.Client;
using MQTTnet;
using System.Text;
using Common.Models;
using System.Text.Json;

namespace Services.MqttService
{
    public class MqttClientWorker : BackgroundService
    {
        private const string _brokerAddress = "eba7082725f24cff9cd6bd0068d8ae35.s2.eu.hivemq.cloud";
        private const string _brokerUsername = "MQTTnet";
        private const string _brokerPassword = "P@ssw0rd";

        private readonly List<string> _topics = new()
        {
            "arduino/dht/humidex"
        };

        readonly MqttFactory _mqttFactory;
        readonly IMqttClient _client;
        private readonly IInfluxDbService _influxDbService;

        public MqttClientWorker(IInfluxDbService influxDbService)
        {
            _mqttFactory = new MqttFactory();
            _client = _mqttFactory.CreateMqttClient();
            _influxDbService = influxDbService;
        }

        /// <summary>
        /// Set up message handlng, connect to MQTT broker, and subscribe to topics.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_brokerAddress)
                .WithCredentials(_brokerUsername, _brokerPassword)
                .WithTls()
                .Build();

            // Setup message handling. This is done before connecting so that queued messages are also handled properly.
            _client.ApplicationMessageReceivedAsync += async e =>
            {
                Console.WriteLine("[Received Message]");

                string payload = Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
                Console.WriteLine($"Payload:\n\r{payload}");

                try
                {
                    Humidex humidex = JsonSerializer.Deserialize<Humidex>(payload) ?? throw new Exception();
                    humidex.Time = DateTime.UtcNow;

                    await _influxDbService.WriteAsync(humidex);
                }
                catch (Exception)
                { }
            };

            // Connect to broker.
            var connectResult = await _client.ConnectAsync(mqttClientOptions, stoppingToken);
            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("MQTT client connected.");
            }
            else
            {
                Console.WriteLine("MQTT client unable to connect.");
                Console.WriteLine($"Code: connectResult.ResultCode");
            }

            // Subscribe to topics.
            foreach (var topic in _topics)
            {
                var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(topic);
                    })
                .Build();

                await _client.SubscribeAsync(mqttSubscribeOptions, stoppingToken);

                Console.WriteLine($"MQTT client subscribed to topic: {topic}");
            }
        }

        public override void Dispose()
        {
            _client.DisconnectAsync().Wait();

            base.Dispose();
        }
    }
}
