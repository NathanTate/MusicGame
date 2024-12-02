import { Directive, ElementRef, HostListener, input, OnDestroy } from '@angular/core';

@Directive({
  selector: '[appOverflowScroll]',
  standalone: true
})
export class OverflowScrollDirective implements OnDestroy {
  nativeElement: Element | undefined;
  resizeObserver: ResizeObserver;
  isScrolled = false;
  target = input<HTMLElement>();

  @HostListener('scroll', ['$event'])
  onScroll(event: Event) {
    if (this.nativeElement && this.nativeElement.scrollTop > 0 && !this.isScrolled) {
      this.isScrolled = true;
      this.addRemoveShadowToTarget(true);
    } else if (this.nativeElement && this.nativeElement.scrollTop === 0) {
      this.isScrolled = false;
      this.addRemoveShadowToTarget(false);
    }
  }

  constructor(el: ElementRef) {
    this.nativeElement = el.nativeElement;

    this.resizeObserver = new ResizeObserver(() => {
      this.checkOverflow();
    })

    if (this.nativeElement) {
      this.resizeObserver.observe(this.nativeElement);
    }
  }

  addRemoveShadowToTarget(add: boolean) {
    const target = this.target();
    if (target && add) {
      target.classList.add('shadow');
    } else if (target) {
      target.classList.remove('shadow')
    }
  }

  checkOverflow() {
    if (!this.nativeElement) {
      return;
    }

    const isOverflowing = this.nativeElement.scrollHeight > this.nativeElement.clientHeight;

    isOverflowing
      ? this.nativeElement.classList.add('overflowing')
      : this.nativeElement.classList.remove('overflowing');
  }

  ngOnDestroy(): void {
    if (this.nativeElement) {
      this.resizeObserver.unobserve(this.nativeElement);
    }
  }

}
