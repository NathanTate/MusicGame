import { BasePagedListResponse } from "../BasePagedListResponse";
import { PlaylistResponse } from "./playlistResponse";

export interface PlaylistListResponse extends BasePagedListResponse {
  items: PlaylistResponse[];
}