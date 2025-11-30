{{/*
Expand the name of the chart.
*/}}
{{- define "akka-monitor.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "akka-monitor.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "akka-monitor.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "akka-monitor.labels" -}}
helm.sh/chart: {{ include "akka-monitor.chart" . }}
{{ include "akka-monitor.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "akka-monitor.selectorLabels" -}}
app.kubernetes.io/name: {{ include "akka-monitor.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app: akka-monitor
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "akka-monitor.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "akka-monitor.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Namespace
*/}}
{{- define "akka-monitor.namespace" -}}
{{- default "monitoring" .Values.namespace }}
{{- end }}
