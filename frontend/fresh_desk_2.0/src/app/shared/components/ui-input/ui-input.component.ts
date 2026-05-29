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
          class="w-full px-5 py-3 text-lg bg-surface text-text-main shadow-lg border border-gray-300 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all placeholder:text-text-muted/50"
          [class.border-red-500]="control().touched && control().invalid"
        />
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
}
