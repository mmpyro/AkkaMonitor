# AkkaMonitor Helm Chart

This directory contains the Helm chart for deploying AkkaMonitor with Grafana and Prometheus Operator to Kubernetes.

## Chart Structure

```
helm/akka-monitor/
├── Chart.yaml                    # Chart metadata and dependencies
├── values.yaml                   # Default configuration values
├── charts/                       # Chart dependencies (Grafana, Prometheus Operator)
└── templates/                    # Kubernetes manifest templates
    ├── _helpers.tpl             # Template helpers
    ├── namespace.yaml           # Namespace definition
    ├── configmap.yaml           # Application configuration
    ├── deployment.yaml          # AkkaMonitor deployment
    ├── service.yaml             # Service for AkkaMonitor
    ├── servicemonitor.yaml      # Prometheus ServiceMonitor CRD
    ├── networkpolicy.yaml       # Network policies
    └── grafana-log-pvc.yaml     # PVC for Grafana logs
```

## Dependencies

This chart includes the following subcharts:

- **Grafana** (v8.6.2): Metrics visualization and dashboarding
- **kube-prometheus-stack** (v65.3.0): Prometheus Operator for metrics collection

## Configuration

The chart can be configured via `values.yaml`. Key configuration sections include:

### AkkaMonitor Configuration

```yaml
akkaMonitor:
  enabled: true
  replicaCount: 1
  image:
    repository: michalmarszalek/akka-monitor
    tag: "1.3"
  resources:
    requests:
      cpu: "100m"
      memory: "260Mi"
    limits:
      cpu: "1000m"
      memory: "1Gi"
```

### Monitoring Configuration

Add HTTP and DNS monitors in `values.yaml`:

```yaml
akkaMonitor:
  config:
    monitors:
      - type: "Http"
        url: "https://example.com"
        expectedStatusCode: 200
        checkInterval: "10"
      - type: "DNS"
        hostname: "example.com"
        checkInterval: "30"
```

### Grafana Configuration

```yaml
grafana:
  enabled: true
  adminPassword: admin
  persistence:
    enabled: true
    size: 200Mi
  ingress:
    enabled: true
    hosts:
      - akka-monitoring.com
```

### Prometheus Configuration

```yaml
prometheus:
  enabled: true

kube-prometheus-stack:
  prometheus:
    prometheusSpec:
      retention: 30d
      storageSpec:
        volumeClaimTemplate:
          spec:
            resources:
              requests:
                storage: 100Mi
```

## Installation

### Prerequisites

1. Kubernetes cluster (v1.19+)
2. Helm 3.x installed
3. `kubectl` configured to access your cluster

### Install with Helm

```bash
# Install dependencies
helm dependency update helm/akka-monitor

# Install the chart
helm install akka-monitor helm/akka-monitor --namespace monitoring --create-namespace

# Or use Skaffold for development
skaffold dev
```

### Install with Skaffold

The project includes a `skaffold.yaml` configuration for streamlined development:

```bash
# Development mode (watches for changes)
skaffold dev

# One-time deployment
skaffold run

# Delete deployment
skaffold delete
```

## Accessing Services

### Grafana

- **URL**: http://akka-monitoring.com (configure in `/etc/hosts` for local development)
- **Default credentials**: admin / admin

### Prometheus

- **URL**: http://akka-monitoring.prometheus.com (configure in `/etc/hosts` for local development)

### Local Development

Add these entries to `/etc/hosts`:

```
127.0.0.1  akka-monitoring.com
127.0.0.1  akka-monitoring.prometheus.com
```

Then set up port forwarding:

```bash
# Grafana
kubectl port-forward -n monitoring svc/akka-monitor-grafana 3000:80

# Prometheus
kubectl port-forward -n monitoring svc/akka-monitor-kube-prometheus-stack-prometheus 9090:9090
```

## ServiceMonitor

The chart automatically creates a ServiceMonitor CRD that enables Prometheus Operator to discover and scrape metrics from the AkkaMonitor service. The metrics endpoint is exposed at:

- **Endpoint**: `http://monitoring-service:8082/metrics`
- **Scrape interval**: 15s

## Network Policies

Network policies are enabled by default to secure communication:

- AkkaMonitor accepts traffic from Prometheus on port 8082
- Grafana can query Prometheus on port 9090
- All egress traffic is allowed for AkkaMonitor

## Upgrading

```bash
# Update dependencies
helm dependency update helm/akka-monitor

# Upgrade the release
helm upgrade akka-monitor helm/akka-monitor --namespace monitoring
```

## Uninstalling

```bash
# Using Helm
helm uninstall akka-monitor --namespace monitoring

# Using Skaffold
skaffold delete
```

## Customization

To customize the deployment, you can:

1. **Override values**: Create a custom `values-override.yaml` and install with:
   ```bash
   helm install akka-monitor helm/akka-monitor -f values-override.yaml
   ```

2. **Set individual values**:
   ```bash
   helm install akka-monitor helm/akka-monitor --set akkaMonitor.replicaCount=2
   ```

## Troubleshooting

### Check pod status

```bash
kubectl get pods -n monitoring
```

### View logs

```bash
# AkkaMonitor logs
kubectl logs -n monitoring -l app=akka-monitor

# Prometheus Operator logs
kubectl logs -n monitoring -l app.kubernetes.io/name=kube-prometheus-stack-prometheus-operator

# Grafana logs
kubectl logs -n monitoring -l app.kubernetes.io/name=grafana
```

### Verify ServiceMonitor

```bash
kubectl get servicemonitor -n monitoring
kubectl describe servicemonitor akka-monitor -n monitoring
```

### Check Prometheus targets

Access Prometheus UI and navigate to Status → Targets to verify that the AkkaMonitor service is being scraped.

## Migration from Raw Manifests

The previous raw Kubernetes manifests (from `k8s/`) have been migrated to this Helm chart. The old manifests are backed up in `k8s-backup/` for reference.

Key changes:
- ConfigMaps are now templated and generated from values
- Grafana and Prometheus are managed as chart dependencies
- Prometheus Operator replaces vanilla Prometheus deployment
- ServiceMonitor CRDs enable automatic service discovery
- Network policies are templated and configurable
