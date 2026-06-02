import { Component, input } from '@angular/core';

@Component({
  selector: 'app-insight-card',
  standalone: true,
  template: `
    <div class="bg-surface border border-gray-200 dark:border-gray-800 rounded-lg p-4 flex gap-4 hover:shadow-md transition-shadow">
      
      <div class="flex-shrink-0 flex items-center justify-center w-20 border-r border-gray-100 dark:border-gray-800 pr-4">
        <span class="flex items-center gap-1 font-bold text-sm" 
              [class.text-red-500]="trendColor() === 'red'" 
              [class.text-green-500]="trendColor() === 'green'">
          @if (trendDirection() === 'up') {
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7"></path></svg>
          } @else {
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
          }
          {{ trendValue() }}
        </span>
      </div>
      
      <div class="flex-1">
        <div class="flex items-baseline gap-2 mb-1">
          <span class="text-xl font-bold text-text-main">{{ metric() }}</span>
          <span class="text-sm font-semibold text-text-main">{{ title() }}</span>
        </div>
        <p class="text-xs text-text-muted leading-relaxed">{{ description() }}</p>
      </div>
      
    </div>
  `
})
export class InsightCardComponent {
  trendDirection = input.required<'up' | 'down'>();
  trendColor = input.required<'red' | 'green'>();
  trendValue = input.required<string>();
  metric = input.required<string>();
  title = input.required<string>();
  description = input.required<string>();
}