#include <Arduino.h>
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <freertos/semphr.h>

#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <DHT_U.h>

#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>
#include <time.h>

#define DHTPIN 4
#define DHTTYPE DHT11
#define LUMIPIN 34

DHT_Unified dht(DHTPIN, DHTTYPE);

// WiFi
const char *ssid = ""; // Enter your Wi-Fi name
const char *password = "";  // Enter Wi-Fi password

// MQTT Broker
const char *mqtt_broker = "5c4d923ba33549faa5f72062b1fac92e.s1.eu.hivemq.cloud";
const char *topic = "/sensor_monitors";
const char *mqtt_username = "admin";
const char *mqtt_password = "YzRPJJ@g6cubt1";
const int mqtt_port = 8883;

const char *machineId = "Maquina1";

const char *topicTemp = "/sensor_monitors/Maquina1/Temperature001";
int intervalTemp = 1000;
const char *topicHumi = "/sensor_monitors/Maquina1/Humidity001";
int intervalHumi = 2000;
const char *topicLumi = "/sensor_monitors/Maquina1/Luminosity001";
int intervalLumi = 3000;

WiFiClientSecure espClient;
PubSubClient client(espClient);

sensors_event_t event;
SemaphoreHandle_t xMutex;

void SensorTemp(void *arg) {
  while(1) {
      if (xSemaphoreTake(xMutex, portMAX_DELAY) == pdTRUE) {
          dht.temperature().getEvent(&event);
          if (isnan(event.temperature)) {
              Serial.println(F("Error reading temperature!"));
          } else {
              Serial.print(F("Temperature: "));
              Serial.print(event.temperature);
              Serial.println(F("°C"));
          }
          String message = "{\"timestamp\":\"" + getFormattedTime() + "\",\"value\":" + String(event.temperature) + "}";
          bool result = client.publish(topicTemp, message.c_str());
          xSemaphoreGive(xMutex);
          vTaskDelay(intervalTemp / portTICK_PERIOD_MS);
      }
  }
}

void SensorHumi(void *arg) {
  while(1) {
      if (xSemaphoreTake(xMutex, portMAX_DELAY) == pdTRUE) {
          dht.humidity().getEvent(&event);
          if (isnan(event.relative_humidity)) {
              Serial.println(F("Error reading humidity!"));
          } else {
              Serial.print(F("Humidity: "));
              Serial.print(event.relative_humidity);
              Serial.println(F("%"));
          }
          String message = "{\"timestamp\":\"" + getFormattedTime() + "\",\"value\":" + String(event.relative_humidity) + "}";
          bool result = client.publish(topicHumi, message.c_str());
          xSemaphoreGive(xMutex);
          vTaskDelay(intervalHumi / portTICK_PERIOD_MS);
      }
  }
}

void SensorLumi(void *arg) {
  while(1) {
      if (xSemaphoreTake(xMutex, portMAX_DELAY) == pdTRUE) {
          Serial.print("Luminosity: ");

          double medida = analogRead(LUMIPIN);

          double percentualLuz = -(100.0/4095.0) * medida + 100;
          
          Serial.println(percentualLuz);
          String message = "{\"timestamp\":\"" + getFormattedTime() + "\",\"value\":" + String(percentualLuz) + "}";
          bool result = client.publish(topicLumi, message.c_str());
          xSemaphoreGive(xMutex);
          vTaskDelay(intervalLumi / portTICK_PERIOD_MS);
      }
  }
}

String getFormattedTime() {
  struct tm timeinfo;
  if (!getLocalTime(&timeinfo)) {
      Serial.println("Failed to obtain time");
      return "N/A";
  }
  
  char timeStringBuff[50]; // buffer to hold the formatted time
  strftime(timeStringBuff, sizeof(timeStringBuff), "%Y-%m-%dT%H:%M:%S", &timeinfo);
  return String(timeStringBuff);
}

void setup(){
  Serial.begin(115200);

  pinMode(LUMIPIN, INPUT);
  dht.begin();

  // Create mutex
  xMutex = xSemaphoreCreateMutex();
  if (xMutex == NULL) {
      Serial.println("Mutex creation failed!");
      while (1);
  }

  // Configuração para ignorar a verificação de certificado (não recomendado para produção)
  espClient.setInsecure();

  // Connecting to a WiFi network
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
      delay(500);
      Serial.println("Connecting to WiFi..");
  }
  
  Serial.println("Connected to the Wi-Fi network");

  // Initialize NTP
  configTime(0, 0, "pool.ntp.org", "time.nist.gov"); // UTC time
  setenv("TZ", "UTC0", 1);  // Ensure time is in UTC
  tzset();

  // Configurando o broker MQTT
  client.setServer(mqtt_broker, mqtt_port);
  client.setBufferSize(512);
  //client.setCallback(callback);
  
  // Tentativa de conexão ao broker MQTT
  while (!client.connected()) {
      String client_id = "esp32-client-";
      client_id += String(WiFi.macAddress());
      Serial.printf("The client %s connects to the public MQTT broker\n", client_id.c_str());
      if (client.connect(client_id.c_str(), mqtt_username, mqtt_password)) {
          Serial.println("Public HiveMQ MQTT broker connected");
      } else {
          Serial.print("Failed with state ");
          Serial.println(client.state());
          delay(2000);
      }
  }
  
  xTaskCreatePinnedToCore(SensorTemp,
                      "TaskOnApp",
                      2048,
                      NULL,
                      4,
                      NULL,
                        APP_CPU_NUM);
  delay(500);
  xTaskCreatePinnedToCore(SensorHumi,
                      "TaskOnApp1",
                      2048,
                      NULL,
                      4,
                      NULL,
                        APP_CPU_NUM);
  delay(500);
  xTaskCreatePinnedToCore(SensorLumi,
                      "TaskOnPro",
                      2048,
                      NULL,
                      8,
                      NULL,
                      PRO_CPU_NUM);
}

void loop(){
   
  client.loop();
  
  String message = "{\"machine_id\":\"Maquina1\",\"sensors\":[{\"sensor_id\":\"Temperature001\",\"data_type\":\"double\",\"data_interval\":1000},{\"sensor_id\":\"Humidity001\",\"data_type\":\"double\",\"data_interval\":2000},{\"sensor_id\":\"Luminosity001\",\"data_type\":\"double\",\"data_interval\":3000}]}";
  Serial.println("Sending " + message + " to topic " + topic);
  bool result = client.publish(topic, message.c_str());

  if (!result) {
      Serial.println("LOOP - Message failed to send");
      
  }

  Serial.println(client.connected());


  if(WiFi.status() != WL_CONNECTED){
    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.println("Connecting to WiFi..");
    }
    Serial.println("Connected to the Wi-Fi network");
  }
  
  while (!client.connected()) {
      String client_id = "esp32-client-";
      client_id += String(WiFi.macAddress());
      Serial.printf("The client %s connects to the public MQTT broker\n", client_id.c_str());
      if (client.connect(client_id.c_str(), mqtt_username, mqtt_password)) {
          Serial.println("Public HiveMQ MQTT broker connected");
      } else {
          Serial.print("Failed with state ");
          Serial.println(client.state());
          delay(2000);
      }
  }

  
  delay(10000);
}
