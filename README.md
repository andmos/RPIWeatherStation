# RPIWeatherStation
Sample code for Raspberry Pi weather setup that registers data to OpenWeatherMap. 

Based on [this tutorial](https://www.instructables.com/id/Raspberry-PI-and-DHT22-temperature-and-humidity-lo/) 
with the [DHT22 2302](https://www.gearbest.com/goods/pp_009360784166.html?lang=en&wid=1433363&utm_source=email_sys&utm_medium=mail&utm_campaign=GB_GB_orderShippedOut_180924) temperature and humidity sensor.

Register new Pi with the Ansible Playbook.

### Dashboard 
The `docker-compose.yml` file contains a simple setup with InfluxDb and Grafana to show data. `FetchOpenWeatherMapMeasures.csx` pumps data from OpenWeatherMap to influx.