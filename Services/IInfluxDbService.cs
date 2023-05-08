using Services.Models;

namespace Services
{
    public interface IInfluxDbService
    {
        /// <summary>
        /// Save humidex in the database.
        /// </summary>
        /// <param name="humidex">Temperature and humidity.</param>
        public Task WriteAsync(Humidex humidex);

        /// <summary>
        /// Get all humidex values from the databse.
        /// </summary>
        /// <returns></returns>
        public Task<ICollection<Humidex>> ReadAllHumidexAsync();
    }
}
