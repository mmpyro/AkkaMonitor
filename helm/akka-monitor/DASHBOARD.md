# Akka Monitor Grafana Dashboard

This document describes the Grafana dashboard for monitoring the Akka Monitor application, including both custom application metrics and standard .NET runtime metrics.

## Dashboard Overview

The dashboard is automatically provisioned when deploying the Helm chart and provides comprehensive monitoring of:
- **Custom Akka Monitor Metrics**: Application-specific metrics exposed by `PrometheusMetricActor`
- **.NET Runtime Metrics**: Standard .NET performance metrics (memory, GC, thread pool, JIT, exceptions, lock contention)

## Metrics Covered

### Custom Akka Monitor Metrics

These metrics are defined in `src/MonitorLib/Actors/PrometheusMetricActor.cs`:

#### 1. `monitor_active_actors` (Gauge)
- **Description**: Number of active actors in the Akka.NET system
- **Type**: Gauge
- **Labels**: None
- **Panel**: "Active Akka Actors" - Line chart showing the count of active actors over time

#### 2. `monitor_up` (Gauge)
- **Description**: Monitor health status (1 = UP/Success, 0 = DOWN/Failed)
- **Type**: Gauge
- **Labels**: 
  - `monitor`: Monitor name
  - `type`: Monitor type (Http, DNS, etc.)
  - `identifier`: Unique identifier for the monitor
- **Panel**: "Monitor Status" - Gauge visualization showing UP/DOWN status for each monitor

#### 3. `monitor_lattency` (Summary)
- **Description**: Monitor execution latency in milliseconds
- **Type**: Summary (with quantiles)
- **Labels**:
  - `monitor`: Monitor name
  - `type`: Monitor type
  - `identifier`: Unique identifier for the monitor
- **Panel**: "Monitor Latency" - Line chart showing:
  - Average latency (calculated from sum/count)
  - 95th percentile (p95)
  - 99th percentile (p99)

### .NET Runtime Metrics

These are standard metrics exposed by the `prometheus-net.DotNetRuntime` library (with `dotnet_` prefix):

#### 4. Memory Metrics
- **Metrics**:
  - `dotnet_total_memory_bytes`: Total memory allocated
  - `dotnet_collection_count_total`: GC collection count by generation
- **Panel**: ".NET Memory Usage" - Shows memory consumption and GC activity

#### 5. Garbage Collection (GC) Metrics
- **Metrics**:
  - `dotnet_gc_pause_seconds_sum`: Total GC pause time
- **Panel**: ".NET GC Pause Time" - Shows GC pause duration in milliseconds

#### 6. Thread Pool Metrics
- **Metrics**:
  - `dotnet_threadpool_num_threads`: Number of threads in the thread pool
  - `dotnet_threadpool_queue_length`: Number of items queued for execution
- **Panel**: ".NET Thread Pool" - Monitors thread pool health and queue depth

#### 7. JIT Compilation Metrics
- **Metrics**:
  - `dotnet_jit_method_total`: Number of methods JIT compiled
  - `dotnet_jit_il_bytes`: IL bytes JIT compiled
- **Panel**: ".NET JIT Compilation" - Shows JIT compilation activity

#### 8. Exception Metrics
- **Metrics**:
  - `dotnet_exceptions_total`: Total number of exceptions thrown
- **Panel**: ".NET Exceptions" - Tracks exception rate

#### 9. Lock Contention Metrics
- **Metrics**:
  - `dotnet_contention_total`: Number of lock contentions
- **Panel**: ".NET Lock Contention" - Monitors lock contention events

## Dashboard Panels

The dashboard consists of 9 panels arranged in a grid layout:

1. **Active Akka Actors** (Top-left) - Shows the number of active actors
2. **Monitor Status** (Top-right) - Gauge showing UP/DOWN status for each monitor
3. **Monitor Latency** (Full-width) - Comprehensive latency view with avg, p95, p99
4. **.NET Memory Usage** (Middle-left) - Memory and GC generation metrics
5. **.NET GC Pause Time** (Middle-right) - GC pause duration
6. **.NET Thread Pool** (Lower-left) - Thread pool metrics
7. **.NET JIT Compilation** (Lower-right) - JIT compilation activity
8. **.NET Exceptions** (Bottom-left) - Exception rate
9. **.NET Lock Contention** (Bottom-right) - Lock contention events

## Deployment

The dashboard is automatically deployed when you install the Helm chart:

```bash
helm install akka-monitor ./helm/akka-monitor -n monitoring --create-namespace
```

Or upgrade an existing deployment:

```bash
helm upgrade akka-monitor ./helm/akka-monitor -n monitoring
```

### How It Works

1. **ConfigMap Creation**: The dashboard JSON is stored in a ConfigMap with the label `grafana_dashboard: "1"`
2. **Sidecar Discovery**: Grafana's sidecar container watches for ConfigMaps with this label
3. **Automatic Provisioning**: The dashboard is automatically loaded into Grafana

### Configuration

The dashboard provisioning is configured in `values.yaml`:

```yaml
grafana:
  sidecar:
    dashboards:
      enabled: true
      label: grafana_dashboard
      folder: /var/lib/grafana/dashboards
      defaultFolderName: default
      searchNamespace: ALL
```

## Accessing the Dashboard

1. **Get Grafana URL**:
   ```bash
   kubectl get ingress -n monitoring
   ```

2. **Login to Grafana**:
   - URL: `http://akka-monitoring.com` (or your configured host)
   - Username: `admin`
   - Password: `admin` (default, change in `values.yaml`)

3. **Find the Dashboard**:
   - Navigate to "Dashboards" → "Browse"
   - Look for "Akka Monitor - Application & .NET Metrics"
   - Or use the search with UID: `akka-monitor-dashboard`

## Customization

### Modifying the Dashboard

1. Edit the dashboard JSON in `helm/akka-monitor/templates/grafana-dashboard.yaml`
2. Redeploy the Helm chart
3. The dashboard will be automatically updated in Grafana

### Adding New Panels

To add new panels for additional metrics:

1. Add the metric to `PrometheusMetricActor.cs` (if custom)
2. Add a new panel definition to the dashboard JSON
3. Update this documentation

### Adjusting Refresh Rate

The dashboard refreshes every 10 seconds by default. To change:

```json
"refresh": "10s"  // Change to "30s", "1m", etc.
```

## Troubleshooting

### Dashboard Not Appearing

1. **Check ConfigMap**:
   ```bash
   kubectl get configmap -n monitoring | grep dashboard
   ```

2. **Verify Label**:
   ```bash
   kubectl get configmap akka-monitor-grafana-dashboard -n monitoring -o yaml | grep grafana_dashboard
   ```

3. **Check Grafana Logs**:
   ```bash
   kubectl logs -n monitoring -l app.kubernetes.io/name=grafana -c grafana-sc-dashboard
   ```

### Metrics Not Showing

1. **Verify Prometheus is Scraping**:
   ```bash
   # Port-forward to Prometheus
   kubectl port-forward -n monitoring svc/akka-monitor-kube-prometheus-stack-prometheus 9090:9090
   
   # Visit http://localhost:9090/targets
   # Check if akka-monitor target is UP
   ```

2. **Check Metrics Endpoint**:
   ```bash
   kubectl port-forward -n monitoring svc/monitoring-service 8080:8080
   curl http://localhost:8080/metrics
   ```

3. **Verify ServiceMonitor**:
   ```bash
   kubectl get servicemonitor -n monitoring
   kubectl describe servicemonitor akka-monitor -n monitoring
   ```

### .NET Metrics Missing

If .NET runtime metrics are not appearing, ensure your application is using `prometheus-net.DotNetRuntime`:

1. Add the NuGet package to your project
2. Initialize the collector in your application startup:
   ```csharp
   IDisposable collector = DotNetRuntimeStatsBuilder
       .Default()
       .StartCollecting();
   ```

## Tags

The dashboard is tagged with:
- `akka-monitor`
- `dotnet`
- `prometheus`

## Time Range

Default time range: **Last 1 hour**

You can adjust this in the Grafana UI or modify the dashboard JSON:

```json
"time": {
  "from": "now-1h",
  "to": "now"
}
```

## Related Files

- Dashboard Template: `helm/akka-monitor/templates/grafana-dashboard.yaml`
- Metrics Actor: `src/MonitorLib/Actors/PrometheusMetricActor.cs`
- Helm Values: `helm/akka-monitor/values.yaml`
- ServiceMonitor: `helm/akka-monitor/templates/servicemonitor.yaml`
