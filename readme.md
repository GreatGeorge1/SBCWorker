# Описание проекта контроллера ARKDefence

Предназначен для работы с терминалом ARKDefence

## Структура проекта

* DevConsole - тестовая консоль с отладочной информацией для Protocol
* Protocol - реализация протокола (см. документация: протокол serial порта) и абстракции для работы с терминалом
* Worker.Core - модели данных для бд
* Worker.EntityFrameworkCore - контекст и миграции EFCore
* Worker.Host - основной програмный модуль
* XUnitTestProject - автотесты

## Структура Worker.Host

TODO

## Стек

**netcoreapp3.1**

### Пакеты

* EntityFrameworkCore 6.0.x
* Microsoft.AspNetCore.SignalR.Client 3.0.x
* Quartz.NET 3.0.x
* Newtonsoft.Json 12.0.x
* RestSharp 106.6.x
* NetStandard.Library 2.0.x
* System.IO.Ports 4.6.x
* System.Device.Gpio 1.0.x

### Другое

* C# 8

### База данных

* Sqlite 3 + EntityFrameworkCore

## Интерфейсы

* RS 485 (терминал)
* USB (терминал)
* GPIO (замок + датчики)

# Целевая платформа

netcore 3.1 arm32 runtime (linux)

# Aвторизация

[OpenId](https://openid.net/connect/) (Auth0)

# Пример конфигурации

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=controllerdb.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ListenerOptions": {
    "Ports": [
      {
        "PortName": "/dev/ttyS2",
        "IsRS485": false
      },
      {
        "PortName": "/dev/ttyS1",
        "IsRS485": true
      }
    ]
  },
  "SignalROptions": {
    "AuthDomain": "https://dev-3ru57p69.eu.auth0.com/",
    "Audience": "https://localhost:5001",
    "Secret": "idcNqrPsARQFI5qeEKOn57SwsloVN-ln1bo-R7aTo_ZTWtnEv2BGAkbuTvm7hq8J",
    "Id": "DYaPShg0nOEptG3AIeDgNBCudk7w3LhI",
    "HubUri": "https://9f6a1e58.ngrok.io/hubs/controllerhub"
  }
}
```

Где ListenerOptions - конфигурация портов.
* PortName - серийный порт линукс
* IsRS485 (bool) - режим работы, true - RS 485, false - USB/UART


SignalROptions - конфигурация транспорта SignalR.

* AuthDomain - домен auth0
* Audience - jwt issuer
* Secret - m2m (Machine to machine) ключ авторизации (auth0)
* Id - идентификатор контроллера в системе
* HubUri - uri хаба для управления контроллерами