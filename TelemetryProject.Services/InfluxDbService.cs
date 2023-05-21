using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Linq;
using InfluxDB.Client.Writes;
using InfluxDB.Client;
using Common.Models;
using Microsoft.Extensions.Options;

namespace Services
{
    public class InfluxDbService : IInfluxDbService
    {
        private readonly InfluxDbSettings _options;

        public InfluxDbService(IOptions<InfluxDbSettings> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc/>
        public async Task WriteAsync(Humidex humidex)
        {
            using var client = new InfluxDBClient(_options.Url, _options.Token);

            var writeApi = client.GetWriteApiAsync();

            // Write by Point. The library also has methods for writing by LineProtocol.
            var point = PointData.Measurement("humidex")
                .Field(nameof(Humidex.Temperature), humidex.Temperature)
                .Field(nameof(Humidex.Humidity), humidex.Humidity)
                .Timestamp(humidex.Time, WritePrecision.Ns);
            
            await writeApi.WritePointAsync(point, _options.Bucket, _options.OrganizationId);

            // Write by POCO. Doesn't seem to work(?)
            //await writeApi.WriteMeasurementAsync(humidex, WritePrecision.Ns, _bucketName, _organizationId);
        }

        /// <inheritdoc/>
        public async Task<ICollection<Humidex>> ReadAllHumidexAsync()
        {
            /* Below is a cumbersome way of querying data not using LINQ. */
            using var client = new InfluxDBClient(_options.Url, _options.Token);
            var data = new Dictionary<string, List<(string Value, DateTime Time)>>();

            var flux = $"from(bucket:\"{_options.Bucket}\") |> range(start: 0)";

            var fluxTables = await client.GetQueryApi().QueryAsync(flux, _options.OrganizationId);
            fluxTables.ForEach(fluxTable =>
            {
                var fluxRecords = fluxTable.Records;
                fluxRecords.ForEach(fluxRecord =>
                {
                    string field = fluxRecord.GetField();
                    string? value = fluxRecord.GetValue().ToString();
                    DateTime time = fluxRecord.GetTime()?.ToDateTimeUtc() ?? new();

                    if (!data.ContainsKey(field))
                    {
                        data[field] = new();
                    }

                    if (value != null)
                    {
                        data[field].Add((value, time));
                    }
                });
            });

            List<Humidex> humidexes = new();
            int count = 0;
            if (data.Any()) count = data.First().Value.Count;

            for (int i = 0; i < count; i++)
            {
                var time = data[nameof(Humidex.Temperature)][i].Time;

                humidexes.Add(new()
                {
                    Temperature = float.Parse(data[nameof(Humidex.Temperature)][i].Value),
                    Humidity = float.Parse(data[nameof(Humidex.Humidity)][i].Value),
                    Time = time,
                });
            }

            return humidexes;
        }

        /// <inheritdoc/>
        public ICollection<Humidex> ReadAllHumidex()
        {
            using var client = new InfluxDBClient(_options.Url, _options.Token);
            var queryApi = client.GetQueryApiSync();

            var query = from s in InfluxDBQueryable<Humidex>.Queryable(_options.Bucket, _options.OrganizationId, queryApi) select s;

            return query.ToList();
        }

        /// <inheritdoc/>
        public ICollection<Humidex> ReadAllHumidex(DateTime startTime, DateTime endTime)
        {
            using var client = new InfluxDBClient(_options.Url, _options.Token);
            var queryApi = client.GetQueryApiSync();

            var query = from s in InfluxDBQueryable<Humidex>.Queryable(_options.Bucket, _options.OrganizationId, queryApi)
                        //where s.Time >= startTime && s.Time <= endTime
                        select s;

            var humidexes = query.ToList();

            return humidexes.Where(h => h.Time >= startTime && h.Time <= endTime).ToList();
        }

        /// <inheritdoc/>
        public Humidex? ReadLatestHumidex()
        {
            using var client = new InfluxDBClient(_options.Url, _options.Token);
            var queryApi = client.GetQueryApiSync();

            try
            {
                Humidex humidex = InfluxDBQueryable<Humidex>.Queryable(_options.Bucket, _options.OrganizationId, queryApi)
                    .OrderByDescending(h => h.Time)
                    .TakeLast(1)
                    .ToList().First();

                return humidex;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
