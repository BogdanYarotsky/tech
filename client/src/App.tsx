import { useEffect, useState } from "react";
import reactLogo from "./assets/react.svg";
import viteLogo from "/vite.svg";
import "./App.css";

type Report = {
  year: number;
  countryId: number;
  salaryUsd: number;
  yearsCoding: number;
  tagsIds: Set<number>;
};

function App() {
  const [reports, setReports] = useState<Report[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [count, setCount] = useState(0);

  useEffect(() => {
    const loadBinaryFile = async () => {
      try {
        // Fetch the binary file
        const response = await fetch("/data.bin");

        if (!response.ok) {
          throw new Error(`Failed to load binary file: ${response.status}`);
        }

        const arrayBuffer = await response.arrayBuffer();
        const dataView = new DataView(arrayBuffer);
        let offset = 0;

        const count = dataView.getInt32(offset, true); // true for little-endian
        offset += 4;

        const newReports: Report[] = new Array(count);
        for (let i = 0; i < count; i++) {
          const salaryUsd = dataView.getInt32(offset, true);
          offset += 4;

          const yearByte = dataView.getUint8(offset);
          const year = 2000 + yearByte;
          offset += 1;

          const yearsCoding = dataView.getUint8(offset);
          offset += 1;

          const countryId = dataView.getUint8(offset);
          offset += 1;

          const tagsCount = dataView.getUint16(offset, true);
          offset += 2;

          const tempTagIds = new Uint16Array(tagsCount);
          for (let j = 0; j < tagsCount; j++) {
            tempTagIds[j] = dataView.getUint16(offset, true);
            offset += 2;
          }
          const tagsIds = new Set(tempTagIds);

          // Create the Report object and add to results
          newReports[i] = {
            year,
            countryId,
            salaryUsd,
            yearsCoding,
            tagsIds,
          };
        }
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

  return (
    <>
      <div>
        {isLoading && <p>Loading binary data...</p>}
        {reports && (
          <p>Binary data loaded successfully! ({reports.length} bytes)</p>
        )}
      </div>

      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Vite + React</h1>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
    </>
  );
}

export default App;
