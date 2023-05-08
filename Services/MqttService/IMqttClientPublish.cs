namespace Services.MqttService
{
    public interface IMqttClientPublish
    {
        /// <summary>
        /// Set servo position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Task ServoAsync(ushort position);
    }
}
