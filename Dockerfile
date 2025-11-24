FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder
WORKDIR /home/app
ENV APP Monitor
COPY . .
RUN mkdir -p $APP/Config && echo "{\"Monitors\": [], \"Alerts\": []}" > $APP/Config/appsettings.json
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
ENV APP Monitor.dll
WORKDIR /home/app
COPY --from=builder /home/app/out .
EXPOSE 80
EXPOSE 9110
EXPOSE 4055
ENTRYPOINT ["dotnet", "Monitor.dll"]