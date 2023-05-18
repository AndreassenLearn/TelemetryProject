using MQTTnet.Client;
using MQTTnet;
using Microsoft.Extensions.Options;

namespace Services.MqttService
{
    public class MqttClientPublish : IMqttClientPublish
    {
        private const string _topic = "arduino/servo";

        private readonly MqttSettings _options;

        public MqttClientPublish(IOptions<MqttSettings> options)
        {
            _options = options.Value;
        }

        public async Task ServoAsync(ushort position)
        {
            var mqttFactory = new MqttFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_options.Address)
                .WithCredentials(_options.Username, _options.Password)
                .WithTls()
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(_topic)
                .WithPayload(position.ToString())
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            await mqttClient.DisconnectAsync();

            Console.WriteLine("MQTT application message is published.");
        }
    }
}
