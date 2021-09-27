monitor='./MonitorConsole/MonitorConsole.csproj'
web='./Web'
image='michalmarszalek/akka-monitor'
tag='1.4'

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

build_image:
	docker build -t $(image):$(tag) .

push_image:
	docker push $(image):$(tag)
	docker tag $(image):$(tag) $(image)
	docker push $(image)