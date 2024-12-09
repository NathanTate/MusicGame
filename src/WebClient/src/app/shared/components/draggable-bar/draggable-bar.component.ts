import { Component, computed, ElementRef, HostListener, input, output, signal, viewChild } from '@angular/core';

@Component({
  selector: 'app-draggable-bar',
  standalone: true,
  imports: [],
  templateUrl: './draggable-bar.component.html',
  styleUrl: './draggable-bar.component.scss'
})
export class DraggableBarComponent {
  private _progressBar = viewChild.required<ElementRef<HTMLElement>>('progressBar');
  isPointerDown = signal<boolean>(false);
  valueChanged = output<number>();
  arrowValueStep = input<number>(5);
  value = input<number>(0);
  maxValue = input.required<number>();
  timeoutId: ReturnType<typeof setTimeout> | null = null;

  private get progressBar() {
    return this._progressBar().nativeElement;
  }

  updateBar = computed(() => {
    if (!this.progressBar) return;
    const percentage = this.value() === 0 ? 0 : this.value() / this.maxValue() * 100;
    this.progressBar.style.setProperty('--progress-width', `${percentage}%`);
  })

  @HostListener('pointerdown', ['$event'])
  onPointerDown(event: PointerEvent) {
    this.isPointerDown.set(true);
    this.setProgressPosition(event);
  }

  @HostListener('pointermove', ['$event'])
  onPointerMove(event: PointerEvent) {
    if (!this.isPointerDown()) return;
    this.setProgressPosition(event);
  }

  @HostListener('document:pointerup', ['$event'])
  onPointerUp(event: PointerEvent) {
    if (!this.isPointerDown()) return;
    setTimeout(() => {
      this.isPointerDown.set(false);
      this.valueFromOffset(event);
    })
  }

  @HostListener('keydown.ArrowLeft')
  onLeftArrowDown() {
    this.arrowHeld(true);
  }

  @HostListener('keydown.ArrowRight')
  onRightArrowDown() {
    this.arrowHeld(false);
  }

  @HostListener('document:keyup.ArrowLeft')
  @HostListener('document:keyup.ArrowRight')
  onArrowUp() {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
      this.timeoutId = null;
    }
  }

  arrowHeld(left: boolean) {
    if (this.timeoutId) return;
    const updateValue = () => {
      const newValue = left ? this.value() - this.arrowValueStep() : this.value() + this.arrowValueStep();
      this.valueChanged.emit(Math.max(0, Math.min(newValue, this.maxValue())))
      this.timeoutId = null;
    }

    updateValue();
    this.timeoutId = setTimeout(updateValue, 300);
  }

  setProgressPosition(pointer: PointerEvent) {
    const width = pointer.offsetX / this.progressBar.clientWidth * 100;
    this.progressBar.style.setProperty('--progress-width', `${width}%`);
  }

  valueFromOffset(pointer: PointerEvent) {
    const percentage = pointer.offsetX / this.progressBar.clientWidth * 100;
    const value = this.maxValue() * (percentage / 100);
    this.valueChanged.emit(value);
  }
}
