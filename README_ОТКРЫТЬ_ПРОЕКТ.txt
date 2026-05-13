FitnessCenterApp WPF FIXED

Что внутри:
- FitnessCenterApp_WPF — WPF-приложение.
- FitnessCenterApp.Tests — тесты xUnit, добавлены в архив и ссылаются на WPF-проект.

Как открыть:
1. В Visual Studio выбери: Файл -> Открыть -> Проект или решение.
2. Открой файл:
   FitnessCenterApp_WPF/FitnessCenterApp.csproj
3. Для тестов открой:
   FitnessCenterApp.Tests/FitnessCenterApp.Tests.csproj

Команды:
dotnet build FitnessCenterApp_WPF/FitnessCenterApp.csproj
dotnet test FitnessCenterApp.Tests/FitnessCenterApp.Tests.csproj

Изменения:
- Минимальные размеры главных окон выставлены как в WinForms: 1500x860.
- Боковая панель сделана шире.
- Кнопки карточек выровнены и больше не должны обрезаться.
- Окна входа и регистрации зафиксированы по размеру.
- Тестовый проект добавлен в архив.
