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
