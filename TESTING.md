# Проверка работоспособности

## Подготовка

```bash
docker compose up --build
```

Дождитесь в логах строки `Exchange rates updated successfully` — значит всё поднялось и курсы загружены.

## Как тестировать

**Через Scalar UI** — откройте в браузере:

- Сервис пользователей: http://localhost:5010/docs/v1
- Сервис финансов: http://localhost:5020/docs/v1

В Scalar есть кнопка «Test request» у каждой конечной точки — можно отправлять запросы прямо из интерфейса, подставлять токены и видеть ответы. Все сценарии ниже можно пройти через него.

**Либо через curl** — ниже приведены все команды для терминала. Нужен `curl` и `jq` (для красивого вывода JSON).

Все запросы идут через Gateway на порту **5050**.

---

## 1. Курсы валют (без авторизации)

Получить все курсы:

```bash
curl -s http://localhost:5050/api/finance/rates | jq '.[0:3]'
```

Должно вернуть массив валют. Первые три будут выглядеть примерно так:

```json
[
    {
        "id": "AUD",
        "name": "Австралийский доллар",
        "rate": 57.149700
    },
    {
        "id": "AZN",
        "name": "Азербайджанский манат",
        "rate": 48.162500
    },
    {
        "id": "DZD",
        "name": "Алжирских динаров",
        "rate": 0.618205
    }
]
```

Проверить количество:

```bash
curl -s http://localhost:5050/api/finance/rates | jq length
```

Должно быть около 50.

---

## 2. Регистрация

```bash
curl -s -X POST http://localhost:5050/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"name": "demo", "password": "Demo123!"}' | jq
```

Ответ — пара токенов:

```json
{
  "accessToken": "eyJhbG...",
  "refreshToken": "dG9rZW4...",
  "expiresAt": "2026-03-24T12:15:00Z"
}
```

Сохраните токены в переменные (скопируйте значения из ответа):

```bash
TOKEN="eyJhbG..."
REFRESH="dG9rZW4..."
```

Повторная регистрация с тем же именем — ошибка:

```bash
curl -s -X POST http://localhost:5050/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"name": "demo", "password": "Other123!"}' | jq
```

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Пользователь 'demo' уже существует",
  "instance": "/api/users/register",
  "traceId": "00-..."
}
```

---

## 3. Логин

```bash
curl -s -X POST http://localhost:5050/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"name": "demo", "password": "Demo123!"}' | jq
```

Возвращает новую пару токенов. Обновите переменные `TOKEN` и `REFRESH`.

Неправильный пароль:

```bash
curl -s -X POST http://localhost:5050/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"name": "demo", "password": "wrong"}' | jq .detail
```

```
"Неверное имя пользователя или пароль"
```

---

## 4. Авторизация

Запрос без токена — 401:

```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost:5050/api/finance/favorites
```

```
401
```

С токеном — работает:

```bash
curl -s http://localhost:5050/api/finance/favorites \
  -H "Authorization: Bearer $TOKEN" | jq
```

```json
[]
```

Пустой массив — у нас пока нет избранных.

---

## 5. Избранные валюты

Добавить USD в избранное:

```bash
curl -s -X POST http://localhost:5050/api/finance/favorites/USD \
  -H "Authorization: Bearer $TOKEN" -w "\nHTTP %{http_code}\n"
```

```
HTTP 200
```

Добавить EUR:

```bash
curl -s -X POST http://localhost:5050/api/finance/favorites/EUR \
  -H "Authorization: Bearer $TOKEN" -w "\nHTTP %{http_code}\n"
```

Проверить избранное:

```bash
curl -s http://localhost:5050/api/finance/favorites \
  -H "Authorization: Bearer $TOKEN" | jq '.[].id'
```

```
"EUR"
"USD"
```

Повторное добавление — конфликт:

```bash
curl -s -X POST http://localhost:5050/api/finance/favorites/USD \
  -H "Authorization: Bearer $TOKEN" | jq .detail
```

```
"Курс валют 'USD' уже в списке избранных"
```

Удалить USD:

```bash
curl -s -X DELETE http://localhost:5050/api/finance/favorites/USD \
  -H "Authorization: Bearer $TOKEN" -w "HTTP %{http_code}\n"
```

```
HTTP 204
```

Удалить несуществующую валюту:

```bash
curl -s -X DELETE http://localhost:5050/api/finance/favorites/XXX \
  -H "Authorization: Bearer $TOKEN" | jq .detail
```

```
"Валюта 'XXX' не найдена в избранном"
```

Добавить несуществующую валюту:

```bash
curl -s -X POST http://localhost:5050/api/finance/favorites/FAKE \
  -H "Authorization: Bearer $TOKEN" | jq .detail
```

```
"Валюта с кодом 'FAKE' не найдена"
```

---

## 6. Обновление токенов

```bash
curl -s -X POST http://localhost:5050/api/users/token/refresh \
  -H "Content-Type: application/json" \
  -d "{\"accessToken\": \"$TOKEN\", \"refreshToken\": \"$REFRESH\"}" | jq
```

Возвращает новую пару токенов. Старый refresh token больше не работает:

```bash
curl -s -X POST http://localhost:5050/api/users/token/refresh \
  -H "Content-Type: application/json" \
  -d "{\"accessToken\": \"$TOKEN\", \"refreshToken\": \"$REFRESH\"}" | jq .detail
```

```
"Невалидный refresh token"
```

---

## 7. Логаут

Обновите `TOKEN` на свежий (из шага 6), затем:

```bash
curl -s -X POST http://localhost:5050/api/users/logout \
  -H "Authorization: Bearer $TOKEN" -w "HTTP %{http_code}\n"
```

```
HTTP 204
```

После логаута refresh с отозванным токеном не работает:

```bash
curl -s -X POST http://localhost:5050/api/users/token/refresh \
  -H "Content-Type: application/json" \
  -d "{\"accessToken\": \"$TOKEN\", \"refreshToken\": \"$REFRESH\"}" | jq .detail
```

```
"Токен был отозван"
```

---

## 8. OpenAPI документация

Откройте в браузере:

- Users API: http://localhost:5010/docs/v1
- Finance API: http://localhost:5020/docs/v1

Scalar UI покажет все endpoints с описаниями, типами запросов и ответов. Можно отправлять запросы прямо из интерфейса через "Test request".

---

## 9. Формат ошибок (ProblemDetails)

Любая ошибка возвращается в формате RFC 9457. Проверить:

```bash
curl -s -D - -X POST http://localhost:5050/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"name": "demo", "password": "test"}' 2>/dev/null | head -15
```

В заголовках: `Content-Type: application/problem+json`

В теле: `type`, `title`, `status`, `detail`, `instance`, `traceId`.

---
