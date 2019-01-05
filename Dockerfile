FROM andmos/dotnet-script

COPY FetchOpenWeatherMapMessures.csx FetchOpenWeatherMapMessures.csx

CMD ["FetchOpenWeatherMapMessures.csx"]