import { BaseQuery } from "./baseQuery";

export class UserSongsQuery extends BaseQuery {
  searchTerm?: string;
  sortOrder: 'asc' | 'desc' = 'asc';
  sortColumn?: string;
  isPrivate?: boolean;
}