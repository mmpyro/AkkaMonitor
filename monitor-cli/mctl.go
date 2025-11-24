package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"

	"github.com/akamensky/argparse"
	"github.com/olekukonko/tablewriter"
)

// Monitor types
type MonitorInfo struct {
	Name       string `json:"name"`
	Identifier string `json:"identifier"`
	Type       string `json:"type"`
	Interval   int    `json:"interval"`
	Mode       string `json:"mode"`
}

type MonitorStatus struct {
	Name       string `json:"name"`
	Identifier string `json:"identifier"`
	Type       string `json:"type"`
	Interval   int    `json:"interval"`
	Mode       string `json:"mode"`
	State      string `json:"state"`
}

type HttpMonitor struct {
	Name               string `json:"name"`
	Url                string `json:"url"`
	Interval           int    `json:"interval"`
	Mode               string `json:"mode"`
	ExpectedStatusCode int    `json:"expectedStatusCode"`
}

type DnsMonitor struct {
	Name     string `json:"name"`
	Hostname string `json:"hostname"`
	Interval int    `json:"interval"`
	Mode     string `json:"mode"`
}

// Alert types
type AlertInfo struct {
	Name string `json:"name"`
	Type string `json:"type"`
}

type AlertDetails struct {
	Name       string      `json:"name"`
	Type       string      `json:"type"`
	Parameters interface{} `json:"parameters"`
}

type SlackAlert struct {
	Name    string `json:"name"`
	Url     string `json:"url"`
	Channel string `json:"channel"`
}

// Monitor operations
func listMonitors(baseURL string) {
	url := fmt.Sprintf("%s/api/v1/monitor", baseURL)
	resp, err := http.Get(url)
	if err != nil {
		log.Fatalf("Error listing monitors: %v\n", err)
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		log.Fatalf("Error reading response: %v\n", err)
	}

	if resp.StatusCode != http.StatusOK {
		fmt.Printf("Error: %s\n", string(body))
		return
	}

	data := []MonitorInfo{}
	if err := json.Unmarshal(body, &data); err != nil {
		log.Fatalf("Error parsing response: %v\n", err)
	}

	if len(data) == 0 {
		fmt.Println("No monitors found.")
		return
	}

	table := tablewriter.NewWriter(os.Stdout)
	table.SetHeader([]string{"Name", "Identifier", "Type", "Interval", "Mode"})
	for _, v := range data {
		table.Append([]string{v.Name, v.Identifier, v.Type, fmt.Sprint(v.Interval), v.Mode})
	}
	table.Render()
}

func getMonitorInfo(baseURL string, name string) {
	url := fmt.Sprintf("%s/api/v1/monitor/%s", baseURL, name)
	resp, err := http.Get(url)
	if err != nil {
		log.Fatalf("Error getting monitor info: %v\n", err)
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		log.Fatalf("Error reading response: %v\n", err)
	}

	if resp.StatusCode == http.StatusNotFound {
		fmt.Printf("Monitor '%s' not found.\n", name)
		return
	}

	if resp.StatusCode != http.StatusOK {
		fmt.Printf("Error: %s\n", string(body))
		return
	}

	data := MonitorStatus{}
	if err := json.Unmarshal(body, &data); err != nil {
		log.Fatalf("Error parsing response: %v\n", err)
	}

	table := tablewriter.NewWriter(os.Stdout)
	table.SetHeader([]string{"Name", "Identifier", "Type", "Interval", "Mode", "State"})
	table.Append([]string{data.Name, data.Identifier, data.Type, fmt.Sprint(data.Interval), data.Mode, data.State})
	table.Render()
}

func deleteMonitor(baseURL string, name string) {
	client := &http.Client{}
	url := fmt.Sprintf("%s/api/v1/monitor/%s", baseURL, name)
	req, err := http.NewRequest(http.MethodDelete, url, nil)
	if err != nil {
		log.Fatalf("Error creating delete request: %v\n", err)
	}

	resp, err := client.Do(req)
	if err != nil {
		log.Fatalf("Error deleting monitor: %v\n", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode == http.StatusNoContent {
		fmt.Printf("Monitor '%s' deleted successfully.\n", name)
	} else {
		body, _ := io.ReadAll(resp.Body)
		fmt.Printf("Error deleting monitor: %s\n", string(body))
	}
}

func createHttpMonitor(baseURL string, monitor *HttpMonitor) {
	url := fmt.Sprintf("%s/api/v1/monitor/http", baseURL)
	data, err := json.Marshal(monitor)
	if err != nil {
		log.Fatalf("Error marshaling monitor: %v\n", err)
	}

	resp, err := http.Post(url, "application/json", bytes.NewBuffer(data))
	if err != nil {
		log.Fatalf("Error creating monitor: %v\n", err)
	}
	defer resp.Body.Close()

	body, _ := io.ReadAll(resp.Body)
	if resp.StatusCode == http.StatusOK {
		fmt.Printf("HTTP monitor '%s' created successfully for URL: %s\n", monitor.Name, monitor.Url)
	} else {
		fmt.Printf("Error creating monitor: %s\n", string(body))
	}
}

func createDnsMonitor(baseURL string, monitor *DnsMonitor) {
	url := fmt.Sprintf("%s/api/v1/monitor/dns", baseURL)
	data, err := json.Marshal(monitor)
	if err != nil {
		log.Fatalf("Error marshaling monitor: %v\n", err)
	}

	resp, err := http.Post(url, "application/json", bytes.NewBuffer(data))
	if err != nil {
		log.Fatalf("Error creating monitor: %v\n", err)
	}
	defer resp.Body.Close()

	body, _ := io.ReadAll(resp.Body)
	if resp.StatusCode == http.StatusOK {
		fmt.Printf("DNS monitor '%s' created successfully for hostname: %s\n", monitor.Name, monitor.Hostname)
	} else {
		fmt.Printf("Error creating monitor: %s\n", string(body))
	}
}

// Alert operations
func listAlerts(baseURL string) {
	url := fmt.Sprintf("%s/api/v1/alert", baseURL)
	resp, err := http.Get(url)
	if err != nil {
		log.Fatalf("Error listing alerts: %v\n", err)
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		log.Fatalf("Error reading response: %v\n", err)
	}

	if resp.StatusCode != http.StatusOK {
		fmt.Printf("Error: %s\n", string(body))
		return
	}

	data := []AlertInfo{}
	if err := json.Unmarshal(body, &data); err != nil {
		log.Fatalf("Error parsing response: %v\n", err)
	}

	if len(data) == 0 {
		fmt.Println("No alerts found.")
		return
	}

	table := tablewriter.NewWriter(os.Stdout)
	table.SetHeader([]string{"Name", "Type"})
	for _, v := range data {
		table.Append([]string{v.Name, v.Type})
	}
	table.Render()
}

func getAlertInfo(baseURL string, name string) {
	url := fmt.Sprintf("%s/api/v1/alert/%s", baseURL, name)
	resp, err := http.Get(url)
	if err != nil {
		log.Fatalf("Error getting alert info: %v\n", err)
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		log.Fatalf("Error reading response: %v\n", err)
	}

	if resp.StatusCode == http.StatusNotFound {
		fmt.Printf("Alert '%s' not found.\n", name)
		return
	}

	if resp.StatusCode != http.StatusOK {
		fmt.Printf("Error: %s\n", string(body))
		return
	}

	data := AlertDetails{}
	if err := json.Unmarshal(body, &data); err != nil {
		log.Fatalf("Error parsing response: %v\n", err)
	}

	fmt.Printf("Alert Name: %s\n", data.Name)
	fmt.Printf("Alert Type: %s\n", data.Type)
	if data.Parameters != nil {
		paramsJSON, _ := json.MarshalIndent(data.Parameters, "", "  ")
		fmt.Printf("Parameters:\n%s\n", string(paramsJSON))
	}
}

func deleteAlert(baseURL string, name string) {
	client := &http.Client{}
	url := fmt.Sprintf("%s/api/v1/alert/%s", baseURL, name)
	req, err := http.NewRequest(http.MethodDelete, url, nil)
	if err != nil {
		log.Fatalf("Error creating delete request: %v\n", err)
	}

	resp, err := client.Do(req)
	if err != nil {
		log.Fatalf("Error deleting alert: %v\n", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode == http.StatusNoContent {
		fmt.Printf("Alert '%s' deleted successfully.\n", name)
	} else {
		body, _ := io.ReadAll(resp.Body)
		fmt.Printf("Error deleting alert: %s\n", string(body))
	}
}

func createSlackAlert(baseURL string, alert *SlackAlert) {
	url := fmt.Sprintf("%s/api/v1/alert/slack", baseURL)
	data, err := json.Marshal(alert)
	if err != nil {
		log.Fatalf("Error marshaling alert: %v\n", err)
	}

	resp, err := http.Post(url, "application/json", bytes.NewBuffer(data))
	if err != nil {
		log.Fatalf("Error creating alert: %v\n", err)
	}
	defer resp.Body.Close()

	body, _ := io.ReadAll(resp.Body)
	if resp.StatusCode == http.StatusOK {
		fmt.Printf("Slack alert '%s' created successfully for channel: %s\n", alert.Name, alert.Channel)
	} else {
		fmt.Printf("Error creating alert: %s\n", string(body))
	}
}

func main() {
	parser := argparse.NewParser("mctl", "AkkaMonitor CLI - Manage monitors and alerts")

	// Global flags
	baseURL := parser.String("e", "endpoint", &argparse.Options{
		Required: false,
		Help:     "Base URL of the monitor server",
		Default:  "http://localhost:8080",
	})

	// Monitor commands
	monitorCmd := parser.NewCommand("monitor", "Manage monitors")

	// Monitor list
	monitorListCmd := monitorCmd.NewCommand("list", "List all monitors")

	// Monitor info
	monitorInfoCmd := monitorCmd.NewCommand("info", "Get monitor status")
	monitorInfoName := monitorInfoCmd.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of the monitor",
	})

	// Monitor delete
	monitorDeleteCmd := monitorCmd.NewCommand("delete", "Delete a monitor")
	monitorDeleteName := monitorDeleteCmd.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of the monitor to delete",
	})

	// Monitor create
	monitorCreateCmd := monitorCmd.NewCommand("create", "Create a new monitor")

	// HTTP monitor
	httpCreateCmd := monitorCreateCmd.NewCommand("http", "Create HTTP monitor")
	httpName := httpCreateCmd.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of the monitor",
	})
	httpUrl := httpCreateCmd.String("u", "url", &argparse.Options{
		Required: true,
		Help:     "URL to monitor",
	})
	httpInterval := httpCreateCmd.Int("i", "interval", &argparse.Options{
		Required: true,
		Help:     "Check interval in seconds",
	})
	httpStatusCode := httpCreateCmd.Int("c", "code", &argparse.Options{
		Required: false,
		Default:  200,
		Help:     "Expected HTTP status code",
	})
	httpMode := httpCreateCmd.String("m", "mode", &argparse.Options{
		Required: false,
		Default:  "Poke",
		Help:     "Monitor mode: Poke or Reschedule",
	})

	// DNS monitor
	dnsCreateCmd := monitorCreateCmd.NewCommand("dns", "Create DNS monitor")
	dnsName := dnsCreateCmd.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of the monitor",
	})
	dnsHostname := dnsCreateCmd.String("a", "hostname", &argparse.Options{
		Required: true,
		Help:     "Hostname to monitor",
	})
	dnsInterval := dnsCreateCmd.Int("i", "interval", &argparse.Options{
		Required: true,
		Help:     "Check interval in seconds",
	})
	dnsMode := dnsCreateCmd.String("m", "mode", &argparse.Options{
		Required: false,
		Default:  "Poke",
		Help:     "Monitor mode: Poke or Reschedule",
	})

	// Alert commands
	alertCmd := parser.NewCommand("alert", "Manage alerts")

	// Alert list
	alertListCmd := alertCmd.NewCommand("list", "List all alerts")

	// Alert info
	alertInfoCmd := alertCmd.NewCommand("info", "Get alert details")
	alertInfoName := alertInfoCmd.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of the alert",
	})

	// Alert delete
	alertDeleteCmd := alertCmd.NewCommand("delete", "Delete an alert")
	alertDeleteName := alertDeleteCmd.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of the alert to delete",
	})

	// Alert create
	alertCreateCmd := alertCmd.NewCommand("create", "Create a new alert")

	// Slack alert
	slackCreateCmd := alertCreateCmd.NewCommand("slack", "Create Slack alert")
	slackName := slackCreateCmd.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of the alert",
	})
	slackUrl := slackCreateCmd.String("u", "url", &argparse.Options{
		Required: true,
		Help:     "Slack webhook URL",
	})
	slackChannel := slackCreateCmd.String("c", "channel", &argparse.Options{
		Required: true,
		Help:     "Slack channel (e.g., #general)",
	})

	// Parse arguments
	err := parser.Parse(os.Args)
	if err != nil {
		fmt.Print(parser.Usage(err))
		os.Exit(1)
	}

	// Execute commands
	switch {
	// Monitor commands
	case monitorListCmd.Happened():
		listMonitors(*baseURL)
	case monitorInfoCmd.Happened():
		getMonitorInfo(*baseURL, *monitorInfoName)
	case monitorDeleteCmd.Happened():
		deleteMonitor(*baseURL, *monitorDeleteName)
	case httpCreateCmd.Happened():
		createHttpMonitor(*baseURL, &HttpMonitor{
			Name:               *httpName,
			Url:                *httpUrl,
			Interval:           *httpInterval,
			ExpectedStatusCode: *httpStatusCode,
			Mode:               *httpMode,
		})
	case dnsCreateCmd.Happened():
		createDnsMonitor(*baseURL, &DnsMonitor{
			Name:     *dnsName,
			Hostname: *dnsHostname,
			Interval: *dnsInterval,
			Mode:     *dnsMode,
		})

	// Alert commands
	case alertListCmd.Happened():
		listAlerts(*baseURL)
	case alertInfoCmd.Happened():
		getAlertInfo(*baseURL, *alertInfoName)
	case alertDeleteCmd.Happened():
		deleteAlert(*baseURL, *alertDeleteName)
	case slackCreateCmd.Happened():
		createSlackAlert(*baseURL, &SlackAlert{
			Name:    *slackName,
			Url:     *slackUrl,
			Channel: *slackChannel,
		})

	default:
		fmt.Print(parser.Usage(nil))
		os.Exit(1)
	}
}
