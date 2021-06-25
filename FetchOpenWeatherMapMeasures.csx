#r "nuget: Newtonsoft.Json, 12.0.3"
#r "nuget: System.Net.Http, 4.3.3"
#r "nuget: InfluxDB.Net.Core, 1.1.22-beta"

using System.Net.Http;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using InfluxDB.Net.Helpers;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Infrastructure.Configuration;

private const string _apiBaseAddress = @"http://api.openweathermap.org/data/3.0/measurements";
private const string _databaseName = "WeatherSensorMeasurements";
private string _weatherServiceAPIKey => Environment.GetEnvironmentVariable("appId");
private string _type = "m";
private int _limit = 2048;
private int _defaultNumberOfHours = 2;
private List<string> _stations = new List<string>
{
    "5bf05033199f0300011f2b9f"
};

private string _influxDbConnectionString = @"http://database:8086";

if(string.IsNullOrWhiteSpace(_weatherServiceAPIKey))
{
    Console.WriteLine("'appId' is empty. export it as env variable.");
    return 1; 
}

var measurementsJson = await GetMeasurementsJson(GetFromDate(), DateTimeOffset.UtcNow.ToUnixTimeSeconds(), _stations.FirstOrDefault());
var sensorMeasurements = JsonConvert.DeserializeObject<IEnumerable<WeatherSensorMeasurement>>(measurementsJson);

Console.WriteLine(measurementsJson);

await WriteMeasurementsToTimeSeriesDb(_influxDbConnectionString, sensorMeasurements);

private long GetFromDate()
{
    if (Args.Any())
    {
        _defaultNumberOfHours = int.Parse(Args[0]);
    }
    return DateTimeOffset.UtcNow.AddHours(- _defaultNumberOfHours).ToUnixTimeSeconds();
}

private async Task<string> GetMeasurementsJson(long fromDate, long toDate, string station)
{
    List<string> jsonRespons = new List<string>();
    using (var client = new HttpClient())
    {
        var response = await client.GetAsync($"{_apiBaseAddress}?" +
                                             $"type={_type}" +
                                             $"&station_id={station}" +
                                             $"&APPID={_weatherServiceAPIKey}" +
                                             $"&limit={_limit}" +
                                             $"&from={fromDate}" +
                                             $"&to={toDate}");
        if (!response.IsSuccessStatusCode)
        {
            throw new NotImplementedException($"Could not find any weather data, {_apiBaseAddress} returned status code {response.StatusCode}");
        }
        return await response.Content.ReadAsStringAsync();
    }
}

private async Task WriteMeasurementsToTimeSeriesDb(string dbConnectionString, IEnumerable<WeatherSensorMeasurement> measurements)
{
	var client = new InfluxDb(dbConnectionString, "root", "root");
  	Pong pong = await client.PingAsync();
	if(!pong.Success)
	{
		Console.WriteLine($"could not connect to database at {dbConnectionString}");
		return;
	}
	var response = await  client.CreateDatabaseAsync(_databaseName);
	
	foreach(var measurement in measurements)
	{
		var point = new Point();
		point.Tags.Add("stationId", measurement.StationId);
		point.Fields.Add("averageTemperature", measurement.Temp.Average);
		point.Fields.Add("averageHumidity", measurement.Humidity.Average);
		point.Measurement = "WeatherSensorMeasurement";
		point.Timestamp = measurement.TimeStamp;

		InfluxDbApiResponse writeResponse = await client.WriteAsync(_databaseName, point);
	}
}

public class WeatherSensorMeasurement
{
    [JsonProperty("station_id")]
    public string StationId { get; set; }
    [JsonProperty("date"), JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime TimeStamp { get; set; }
    [JsonProperty("temp")]
    public Temperature Temp { get; set; }
    [JsonProperty("humidity")]
    public Humidity Humidity { get; set; }
}

public class Temperature
{
    [JsonProperty("min")]
    public double Min { get; set; }
    [JsonProperty("max")]
    public double Max { get; set; }
    [JsonProperty("average")]
    public double Average { get; set; }
}

public class Humidity
{
    [JsonProperty("average")]
    public double Average { get; set; }
}
