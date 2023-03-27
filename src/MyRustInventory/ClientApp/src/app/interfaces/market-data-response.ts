export  interface MarketDataResponse {
  assetId: string | null;
  success: boolean | null;
  lowest_Price: number | 0.00;
  volume: string | null;
  median_Price: string | null;
}
