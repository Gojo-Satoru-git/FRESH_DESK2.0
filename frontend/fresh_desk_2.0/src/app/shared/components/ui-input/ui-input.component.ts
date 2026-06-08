import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-ui-input',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="space-y-1 w-full">
      <label [for]="id()" class="block text-xl font-medium text-text-main transition-colors">
        {{ label() }}
      </label>

      <div class="relative transition-all">
        <input
          [id]="id()"
          [type]="type()"
          [formControl]="control()"
          [placeholder]="placeholder()"
          [readonly]="isDropdown()"
          class="w-full px-5 py-3 text-lg bg-surface text-text-main shadow-lg border border-gray-300 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all placeholder:text-text-muted/50"
          [class.border-red-500]="control().touched && control().invalid"
          [class.cursor-pointer]="isDropdown()"
          [class.pr-12]="isDropdown()" 
        />

        @if (isDropdown()) {
          <div class="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none">
            <svg class="w-5 h-5 text-text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
            </svg>
          </div>
        }
      </div>

      <div
        class="overflow-hidden transition-all duration-300"
        [style.maxHeight]="control().touched && control().invalid ? '40px' : '0px'"
      >
        <div class="pt-1">
          @if (control().touched && control().hasError('required')) {
            <span class="text-xs font-medium text-red-500 dark:text-red-400"
              >{{ label() }} is required</span
            >
          }
          @if (control().touched && control().hasError('email')) {
            <span class="text-xs font-medium text-red-500 dark:text-red-400"
              >Please enter a valid email format</span
            >
          }
        </div>
      </div>
    </div>
  `,
})
export class UiInputComponent {
  control = input.required<FormControl>();
  label = input.required<string>();
  id = input.required<string>();
  type = input<'text' | 'email' | 'password' | 'number'>('text');
  placeholder = input<string>('');
  
  // NEW: The flag to turn this input into a dropdown visual
  isDropdown = input<boolean>(false);
}