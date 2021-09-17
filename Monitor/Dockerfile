FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder
WORKDIR /home/app
COPY . .
RUN echo "{\"Monitors\": [], \"Alerts\": []}" > appsettings.json
RUN dotnet publish ./Monitor.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:5.0
ENV APP Monitor.dll
WORKDIR /home/app
COPY --from=builder /home/app/out .
ENTRYPOINT dotnet $APP