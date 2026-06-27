import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QueueService, Queue, Agent } from '../services/queue.service';

@Component({
  selector: 'app-queue-master',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `<div class="h-full flex flex-col bg-background animate-fade-in">
    <div
      class="flex items-center justify-between px-6 py-4 bg-surface border-b border-disabled-gray dark:border-gray-800 flex-shrink-0"
    >
      <div>
        <h1 class="text-xl font-bold font-heading text-text-dark dark:text-text-white">
          Queue Master
        </h1>
        <p class="text-xs font-sans text-text-muted mt-1 font-mono">Administration / Queues</p>
      </div>
      <div class="flex gap-3">
        <button
          class="px-5 py-2 text-sm font-bold font-sans text-text-dark border border-outline-gray rounded-full hover:bg-table-light-gray transition-colors"
        >
          Discard
        </button>
        <button
          class="px-5 py-2 text-sm font-bold font-sans bg-primary-blue hover:bg-primary-hover text-text-white rounded-full shadow-md transition-all"
        >
          Save Queue
        </button>
      </div>
    </div>

    <div class="flex flex-1 overflow-hidden">
      <div
        class="w-72 bg-card-white dark:bg-surface border-r border-table-dark-gray dark:border-gray-800 flex flex-col overflow-y-auto"
      >
        <div
          class="p-5 sticky top-0 bg-card-white dark:bg-surface z-10 border-b border-table-dark-gray dark:border-gray-800"
        >
          <span
            class="text-[10px] font-bold font-heading tracking-widest uppercase text-primary-blue"
            >Queue Master</span
          >
          <h2 class="text-lg font-bold font-heading text-text-dark dark:text-text-white mt-1">
            Queues
          </h2>
          <p class="text-[11px] text-text-muted font-sans mt-0.5">
            Defined once · referenced by stages
          </p>
        </div>

        <div class="flex flex-col">
          @for (queue of queues(); track queue.id) {
            <div
              (click)="selectQueue(queue.id)"
              class="flex items-center gap-3 p-4 cursor-pointer border-l-4 transition-colors hover:bg-table-light-gray dark:hover:bg-gray-800/50"
              [class.bg-table-light-gray]="selectedQueueId() === queue.id"
              [class.dark:bg-gray-800]="selectedQueueId() === queue.id"
              [class.border-primary-blue]="selectedQueueId() === queue.id"
              [class.border-transparent]="selectedQueueId() !== queue.id"
            >
              <div
                class="w-2.5 h-2.5 rounded-sm flex-shrink-0"
                [style.backgroundColor]="queue.color"
              ></div>
              <div class="flex-1 min-w-0">
                <div
                  class="font-bold text-sm text-text-dark dark:text-text-white truncate font-sans"
                >
                  {{ queue.name }}
                </div>
                <div class="text-[10px] text-text-muted font-mono mt-0.5 truncate">
                  {{ queue.dispatchers.join(' · ') }}
                </div>
              </div>
              <div class="text-right flex-shrink-0">
                <div class="text-[10px] text-text-muted font-mono">
                  {{ queue.members.length }} members
                </div>
                <div class="text-[10px] text-text-muted font-mono mt-0.5">
                  {{ queue.slaN }}{{ queue.slaU === 'min' ? 'm' : 'h' }} pickup
                </div>
              </div>
            </div>
          }
        </div>
        <div class="p-4">
          <button
            (click)="addQueue()"
            class="w-full py-2.5 border border-dashed border-primary-blue bg-light-blue/20 text-primary-blue font-bold text-sm rounded-lg hover:bg-light-blue/40 transition-colors"
          >
            ＋ New Queue
          </button>
        </div>
      </div>

      <div class="flex-1 p-8 overflow-y-auto">
        @if (activeQueue()) {
          <div class="max-w-4xl mx-auto space-y-8 animate-fade-in">
            <div>
              <div class="flex items-start gap-4">
                <div
                  class="w-8 h-8 rounded-lg mt-1"
                  [style.backgroundColor]="activeQueue()!.color"
                ></div>
                <div class="flex-1">
                  <div class="flex items-center justify-between">
                    <h1 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">
                      {{ activeQueue()!.name }}
                    </h1>
                    <span
                      class="px-3 py-1 bg-light-success text-success-green border border-success-green/30 text-xs font-bold rounded-full"
                      >● Active</span
                    >
                  </div>
                  <div class="text-xs text-text-muted font-mono mt-1">
                    queue · {{ activeQueue()!.id }} · referenced by
                    {{ activeQueue()!.usedBy.length }} stage(s)
                  </div>
                </div>
              </div>
              <p class="mt-4 text-sm text-text-muted leading-relaxed font-sans">
                {{ activeQueue()!.desc }}
              </p>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <div class="lg:col-span-2 space-y-6">
                <div
                  class="bg-card-white dark:bg-surface border border-table-dark-gray dark:border-gray-800 rounded-xl p-5 shadow-sm"
                >
                  <div class="flex items-center justify-between mb-2">
                    <h3 class="text-sm font-bold font-heading text-text-dark dark:text-text-white">
                      Members
                    </h3>
                    <span
                      class="text-[10px] font-mono bg-table-light-gray dark:bg-gray-800 text-text-muted px-2 py-0.5 rounded-md border border-outline-gray"
                      >from Agents · FS-03</span
                    >
                  </div>
                  <p class="text-xs text-text-muted mb-4">
                    Agents eligible to receive tickets dispatched from this queue.
                  </p>

                  <div class="flex flex-wrap gap-2">
                    @for (memberId of activeQueue()!.members; track memberId) {
                      <div
                        class="flex items-center gap-2 bg-table-light-gray dark:bg-gray-800 border border-outline-gray rounded-full py-1.5 pl-1.5 pr-3 text-xs font-semibold text-text-dark dark:text-text-white"
                      >
                        <div
                          class="w-6 h-6 rounded-full text-[9px] text-white flex items-center justify-center font-bold"
                          [style.backgroundColor]="getAgent(memberId)?.color"
                        >
                          {{ getAgent(memberId)?.initial }}
                        </div>
                        {{ getAgent(memberId)?.name }}
                        <span class="text-[9px] font-mono text-text-muted ml-1">{{
                          getAgent(memberId)?.role
                        }}</span>
                        <button
                          (click)="removeMember(memberId)"
                          class="ml-1 text-text-muted hover:text-error-red"
                        >
                          ✕
                        </button>
                      </div>
                    }
                    <select
                      class="bg-card-white border border-dashed border-outline-gray text-primary-blue text-xs font-bold rounded-full px-4 py-1.5 focus:outline-none"
                      (change)="addMember($event)"
                    >
                      <option value="">＋ Add member...</option>
                      @for (agent of availableAgents(); track agent.id) {
                        <option [value]="agent.id">{{ agent.name }} ({{ agent.role }})</option>
                      }
                    </select>
                  </div>
                </div>

                <div
                  class="bg-card-white dark:bg-surface border border-table-dark-gray dark:border-gray-800 rounded-xl p-5 shadow-sm"
                >
                  <h3
                    class="text-sm font-bold font-heading text-text-dark dark:text-text-white mb-2"
                  >
                    Pickup SLA & Fallback
                  </h3>
                  <p class="text-xs text-text-muted mb-5">
                    How long a ticket may wait unassigned, and what happens upon breach.
                  </p>

                  <div class="space-y-4">
                    <div>
                      <label class="block text-xs font-bold text-text-muted mb-2"
                        >Time-to-assignment target</label
                      >
                      <div class="flex items-center gap-2">
                        <input
                          type="number"
                          [value]="activeQueue()!.slaN"
                          (input)="updateSlaN($event)"
                          class="w-20 text-center font-mono font-bold text-sm py-2 bg-background border border-outline-gray rounded-lg focus:ring-2 focus:ring-primary-blue outline-none text-text-dark"
                        />
                        <select
                          [value]="activeQueue()!.slaU"
                          (change)="updateSlaU($event)"
                          class="py-2 px-3 bg-background border border-outline-gray rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-blue text-text-dark"
                        >
                          <option value="min">Minutes</option>
                          <option value="hr">Hours</option>
                        </select>
                      </div>
                    </div>

                    <div>
                      <label class="block text-xs font-bold text-text-muted mb-2"
                        >Auto-dispatch fallback strategy</label
                      >
                      <select
                        [value]="activeQueue()!.fb"
                        (change)="updateFb($event)"
                        class="w-full py-2 px-3 bg-background border border-outline-gray rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-blue text-text-dark"
                      >
                        <option value="least-load">Least-load</option>
                        <option value="round-robin">Round-robin</option>
                        <option value="escalate to manager">Escalate to Manager</option>
                      </select>
                    </div>
                  </div>
                </div>
              </div>

              <div class="space-y-6">
                <div
                  class="bg-card-white dark:bg-surface border border-table-dark-gray dark:border-gray-800 rounded-xl p-5 shadow-sm"
                >
                  <h3
                    class="text-sm font-bold font-heading text-text-dark dark:text-text-white mb-2"
                  >
                    Referenced by
                  </h3>
                  <p class="text-xs text-text-muted mb-4">Stages routing tickets here.</p>
                  <div class="space-y-3">
                    @for (ref of activeQueue()!.usedBy; track ref.workflow) {
                      <div
                        class="flex items-center justify-between pb-3 border-b border-table-dark-gray last:border-0 last:pb-0"
                      >
                        <span class="text-xs font-bold text-text-dark dark:text-text-white">{{
                          ref.workflow
                        }}</span>
                        <span class="text-[10px] font-mono text-text-muted">{{ ref.stage }}</span>
                      </div>
                    }
                  </div>
                </div>

                <div
                  class="bg-light-yellow/30 border border-yellow/40 rounded-xl p-4 text-xs text-text-dark leading-relaxed"
                >
                  <strong class="text-revision-amber block mb-1">Source for Routing</strong>
                  The “route to” dropdown in a stage's routing rules lists these exact queues.
                </div>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  </div>`,
})
export class QueueMasterComponent implements OnInit {
  private queueService = inject(QueueService);

  // State
  agents = signal<Record<string, Agent>>({});
  queues = signal<Queue[]>([]);
  selectedQueueId = signal<string | null>(null);

  // Computed
  activeQueue = computed(() => this.queues().find((q) => q.id === this.selectedQueueId()));

  availableAgents = computed(() => {
    const active = this.activeQueue();
    if (!active) return [];
    return Object.values(this.agents()).filter((a) => !active.members.includes(a.id));
  });

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.queueService.getAgents().subscribe((data) => this.agents.set(data));
    this.queueService.getQueues().subscribe((data) => {
      this.queues.set(data);
      if (data.length > 0 && !this.selectedQueueId()) {
        this.selectedQueueId.set(data[0].id);
      }
    });
  }

  // Actions
  selectQueue(id: string) {
    this.selectedQueueId.set(id);
  }
  getAgent(id: string) {
    return this.agents()[id];
  }

  removeMember(agentId: string) {
    const active = this.activeQueue();
    if (!active) return;

    const newMembers = active.members.filter((m) => m !== agentId);
    // Optimistic UI update
    this.updateLocalQueue({ members: newMembers });
    // API Call
    this.queueService.updateMembers(active.id, newMembers).subscribe();
  }

  addMember(event: Event) {
    const select = event.target as HTMLSelectElement;
    const agentId = select.value;
    const active = this.activeQueue();
    if (!agentId || !active) return;

    const newMembers = [...active.members, agentId];
    // Optimistic UI update
    this.updateLocalQueue({ members: newMembers });
    // API Call
    this.queueService.updateMembers(active.id, newMembers).subscribe();
    select.value = '';
  }

  updateSlaN(event: Event) {
    const val = Number((event.target as HTMLInputElement).value);
    this.updateLocalQueue({ slaN: val });
    this.queueService.updateQueue(this.activeQueue()!.id, { slaN: val }).subscribe();
  }

  updateSlaU(event: Event) {
    const val = (event.target as HTMLSelectElement).value;
    this.updateLocalQueue({ slaU: val });
    this.queueService.updateQueue(this.activeQueue()!.id, { slaU: val }).subscribe();
  }

  updateFb(event: Event) {
    const val = (event.target as HTMLSelectElement).value;
    this.updateLocalQueue({ fb: val });
    this.queueService.updateQueue(this.activeQueue()!.id, { fb: val }).subscribe();
  }

  private updateLocalQueue(partial: Partial<Queue>) {
    this.queues.update((qs) =>
      qs.map((q) => (q.id === this.selectedQueueId() ? { ...q, ...partial } : q)),
    );
  }

  addQueue() {
    const newQueue: Partial<Queue> = {
      name: 'New Queue',
      color: '#747480',
      desc: 'Describe what this queue is for.',
      members: [],
      dispatchers: ['Lead'],
      slaN: 15,
      slaU: 'min',
      fb: 'least-load',
      usedBy: [],
    };

    this.queueService.createQueue(newQueue).subscribe((createdQueue) => {
      this.queues.update((qs) => [...qs, createdQueue]);
      this.selectedQueueId.set(createdQueue.id);
    });
  }
}
