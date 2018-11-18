# RPIWeatherStation
Sample code for my RaspberryPi weather setup that registers data to OpenWeatherMaps. 

based on: https://www.instructables.com/id/Raspberry-PI-and-DHT22-temperature-and-humidity-lo/ 
with the [DHT22 2302](https://www.gearbest.com/goods/pp_009360784166.html?lang=en&wid=1433363&utm_source=email_sys&utm_medium=mail&utm_campaign=GB_GB_orderShippedOut_180924) temperature and humidity sensor.

## Register new Pi: 

Use `Ansible`, or the manual setup: 

```
cd /home/pi
git clone https://github.com/adafruit/Adafruit_Python_DHT.git
sudo apt-get install build-essential python-dev python-openssl
cd Adafruit_Python_DHT
sudo python setup.py install
cd examples
# drop RegisterSensorReading.py here 
```
Create a new [OpenWeatherMapStation](https://openweathermap.org/stations#main) and drop response to `station.json` - and append a field, `"appid":` to the JSON file. 

Add `RegisterSensorReading.py` to crontab. 
Reading GPIO might need root.