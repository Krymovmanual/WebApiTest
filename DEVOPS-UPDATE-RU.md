# DEV и STAGE

Один репозиторий payments-tests, ветка main, проект WebApiTest.Extended.
Тесты находятся в Tests/Dev и Tests/Stage, namespace разделяет их в Test Explorer.
Группировка: Namespace → Class → Method. Trait Environment позволяет выбирать среду.

DEV: 78 методов. STAGE: 24 подтвержденных метода (Bitolo 3, CoinPayments 4,
FacilitaPay 7, Fireblocks 10). Остальные STAGE методы добавляются после получения
их данных. Infura, ZeroHash и операции записи в STAGE не включены.

## Локально

Выбрать Test → Configure Run Settings → Select Solution Wide runsettings File:
WebApiTest.Extended/dev.runsettings или stage.runsettings. Затем Run All.
Каждый файл содержит свой фильтр Environment, URL и WEBAPI_ENVIRONMENT.

Команды из корня:
```powershell
dotnet test WebApiTest.Extended/WebApiTest.Extended.csproj --settings WebApiTest.Extended/dev.runsettings
dotnet test WebApiTest.Extended/WebApiTest.Extended.csproj --settings WebApiTest.Extended/stage.runsettings
```

Логин/пароль: DEV_USERNAME/DEV_PASSWORD либо STAGE_USERNAME/STAGE_PASSWORD.
Локальные альтернативы: dev_creds.json и stage_creds.json в корне (userName, password).
Секреты не коммитить. BEARER_TOKEN остается совместимым только с DEV;
для STAGE поддерживается STAGE_BEARER_TOKEN.

Полные независимые каталоги: Contracts/requests.dev.json и requests.stage.json.
STAGE никогда не читает DEV каталог. Пустые массивы и нулевые балансы допустимы.
Исторические суммы и статусы не закрепляются как ожидаемые значения.

Необязательные локальные переопределения: api-requests.dev.local.json и
api-requests.stage.local.json. Старый api-requests.local.json поддерживается
только для DEV. WEBAPI_REQUESTS_FILE явно выбирает другой файл текущего запуска.

## Azure DevOps — одноразовая настройка

1. Обновить исходники и ОБА YAML одновременно в payments-tests/main.
2. Существующий WebApiTest-DEV продолжает использовать azure-pipelines-dev-hourly.yml
   и группу payments-tests. В команду добавлен фильтр Environment=DEV.
   Telegram шаг сохранен.
3. Создать группу payments-tests-stage: STAGE_USERNAME, STAGE_PASSWORD (секрет),
   TELEGRAM_BOT_TOKEN (секрет), TELEGRAM_CHAT_ID. Разрешить доступ новому pipeline.
4. Создать WebApiTest-STAGE из azure-pipelines-stage-hourly.yml.
5. Выполнить ручной запуск и проверить отдельный отчет STAGE API methods.
   В YAML расписание на 30-й минуте каждого часа; DEV — на 0-й.
   После создания pipeline расписание уже может запускаться: настройку выполнить,
   когда готовы доступ и секреты.
6. Дальше обновлять исходники в main. Группы переменных заново создавать не нужно.

Фильтры выбирают только свою среду; дополнительная проверка запрещает сочетание
DEV конфигурации со STAGE URL и наоборот. Существующий DEV createTransaction
сохраняет поведение: один тестовый перевод с новыми ID на каждый запуск.

GitHub CI проверяет сборку, число обнаруженных методов отдельно по каждой среде
и запуск с WEBAPI_RUN_LIVE=0. Проверки не обращаются к API.
