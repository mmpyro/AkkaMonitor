package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"os"

	"github.com/akamensky/argparse"
	"github.com/olekukonko/tablewriter"
)

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

type MonitorDisplay interface {
	Display() string
}

type HttpMonitor struct {
	Name               string `json:"name"`
	Url                string `json:"url"`
	Interval           int    `json:"interval"`
	Mode               string `json:"mode"`
	ExpectedStatusCode int    `json:"expectedStatusCode"`
}

func (m HttpMonitor) Display() string {
	return fmt.Sprintf("Monitor %s of Http type with %s Identifier was created.", m.Name, m.Url)
}

type DnsMonitor struct {
	Name     string `json:"name"`
	Hostname string `json:"hostname"`
	Interval int    `json:"interval"`
	Mode     string `json:"mode"`
}

func (m DnsMonitor) Display() string {
	return fmt.Sprintf("Monitor %s of DNS type with %s Identifier was created.", m.Name, m.Hostname)
}

func list(url string) {
	resp, err := http.Get(url)
	if err != nil {
		log.Fatalln(err)
	}
	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		log.Fatalln(err)
	}

	data := []MonitorInfo{}
	json.Unmarshal(body, &data)

	table := tablewriter.NewWriter(os.Stdout)
	table.SetHeader([]string{"Name", "Identifier", "Type", "Interval", "Mode"})

	for _, v := range data {
		table.Append([]string{v.Name, v.Identifier, v.Type, fmt.Sprint(v.Interval), v.Mode})
	}
	table.Render()
}

func info(url string, name string) {
	uri := fmt.Sprintf("%s/%s", url, name)
	resp, err := http.Get(uri)
	if err != nil {
		log.Fatalln(err)
	}
	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		log.Fatalln(err)
	}

	data := MonitorStatus{}
	json.Unmarshal(body, &data)

	table := tablewriter.NewWriter(os.Stdout)
	table.SetHeader([]string{"Name", "Identifier", "Type", "Interval", "Mode", "State"})
	table.Append([]string{data.Name, data.Identifier, data.Type, fmt.Sprint(data.Interval), data.Mode, data.State})
	table.Render()
}

func delete(url string, name string) {
	client := &http.Client{}
	uri := fmt.Sprintf("%s/%s", url, name)
	req, err := http.NewRequest(http.MethodDelete, uri, nil)
	if err != nil {
		log.Fatalln(err)
	}
	_, err = client.Do(req)
	if err != nil {
		log.Fatalln(err)
	}
	fmt.Printf("Monitor %s deleted.\n", name)
}

func createMonitor(url string, monitorType string, monitor MonitorDisplay) {
	uri := fmt.Sprintf("%s/%s", url, monitorType)
	data, err := json.Marshal(monitor)
	if err != nil {
		log.Fatalln(err)
	}
	resp, err := http.Post(uri, "application/json", bytes.NewBuffer(data))
	if err != nil {
		log.Fatalln(err)
	} else if resp.StatusCode == 200 {
		fmt.Println(monitor.Display())
	} else {
		body, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			log.Fatalln(err)
		}
		fmt.Println(string(body))
	}
}

func main() {
	parser := argparse.NewParser("mctl", "Monitor cli")
	url := parser.String("e", "endpoint", &argparse.Options{
		Required: false,
		Help:     "Url of monitor server.",
		Default:  "https://localhost:5001/api/v1/Monitor",
	})
	commandMonitor := parser.NewCommand("monitor", "")
	monitorList := commandMonitor.NewCommand("list", "List monitors")
	monitorInfo := commandMonitor.NewCommand("info", "Info about monitor state")
	monitorInfoName := monitorInfo.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of monitor.",
	})
	monitorDelete := commandMonitor.NewCommand("delete", "Delete monitor")
	monitorDeleteName := monitorDelete.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of monitor.",
	})
	monitorCreate := commandMonitor.NewCommand("create", "Create monitor")
	httpCreate := monitorCreate.NewCommand("http", "Http monitor")
	httpName := httpCreate.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of monitor.",
	})
	httpUrl := httpCreate.String("u", "url", &argparse.Options{
		Required: true,
		Help:     "Url to monitor. It's Http monitor identifier.",
	})
	httpInterval := httpCreate.Int("i", "interval", &argparse.Options{
		Required: true,
		Help:     "Interval how often monitor is executed in seconds.",
	})
	httpExpectedStatusCode := httpCreate.Int("c", "code", &argparse.Options{
		Required: false,
		Default:  200,
		Help:     "Expected status code of http response.",
	})
	httpMode := httpCreate.String("m", "mode", &argparse.Options{
		Required: false,
		Default:  "Poke",
		Help:     "Mode: Reschedule or Poke. Poke is default.",
	})
	dnsCreate := monitorCreate.NewCommand("dns", "DNS monitor")
	dnsName := dnsCreate.String("n", "name", &argparse.Options{
		Required: true,
		Help:     "Name of monitor.",
	})
	dnsHostname := dnsCreate.String("a", "hostname", &argparse.Options{
		Required: true,
		Help:     "Hostname to monitor. It's DNS monitor identifier.",
	})
	dnsInterval := dnsCreate.Int("i", "interval", &argparse.Options{
		Required: true,
		Help:     "Interval how often monitor is executed in seconds.",
	})
	dnsMode := dnsCreate.String("m", "mode", &argparse.Options{
		Required: false,
		Default:  "Poke",
		Help:     "Mode: Reschedule or Poke. Poke is default.",
	})

	err := parser.Parse(os.Args)
	if err != nil {
		fmt.Println(err)
		os.Exit(1)
	}

	switch {
	case monitorList.Happened():
		list(*url)
	case monitorInfo.Happened():
		info(*url, *monitorInfoName)
	case monitorDelete.Happened():
		delete(*url, *monitorDeleteName)
	case monitorCreate.Happened():
		switch {
		case httpCreate.Happened():
			createMonitor(*url, "http", &HttpMonitor{
				Name:               *httpName,
				Url:                *httpUrl,
				Interval:           *httpInterval,
				ExpectedStatusCode: *httpExpectedStatusCode,
				Mode:               *httpMode,
			})
		case dnsCreate.Happened():
			createMonitor(*url, "dns", &DnsMonitor{
				Name:     *dnsName,
				Hostname: *dnsHostname,
				Interval: *dnsInterval,
				Mode:     *dnsMode,
			})
		}
	}
}
