# NotBreadChat

Мессенджер с использованием ASP.NET Core MVC и SignalR.

[notbreadchat.xyz](https://notbreadchat.xyz)

Чтобы быстро опробовать все возможности приложения, нужно:

1. Зарегистрировать двух пользователей с разных браузеров
2. Найти второго пользователя при помощи поля "Найти пользователя..."
3. Отправить ему сообщение. После этого диалог появится у обоих пользователей на панели слева

Мессенджер позволяет отправлять сообщения в реальном времени, редактировать их, удалять (для обоих пользователей или только для себя). Сообщения остаются непрочитанными до тех пор, пока пользователь их не увидит. Положение скролла для каждого диалога сохраняется в Local storage.

Если ссылка не работает, приложение можно быстро развернуть локально при помощи Docker Compose:

1. Установить Docker ([https://docs.docker.com/get-started/get-docker](https://docs.docker.com/get-started/get-docker))
2. [Скачать архив](https://github.com/timofeykovalenok/NotBreadChat/issues/1https://github.com/user-attachments/files/17662132/docker-compose.zip) с Docker Compose файлом и распаковать
3. Открыть папку с Compose файлом в командной строке
4. Выполнить команду `docker compose up`

После этого приложение будет доступно в браузере по адресу `localhost:5010`.

## Используемые технологии

* ASP.NET Core MVC
* SignalR Core
* База данных: PostgreSQL
* [FluentMigrator](https://github.com/fluentmigrator/fluentmigrator)
* ORM: [linq2db](https://github.com/linq2db/linq2db)
* TypeScript
* Bootstrap
* [Excubo WebCompiler](https://github.com/excubo-ag/WebCompiler)
