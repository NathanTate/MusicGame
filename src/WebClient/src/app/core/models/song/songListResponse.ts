import { BasePagedListResponse } from "../BasePagedListResponse";
import { SongResponse } from "./songResponse";

export interface SongListResponse extends BasePagedListResponse {
  items: SongResponse[];
}