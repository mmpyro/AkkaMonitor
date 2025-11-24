# AkkaMonitor CLI (mctl)

A command-line interface for managing monitors and alerts in the AkkaMonitor system.

## Installation

```bash
cd monitor-cli
go build -o mctl
```

## Usage

### Global Options

- `-e, --endpoint`: Base URL of the monitor server (default: `http://localhost:8080`)

### Monitor Commands

#### List all monitors
```bash
./mctl monitor list
./mctl -e http://localhost:8080 monitor list
```

#### Get monitor status
```bash
./mctl monitor info -n <monitor-name>
./mctl monitor info --name TestMonitor-Http
```

#### Delete a monitor
```bash
./mctl monitor delete -n <monitor-name>
./mctl monitor delete --name TestMonitor-Http
```

#### Create HTTP monitor
```bash
./mctl monitor create http -n <name> -u <url> -i <interval> [-c <status-code>] [-m <mode>]

# Examples:
./mctl monitor create http -n GoogleMonitor -u https://google.com -i 60
./mctl monitor create http -n ApiMonitor -u https://api.example.com -i 30 -c 200 -m Poke
```

Options:
- `-n, --name`: Name of the monitor (required)
- `-u, --url`: URL to monitor (required)
- `-i, --interval`: Check interval in seconds (required)
- `-c, --code`: Expected HTTP status code (default: 200)
- `-m, --mode`: Monitor mode - Poke or Reschedule (default: Poke)

#### Create DNS monitor
```bash
./mctl monitor create dns -n <name> -a <hostname> -i <interval> [-m <mode>]

# Examples:
./mctl monitor create dns -n GoogleDNS -a 8.8.8.8 -i 60
./mctl monitor create dns -n CloudflareDNS -a 1.1.1.1 -i 30 -m Reschedule
```

Options:
- `-n, --name`: Name of the monitor (required)
- `-a, --hostname`: Hostname to monitor (required)
- `-i, --interval`: Check interval in seconds (required)
- `-m, --mode`: Monitor mode - Poke or Reschedule (default: Poke)

### Alert Commands

#### List all alerts
```bash
./mctl alert list
```

#### Get alert details
```bash
./mctl alert info -n <alert-name>
./mctl alert info --name TestAlert-Slack
```

#### Delete an alert
```bash
./mctl alert delete -n <alert-name>
./mctl alert delete --name TestAlert-Slack
```

#### Create Slack alert
```bash
./mctl alert create slack -n <name> -u <webhook-url> -c <channel>

# Example:
./mctl alert create slack -n ProductionAlert -u https://hooks.slack.com/services/XXX/YYY/ZZZ -c "#alerts"
```

Options:
- `-n, --name`: Name of the alert (required)
- `-u, --url`: Slack webhook URL (required)
- `-c, --channel`: Slack channel, e.g., #general (required)

## Examples

### Complete Workflow

```bash
# 1. Create an HTTP monitor
./mctl monitor create http -n WebsiteMonitor -u https://example.com -i 60 -c 200

# 2. Create a DNS monitor
./mctl monitor create dns -n DNSMonitor -a 8.8.8.8 -i 30

# 3. List all monitors
./mctl monitor list

# 4. Get monitor status
./mctl monitor info -n WebsiteMonitor-Http

# 5. Create a Slack alert
./mctl alert create slack -n CriticalAlert -u https://hooks.slack.com/services/XXX -c "#ops"

# 6. List all alerts
./mctl alert list

# 7. Get alert details
./mctl alert info -n CriticalAlert-Slack

# 8. Delete a monitor
./mctl monitor delete -n WebsiteMonitor-Http

# 9. Delete an alert
./mctl alert delete -n CriticalAlert-Slack
```

## API Endpoint Coverage

The CLI covers all available API endpoints:

### Monitor Endpoints
- ✅ `GET /api/v1/monitor` - List all monitors
- ✅ `GET /api/v1/monitor/{name}` - Get monitor status
- ✅ `DELETE /api/v1/monitor/{name}` - Delete a monitor
- ✅ `POST /api/v1/monitor/http` - Create HTTP monitor
- ✅ `POST /api/v1/monitor/dns` - Create DNS monitor

### Alert Endpoints
- ✅ `GET /api/v1/alert` - List all alerts
- ✅ `GET /api/v1/alert/{name}` - Get alert details
- ✅ `DELETE /api/v1/alert/{name}` - Delete an alert
- ✅ `POST /api/v1/alert/slack` - Create Slack alert

## Building for Different Platforms

```bash
# Linux
GOOS=linux GOARCH=amd64 go build -o mctl-linux

# macOS
GOOS=darwin GOARCH=amd64 go build -o mctl-macos

# Windows
GOOS=windows GOARCH=amd64 go build -o mctl.exe
```

## Error Handling

The CLI provides clear error messages for:
- Network connectivity issues
- Invalid API responses
- Missing or invalid parameters
- Resource not found (404)
- Server errors (500)
