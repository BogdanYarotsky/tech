import { useEffect, useMemo, useState } from "react";
import {
  ScatterChart,
  Scatter,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  ZAxis,
} from "recharts";
import "./App.css";
import { fromBinary, type Report } from "./report";

type ScatterPoint = {
  x: number; // Years coding
  y: number; // Salary in USD
  z: number; // For bubble size (we'll use 1 for all points)
  year: number; // Survey year
};

type ChartData = {
  year: number;
  color: string;
  data: ScatterPoint[];
};

const yearColors = {
  2021: "#8884d8",
  2022: "#82ca9d",
  2023: "#ffc658",
  2024: "#ff7300",
};

type Tag = {
  type: number;
  name: string;
};

function App() {
  const [reports, setReports] = useState<Report[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Processed data for chart
  const chartData = useMemo(() => {
    if (reports.length === 0) return [];

    // Group reports by year
    const reportsByYear: Record<number, Report[]> = {};

    reports.forEach((report) => {
      if (!reportsByYear[report.year]) {
        reportsByYear[report.year] = [];
      }
      reportsByYear[report.year].push(report);
    });

    // Convert to format needed for scatter chart
    return Object.keys(reportsByYear)
      .map((yearStr) => {
        const year = parseInt(yearStr);
        const yearReports = reportsByYear[year];

        // Take a sample of reports for better visualization
        // (using all points would be too dense)
        const sampleSize = Math.min(1000, yearReports.length);
        const sampleStep = Math.floor(yearReports.length / sampleSize);

        const data: ScatterPoint[] = [];
        for (let i = 0; i < yearReports.length; i += sampleStep) {
          const report = yearReports[i];
          data.push({
            x: report.yearsCoding,
            y: report.salaryUsd,
            z: 1,
            year: report.year,
          });
        }

        return {
          year,
          color: yearColors[year as keyof typeof yearColors] || "#000000",
          data,
        };
      })
      .sort((a, b) => a.year - b.year);
  }, [reports]);

  useEffect(() => {
    const loadBinaryFile = async () => {
      try {
        // Fetch the binary file
        const response = await fetch("/data.bin");

        if (!response.ok) {
          throw new Error(`Failed to load binary file: ${response.status}`);
        }

        const arrayBuffer = await response.arrayBuffer();
        const newReports = fromBinary(arrayBuffer);

        setReports(newReports);
        setIsLoading(false);
      } catch (err) {
        console.error("Error loading binary file:", err);
        // setError(err.message);
        // setIsLoading(false);
      }
    };

    loadBinaryFile();
  }, []);

  // Custom tooltip for the scatter chart
  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload;
      return (
        <div
          className="custom-tooltip"
          style={{
            backgroundColor: "#fff",
            padding: "10px",
            border: "1px solid #ccc",
            borderRadius: "4px",
          }}
        >
          <p className="label">{`Year: ${data.year}`}</p>
          <p className="label">{`Years Coding: ${data.x}`}</p>
          <p className="label">{`Salary: $${data.y.toLocaleString()}`}</p>
        </div>
      );
    }
    return null;
  };

  return (
    <div
      className="container"
      style={{ maxWidth: "1200px", margin: "0 auto", padding: "20px" }}
    >
      <header style={{ marginBottom: "2rem", textAlign: "center" }}>
        <h1>Developer Salary vs. Experience</h1>
        <p>Analysis of Stack Overflow Survey Data (2021-2024)</p>
      </header>

      {isLoading ? (
        <div style={{ textAlign: "center", padding: "2rem" }}>
          <p>Loading survey data...</p>
        </div>
      ) : (
        <div>
          <div
            className="chart-container"
            style={{ height: "600px", marginBottom: "2rem" }}
          >
            <ResponsiveContainer width="100%" height="100%">
              <ScatterChart
                margin={{
                  top: 20,
                  right: 20,
                  bottom: 20,
                  left: 60,
                }}
              >
                <CartesianGrid />
                <XAxis
                  type="number"
                  dataKey="x"
                  name="Years Coding"
                  label={{
                    value: "Years of Professional Coding Experience",
                    position: "insideBottom",
                    offset: -10,
                  }}
                  domain={[0, 50]}
                />
                <YAxis
                  type="number"
                  dataKey="y"
                  name="Salary (USD)"
                  label={{
                    value: "Annual Salary (USD)",
                    angle: -90,
                    position: "insideLeft",
                  }}
                  domain={[0, 300000]}
                  tickFormatter={(value) => `$${(value / 1000).toFixed(0)}K`}
                />
                <ZAxis type="number" dataKey="z" range={[10, 10]} />
                <Tooltip content={<CustomTooltip />} />
                <Legend />

                {chartData.map((yearData) => (
                  <Scatter
                    key={yearData.year}
                    name={`${yearData.year} Survey`}
                    data={yearData.data}
                    fill={yearData.color}
                    opacity={0.6}
                  />
                ))}
              </ScatterChart>
            </ResponsiveContainer>
          </div>

          <div
            className="stats"
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))",
              gap: "1rem",
              marginBottom: "2rem",
            }}
          >
            <div
              className="stat-card"
              style={{
                backgroundColor: "#f5f5f5",
                padding: "1rem",
                borderRadius: "8px",
                boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
              }}
            >
              <h3>Total Reports</h3>
              <p className="stat-value">{reports.length.toLocaleString()}</p>
            </div>

            <div
              className="stat-card"
              style={{
                backgroundColor: "#f5f5f5",
                padding: "1rem",
                borderRadius: "8px",
                boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
              }}
            >
              <h3>Avg. Salary (USD)</h3>
              <p className="stat-value">
                $
                {Math.round(
                  reports.reduce((sum, r) => sum + r.salaryUsd, 0) /
                    reports.length
                ).toLocaleString()}
              </p>
            </div>

            <div
              className="stat-card"
              style={{
                backgroundColor: "#f5f5f5",
                padding: "1rem",
                borderRadius: "8px",
                boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
              }}
            >
              <h3>Avg. Experience</h3>
              <p className="stat-value">
                {Math.round(
                  reports.reduce((sum, r) => sum + r.yearsCoding, 0) /
                    reports.length
                )}{" "}
                years
              </p>
            </div>

            <div
              className="stat-card"
              style={{
                backgroundColor: "#f5f5f5",
                padding: "1rem",
                borderRadius: "8px",
                boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
              }}
            >
              <h3>Survey Years</h3>
              <p className="stat-value">
                {Array.from(new Set(reports.map((r) => r.year)))
                  .sort()
                  .join(", ")}
              </p>
            </div>
          </div>

          <div
            className="observations"
            style={{
              backgroundColor: "#f9f9f9",
              padding: "1.5rem",
              borderRadius: "8px",
              marginBottom: "2rem",
            }}
          >
            <h2>Key Observations</h2>
            <ul>
              <li>
                There's a positive correlation between years of coding
                experience and salary
              </li>
              <li>
                The correlation appears to be logarithmic rather than linear -
                early years show steeper salary growth
              </li>
              <li>
                Significant salary variability exists even for the same
                experience level
              </li>
              <li>
                Salary ceiling seems to increase with each survey year
                (inflation and market demand)
              </li>
            </ul>
          </div>
        </div>
      )}

      <footer style={{ textAlign: "center", marginTop: "2rem", color: "#666" }}>
        <p>Data from Stack Overflow Developer Survey 2021-2024</p>
      </footer>
    </div>
  );
}

export default App;
