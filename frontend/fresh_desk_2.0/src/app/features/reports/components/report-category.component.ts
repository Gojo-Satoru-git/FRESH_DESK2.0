import { Component, input } from '@angular/core';
// Make sure this path correctly points to where you saved the pipe!
import { SafeHtmlPipe } from '../../../shared/pipes/safe-html.pipes'; 

export interface ReportItem {
  title: string;
  iconSvg: string;
  colorClass: string;
}

@Component({
  selector: 'app-report-category',
  standalone: true,
  // CRITICAL FIX 1: You MUST import the pipe here so this standalone component can use it
  imports: [SafeHtmlPipe], 
  template: `
    <div class="mb-10">
      <h3 class="text-xs font-bold text-text-muted uppercase tracking-wider mb-6">{{ title() }}</h3>
      
      <div class="flex flex-wrap gap-8">
        @for (report of reports(); track report.title) {
          <button class="flex flex-col items-center group w-24 text-center">
            
            <div [class]="'w-16 h-16 rounded-2xl flex items-center justify-center mb-3 transition-transform group-hover:-translate-y-1 group-hover:shadow-lg ' + report.colorClass">
               
               <div [innerHTML]="report.iconSvg | safeHtml"></div>
               
            </div>
            
            <span class="text-sm font-medium text-text-muted group-hover:text-text-main transition-colors leading-tight">
              {{ report.title }}
            </span>
            
          </button>
        }
      </div>
      
      <div class="h-px w-full bg-gray-100 dark:bg-gray-800 mt-10"></div>
    </div>
  `
})
export class ReportCategoryComponent {
  title = input.required<string>();
  reports = input.required<ReportItem[]>();
}