# SCTools — Помощник Star Citizen

[![Build & Test](https://github.com/h0useRus/StarCitizen/actions/workflows/build.yml/badge.svg)](https://github.com/h0useRus/StarCitizen/actions/workflows/build.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Утилита-компаньон для **Star Citizen** — управление локализациями, настройками игры и авто-обновлениями.

Построена на .NET 10, WPF UI (Fluent Design), современный C# 14.

[English](README.md) | **Русский**

---

## Возможности

- **Управление локализацией** — установка, обновление и удаление языковых пакетов (LIVE, PTU, EPTU)
- **Инкрементальная загрузка** — загружаются только изменённые файлы, экономия трафика
- **Авто-обновления** — дельта-обновления через [Velopack](https://velopack.io/)
- **Мультиязычный интерфейс** — английский, русский, украинский, корейский, китайский
- **Системный трей** — сворачивание в трей, фоновая проверка обновлений
- **Fluent Design** — WPF UI с Mica и тёмной темой
- **Обнаружение игры** — автоматическое обнаружение LIVE, PTU, EPTU установок

## Установка

### Скачать

Скачайте последнюю версию из [GitHub Releases](https://github.com/h0useRus/StarCitizen/releases).

| Платформа | Файл |
|-----------|------|
| Windows x64 | `SCTools-win-x64.zip` |
| Windows ARM64 | `SCTools-win-arm64.zip` |

Приложение самодостаточное — установка .NET не требуется.

### Авто-обновление

После установки SCTools обновляется автоматически через дельта-обновления Velopack.

## Сборка из исходников

### Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (preview)
- Windows 10/11 (WPF требует Windows)
- Опционально: [Podman](https://podman.io/) для контейнерной разработки Core на Linux

### Сборка

```bash
# Полное решение (только Windows)
dotnet build -c Release

# Только Core библиотека (кросс-платформенная)
dotnet build src/SCTools.Core/ -c Release

# Запуск тестов
dotnet test -c Release

# Публикация
dotnet publish src/SCTools.App/ -c Release -r win-x64 --self-contained
```

### Контейнерная разработка

Core библиотека и тесты собираются в контейнере Podman (Debian trixie, .NET 10 SDK):

```bash
podman build -t sctools-dev -f Containerfile .
podman run --rm -v ./:/workspace:Z sctools-dev dotnet test tests/SCTools.Tests/ -c Release
```

## Участие в разработке

1. Форкните репозиторий
2. Создайте feature-ветку
3. Напишите тесты
4. Убедитесь: 0 warnings при `dotnet build -c Release`
5. Убедитесь: все тесты проходят
6. Откройте pull request

## Безопасность

Смотрите [SECURITY.md](SECURITY.md) для политики сообщения об уязвимостях.

## Лицензия

MIT — см. файл [LICENSE](LICENSE).

## Благодарности

- Оригинальный [SCTools](https://github.com/h0useRus/StarCitizen) от h0useRus
- Сообщество локализации Star Citizen
