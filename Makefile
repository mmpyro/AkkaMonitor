monitor='./Monitor/Monitor.csproj'
web='./Web'

build:
	dotnet build

restore:
	dotnet restore

rebuild:
	dotnet clean
	dotnet build

build_monitor:
	dotnet build $(monitor)

run:
	dotnet run --project $(monitor) --no-restore

start:
	npm start --prefix $(web)
