import { DOCUMENT } from "@angular/common";
import { inject, Injectable } from "@angular/core";
import { Observable, Subject } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class GlobalEventsService {
  private readonly document: Document = inject(DOCUMENT);
  private readonly documentEventMap = new Map<string, Observable<Event>>();

  public fromDocument<T extends Event>(eventName: keyof DocumentEventMap): Observable<T> {
    if (!this.documentEventMap.has(eventName)) {
      const emitter = new Subject<Event>();

      this.document.addEventListener(eventName, (event) => emitter.next(event));
      this.documentEventMap.set(eventName, emitter.asObservable());
    }

    return this.documentEventMap.get(eventName) as Observable<T>;
  }
}