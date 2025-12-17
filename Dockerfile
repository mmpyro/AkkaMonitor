FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder
WORKDIR /home/app
ENV APP Monitor
COPY . .
RUN mkdir -p $APP/Config && echo "{\"Monitors\": [], \"Alerts\": []}" > $APP/Config/appsettings.json
RUN dotnet test
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV APP Monitor.dll
WORKDIR /home/app
COPY --from=builder /home/app/out .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
EXPOSE 9110
EXPOSE 4055
ENTRYPOINT ["dotnet", "Monitor.dll"]