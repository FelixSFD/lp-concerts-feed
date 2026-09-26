/**
 * A coarse world coastline, as [longitude, latitude] rings.
 *
 * Hand-drawn and approximate — good enough to read as Earth behind a scatter of
 * tour pins at low opacity, but not survey data. If accuracy ever matters here,
 * replace this with Natural Earth geometry (world-atlas + topojson-client);
 * the drawing code only needs the ring format to stay the same.
 *
 * Antarctica is omitted and the south is clipped around -58°, which is where
 * the inhabited world stops. See WORLD_VIEWBOX for the projection this assumes.
 */
export const WORLD_OUTLINE: readonly (readonly [number, number][])[] = [
  // North America
  [
    [-168, 66], [-160, 71], [-140, 70], [-125, 70], [-110, 69], [-95, 70], [-85, 73],
    [-75, 68], [-62, 60], [-55, 50], [-60, 46], [-66, 45], [-70, 41], [-76, 35],
    [-81, 25], [-84, 30], [-90, 29], [-97, 26], [-92, 18], [-87, 16], [-84, 10],
    [-78, 8], [-83, 14], [-95, 16], [-105, 20], [-110, 23], [-114, 28], [-117, 32],
    [-122, 37], [-124, 42], [-124, 48], [-131, 54], [-138, 58], [-146, 60],
    [-155, 58], [-165, 60],
  ],
  // Greenland
  [
    [-45, 60], [-53, 65], [-56, 71], [-50, 77], [-35, 82], [-22, 80], [-20, 74],
    [-27, 68], [-35, 63],
  ],
  // South America
  [
    [-78, 8], [-72, 11], [-62, 10], [-52, 5], [-50, 0], [-44, -2], [-35, -6],
    [-39, -13], [-48, -25], [-54, -34], [-58, -38], [-62, -41], [-66, -46],
    [-69, -52], [-75, -53], [-74, -45], [-72, -37], [-71, -25], [-70, -18],
    [-76, -14], [-81, -6], [-80, 0],
  ],
  // Africa
  [
    [-17, 15], [-16, 21], [-10, 26], [-2, 31], [10, 37], [20, 32], [32, 31],
    [35, 24], [39, 15], [43, 11], [51, 12], [48, 4], [42, -2], [40, -10],
    [35, -18], [33, -26], [27, -33], [18, -34], [13, -23], [12, -16], [9, -1],
    [9, 4], [3, 6], [-8, 5], [-13, 9],
  ],
  // Eurasia
  [
    [-10, 36], [-9, 43], [-2, 48], [3, 51], [8, 54], [8, 58], [14, 55], [21, 56],
    [24, 60], [30, 60], [28, 70], [40, 68], [55, 70], [70, 73], [85, 74],
    [100, 76], [115, 74], [130, 72], [145, 70], [160, 69], [172, 66], [180, 64],
    [170, 60], [160, 58], [150, 59], [142, 52], [135, 45], [130, 42], [127, 35],
    [122, 30], [118, 24], [110, 20], [105, 10], [100, 5], [98, 10], [95, 16],
    [92, 21], [88, 22], [80, 14], [77, 8], [73, 17], [70, 22], [65, 25],
    [57, 25], [50, 28], [45, 32], [36, 36], [28, 40], [20, 42], [14, 38],
    [10, 44], [3, 42], [-5, 36],
  ],
  // Indonesia and the Philippines, loosely
  [
    [95, 5], [105, -2], [115, -8], [125, -9], [135, -4], [140, -8], [130, -8],
    [120, -9], [110, -7], [100, 1],
  ],
  // Australia
  [
    [113, -22], [114, -31], [118, -35], [129, -32], [137, -35], [141, -38],
    [147, -39], [151, -34], [153, -28], [146, -19], [142, -11], [136, -12],
    [130, -12], [126, -14], [121, -19],
  ],
  // New Zealand
  [
    [173, -35], [178, -38], [176, -41], [171, -44], [167, -46], [168, -43], [172, -40],
  ],
  // Japan
  [
    [130, 32], [136, 35], [141, 40], [145, 44], [142, 45], [138, 37], [132, 34],
  ],
  // British Isles
  [
    [-6, 50], [-2, 52], [-1, 56], [-3, 59], [-6, 58], [-8, 54], [-10, 52],
  ],
];

/**
 * Equirectangular, one SVG unit per degree: x = lng + 180, y = 78 - lat.
 * Keeping it this simple means the viewBox does the scaling, so nothing has to
 * listen for resizes.
 */
export const WORLD_VIEWBOX = "0 0 360 136";

export function projectLongitude(longitude: number): number {
  return longitude + 180;
}

export function projectLatitude(latitude: number): number {
  return 78 - latitude;
}

/** Rings as SVG path data, ready to drop straight into a <path d="…">. */
export const WORLD_OUTLINE_PATHS: string[] = WORLD_OUTLINE.map(ring =>
  ring
    .map(([lng, lat], index) =>
      `${index === 0 ? "M" : "L"}${projectLongitude(lng).toFixed(1)} ${projectLatitude(lat).toFixed(1)}`
    )
    .join(" ") + " Z"
);
