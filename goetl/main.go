package main

import (
	"encoding/csv"
	"fmt"
	"io"
	"log"
	"os"
	"strconv"
	"strings"
)

type Report struct {
	Salary      int
	YearsCoding int
	Tags        []Tag
}

type Tag struct {
	Type TagType
	Name string
}

type TagType int

const (
	None TagType = iota
	Language
)

func main() {
	file, err := os.Open("../ETL/surveys/2021.csv")
	check(err, "Cannot open CSV file")
	defer file.Close()

	reader := csv.NewReader(file)
	header, err := reader.Read()
	check(err, "Can't read header")

	columns := make(map[string]int)
	for i, col := range header {
		columns[col] = i
	}

	var reports []Report
	for {
		row, err := reader.Read()
		if err == io.EOF {
			break
		}
		check(err, "Can't read row")

		if !strings.HasPrefix(row[columns["MainBranch"]], "I am a dev") {
			continue
		}

		if !strings.HasSuffix(row[columns["Employment"]], "full-time") {
			continue
		}

		salary, err := strconv.Atoi(row[columns["ConvertedCompYearly"]])
		if err != nil || salary < 500 {
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

		report := Report{
			Salary:      salary,
			YearsCoding: yearsCoding,
			Tags:        []Tag{},
		}

		fmt.Printf("%+v\n", report)
		reports = append(reports, report)
	}

	println(len(reports))
}

func check(err error, message string) {
	if err != nil {
		log.Fatal(message, err)
	}
}
