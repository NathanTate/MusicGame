import { BaseQuery } from "./baseQuery";

export class GenresQuery extends BaseQuery {
  searchTerm: string = '';
  sortOrder: string = 'asc';
  isSystemDefined: boolean | null = null;
}