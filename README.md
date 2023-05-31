# TelemetryProject
This project demonstrates how both telemetry, state, and configuration data tied to an IoT device can be viewed and communicated across networks from a user client application. Key technologies and services used are:
* **MQTT** with HiveMQ as the broker.
* **InfluxDB** for storing telemetry data.
* **.NET MAUI** for the client application.
    * **Polly** for resilience.
* **.NET minimal API** for bridging all of the above.

## Requirements
- [x] Display last humidex measurement and time of measurement in local time.
- [x] Display a graph over humidex measurements with selectable time interval.
- [x] Control servo position.
- [x] Control LED state.
- [x] Use MVVM design pattern with DI.
- [x] Show last received data when the internet connection is down.
- [x] Resilient against unstable network connections.
- [x] Live measurement sessions.
- [ ] Set min. and max. humidex values and get notified when the range is exceeded.

## Architecture
Here, the flow between all parts (both in- and outside this repository; i.e. HiveMQ and InfluxDB) is depicted with a flow diagram. In order to keep the diagram simple, descriptions of what's inferred to with for instance 'Telemetry' can be found below the diagram along with a full description of the entire flow.

```mermaid
flowchart TB
    subgraph C[Web API]
        CA([MqttClientWorker])-->CB([InfluxDbService])
        CC([MqttClientPublish])
        linkStyle 0 stroke:lightblue
    end

    subgraph A[Arduino MKR 1010 WiFi]
        direction TB
        AA{{DHT11}}
        AB{{Servo}}
        AC{{LED}}
    end

    A--"Telemetry"-->B["HiveMQ
    (MQTT Broker)"]
    B--"Status"-->A

    B-->CA
    CC-->B

    CB<-->D[(InfluxDB)]
    
    CB-.->E["MauiClient
    (.NET MAUI)"]
    E-.->CC

    linkStyle 1,3,5,6 stroke:lightblue
    linkStyle 2,4,7 stroke:pink
```

### Telemetry
Telemetry data is sent via all the **light blue** links. The telemetry data is a humidex (temperature and humidity) measured by the DHT11 sensor attached to the Arduino board.

1. The Arduino publishes an MQTT message with a JSON object as the payload to the topic; `arduino/dht/humidex`.
2. The `MqttClientWorker` running as a background service within the web API is subscribed to the `arduino/dht/humidex` topic. It recieves the MQTT message as the broker (HiveMQ) relays it to all subscribers.
3. If the payload of the message can be correctly parsed as a `Humidex` object, the web API marks it with a timestamp and sends it of to be stored in an external InfluxDB via the `InfluxDbService`.
4. Once humidex data is requested (i.e. from the MauiClient) using a `GET` endpoint, it is queried from the InfluxDB and responded with to the requesting client.

### Status
Status is sent via all the **pink** links. In the context of this project *status* is communication for controlling the LED and servo attached to the Arduino board (see [MQTT Payloads](#mqtt-payloads)).

1. Once a client (i.e. from the MauiClient) uses a `POST` endpoint to send status information, an MQTT message with a payload equivalent to the `POST` method is created.
2. Using the `MqttClientPublish` class, the web API publishes the MQTT message to the topic; `arduino/led`, `arduino/servo`, or just `arduino/status` (see [Known Limitations & Issues](#known-limitations-issues)).
3. The Arduino is subscribed to all the topics of the previous step and will as a result; receive the message, read the topic and payload, and act accordingly.

## API Overview
| API | Description | Request body | Reponse body | Codes |
|--------|-------------|--------------|--------------|-------|
| `POST /servo/{position}` | Set the servo position (0-180 degrees) | None | None | `200 OK` |
| `POST /led/{state}` | Turn LED on or off ("on"/"off") | None | None | `200 OK`, *Fake `500 Internal Server Error` 50% of the time* |
| `GET /humidex` | Get all humidexes | None | Array of humidexes | `200 OK` |
| `GET /humidex/{startTime}/{endTime}` | Get all humidexes between a specific time interval | None | Array of humidexes | `200 OK` |
| `GET /humidex/latest` | Get the latest humidex | None | Single humidex | `200 OK`, `404 Not Found` |

## MQTT Payloads
### Topic: `arduino/dht/humidex`
This is where the Arduino board publishes the telemetry data. The `MqttClientWorker` running in the background of the web API is subscibed to this topic and will store the received data in the InfluxDB using the `InfluxDbService`.
``` JSON
{
  "Temperature": 12.34,
  "Humidity": 12.34
}
```

### Topic: `arduino/led`
The Arduino board is subscribed to this topic. If the payload equals "on" the LED will turn on. Likewise, if the payload equals "off" it will turn off.
```
on
```

### Topic: `arduino/servo`
The Arduino board is subscribed to this topic. It will attempt to parse the payload as an integer and set the servo position to this value. The range is **0** to **180** degrees.
```
123
```

## MAUI Client Application (MauiClient)
### Hamburger Menu or Rather; Lack Thereof
The MauiClient doesn't feature a hamburger menu (or flyout) in the top left corner as is sometimes seen in other GUIs. A hamburger menu is great for keeping navigation options close by when screen space is limited. The function of this UI element is also widely understood. However, when hiding information away from the user, it unsurprisingly becomes less visible, thus making it harder to find information at first glance. Moreover, the position in the top left corner makes the menu hard to reach on a mobile phone. Opening a hamburger menu, therefore requires a certain will from the user to go explore options in the UI for themselves.

The MauiClient instead uses a tab bar in the bottom of the screen with icons and text. This keeps navigation options clearly visible and accessible to the user and only requires a single click/tab, whereas a hamburger menu would require at least two in addition to an awkward reach to the top left corner of the screen.

Currently, there aren't enough pages to justify using the build-in 'More' option of the tab bar. However, if there were to become more, this feature would provide the utility of a traditional hamburger menu, while keeping the ease of access a tab bar provides. Essentially, non of the mentioned drawbacks of a hamburger menu are derived as these are overridden by still having a tab bar.

## Blazor Server Application (BlazorClient)
### Auth0
Authentication has been added using [this](https://auth0.com/blog/what-is-blazor-tutorial-on-building-webapp-with-authentication/) guide.

The following steps must be followed before running the Blazor application.

1. Go to your [Auth0 dashboard](https://manage.auth0.com/) and add a new 'Regular Web Application' with an appropriate name.
2. Go to the settings tab of the newly registered application.
    * Take of note of 'Domain' and 'Client ID'.
3. Add `https://localhost:<PORT>/callback` the 'Allowed Callback URLs' and `https://localhost:<PORT>/` to the 'Allowed Logout URLs' replacing `<PORT>` with the port number of the Blazor application.
4. In the `appsettings.json` file of the BlazorClient project; insert the Auth0 'Domain' and 'Client ID' under the 'Auth0' section.

> The current implementation of Auth0 doesn't play nicely with dev tunnels. Therefore, dev tunnels cannot be used for running the BlazorClient. However, the BlazorClient can communicate with an API running through a dev tunnel without any issues.

## Usage
To get the project running in a new environment, the following steps must be taken.

1. Visit [HiveMQ Cloud](https://www.hivemq.com/mqtt-cloud-broker/); create an account and new a cluster.
    - Take note of 'Cluster URL'.
2. In HiveMQ Cloud; create a new set of access credentials with 'Publish and Subsribe' permission and note down the 'Username' and 'Password'.
3. Visit [InfluxDB Cloud](https://www.influxdata.com/products/influxdb-cloud/); create an account, an organization, and a new bucket.
    - Take note of 'Organization ID', bucket name and 'Cluster URL (Host Name)'.
4. In InfluxDB Cloud; create a new 'All Access API Token' and note it down.
5. In the `appsettings.json` file of the WebApi project; insert all information from the above.
6. Verify the [embedded hardware](#embedded-hardware).
7. In the `arduino_secrets.json` file found in the 'include' directory of the embedded application; insert HiveMQ information in all the macro definitions stating with "BROKER_HIVE_MQ".
8. Again in the `arduino_secrets.json` file; insert WiFi SSID and password (`SECRET_SSID` and `SECRET_PASSWORD`) for the network the Arduino board will be connecting to.
9. Start the web API (WebApi).
10. Start the Arduino and wait for it to connect to the MQTT broker. The tricolor LED will turn green upon connection establishment.

### *using MAUI*
11. Start the MAUI application (MauiClient).

### *using Blazor*
11. Follow the steps to setup [Auth0](#auth0).
12. Start the Blazor application (BlazorClient).

### Embedded Hardware
The embebbed code is designed and tested to run on an Arduino MKR 1010 WiFi with a DHT11 and servo connected. When using the default pin configuration of the embedded program, the peripherals should be connected as described below.

* DHT11: D1 (PA23)
* Servo: D3 (PA11)

## Known Limitations & Issues
* The current implementation of the project only allows for controlling and monitoring a single Arduino board. Connecting to multiple boards is outside the scope of this project. However, it could be accomplished by assigning an ID to each board and then include it in the payload of the MQTT messages targeted towards specific boards. When using Azure IoT Hub, the embedded application is already able to receive messages of the following format.
    ``` JSON
    {
      "led": "on",
      "servo": 123
    }
    ```
    When using the raw MQTT implementation, the program could be updated to use message payloads like the one below. This could be published to the `arduino/status` topic. Each board will then be able to react to only messages assigned with their ID.
    ``` JSON
    {
      "ids": [ "01", "02" ],
      "led": "on",
      "servo": 123
    }
    ```
    Furthermore, the format for storing humidex measurements would have to be reconsidered, as each measurement must be back traceable to the board it originated from in order to determine which location the measurement is valid for. This would likely be overcome by including a board ID and/or location record for all measurements in InfluxDB.

### WebApi
* In the `ReadAllHumidex(DateTime startTime, DateTime endTime)` method of the `InfluxDbService`, records are not filtered by date and time during execution of the database query. This only happens after all the data has been queried. Therefore, poor performance is to be expected.

### MauiClient
* The client application only runs on Android.

## Change Log
### v1.0.0
* Initial version.
