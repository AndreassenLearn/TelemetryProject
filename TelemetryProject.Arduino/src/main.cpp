/*
  This program connects to a MQTT broker and subscribes to multiple topics.
  It also publishes telemetry data in the form of temperature and humidity from a DHT sensor.

  Will run on the following hardware:
  - Arduino MKR 1000
  - MKR 1010
  - Uno WiFi Rev.2

  LED color status:
  - Red LED: Connecting to WiFi.
  - Blue LED: Connecting to MQTT broker (or Azure IoT Hub).
  - Green LED: Connected and sending telemetry data (humidex from DHT sensor).

  Before building and running:
  - Review preprocessor macros for broker and MQTT configuration. Just before and after includes respectively.
*/

// Broker configuration.
//#define AZURE
#define HIVE_MQ
#define USE_TLS true

#include <Arduino.h>
#include <Servo.h>
#include <DHT.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <ArduinoMqttClient.h>
#include <ArduinoJson.h>
#if defined(ARDUINO_SAMD_MKRWIFI1010) || defined(ARDUINO_SAMD_NANO_33_IOT) || defined(ARDUINO_AVR_UNO_WIFI_REV2)
  #include <WiFiNINA.h>
#elif defined(ARDUINO_SAMD_MKR1000)
  #include <WiFi101.h>
#elif defined(ARDUINO_ARCH_ESP8266)
  #include <ESP8266WiFi.h>
#elif defined(ARDUINO_PORTENTA_H7_M7) || defined(ARDUINO_NICLA_VISION) || defined(ARDUINO_ARCH_ESP32) || defined(ARDUINO_GIGA)
  #include <WiFi.h>
#endif
#ifdef AZURE
#include <ArduinoBearSSL.h>
#include <ArduinoECCX08.h>
#include <utility/ECCX08SelfSignedCert.h>
#endif

#include "arduino_secrets.h"

// MQTT configuration.
#define QOS_0 0
#define QOS_1 1
#ifdef AZURE
#define QOS_2 QOS_1 // Azure only supports QoS 1 as the highest quality setting.
#else
#define QOS_2 2
#endif

#define PERSISTENT_SESSION
#define SUBSCRIBE_TO_TOPICS

// Peripherals.
#define DHT_TYPE DHT11
#define DISPLAY_SCREEN_WIDTH 128
#define DISPLAY_SCREEN_HEIGHT 64
#define DISPLAY_OLED_RESET -1

const char SSID[] = SECRET_SSID; // Your network SSID (name)
const char PASSWORD[] = SECRET_PASSWORD; // Your network password (use for WPA, or use as key for WEP)

#if USE_TLS || defined(AZURE)
const int BROKER_PORT = 8883;
#else
const int BROKER_PORT = 1883;
#endif

#if defined(AZURE)
const char BROKER_ADDRESS[] = BROKER_AZURE_ADDRESS;
const String DEVICE_ID = BROKER_AZURE_DEVICE_ID;
#elif defined(HIVE_MQ)
const char BROKER_ADDRESS[] = BROKER_HIVE_MQ_ADDRESS;
const char BROKER_USERNAME[] = BROKER_HIVE_MQ_USERNAME;
const char BROKER_PASSWORD[] = BROKER_HIVE_MQ_PASSWORD;
#else
const char BROKER_ADDRESS[] = BROKER_PEACHRAPTOR_ADDRESS;
const char BROKER_USERNAME[] = BROKER_PEACHRAPTOR_USERNAME;
const char BROKER_PASSWORD[] = BROKER_PEACHRAPTOR_PASSWORD;
#endif

#if defined(AZURE)
const String TOPIC_PUBLISH = "devices/" + DEVICE_ID + "/messages/events/$.ct=application%2Fjson&$.ce=utf-8/";
const String TOPIC_SUBSCRIBE = "devices/" + DEVICE_ID + "/messages/devicebound/#";
const String TOPIC_HUMIDEX = TOPIC_PUBLISH;

const char TOPIC_LED[] = "led";
const char TOPIC_SERVO[] = "servo";
#else
const char TOPIC_HUMIDEX[] = "arduino/dht/humidex";
const char TOPIC_STATUS[] = "arduino/status";

const char TOPIC_LED[] = "arduino/led";
const char TOPIC_SERVO[] = "arduino/servo";
#endif

const int PIN_GREEN_LED = 25;
const int PIN_RED_LED = 26;
const int PIN_BLUE_LED = 27;

const int PIN_SERVO = 3;
const int PIN_DHT = 1;

const int INTERVAL_DHT = 30000;

#if USE_TLS && !defined(AZURE)
WiFiSSLClient wifiClient;
#else
WiFiClient wifiClient;
#endif

#ifdef AZURE
BearSSLClient sslClient(wifiClient);
MqttClient mqttClient(sslClient);
#else
MqttClient mqttClient(wifiClient);
#endif

Servo servo;
DHT dht(PIN_DHT, DHT_TYPE);
Adafruit_SSD1306 display(DISPLAY_SCREEN_WIDTH, DISPLAY_SCREEN_HEIGHT, &Wire, DISPLAY_OLED_RESET);

void InitializePeripherals();
#ifdef AZURE
unsigned long GetTime();
void InitializeCertificate();
#endif
void CheckAndConnectWifi();
void CheckAndConnectMqtt();
void Subscribe(String, uint8_t);
void OnMqttMessage(int);
void ReactLED(String);
void ReactServo(int);
void SendTemperatureAndHumidity();

int previousMillis = 0;

void setup()
{
  InitializePeripherals();

  Serial.begin(9600);
  while (!Serial); // Wait for serial port to connect.

#ifdef AZURE
  InitializeCertificate();
  mqttClient.setId(DEVICE_ID);

  // Set the username to "<broker>/<device id>/api-version=2018-06-30" and empty password.
  String username = BROKER_ADDRESS + String("/") + DEVICE_ID + String("/api-version=2018-06-30");
  mqttClient.setUsernamePassword(username, "");
#else
  // Explicitly set MQTT client ID. The default is Arduino-millis().
  mqttClient.setId("Arduino");

  // Username and password for authentication on the broker.
  mqttClient.setUsernamePassword(BROKER_USERNAME, BROKER_PASSWORD);

#ifdef PERSISTENT_SESSION
  // Set persistent session. The default is a clean session.
  mqttClient.setCleanSession(false);
#endif

  // Configure LWT (Last Will and Testament).
  String willPayload = "offline";
  mqttClient.beginWill(TOPIC_STATUS, willPayload.length(), true, QOS_2);
  mqttClient.print(willPayload);
  mqttClient.endWill();
#endif

  // Set message receive callback.
  mqttClient.onMessage(OnMqttMessage);
}

void loop()
{
  CheckAndConnectWifi();
  CheckAndConnectMqtt();

  // Call poll() regularly to allow the library to receive MQTT messages and send MQTT keep alives which avoids being disconnected by the broker.
  mqttClient.poll();

  int currentMillis = millis();
  if (currentMillis - previousMillis >= INTERVAL_DHT)
  {
    previousMillis = currentMillis;
    SendTemperatureAndHumidity();
  }
}

void InitializePeripherals()
{
  // RGB LED on board.
  WiFiDrv::pinMode(PIN_GREEN_LED, OUTPUT);
  WiFiDrv::pinMode(PIN_RED_LED, OUTPUT);
  WiFiDrv::pinMode(PIN_BLUE_LED, OUTPUT);

  // Servo.
  servo.attach(PIN_SERVO);
  servo.write(0);

  // DHT (Temperature and humidity).
  dht.begin();
}

#ifdef AZURE
unsigned long GetTime() 
{ 
  return WiFi.getTime();
}

void InitializeCertificate()
{
  if (!ECCX08.begin())
  {
    Serial.println("No ECCX08 present!");
    while (true); // Stop here.
  }

  // Set a callback to get the current time. This is used to validate the certificate on the server when using TLS.
  ArduinoBearSSL.onGetTime(GetTime);

  // Reconstruct the self signed certificate.
  ECCX08SelfSignedCert.beginReconstruction(0, 8);
  ECCX08SelfSignedCert.setCommonName(ECCX08.serialNumber());
  ECCX08SelfSignedCert.endReconstruction();

  // Set the ECCX08 slot to use for the private key and the accompanying public certificate for it.
  sslClient.setEccSlot(0, ECCX08SelfSignedCert.bytes(), ECCX08SelfSignedCert.length());
}
#endif

void CheckAndConnectWifi()
{
  if (WiFi.status() == WL_CONNECTED) return;

  WiFiDrv::digitalWrite(PIN_GREEN_LED, LOW);
  WiFiDrv::digitalWrite(PIN_RED_LED, HIGH);
  WiFiDrv::digitalWrite(PIN_BLUE_LED, LOW);

  // Connect to WiFi network.
  Serial.print("Attempting to connect to SSID: ");
  Serial.print(SSID);
  Serial.print(" ");
  while (WiFi.begin(SSID, PASSWORD) != WL_CONNECTED)
  {
    // Failed; retry in 5 sec.
    Serial.print(".");
    delay(5000);
  }
  Serial.println();
  Serial.println("You're connected to the network.");
}

void CheckAndConnectMqtt()
{
  if (mqttClient.connected()) return;

  WiFiDrv::digitalWrite(PIN_GREEN_LED, LOW);
  WiFiDrv::digitalWrite(PIN_RED_LED, LOW);
  WiFiDrv::digitalWrite(PIN_BLUE_LED, HIGH);

  // Connect to MQTT broker.
  Serial.print("Attempting to MQTT broker: ");
  Serial.print(BROKER_ADDRESS);
  Serial.print(" ");

  while (!mqttClient.connect(BROKER_ADDRESS, BROKER_PORT))
  {
    // Failed; retry in 5 sec.
    Serial.print(".");
    delay(5000);
  }
  Serial.println();
  Serial.println("You're connected to the MQTT broker.");

  WiFiDrv::digitalWrite(PIN_GREEN_LED, HIGH);
  WiFiDrv::digitalWrite(PIN_RED_LED, LOW);
  WiFiDrv::digitalWrite(PIN_BLUE_LED, LOW);

#ifndef AZURE
  // Set status
  mqttClient.beginMessage(TOPIC_STATUS, true, QOS_1);
  mqttClient.print("online");
  mqttClient.endMessage();
#endif

  // Subscribe to topics.
#if defined(AZURE)
  mqttClient.subscribe(TOPIC_SUBSCRIBE);
#elif defined(SUBSCRIBE_TO_TOPICS)
  Subscribe(TOPIC_LED, QOS_0);
  Subscribe(TOPIC_SERVO, QOS_0);
#endif
}

void SendTemperatureAndHumidity()
{
  float temperature = dht.readTemperature();
  float humidity = dht.readHumidity();

  //Serial.print("Temperature: ");
  //Serial.println(temperature);
  //Serial.print("Humidity: ");
  //Serial.println(humidity);

  // Create JSON.
  StaticJsonDocument<200> doc;
  doc["Temperature"] = temperature;
  doc["Humidity"] = humidity;

  // Send message.
  mqttClient.beginMessage(TOPIC_HUMIDEX, false);
  serializeJson(doc, mqttClient);
  mqttClient.endMessage();
}

#ifndef AZURE
void Subscribe(String topic, uint8_t qos)
{
  Serial.print("Subscribing to topic: ");
  Serial.println(topic);

  mqttClient.subscribe(topic, qos);
}
#endif

void OnMqttMessage(int messageSize)
{
  String msgTopic = mqttClient.messageTopic();
  Serial.println("[Received Message]");
  Serial.print("Topic: ");
  Serial.println(msgTopic);
  String msg = mqttClient.readString();
  Serial.println("Payload: ");
  Serial.println(msg);

#ifdef AZURE
  StaticJsonDocument<200> doc;
  deserializeJson(doc, msg);
  const char* ledValue = doc["led"];
  int servoValue = doc["servo"];

  ReactLED(ledValue);
  ReactServo(servoValue);
#else
  if (msgTopic == TOPIC_LED)
  {
    ReactLED(msg);
  }
  else if (msgTopic == TOPIC_SERVO)
  {
    ReactServo(msg.toInt());
  }
#endif
}

void ReactLED(String msg)
{
  Serial.print("ReactLED() parameters: ");
  Serial.println(msg);

  if (msg == "on")
  {
    digitalWrite(LED_BUILTIN, HIGH);
  }
  else if (msg == "off")
  {
    digitalWrite(LED_BUILTIN, LOW);
  }
}

void ReactServo(int pos)
{
  Serial.print("ReactServo() parameters: ");
  Serial.println(pos);

  servo.write(pos);
}
