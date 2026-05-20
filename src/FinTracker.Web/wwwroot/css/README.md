# Стили FinTracker

Каждая страница и диалог подключают **свой** CSS-файл — меняйте только нужный файл.

## Глобальные (в `App.razor`)

| Файл | Назначение |
|------|------------|
| `00-variables.css` | Цвета, отступы, шрифты — **начните отсюда** (`:root` / `[data-theme="dark"]`) |
| `01-site.css` | body, ошибки Blazor |
| `layout-main.css` | Шапка, drawer, контент |
| `layout-nav.css` | Боковое меню |
| `auth.css` | **Вход и регистрация** (`/login`, `/register`) — подключается в `App.razor` |

Тема (светлая / тёмная / системная) переключается кнопкой в шапке или на страницах входа. Выбор сохраняется в `localStorage` (`ft-theme`). Для своих стилей используйте переменные `--ft-*` из `00-variables.css`.

## Страницы (`css/pages/`)

| Файл | Страница |
|------|----------|
| `dashboard.css` | `/` |
| `transactions.css` | `/transactions` |
| `accounts.css` | `/accounts` |
| `categories.css` | `/categories` |
| `budgets.css` | `/budgets` |
| `tags.css` | `/tags` |
| `login.css` | устарело — используйте `auth.css` |
| `register.css` | устарело — используйте `auth.css` |
| `error.css` | `/Error` |
| `not-found.css` | `/not-found` |

## Диалоги (`css/dialogs/`)

| Файл | Компонент |
|------|-----------|
| `transaction.css` | `TransactionDialog.razor` |
| `account.css` | `AccountDialog.razor` |
| `category.css` | `CategoryDialog.razor` |
| `budget.css` | `BudgetDialog.razor` |
