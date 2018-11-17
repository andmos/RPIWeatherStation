# RPIWeatherStation
Sample code for my RaspberryPi weather setup that registers data to OpenWeatherMaps. 

based on: https://www.instructables.com/id/Raspberry-PI-and-DHT22-temperature-and-humidity-lo/ 

## Register new Pi: 

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