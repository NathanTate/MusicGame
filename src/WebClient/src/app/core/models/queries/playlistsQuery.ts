import { BaseQuery } from "./baseQuery";

export class PlaylistsQuery extends BaseQuery {
  searchTerm: string = '';
  sortOrder: 'asc' | 'desc' = 'asc';
  sortColumn: string = '';
  isPrivate?: boolean;
}