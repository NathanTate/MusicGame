import { BaseQuery } from "./baseQuery";

export class SongsQuery extends BaseQuery {
  searchTerm: string = '';
  sortOrder: string = 'asc';
  sortColumn: string = '';
}