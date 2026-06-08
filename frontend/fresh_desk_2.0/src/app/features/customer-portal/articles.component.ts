import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-articles',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-slate-50 px-6 py-6">

      <!-- BACK BUTTON -->
      <button
        (click)="goBack()"
        class="
          flex items-center gap-2
          px-5 py-3
          mb-8
          rounded-xl
          bg-white shadow-2xl 
          text-black 
          text-lg
          font-semibold
          hover:opacity-90
          hover:bg-blue-500
          hover:text-white
          transition
        "
      >
        ← Back
      </button>

      <!-- ARTICLES LIST -->
      <div class="grid gap-6 max-w-5xl">
        <div
          *ngFor="let article of articles"
          class="
            bg-white
            border
            border-gray-200
            rounded-xl
            p-6
            shadow-sm
            hover:shadow-md
            transition
          "
        >
          <h2 class="text-2xl font-bold text-[#012A4A] mb-1">
            {{ article.title }}
          </h2>

          <p class="text-sm text-gray-500 mb-3">
            By {{ article.author }}
          </p>

          <p class="text-lg text-gray-700">
            {{ article.description }}
          </p>
        </div>
      </div>

    </div>
  `
})
export class ArticlesComponent {
  constructor(private router: Router) {}

  articles = [
    {
      title: 'Getting Started with Dashboard',
      author: 'Admin Team',
      description: 'Learn how to navigate and use the dashboard effectively.'
    },
    {
      title: 'Managing User Roles',
      author: 'Support Team',
      description: 'Understand permissions, roles, and access control.'
    },
    {
      title: 'Exporting Reports',
      author: 'Product Team',
      description: 'Step-by-step guide to export reports in different formats.'
    }
  ];

  goBack() {
    this.router.navigate(['customer-portal/knowledge-base']);
  }
}