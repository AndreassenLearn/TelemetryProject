using Common.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;
using TelemetryProject.Services.SignalR;

namespace Services.MqttService
{
    public class MqttClientWorker : BackgroundService, IAsyncDisposable
    {
        private readonly List<string> _topics = new()
        {
            "arduino/dht/humidex"
        };

        readonly MqttFactory _mqttFactory;
        readonly IMqttClient _client;
        private readonly MqttSettings _options;
        private readonly IInfluxDbService _influxDbService;
        private readonly ISignalRService _signalRService;
        private readonly ILogger<MqttClientWorker> _logger;

        public MqttClientWorker(ILogger<MqttClientWorker> logger, IOptions<MqttSettings> options, IInfluxDbService influxDbService, ISignalRService signalRService)
        {
            _mqttFactory = new MqttFactory();
            _client = _mqttFactory.CreateMqttClient();
            _options = options.Value;
            _influxDbService = influxDbService;
            _signalRService = signalRService;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            await _client.DisconnectAsync();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Set up message handlng, connect to MQTT broker, and subscribe to topics.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_options.Address)
                .WithCredentials(_options.Username, _options.Password)
                .WithTls()
                .Build();

            // Setup message handling. This is done before connecting so that queued messages are also handled properly.
            _client.ApplicationMessageReceivedAsync += async e =>
            {
                string payload = Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
                _logger.LogInformation("Received message: {Payload}", payload);

                try
                {
                    Humidex humidex = JsonSerializer.Deserialize<Humidex>(payload) ?? throw new Exception("Unable to read humidex object from MQTT payload.");
                    humidex.Time = DateTime.UtcNow;

                    await _influxDbService.WriteAsync(humidex);
                    await _signalRService.PublishLiveHumidexAsync(humidex);
                }
                    catch (Exception ex)
                {
                    _logger.LogWarning("Warning: {Message}", ex.Message);
                }
            };

            // Connect to broker.
            var connectResult = await _client.ConnectAsync(mqttClientOptions, stoppingToken);
            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                _logger.LogInformation("MQTT client connected.");
            }
            else
            {
                _logger.LogWarning("MQTT client unable to connect. MQTT return code: {Code}", connectResult.ResultCode);
            }

            // Subscribe to topics.
            foreach (var topic in _topics)
            {
                var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f =>
                    {
                        f.WithTopic(topic);
                    })
                .Build();

                await _client.SubscribeAsync(mqttSubscribeOptions, stoppingToken);

                _logger.LogInformation("MQTT client subscribed to topic: {Topic}", topic);
            }
        }
    }
}
