import { BaseQuery } from "./baseQuery";

export class UserPlaylistsQuery extends BaseQuery {
  searchTerm?: string;
  sortOrder: 'asc' | 'desc' = 'asc';
  sortColumn?: string;
  isPrivate?: boolean;
}