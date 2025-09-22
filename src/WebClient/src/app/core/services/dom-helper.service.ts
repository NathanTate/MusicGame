import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class DomHelperService {

  scrollToElement(el: Element) {
    window.scroll({
      behavior: 'smooth',
      left: 0,
      top: el.getBoundingClientRect().top + window.scrollY - 160
    })
  }

  scrollToFirstInvalidControl(selector: string = '', timeout: number = 100) {
    setTimeout(() => {
      const form = selector ? document.querySelector(selector) : document.getElementsByTagName('form')[0];
      if (!form) {
        return
      }
      const firstInvalidControl = form.querySelector('.is-invalid')
      if (firstInvalidControl) {
        this.scrollToElement(firstInvalidControl);
      }
    }, timeout)
  }
}