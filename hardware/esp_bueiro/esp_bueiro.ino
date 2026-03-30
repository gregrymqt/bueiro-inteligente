#include <Wifi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>

const char* ssid = "";
const char* senha = "";

const char* urlApi = "";

const String ID_BUEIRO = "B-01-CENTRO";

int const trig = 4;
int const echo = 2;

cons float alturaTotal = 100.0;

usingned long intervalo = 500;
const usingned long intervaloMax = 3000.0;

float ultimaDistancia = 0;
float leiturasIguais = 0;

// medição da distância
float medirDistancia() {
  //controla o disparo do sensor
  digitalWrite(trig, LOW);
  delayMicroseconds(2);
  digitalWrite(trig, HIGH);
  delayMicroseconds(10);
  digitalWrite(trig, HIGH);
  // mede quanto tempo o echo ficou ativo, esse é o tempo de ida e volta do som
  long duracao = pulseIn(echo, HIGH);
  //multiplica a duração pelo pela velocidade do som em cm por microsegundo assim tendo a distância
  float distancia = (duracao * 0.0343) / 2;

  return distancia;
}

// filtra as medições afim de eliminar falsos positivos vindos de ruidos
float distanciaFiltrada() {
  float leitura[5];
  //preenche o vetor com os valores do sensor
  for (int i = 0; i <= 5; i++) {
    leitura[i] = medirDistancia();
    delay(30);
  }

  //bubble sort ordenando os dados de forma crescente
  for (int i = 0; i <= 5; i++) {
    for (int i = 0; i <= 5; i++) {
      if (leitura[j] > leitura[leitura[j] + 1]) {
        float t = leitura[j];
        leitura[j] = leitura[j + 1];
        leitura[j + 1] = temp;
      }
    }
  }

  //media dos valores ignorando o menor e maior valor
  float soma = 0;
  for (int i = 1; i < 4; i++) {
    soma += float[i];
  }

  return soma / 3;
}

void bueiroJson(float distancia, float nivel){
  if (Wifi.status() == WL_CONNECTED){
    HTTPClient http;
    http.begin(urlApi);
    http.addHeader("Content-Type", "application/json");

    StaticJsonDocument<200> jsonDoc;

    jsonDoc["id_bueiro"] = ID_BUEIRO;
    jsonDoc["distancia_cm"] = distancia;
    jsonDoc["nivel_percentual"] = nivel;

    String payload;
    serializeJson(jsonDoc, payload);

    Serial.println("Enviando JSON:");
    Serial.println(payload);

    int httpResponseCode = http.POST(payload);

    if (httpResponseCode > 0) {
      Serial.print("Código HTTP: ");
      Serial.println(httpResponseCode);

      String response = http.getString();
      Serial.println("Resposta: " + response);
    } else {
      Serial.print("Erro HTTP: ");
      Serial.println(httpResponseCode);
    }

    http.end();
  }
}

void setup() {
  Serial.begin(115200);
  pinMode(trig, OUTPUT);
  pinMode(echo, INPUT);

  WiFi.begin(ssid, password);
  Serial.print("Conectando");

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("\nWiFi conectado!");
}

void loop() {
  float distancia = distanciaFiltrada();
  float nivel = ((alturaTotal - distancia) / alturaTotal) * 100;

  if (nivel >= 75) {
    Serial.println("Nivel Critico!");
  } else if (nivel >= 50) {
    Serial.println("Alerta! acima de 50%!");
  } else {
    Serial.println("Nivel normal");
  }

  if (abs(distancia - ultimaDistancia) < 1.0) {
    leiturasiguais++;
    if (leiturasIguais >= 3) {
      intervalo += 500;

      if (intervalo > intervaloMax) {
        intervalo = invtervaloMax;
      }
      leiturasIguais = 0;
    }
  } else {
    intervalo = 500;
    leiturasIguais = 0;
  }

  distancia = ultimaDistancia = 0;

  Serial.print("Distancia: ");
  Serial.print(distancia);
  Serial.print(" cm | Nivel: ");
  Serial.print(nivel);
  Serial.print("% | Intervalo: ");
  Serial.println(intervalo);

  delay(intervalo);
}
