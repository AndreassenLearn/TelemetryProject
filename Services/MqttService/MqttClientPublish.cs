using MQTTnet.Client;
using MQTTnet;

namespace Services.MqttService
{
    public class MqttClientPublish : IMqttClientPublish
    {
        private const string _brokerAddress = "eba7082725f24cff9cd6bd0068d8ae35.s2.eu.hivemq.cloud";
        private const string _brokerUsername = "MQTTnet";
        private const string _brokerPassword = "P@ssw0rd";

        private const string _topic = "arduino/servo";

        public async Task ServoAsync(ushort position)
        {
            var mqttFactory = new MqttFactory();
            using var mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_brokerAddress)
                .WithCredentials(_brokerUsername, _brokerPassword)
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
