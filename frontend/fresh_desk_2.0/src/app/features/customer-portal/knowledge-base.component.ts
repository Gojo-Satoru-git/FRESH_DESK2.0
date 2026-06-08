import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-knowledge-base',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col items-start justify-between text-center px-4">
      <h1 class="text-5xl font-bold text-[#012A4A] mb-3">
        Knowledge Base
      </h1>

      <p class="text-gray-600 text-3xl mb-8">
        Find answers, guides and release information
      </p>

      <!-- SEARCH BAR -->
      <div class="relative w-full max-w-xl mb-12">
        <img
          src="search.png"
          alt="search"
          class="absolute left-4 top-1/2 -translate-y-1/2 w-6 h-6 opacity-60"
        />

        <input
          type="text"
          placeholder="Search the knowledge base..."
          class="
            w-full
            pl-14
            pr-4
            py-4
            text-lg
            border
            border-gray-300
            rounded-xl
            focus:outline-none
            focus:ring-2
            focus:ring-[#012A4A]
          "
        />
      </div>

      <!-- CARDS -->
    <!-- CARDS -->
<div class="grid grid-cols-1 md:grid-cols-3 gap-8 w-full max-w-6xl">
  @for (card of cards; track card.title) {
    <div
      class="
        bg-white
        border
        border-gray-200
        rounded-2xl
        p-10
        shadow-sm
        hover:shadow-md
        transition
        cursor-pointer
      "
      (click)="navigate(card.title)"
    >
      <h2 class="text-2xl font-bold text-left text-[#012A4A] mb-2">
        {{ card.title }}
      </h2>

      <p class="text-gray-600 text-left text-lg mb-6">
        {{ card.description }}
      </p>

      <span class="text-sm font-semibold text-gray-500 hover:underline">
        {{ card.count }}
      </span>
    </div>
  }
</div>
    </div>
  `
})
export class KnowledgeBaseComponent {
     constructor(private router: Router) {}
  cards = [
    {
      title: 'ARTICLE',
      description: 'Detailed articles explaining features and workflows.',
      count: '128 Articles'
    },
    {
      title: 'FAQ',
      description: 'Frequently asked questions and quick answers.',
      count: '64 FAQs'
    },
    {
      title: 'MANUAL',
      description: 'Step-by-step manuals and technical documentation.',
      count: '32 Manuals'
    }
  ];
  navigate(type: string) {
    if (type === 'ARTICLE') {
      this.router.navigate(['/customer-portal/knowledge-base/articles']);
    }
  }
}