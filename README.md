# Projeto de Monitoramento de Ambientes Fechados
## Introdução
Este projeto foi desenvolvido com o objetivo de monitorar a temperatura, umidade e luminosidade de ambientes fechados, como um quarto, visando otimizar a experiência dos usuários e economizar energia elétrica. Com base nos dados coletados, o sistema é capaz de acionar dispositivos como ar condicionado, umidificador de ar e controlar a abertura de cortinas e iluminação, garantindo um ambiente mais confortável e eficiente.

## Descrição do Projeto
1. Sensores: Sensores de luminosidade, temperatura e umidade são responsáveis por coletar os dados ambientais.
2. ESP32: Os dados coletados pelos sensores são enviados para o ESP32, que publica essas informações para um broker MQTT (HiveMQ).
3. MQTT Broker (HiveMQ): Centraliza e distribui os dados dos sensores para os assinantes interessados.
4. DataProcessor: Este componente assina os tópicos MQTT, processa os dados recebidos dos sensores e envia-os para serem armazenados em um banco de dados MySQL.
5. Banco de Dados (MySQL): Armazena os dados processados para posterior análise e visualização.
6. Grafana: Utiliza os dados armazenados no banco de dados para gerar  dashboards e gráficos que permitem a visualização em tempo real das condições ambientais do ambiente monitorado.
7. Ação Automática: Com base nos dados coletados e processados, o sistema pode acionar automaticamente dispositivos como ar condicionado, umidificador de ar, cortinas e iluminação, otimizando o consumo de energia e a experiência dos usuários.

## Tecnologias Utilizadas
- Linguagem de Programação: C#
- Docker: Utilizado para criar containers interligados, por meio de uma rede, para o DataProcessor, banco de dados MySQL e Grafana, garantindo um ambiente isolado e facilmente replicável.

## Arquitetura do Sistema
<div align="center">
<img width="423" alt="image" src="https://github.com/user-attachments/assets/28cf5534-54f2-4af8-9e2d-2123dd1ce063">
</div>

## Arquitetura do Banco de Dados
O banco de dados MySQL armazena os dados dos sensores em tabelas específicas para temperatura, umidade e luminosidade, permitindo consultas e visualizações eficientes.
- Tabela temperatura: Armazena a data, hora e valor da temperatura (°C)
- Tabela umidade: Armazena a data, hora e valor da umidade (g/m³)
- Tabela luminosidade: Armazena a data, hora e valor da luminosidade (cd) 

<div align="center">
<img width="631" alt="image" src="https://github.com/user-attachments/assets/2dbeaac3-3add-4165-9774-29d22023cd42">
</div>
