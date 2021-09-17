monitor='./Monitor/Monitor.csproj'
web='./Web'
image='michalmarszalek/akka-monitor'
tag='1.0'

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
	cd Monitor;docker build -t $(image):$(tag) .

push_image:
	cd Monitor;docker push $(image):$(tag)
	cd Monitor;docker tag $(image):$(tag) $(image)
	cd Monitor;docker push $(image)