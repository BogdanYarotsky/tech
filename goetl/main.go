package main

import (
	"encoding/csv"
	"io"
	"log"
	"os"
	"strconv"
	"strings"
	"sync"
)

type Report struct {
	Country     string
	YearsCoding int
	Salary      int
	Tags        []Tag
}

type Tag struct {
	Name string
	Type TagType
}

type TagType int

const (
	None TagType = iota
	DevType
	Language
	Database
	Platform
	WebFramework
	MiscTech
	Tools
	CollabTools
)

type TagColumn struct {
	Name string
	Type TagType
}

var tagsColumns = [...]TagColumn{
	{"DevType", DevType},
	{"LanguageHaveWorkedWith", Language},
	{"DatabaseHaveWorkedWith", Database},
	{"PlatformHaveWorkedWith", Platform},
	{"WebframeHaveWorkedWith", WebFramework},
	{"MiscTechHaveWorkedWith", MiscTech},
	{"ToolsTechHaveWorkedWith", Tools},
	{"NEWCollabToolsHaveWorkedWith", CollabTools},
}

const (
	MinValidSalary = 500
	DevPrefix      = "I am a dev"
	FullTime       = "full-time"
	NotAvailable   = "NA"
)

var csvFiles = [...]string{
	"../ETL/surveys/2021.csv",
	"../ETL/surveys/2022.csv",
	"../ETL/surveys/2023.csv",
	"../ETL/surveys/2024.csv",
}

func main() {
	ch := make(chan Report)

	var wg sync.WaitGroup
	wg.Add(len(csvFiles))

	for _, f := range csvFiles {
		go func(filename string) {
			defer wg.Done()
			parseReports(f, ch)
		}(f)
	}

	go func() {
		wg.Wait()
		close(ch)
	}()

	var reports []Report
	for r := range ch {
		reports = append(reports, r)
	}

	// goTag := Tag{"Go", Language}

	// for _, r := range reports {
	// 	if slices.Contains(r.Tags, goTag) {
	// 		// goreports = append(goreports, r)
	// 	}
	// }

	println(len(reports))

	// sort.Slice(goreports, func(i, j int) bool {
	// 	return goreports[i].Salary < goreports[j].Salary
	// })

}

func parseReports(csvPath string, ch chan<- Report) {
	file, err := os.Open(csvPath)
	check(err, "Cannot open CSV file")
	defer file.Close()

	reader := csv.NewReader(file)
	header, err := reader.Read()
	check(err, "Can't read header")

	columns := make(map[string]int)
	for i, col := range header {
		columns[col] = i
	}

	for {
		row, err := reader.Read()
		if err == io.EOF {
			break
		}
		check(err, "Can't read row")

		if !strings.HasPrefix(row[columns["MainBranch"]], DevPrefix) {
			continue
		}

		if !strings.HasSuffix(row[columns["Employment"]], FullTime) {
			continue
		}

		salary, err := strconv.Atoi(row[columns["ConvertedCompYearly"]])
		if err != nil || salary < MinValidSalary {
			continue
		}

		yearsString := row[columns["YearsCodePro"]]
		var yearsCoding int
		switch yearsString[0] {
		case 'N':
			continue
		case 'M':
			yearsCoding = 51
		case 'L':
			yearsCoding = 0
		default:
			yearsCoding, err = strconv.Atoi(yearsString)
			if err != nil {
				continue
			}
		}

		country := row[columns["Country"]]
		switch country {
		case "Republic of Korea":
			country = "South Korea"
		case "The former Yugoslav Republic of Macedonia":
			country = "Republic of North Macedonia"
		}

		var tags []Tag
		for _, col := range tagsColumns {
			tagsString := row[columns[col.Name]]
			if tagsString == NotAvailable {
				continue
			}
			parts := strings.Split(tagsString, ";")
			for _, tagName := range parts {
				tags = append(tags, Tag{
					Name: tagName,
					Type: col.Type,
				})
			}
		}

		report := Report{
			Country:     country,
			YearsCoding: yearsCoding,
			Salary:      salary,
			Tags:        tags,
		}

		ch <- report
	}
}

func check(err error, message string) {
	if err != nil {
		log.Fatal(message, err)
	}
}
