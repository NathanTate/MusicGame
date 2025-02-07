import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { SearchQuery } from "../models/queries/searchQuery";
import { BehaviorSubject } from "rxjs";
import { SearchData } from "../models/searchData";
import { generateHttpParams } from "../../shared/helpers/httpParamsHelper";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private readonly _baseUrl = environment.apiUrl + 'search/';
  public readonly searchQuery = new SearchQuery();
  private readonly searchData = new BehaviorSubject<SearchData | null>(null);
  public readonly searchData$ = this.searchData.asObservable();
  private readonly router = inject(Router);

  private readonly http = inject(HttpClient);

  search(query: SearchQuery): void {
    const httpParams = generateHttpParams(query);

    this.http.get<SearchData>(this._baseUrl, { params: httpParams }).subscribe({
      next: (data: SearchData) => {
        this.searchData.next(data);
        if (!this.router.url.includes('search')) {
          this.router.navigate(['/search', query.searchTerm])
        }
      }
    });
  }
}