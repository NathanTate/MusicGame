import { BaseQuery } from "./baseQuery";

export class GenresQuery extends BaseQuery {
  searchTerm: string = '';
  sortOrder: 'asc' | 'desc' = 'asc';
  isSystemDefined: boolean | null = null;
}