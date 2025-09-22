import { BaseQuery } from "./baseQuery";

export class SongsQuery extends BaseQuery {
  searchTerm: string = '';
  sortOrder: 'asc' | 'desc' = 'asc';
  sortColumn: string = '';
}