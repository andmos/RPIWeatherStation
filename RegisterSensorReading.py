#!/usr/bin/python

import Adafruit_DHT
import json
import urllib2
import os.path
import time

registeredStationFile = 'station.json'
openWeatherMapEndpoint = 'http://api.openweathermap.org/data/3.0/measurements?appid='

# Sensor should be set to Adafruit_DHT.DHT11,
# Adafruit_DHT.DHT22, or Adafruit_DHT.AM2302.
sensor = Adafruit_DHT.DHT22

# Example using a Raspberry Pi with DHT sensor
# connected to GPIO23.
pin = 4

# Try to grab a sensor reading.  Use the read_retry method which will retry up
# to 15 times to get a sensor reading (waiting 2 seconds between each retry).
humidity, temperature = Adafruit_DHT.read_retry(sensor, pin)

# Note that sometimes you won't get a reading and
# the results will be null (because Linux can't
# guarantee the timing of calls to read the sensor).
# If this happens try again!
if humidity is None and temperature is None:
    print('Failed to get reading. Try again!')

if os.path.isfile(registeredStationFile) == True:
    station = json.loads(open(registeredStationFile).read())
    id = station['ID']
    appid = station['appid']
    stationReading = '[ {{ "station_id": "{0}", "dt": {1}, "temperature": {2:0.1f}, "humidity": {3:01f}}} ]'.format(id,time.time(),temperature,humidity)
    openWeatherMapEndpoint = openWeatherMapEndpoint + appid
    request = urllib2.Request(openWeatherMapEndpoint)
    request.add_header('Content-Type', 'application/json')
    response = urllib2.urlopen(request, stationReading)
    responseJson = '{{ "response_code": {0} }}'.format(response.getcode())

    print(stationReading)
    print(responseJson)

else:
    tempjson = '[{{"temp": {0:0.1f}, "hum": {1:0.1f}}} ]'.format(temperature, humidity)
    print(tempjson)