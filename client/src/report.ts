export type Report = {
  year: number;
  countryId: number;
  salaryUsd: number;
  yearsCoding: number;
  tagsIds: Set<number>;
};

export function fromBinary(arrayBuffer: ArrayBuffer) {
  const dataView = new DataView(arrayBuffer);
  let offset = 0;

  const count = dataView.getInt32(offset, true); // true for little-endian
  offset += 4;

  const reports: Report[] = new Array(count);
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
    reports[i] = {
      year,
      countryId,
      salaryUsd,
      yearsCoding,
      tagsIds,
    };
  }

  return reports;
}
