# FinTracker — финансовый трекер

Веб-приложение на **.NET 10**, **Blazor Server**, **EF Core Code First**, **PostgreSQL**.

## Возможности

- Учёт доходов и расходов по счетам и категориям
- Дашборд с графиками (круговая диаграмма по категориям, линейный тренд)
- Бюджеты с прогресс-баром
- Теги (N:N с транзакциями и категориями)
- ASP.NET Core Identity (регистрация / вход)
- Docker: multi-stage build + PostgreSQL

## Архитектура

| Проект | Назначение |
|--------|------------|
| `FinTracker.Domain` | Сущности, enum |
| `FinTracker.Application` | DTO, сервисы, FluentValidation |
| `FinTracker.Infrastructure` | EF Core, репозитории, Identity, миграции |
| `FinTracker.Web` | Blazor Server + MudBlazor |

Связи в БД: **1:1** UserProfile↔User, **1:N** Account/Transaction/Budget, **N:N** Transaction↔Tag, Category↔Tag.

## Стили (CSS)

Все стили в `src/FinTracker.Web/wwwroot/css/` — см. [css/README.md](src/FinTracker.Web/wwwroot/css/README.md).

- Глобальная тема: `00-variables.css`
- Каждая страница: `css/pages/<имя>.css`
- Диалоги: `css/dialogs/<имя>.css`

## Быстрый старт (Docker)

Скопируйте переменные окружения (по желанию):

```bash
cp .env.example .env
```

**Всё в контейнерах** (приложение + PostgreSQL):

```bash
# BuildKit ускоряет повторные сборки (кэш NuGet)
$env:DOCKER_BUILDKIT=1
docker compose up -d --build
```

Приложение: http://localhost:8080

**Только БД** (приложение через `dotnet run` на http://localhost:5024):

```bash
docker compose -f docker-compose.db.yml up -d
dotnet run --project src/FinTracker.Web
```

Демо-аккаунт: `demo@fintracker.local` / `Demo123!`

Остановка: `docker compose down` (данные БД сохраняются в volume `fintracker-pgdata`).

## Локальная разработка

1. PostgreSQL на `localhost:5432` (см. `appsettings.Development.json` или `docker compose -f docker-compose.db.yml up -d`)
2. Миграции:

```bash
dotnet ef migrations add InitialCreate -p src/FinTracker.Infrastructure -s src/FinTracker.Web
dotnet ef database update -p src/FinTracker.Infrastructure -s src/FinTracker.Web
```

3. Запуск:

```bash
dotnet run --project src/FinTracker.Web
```

## Тесты

```bash
dotnet test
```

## Ветки Git (рекомендуется)

- `main` — стабильная версия
- `dev` — интеграция
- `feature/*` — фичи
