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

        /// <summary>
        /// Turn LED on or off.
        /// </summary>
        /// <param name="state">True is on and false is off.</param>
        /// <returns></returns>
        public Task LedAsync(bool state);
    }
}
