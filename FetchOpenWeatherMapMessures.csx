#! "netcoreapp2.1"

#r "nuget: Newtonsoft.Json, 10.0.3"
#r "nuget: System.Net.Http, 4.3.3"

using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;

private const string _apiBaseAddress = @"http://api.openweathermap.org/data/3.0/measurements";
private string _weatherServiceAPIKey => Environment.GetEnvironmentVariable("appId");
private string _type = "m";
private int _limit = 100;
private List<string> _stations = new List<string>
{
    "5bf05033199f0300011f2b9f"
};

if (Args.Any())
{
    long fromDate = long.Parse(Args[0]);

    long toDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var mesurementsJson = await GetMesurementsJson(fromDate, toDate, _stations.FirstOrDefault());

    var jsonObject = JArray.Parse(mesurementsJson);

    Console.WriteLine(mesurementsJson);
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