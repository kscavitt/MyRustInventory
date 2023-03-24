import { Tag } from "./tag";

export interface RustItemDto {
  classid: string | null;
  backgroundColor: string | null;
  iconUrl: string | null;
  iconUrlLarge: string | null;
  description: string | null;
  tradable: number | null;
  name: string | null;
  nameColor: string | null;
  type: string | null;
  marketName: string | null;
  commodity: number | null;
  marketTradableRestriction: number | null;
  marketMarketableRestriction: number | null;
  marketable: number | null;
  tags: Tag[] | null;
  amount: string | null;
  assetid: string | null;
  marketHashName: string | null;
  lowestPrice: string | null;
}
