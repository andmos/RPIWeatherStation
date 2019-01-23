FROM andmos/dotnet-script

COPY FetchOpenWeatherMapMeasures.csx FetchOpenWeatherMapMeasures.csx

CMD ["FetchOpenWeatherMapMeasures.csx"]