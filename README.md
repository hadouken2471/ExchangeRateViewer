# ExchangeRateViewer

Тестовое задание.

## Что просили сделать

Два микросервиса с подходом CQRS и Clean Architecture:

1. **Сервис пользователей** — регистрация, вход, выход
2. **Сервис финансов** — курсы валют ЦБ РФ и управление избранными валютами пользователя

Плюс фоновый сервис, который раз в день забирает актуальные курсы с [cbr.ru](http://www.cbr.ru/scripts/XML_daily.asp), сервис миграции базы данных и API Gateway поверх всего этого.

Юнит-тесты для обоих микросервисов.

## Что получилось

Пять сервисов, каждый в своём Docker-контейнере:

- **Users API** — регистрация, логин, логаут, обновление токенов. JWT access + refresh, пароли через BCrypt, blacklist отозванных токенов в БД.
- **Finance API** — список всех курсов, добавление/удаление избранных валют. Курсы получать может кто угодно, избранное — только авторизованный пользователь.
- **API Gateway** — YARP reverse proxy. Валидирует JWT на входе, проксирует в нужный сервис. Единая точка входа.
- **ExchangeRateMonitor** — фоновый Worker Service. При старте и далее раз в сутки забирает XML с курсами ЦБ РФ, парсит, кладёт в PostgreSQL. Polly retry с exponential backoff на случай если ЦБ не отвечает.
- **Migrator** — применяет EF Core миграции при старте и завершается. В docker-compose запускается перед остальными сервисами.

Одна база PostgreSQL, изоляция данных через схемы (`users`, `finance`).

Тестовая инфраструктура: xUnit, NSubstitute для моков, EF Core InMemory

### Что ещё есть

- **ProblemDetails (RFC 9457)** — все ошибки возвращаются в стандартном формате
- **Serilog** — структурированное логирование во всех сервисах
- **.editorconfig + dotnet format** — единый стиль кода
- **Scalar** — OpenAPI UI на `/docs/v1` (в Development mode)

## Чего в коде нет

Сознательно не добавлено, чтобы не раздувать тестовое задание:

- **Ocelot** — в качестве API Gateway выбран YARP. Он проще, производительнее и активно поддерживается Microsoft. Ocelot практически заброшен.
- **MediatR** — CQRS реализован через собственные `ICommandDispatcher`/`IQueryDispatcher`. Они делают то же самое, но без лишней зависимости. В реальном проекте MediatR был бы оправдан для pipeline behaviors (валидация, логирование, транзакции).
- **FluentValidation / отдельный слой валидации** — валидация входных данных происходит в CommandHandler. Для текущего решения видится избыточным.
- **Доменные события** — нет межсервисного взаимодействия через события. Сервисы общаются только через REST (через Gateway).
- **Кеширование** — курсы валют читаются напрямую из БД
- **Rate limiting** — нет ограничения количества запросов. В проде для Gateway можно было бы использовать `Microsoft.AspNetCore.RateLimiting`.
- **Health checks** — есть только для PostgreSQL в docker-compose. На сервисах конечных точек `/health` нет.
- **CI/CD** — нет GitHub Actions и пр.
- **Логирование в файл / ELK** — только консоль.

## Как запустить

### Что нужно

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (или Docker + Docker Compose)

Больше ничего. Всё остальное внутри контейнеров.

### Быстрый старт

```bash
docker compose up --build
```

Сервисы запускаются в режиме Development — доступна документация OpenAPI через [Scalar UI](#порты).

Порядок запуска:

1. PostgreSQL поднялся и прошёл healthcheck
2. Migrator применил миграции и завершился с кодом 0
3. ExchangeRateMonitor загрузил перечень валют с сайта ЦБ РФ
4. Users API, Finance API и Gateway запустились

### Порты

| Сервис | URL |
|--------|-----|
| API Gateway (основная точка входа) | http://localhost:5050 |
| Users API (напрямую) | http://localhost:5010 |
| Finance API (напрямую) | http://localhost:5020 |
| Users API — Scalar (OpenAPI UI) | http://localhost:5010/docs/v1 |
| Finance API — Scalar (OpenAPI UI) | http://localhost:5020/docs/v1 |
| PostgreSQL | localhost:5432 (user: postgres, pass: postgres) |


## Проверка работоспособности

Подробная инструкция с curl-командами и ожидаемыми результатами — в [TESTING.md](TESTING.md).
