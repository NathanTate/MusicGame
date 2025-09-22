import { ResolveFn } from "@angular/router";
import { inject } from "@angular/core";
import { SongService } from "../services/song.service";
import { SongResponse } from "../models/song/songResponse";
import { LoadingService } from "../services/loading.service";
import { finalize } from "rxjs";

export const songResolver: ResolveFn<SongResponse> = (route, state) => {
  const id = route.params['id'];
  const loadingService = inject(LoadingService);
  loadingService.busy();
  return inject(SongService).getSong(id).pipe(finalize(() => loadingService.idle()));
}