FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder
WORKDIR /home/app
ENV APP MonitorConsole
COPY . .
RUN mkdir -p $APP/Config && echo "{\"Monitors\": [], \"Alerts\": []}" > $APP/Config/appsettings.json
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:5.0
ENV APP MonitorConsole.dll
WORKDIR /home/app
COPY --from=builder /home/app/out .
ENTRYPOINT dotnet $APP