import { BaseQuery } from "./baseQuery";

export class PlaylistsQuery extends BaseQuery {
  searchTerm: string = '';
  sortOrder: string = 'asc';
  sortColumn: string = '';
  isPrivate: boolean | null = null;
}