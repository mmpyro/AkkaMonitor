monitor='./Monitor/Monitor.csproj'
web='./Web'

build_sln:
	dotnet build

restore_sln:
	dotnet restore

build:
	dotnet build $(monitor)

run:
	dotnet run --project $(monitor) --no-restore

start:
	npm start --prefix $(web)
