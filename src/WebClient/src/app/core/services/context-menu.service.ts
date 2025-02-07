import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { PlaylistResponse } from "../models/playlist/playlistResponse";
import { SongResponse } from "../models/song/songResponse";

class ContextMenuService<T> {
  protected item: T | null = null;
  public readonly onItemDeleted = new Subject<T>();
  public readonly onMenuOpened = new Subject<ContextMenuEvent<T>>();
  public readonly onHide = new Subject<T>();

  open(event: MouseEvent, item: T) {
    this.item = item;
    const contextMenuEvent: ContextMenuEvent<T> = {
      item: item,
      event: event
    }

    this.onMenuOpened.next(contextMenuEvent);
  }

  getActiveItem() {
    return this.item;
  }

  itemDeleted(item: T) {
    this.onItemDeleted.next(item)
  }

  hide() {
    this.item = null;
  }
}

export interface ContextMenuEvent<T> {
  item: T;
  event: MouseEvent;
}

@Injectable({
  providedIn: 'root'
})
export class PlaylistContextService extends ContextMenuService<PlaylistResponse> {
}

@Injectable({
  providedIn: 'root'
})
export class SongContextService extends ContextMenuService<SongResponse> {
  public playlist?: PlaylistResponse;
}