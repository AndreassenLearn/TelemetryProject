using MQTTnet.Client;
using MQTTnet;
using Microsoft.Extensions.Options;

namespace TelemetryProject.Services.MqttService
{
    public class MqttClientPublish : IMqttClientPublish, IAsyncDisposable
    {
        private const string _servoTopic = "arduino/servo";
        private const string _ledTopic = "arduino/led";

        private readonly MqttSettings _options;
        private readonly IMqttClient _mqttClient;

        public MqttClientPublish(IOptions<MqttSettings> options)
        {
            _options = options.Value;
            _mqttClient = new MqttFactory().CreateMqttClient();
        }

        public async ValueTask DisposeAsync()
        {
            await _mqttClient.DisconnectAsync();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async Task ServoAsync(ushort position)
        {
            await EnsureConnected();

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(_servoTopic)
                .WithPayload(position.ToString())
                .Build();

            await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task LedAsync(bool state)
        {
            await EnsureConnected();

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(_ledTopic)
                .WithPayload(state ? "on" : "off")
                .Build();

            await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        }

        /// <summary>
        /// Connect to MQTT broker.
        /// </summary>
        /// <returns></returns>
        private async Task EnsureConnected()
        {
            if (_mqttClient.IsConnected) return;

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Address)
                .WithCredentials(_options.Username, _options.Password);
            if (_options.UseTls)
            {
                mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithTls();
            }

            await _mqttClient.ConnectAsync(mqttClientOptionsBuilder.Build(), CancellationToken.None);
        }
    }
}
