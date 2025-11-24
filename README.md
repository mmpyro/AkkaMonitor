# Akka Monitor

Akka Monitor is a monitoring solution for Akka.NET clusters. It provides visibility into cluster state, actor metrics, and system health.

## Repository Structure

The repository is organized as follows:

- **`src/`**: Contains the source code for the main .NET application.
    - **`Monitor/`**: The main Web API application that serves the monitoring dashboard and API.
    - **`MonitorLib/`**: A shared library containing core logic and data structures.
- **`monitor-cli/`**: A Go-based command-line interface tool for interacting with the monitor.
- **`k8s/`**: Kubernetes configuration files for deploying the monitor and related services (Prometheus, Grafana).
- **`Dockerfile`**: The Docker configuration for building the Monitor application.
- **`docker-compose.yaml`**: A Docker Compose file for running the application locally.

## Getting Started

### Prerequisites

- Docker
- Docker Compose

### Running with Docker Compose

To start the application locally using Docker Compose, run:

```bash
docker-compose up --build -d
```

The application will be accessible at `http://localhost:8080`.

### Building Locally

To build the .NET application locally:

1. Navigate to the `src` directory.
2. Run `dotnet build`.

To build the Docker image manually:

```bash
docker build -t akka-monitor .
```

## Running Tests

### Integration Tests

Run integration tests for the Monitor API:

```bash
docker run --rm -v $(pwd):/app -w /app/src/Monitor.IntegrationTests mcr.microsoft.com/dotnet/sdk:8.0 dotnet test
```

### Unit Tests

Run unit tests for MonitorLib:

```bash
docker run --rm -v $(pwd):/app -w /app/src/MonitorLib.Tests mcr.microsoft.com/dotnet/sdk:8.0 dotnet test
```

Run unit tests for Monitor:

```bash
docker run --rm -v $(pwd):/app -w /app/src/Monitor.Tests mcr.microsoft.com/dotnet/sdk:8.0 dotnet test
```

Or run all tests:

```bash
```bash
docker run --rm -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet test
```

## Using the CLI

The `monitor-cli` provides a command-line interface for managing monitors and alerts.

### Building the CLI

```bash
cd monitor-cli
go build -o mctl
```

### Quick Start

```bash
# List all monitors
./mctl monitor list

# Create an HTTP monitor
./mctl monitor create http -n WebsiteMonitor -u https://example.com -i 60

# Create a DNS monitor
./mctl monitor create dns -n DNSMonitor -a 8.8.8.8 -i 30

# Get monitor status
./mctl monitor info -n WebsiteMonitor-Http

# List all alerts
./mctl alert list

# Create a Slack alert
./mctl alert create slack -n ProductionAlert -u https://hooks.slack.com/services/XXX -c "#alerts"

# Delete a monitor
./mctl monitor delete -n WebsiteMonitor-Http
```

For detailed CLI documentation, see [`monitor-cli/README.md`](monitor-cli/README.md).

## API Endpoints

The Monitor API provides the following endpoints:

### Monitors
- `GET /api/v1/monitor` - List all monitors
- `GET /api/v1/monitor/{name}` - Get monitor status
- `DELETE /api/v1/monitor/{name}` - Delete a monitor
- `POST /api/v1/monitor/http` - Create HTTP monitor
- `POST /api/v1/monitor/dns` - Create DNS monitor

### Alerts
- `GET /api/v1/alert` - List all alerts
- `GET /api/v1/alert/{name}` - Get alert details
- `DELETE /api/v1/alert/{name}` - Delete an alert
- `POST /api/v1/alert/slack` - Create Slack alert

Swagger documentation is available at `http://localhost:8080/swagger` when running the application.
