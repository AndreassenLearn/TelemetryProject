using MQTTnet.Client;
using MQTTnet;
using Microsoft.Extensions.Options;

namespace Services.MqttService
{
    public class MqttClientPublish : IMqttClientPublish, IDisposable
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

        public void Dispose()
        {
            _mqttClient.DisconnectAsync().GetAwaiter().GetResult();
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

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_options.Address)
                .WithCredentials(_options.Username, _options.Password)
                .WithTls()
                .Build();

            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        }
    }
}
