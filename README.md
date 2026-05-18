Рабочий процесс
На маке (пишешь код)
1. Открываешь проект:
bashcode ~/Documents/RAGEMP/MyRageMPServer
2. Пишешь код в VS Code
3. Проверяешь что компилируется:
bashcd ~/Documents/RAGEMP/MyRageMPServer
dotnet build
4. Пушишь на GitHub:
bashgit add .
git commit -m "Описание изменений"
git push

В браузере (Codespace — запуск сервера)
Открываешь: github.com/codespaces
5. Обновляешь код:
bashcd /workspaces/RAGERP
git pull
6. Собираешь и копируешь dll:
bashdotnet build
cp bin/Debug/netcoreapp3.1/MyRageMPServer.dll ragemp-srv/dotnet/resources/MyRageMPServer/
7. Запускаешь сервер:
bashcd ragemp-srv
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 ./ragemp-server

Важно

Codespace останавливается когда закрываешь браузер — сервер тоже останавливается
Каждый раз при новом запуске Codespace нужно повторять шаги 5-7
Для постоянного сервера нужен VPS
