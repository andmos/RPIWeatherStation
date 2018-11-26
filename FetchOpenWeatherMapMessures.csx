#! "netcoreapp2.1"

#r "nuget: Newtonsoft.Json, 11.0.2"
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
private string _weatherServiceAPIKey => Environment.GetEnvironmentVariable("appId");
private string _type = "m";
private int _limit = 400;
private List<string> _stations = new List<string>
{
    "5bf05033199f0300011f2b9f"
};

private string _influxDbConnectionString = @"http://127.0.0.1:8086";

if (Args.Any())
{
    long fromDate = long.Parse(Args[0]);

    long toDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var mesurementsJson = await GetMesurementsJson(fromDate, toDate, _stations.FirstOrDefault());

    var sensorMessurements = JsonConvert.DeserializeObject<IEnumerable<WeatherSensorMessurement>>(mesurementsJson);

    Console.WriteLine(mesurementsJson);

	await WriteMesurementsToTimeSeriesDb(_influxDbConnectionString, sensorMessurements);

}

private async Task<string> GetMesurementsJson(long fromDate, long toDate, string station)
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

private async Task WriteMesurementsToTimeSeriesDb(string dbConnectionString, IEnumerable<WeatherSensorMessurement> messurements)
{
	var client = new InfluxDb(dbConnectionString, "root", "root");
  	Pong pong = await client.PingAsync();
	if(!pong.Success)
	{
		Console.WriteLine($"could not connect to database at {dbConnectionString}");
		return;
	}
	var response = await  client.CreateDatabaseAsync("WeatherSensorMessurements");
	
	foreach(var messurement in messurements)
	{
		var poin = new Point();
		poin.Tags.Add("stationId", messurement.StationId);
		poin.Fields.Add("averageTemperature", messurement.Temp.Average);
		poin.Fields.Add("averageHumidity", messurement.Humidity.Average);
		poin.Measurement = "WeatherSensorMessurement";
		poin.Timestamp = messurement.TimeStamp;

		InfluxDbApiResponse writeResponse =await client.WriteAsync("WeatherSensorMessurements", poin);
	}
}

public class WeatherSensorMessurement
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