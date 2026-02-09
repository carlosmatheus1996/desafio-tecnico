# API de Gestão de Pedidos

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![MongoDB](https://img.shields.io/badge/MongoDB-4.4-47A248?style=flat&logo=mongodb)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.9-FF6600?style=flat&logo=rabbitmq)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker)

Este projeto é uma API RESTful desenvolvida em **.NET 8** para o gerenciamento de pedidos. A aplicação utiliza uma arquitetura baseada em microsserviços, com persistência em **MongoDB** e mensageria assíncrona via **RabbitMQ**.

## 🚀 Funcionalidades

- **Criação de Pedidos:** Recebe pedidos, calcula impostos dinamicamente e salva no banco.
- **Cálculo de Impostos:** Utiliza o padrão *Strategy* e *Feature Flags* para alternar entre regras de cálculo (Padrão vs. Reforma).
- **Publicação de Eventos:** Publica mensagens em uma fila RabbitMQ após o processamento com alta performance.
- **Consultas:** Busca pedidos por ID ou filtra por Status.
- **Resiliência:** Tratamento de erros para duplicidade e falhas de infraestrutura.

## 🛠️ Tecnologias e Arquitetura

- **.NET 8 (C#)**
- **MongoDB** (Driver oficial com otimização de GUIDs e índices).
- **RabbitMQ** (Implementação otimizada com conexão *Singleton* e *DispatchConsumersAsync*).
- **Docker & Docker Compose** (Para orquestração do ambiente).
- **XUnit, NSubstitute & FluentAssertions** (Testes unitários).

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado:
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

## 🔧 Como Executar

### 1. Subir a Infraestrutura (Banco e Fila)
Utilize o Docker Compose para iniciar o MongoDB e o RabbitMQ sem precisar instalar nada manualmente. Na raiz do projeto onde está localizado o docker-compose.yml, execute:

```bash
docker compose up -d