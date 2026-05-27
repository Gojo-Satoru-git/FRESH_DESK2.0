import { Component, input } from '@angular/core';

@Component({
  selector: 'app-ui-button',
  standalone: true,
  template: `
    <button 
      [type]="type()" 
      [disabled]="disabled()"
      class="w-full bg-primary hover:bg-primary-hover disabled:opacity-60 disabled:cursor-not-allowed text-white font-semibold py-2 px-4 rounded-lg transition-colors flex justify-center items-center h-10"
    >
      <ng-content></ng-content>
    </button>
  `
})
export class UiButtonComponent {
  // Using Angular Signal inputs with default values
  type = input<'button' | 'submit' | 'reset'>('button');
  disabled = input<boolean>(false);
}