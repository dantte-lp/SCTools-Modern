Когда нужна документация по любой библиотеке:

1. Используй MCP Context7: сначала `resolve-library-id` для поиска ID
2. Затем `query-docs` с конкретным вопросом
3. Применяй найденные паттерны к нашему коду

Ключевые библиотеки проекта и их Context7 ID:
- WPF UI: `/lepoco/wpfui`
- CommunityToolkit.Mvvm: поиск через resolve-library-id
- xUnit: поиск через resolve-library-id
- Serilog: поиск через resolve-library-id
- Velopack: поиск через resolve-library-id

Всегда проверяй актуальность API через Context7 перед написанием кода.
