import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoutingRuleService, Factor, Rule } from '../services/routing-rule.service';

@Component({
  selector: 'app-queue-routing-rules',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col bg-background animate-fade-in">
      <div
        class="flex items-center justify-between px-6 py-4 bg-surface border-b border-disabled-gray dark:border-gray-800 flex-shrink-0"
      >
        <div>
          <h1 class="text-xl font-bold font-heading text-text-dark dark:text-text-white">
            Queue Routing
          </h1>
          <p class="text-xs font-sans text-text-muted mt-1 font-mono">
            Incident Management / New / Assignment
          </p>
        </div>
        <div class="flex gap-3">
          <button
            class="px-5 py-2 text-sm font-bold font-sans text-text-dark border border-outline-gray rounded-full hover:bg-table-light-gray transition-colors"
          >
            Cancel
          </button>

          <button
            (click)="saveRouting()"
            [disabled]="isSaving() || isLoading()"
            class="px-5 py-2 text-sm font-bold font-sans bg-primary-blue hover:bg-primary-hover text-text-white rounded-full shadow-md transition-all disabled:opacity-50 flex items-center gap-2"
          >
            @if (isSaving()) {
              <svg class="animate-spin h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                <circle
                  class="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  stroke-width="4"
                ></circle>
                <path
                  class="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                ></path>
              </svg>
              Saving...
            } @else {
              Save Routing
            }
          </button>
        </div>
      </div>

      <div class="flex flex-1 overflow-hidden">
        <div class="flex-1 overflow-y-auto p-8 relative">
          @if (isLoading()) {
            <div
              class="absolute inset-0 flex items-center justify-center bg-background/50 backdrop-blur-sm z-10"
            >
              <div class="flex flex-col items-center gap-3 text-text-muted">
                <svg class="animate-spin h-8 w-8 text-primary-blue" fill="none" viewBox="0 0 24 24">
                  <circle
                    class="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    stroke-width="4"
                  ></circle>
                  <path
                    class="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  ></path>
                </svg>
                <span class="text-sm font-bold font-sans">Loading Routing Rules...</span>
              </div>
            </div>
          }

          <div class="max-w-4xl mx-auto">
            <div class="mb-8">
              <span
                class="text-[11px] font-bold font-heading tracking-widest uppercase text-revision-amber"
                >Queue Routing Rules</span
              >
              <h1
                class="text-2xl font-bold font-heading text-text-dark dark:text-text-white mt-2 mb-3"
              >
                How the New stage picks a queue
              </h1>
              <p class="text-sm text-text-muted leading-relaxed font-sans max-w-3xl">
                On first assignment, this stage routes each ticket to a <b>queue</b>, not an agent.
                These rules decide <b>which</b> queue. Each rule is a set of criteria (ALL must
                match) and a target queue. Rules run
                <b>top to bottom; the first one that matches wins</b>, so order matters.
              </p>
              <div
                class="inline-flex items-center gap-2 mt-4 text-xs font-mono text-text-muted bg-card-white border border-outline-gray rounded-full px-4 py-1.5 shadow-sm"
              >
                <span class="w-2 h-2 rounded-full bg-revision-amber"></span>
                ticket attributes → first matching rule → target queue → Lead dispatches
              </div>
            </div>

            <div class="space-y-4">
              @for (rule of rules(); track $index; let ruleIndex = $index) {
                <div
                  class="bg-card-white dark:bg-surface border border-outline-gray dark:border-gray-800 rounded-xl shadow-sm overflow-hidden flex flex-col"
                >
                  <div
                    class="flex items-center gap-3 p-3 border-b border-table-dark-gray dark:border-gray-800 bg-table-light-gray dark:bg-gray-800/50"
                  >
                    <div class="flex items-center gap-1">
                      <div
                        class="w-7 h-7 rounded-md bg-text-dark text-white flex items-center justify-center font-bold font-heading text-sm"
                      >
                        {{ ruleIndex + 1 }}
                      </div>
                      <div class="flex flex-col gap-0.5">
                        <button
                          (click)="moveRule(ruleIndex, -1)"
                          [disabled]="ruleIndex === 0"
                          class="w-5 h-3.5 bg-background border border-outline-gray rounded text-[8px] flex items-center justify-center text-text-muted hover:bg-disabled-gray disabled:opacity-30 cursor-pointer"
                        >
                          ▲
                        </button>
                        <button
                          (click)="moveRule(ruleIndex, 1)"
                          [disabled]="ruleIndex === rules().length - 1"
                          class="w-5 h-3.5 bg-background border border-outline-gray rounded text-[8px] flex items-center justify-center text-text-muted hover:bg-disabled-gray disabled:opacity-30 cursor-pointer"
                        >
                          ▼
                        </button>
                      </div>
                    </div>
                    <span class="font-bold font-heading text-sm text-text-dark dark:text-text-white"
                      >Rule {{ ruleIndex + 1 }}</span
                    >
                    <span class="ml-auto font-mono text-[11px] text-text-muted truncate max-w-[45%]"
                      >{{ compileRule(rule) }} &nbsp;→&nbsp; {{ rule.then }}</span
                    >
                    <button
                      (click)="deleteRule(ruleIndex)"
                      class="w-7 h-7 ml-2 border border-outline-gray rounded-md bg-card-white text-text-muted hover:border-error-red hover:text-error-red flex items-center justify-center transition-colors"
                    >
                      ✕
                    </button>
                  </div>

                  <div class="p-4">
                    <div
                      class="text-[10.5px] font-bold tracking-widest uppercase text-revision-amber mb-3"
                    >
                      When — all criteria match
                    </div>

                    @for (cond of rule.when; track condIndex; let condIndex = $index) {
                      @if (condIndex > 0) {
                        <div
                          class="inline-block text-[9.5px] font-bold tracking-widest text-text-muted bg-background border border-table-dark-gray rounded px-2 py-0.5 my-1"
                        >
                          AND
                        </div>
                      }

                      <div
                        class="border border-outline-gray border-l-4 border-l-revision-amber rounded-lg p-3 mb-2 bg-background/50"
                      >
                        <div class="flex flex-wrap items-center gap-2">
                          <select
                            [ngModel]="cond.field"
                            (ngModelChange)="updateCondField(ruleIndex, condIndex, $event)"
                            class="px-3 py-1.5 bg-card-white border border-outline-gray rounded-lg text-sm font-bold text-text-dark focus:ring-2 focus:ring-revision-amber outline-none min-w-[150px]"
                          >
                            @for (f of validFactors(); track f.id) {
                              <option [value]="f.id">{{ f.name }}</option>
                            }
                          </select>

                          <span
                            class="text-[9px] font-bold tracking-wider px-1.5 py-0.5 rounded-full uppercase"
                            [ngClass]="getKindClasses(getFactor(cond.field)?.kind)"
                          >
                            {{ getFactor(cond.field)?.kind }}
                          </span>

                          <select
                            [ngModel]="cond.op"
                            (ngModelChange)="updateCondOp(ruleIndex, condIndex, $event)"
                            class="px-3 py-1.5 bg-card-white border border-outline-gray rounded-lg text-sm text-text-muted focus:ring-2 focus:ring-revision-amber outline-none min-w-[104px]"
                          >
                            @for (op of getOps(cond.field); track op) {
                              <option [value]="op">{{ op }}</option>
                            }
                          </select>

                          @if (rule.when.length > 1) {
                            <button
                              (click)="deleteCond(ruleIndex, condIndex)"
                              class="ml-auto w-6 h-6 border border-outline-gray rounded bg-card-white text-text-muted hover:border-error-red hover:text-error-red flex items-center justify-center"
                            >
                              ✕
                            </button>
                          }
                        </div>

                        <div class="mt-3 pl-1">
                          @if (getFactor(cond.field)?.type === 'num') {
                            <div
                              class="flex items-center gap-2 text-[10.5px] font-mono text-revision-amber mb-2"
                            >
                              <svg
                                class="w-3 h-3"
                                viewBox="0 0 24 24"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2"
                              >
                                <path d="M10 13a5 5 0 0 0 7 0l3-3a5 5 0 0 0-7-7l-1 1" />
                                <path d="M14 11a5 5 0 0 0-7 0l-3 3a5 5 0 0 0 7 7l1-1" />
                              </svg>
                              numeric — no master list
                            </div>
                            <div class="flex items-center gap-2">
                              <input
                                type="number"
                                [ngModel]="cond.vals[0] || ''"
                                (ngModelChange)="updateCondNum(ruleIndex, condIndex, $event)"
                                placeholder="e.g. 80"
                                class="w-24 px-3 py-1.5 bg-card-white border border-outline-gray rounded-lg text-sm focus:ring-2 focus:ring-revision-amber outline-none"
                              />
                              <span class="text-sm font-bold text-text-muted">%</span>
                            </div>
                          } @else {
                            <div
                              class="flex items-center gap-2 text-[10.5px] font-mono text-revision-amber mb-2"
                            >
                              <svg
                                class="w-3 h-3"
                                viewBox="0 0 24 24"
                                fill="none"
                                stroke="currentColor"
                                stroke-width="2"
                              >
                                <path d="M10 13a5 5 0 0 0 7 0l3-3a5 5 0 0 0-7-7l-1 1" />
                                <path d="M14 11a5 5 0 0 0-7 0l-3 3a5 5 0 0 0 7 7l1-1" />
                              </svg>
                              values from
                              <b class="text-text-dark dark:text-text-white">{{
                                getFactor(cond.field)?.name
                              }}</b>
                              master
                            </div>
                            <div class="flex flex-wrap gap-1.5">
                              @for (val of getFactor(cond.field)?.master; track val) {
                                <button
                                  (click)="toggleCondVal(ruleIndex, condIndex, val, cond.op)"
                                  class="px-3 py-1 rounded-full border text-xs transition-colors"
                                  [ngClass]="
                                    cond.vals.includes(val)
                                      ? 'bg-revision-amber border-revision-amber text-white font-bold'
                                      : 'bg-card-white border-outline-gray text-text-muted hover:border-revision-amber hover:text-revision-amber'
                                  "
                                >
                                  {{ val }}
                                </button>
                              }
                            </div>
                          }
                        </div>
                      </div>
                    }

                    <button
                      (click)="addCond(ruleIndex)"
                      class="mt-1 text-xs font-bold text-revision-amber hover:text-amber-700 flex items-center gap-1 py-1"
                    >
                      ＋ Add criterion (AND)
                    </button>

                    <div
                      class="text-[10.5px] font-bold tracking-widest uppercase text-primary-blue mt-4 mb-2"
                    >
                      Then route to
                    </div>
                    <div class="flex items-center gap-3">
                      <span class="text-lg font-bold text-primary-blue">→</span>
                      <select
                        [(ngModel)]="rule.then"
                        class="px-3 py-2 bg-primary-blue/5 border border-primary-blue/30 rounded-lg text-sm font-bold text-primary-blue focus:ring-2 focus:ring-primary-blue outline-none min-w-[210px]"
                      >
                        @for (q of queues(); track q) {
                          <option [value]="q">{{ q }}</option>
                        }
                      </select>
                    </div>
                  </div>
                </div>
              }

              <div
                class="bg-card-white/50 dark:bg-surface/50 border border-dashed border-outline-gray rounded-xl p-4"
              >
                <div class="flex items-center gap-3 mb-2">
                  <div
                    class="w-7 h-7 rounded-md bg-disabled-gray text-text-muted flex items-center justify-center font-bold text-sm"
                  >
                    ⤓
                  </div>
                  <span class="font-bold font-heading text-sm text-text-dark dark:text-text-white"
                    >Fallback</span
                  >
                  <span
                    class="px-2 py-0.5 bg-background border border-outline-gray rounded-md text-[10px] font-bold text-text-muted uppercase tracking-wider"
                    >Always Last</span
                  >
                  <span class="ml-auto font-mono text-[11px] text-text-muted"
                    >no rule matched &nbsp;→&nbsp; {{ fallbackQueue() }}</span
                  >
                </div>
                <p class="text-xs text-text-muted ml-10">
                  Any ticket that matches none of the rules above lands here, so first assignment
                  never fails.
                </p>
              </div>
            </div>

            <button
              (click)="addRule()"
              class="w-full mt-4 py-3.5 bg-revision-amber/10 border-2 border-dashed border-revision-amber/30 text-revision-amber font-bold rounded-xl hover:bg-revision-amber/20 transition-colors flex items-center justify-center gap-2"
            >
              ＋ Add Routing Rule
            </button>
          </div>
        </div>

        <div
          class="w-80 bg-card-white dark:bg-surface border-l border-table-dark-gray dark:border-gray-800 flex flex-col overflow-y-auto p-5"
        >
          <h4
            class="text-[11px] font-bold font-heading tracking-widest uppercase text-text-muted mb-2"
          >
            Criteria Source
          </h4>
          <div class="bg-revision-amber/10 border border-revision-amber/30 rounded-xl p-3 mb-5">
            <div class="flex items-center gap-2 text-xs font-bold font-heading text-amber-800 mb-1">
              ◆ Factor Masters
              <span
                class="ml-auto text-[9.5px] font-mono bg-white border border-revision-amber/30 text-revision-amber px-1.5 py-0.5 rounded-md"
                >FS-04</span
              >
            </div>
            <p class="text-[11.5px] text-amber-700 leading-relaxed m-0">
              The field list and allowed values come straight from your Factor Masters — reusing one
              source of truth.
            </p>
          </div>

          <h4
            class="text-[11px] font-bold font-heading tracking-widest uppercase text-text-muted mb-3"
          >
            Usable as criteria
          </h4>
          <div class="space-y-0">
            @for (f of validFactors(); track f.id) {
              <div
                class="flex items-center justify-between py-2.5 border-b border-table-dark-gray last:border-0"
              >
                <span class="text-xs font-bold text-text-dark dark:text-text-white">{{
                  f.name
                }}</span>
                <div class="flex items-center gap-2">
                  <span
                    class="text-[9px] font-bold tracking-wider px-1.5 py-0.5 rounded-full uppercase"
                    [ngClass]="getKindClasses(f.kind)"
                    >{{ f.kind }}</span
                  >
                  <span class="text-[10.5px] font-mono text-text-muted w-14 text-right">{{
                    f.type === 'num' ? 'numeric' : f.master?.length + ' vals'
                  }}</span>
                </div>
              </div>
            }
          </div>

          <div class="mt-6 pt-4 border-t border-table-dark-gray">
            @for (f of invalidFactors(); track f.id) {
              <div class="flex items-center justify-between py-2 opacity-50">
                <span class="text-xs font-bold text-text-dark dark:text-text-white line-through">{{
                  f.name
                }}</span>
                <span
                  class="text-[9px] font-bold tracking-wider px-1.5 py-0.5 rounded-full uppercase bg-disabled-gray text-text-muted"
                  >{{ f.kind }}</span
                >
              </div>
              <div class="text-[10.5px] text-text-muted mb-2 leading-relaxed ml-1">
                • <b>{{ f.name }}</b> — {{ f.reason }}
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `,
})
export class QueueRoutingRulesComponent implements OnInit {
  private routingService = inject(RoutingRuleService);

  // State Signals
  factors = signal<Factor[]>([]);
  queues = signal<string[]>([]);
  fallbackQueue = signal<string>('General Triage');
  rules = signal<Rule[]>([]);

  isSaving = signal(false);
  isLoading = signal(true);

  // Computed Helpers
  validFactors = computed(() => this.factors().filter((f) => f.criteria));
  invalidFactors = computed(() => this.factors().filter((f) => !f.criteria));

  ngOnInit() {
    this.loadConfig();
  }

  loadConfig() {
    this.isLoading.set(true);
    this.routingService.getConfig().subscribe({
      next: (config) => {
        this.factors.set(config.factors);
        this.queues.set(config.availableQueues);
        this.fallbackQueue.set(config.fallbackQueue);
        this.rules.set(config.rules);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load routing config', err);
        this.isLoading.set(false);
      },
    });
  }

  saveRouting() {
    this.isSaving.set(true);
    this.routingService.saveConfig(this.rules(), this.fallbackQueue()).subscribe({
      next: () => {
        this.isSaving.set(false);
      },
      error: (err) => {
        console.error('Failed to save routing config', err);
        this.isSaving.set(false);
      },
    });
  }

  getFactor(id: string) {
    return this.factors().find((f) => f.id === id);
  }

  getOps(fieldId: string) {
    const f = this.getFactor(fieldId);
    return f?.type === 'num'
      ? ['=', '≠', '>', '≥', '<', '≤']
      : ['is', 'is not', 'is one of', 'is not one of'];
  }

  getKindClasses(kind: string | undefined) {
    switch (kind) {
      case 'hard':
        return 'bg-disabled-gray text-text-dark';
      case 'soft':
        return 'bg-primary-blue/10 text-primary-blue';
      case 'attr':
        return 'bg-success-green/10 text-success-green';
      default:
        return 'bg-background text-text-muted';
    }
  }

  compileRule(rule: Rule): string {
    return rule.when
      .map((c) => {
        const f = this.getFactor(c.field);
        if (!f) return '...';
        const v =
          f.type === 'num' ? (c.vals[0] ?? '...') : '[' + (c.vals.join(', ') || '...') + ']';
        return `${f.name} ${c.op} ${v}`;
      })
      .join('  AND  ');
  }

  // --- MUTATIONS ---
  addRule() {
    this.rules.update((rs) => [
      ...rs,
      { when: [{ field: 'category', op: 'is one of', vals: [] }], then: this.fallbackQueue() },
    ]);
  }

  deleteRule(index: number) {
    this.rules.update((rs) => rs.filter((_, i) => i !== index));
  }

  moveRule(index: number, direction: number) {
    this.rules.update((rs) => {
      const newRs = [...rs];
      const target = index + direction;
      [newRs[index], newRs[target]] = [newRs[target], newRs[index]];
      return newRs;
    });
  }

  addCond(ruleIndex: number) {
    this.rules.update((rs) => {
      const newRs = [...rs];
      newRs[ruleIndex].when.push({ field: 'priority', op: 'is one of', vals: [] });
      return newRs;
    });
  }

  deleteCond(ruleIndex: number, condIndex: number) {
    this.rules.update((rs) => {
      const newRs = JSON.parse(JSON.stringify(rs));
      newRs[ruleIndex].when.splice(condIndex, 1);
      return newRs;
    });
  }

  updateCondField(ruleIndex: number, condIndex: number, field: string) {
    this.rules.update((rs) => {
      const newRs = JSON.parse(JSON.stringify(rs));
      const f = this.getFactor(field);
      newRs[ruleIndex].when[condIndex].field = field;
      newRs[ruleIndex].when[condIndex].op = f?.type === 'num' ? '=' : 'is one of';
      newRs[ruleIndex].when[condIndex].vals = [];
      return newRs;
    });
  }

  updateCondOp(ruleIndex: number, condIndex: number, op: string) {
    this.rules.update((rs) => {
      const newRs = JSON.parse(JSON.stringify(rs));
      newRs[ruleIndex].when[condIndex].op = op;
      return newRs;
    });
  }

  updateCondNum(ruleIndex: number, condIndex: number, val: number) {
    this.rules.update((rs) => {
      const newRs = JSON.parse(JSON.stringify(rs));
      newRs[ruleIndex].when[condIndex].vals = [val];
      return newRs;
    });
  }

  toggleCondVal(ruleIndex: number, condIndex: number, val: string, op: string) {
    this.rules.update((rs) => {
      const newRs = JSON.parse(JSON.stringify(rs));
      const single = op === 'is' || op === 'is not';
      let vals = newRs[ruleIndex].when[condIndex].vals;

      if (single) {
        vals = [val];
      } else {
        const i = vals.indexOf(val);
        i >= 0 ? vals.splice(i, 1) : vals.push(val);
      }

      newRs[ruleIndex].when[condIndex].vals = vals;
      return newRs;
    });
  }
}
