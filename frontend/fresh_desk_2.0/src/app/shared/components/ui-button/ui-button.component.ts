import { Component, input } from '@angular/core';

@Component({
  selector: 'app-ui-button',
  standalone: true,
  template: `
    <button 
      [type]="type()" 
      [disabled]="disabled()"
      class="w-full bg-primary hover:bg-primary-hover disabled:opacity-60 disabled:cursor-not-allowed text-white font-semibold py-3 text-lg rounded-xl shadow-lg transition-colors flex justify-center items-center"
    >
      <ng-content></ng-content>
    </button>
  `
})
export class UiButtonComponent {
  type = input<'button' | 'submit' | 'reset'>('button');
  disabled = input<boolean>(false);
}