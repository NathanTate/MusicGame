import { BasePagedListResponse } from "../BasePagedListResponse";
import { GenreResponse } from "./genreResponse";

export interface GenreListResponse extends BasePagedListResponse {
  items: GenreResponse[];
}