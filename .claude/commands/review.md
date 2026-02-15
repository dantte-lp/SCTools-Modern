Проведи ревью текущих изменений по следующим направлениям:

## Безопасность
- Хардкоженные секреты и API-ключи
- Path traversal в файловых операциях
- Небезопасный Process.Start()
- Plaintext хранение токенов

## Производительность
- Блокирующие вызовы (.Result, .Wait()) в UI-потоке
- Отсутствие ConfigureAwait(false) в Core-сервисах
- async void вне обработчиков событий
- Утечки ресурсов (IDisposable без using)

## Качество
- Отсутствие XML-документации на публичных API
- Bare catch блоки без логирования
- Магические числа без констант
- Отсутствие тестов для нового кода

## Analyzer Compliance
- Запусти `dotnet build -c Release` и проверь 0 warnings
- Проверь `dotnet format --verify-no-changes`

Приоритизируй находки: Critical / Warning / Info.
