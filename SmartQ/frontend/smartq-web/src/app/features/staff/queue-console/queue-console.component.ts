import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import Swal, { SweetAlertIcon, SweetAlertOptions, SweetAlertResult } from 'sweetalert2';
import { CounterApiService } from '../../../core/api/counter-api.service';
import { StaffConsoleApiService } from '../services/staff-console-api.service';
import { StaffStateService } from '../services/staff-state.service';

@Component({
  selector: 'app-queue-console',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './queue-console.component.html',
  styleUrl: './queue-console.component.scss'
})
export class QueueConsoleComponent {
  readonly state = inject(StaffStateService);
  private readonly api = inject(StaffConsoleApiService);
  private readonly counterApi = inject(CounterApiService);
  private readonly busy = signal(false);
  readonly counters = signal<{ id: number; counterNo: string; counterName: string }[]>([]);
  readonly actionBusy = computed(() => this.busy());
  readonly active = computed(() => this.state.activeSession());
  readonly queue = computed(() => this.state.queue());
  readonly nextInLine = computed(() => this.queue()[0]?.tokenNo ?? '—');
  readonly vipCount = computed(() => this.queue().filter((q) => q.priority === 'VIP').length);
  readonly elapsedDisplay = computed(() => {
    const secs = this.active()?.elapsedSeconds ?? 0;
    const m = Math.floor(secs / 60).toString().padStart(2, '0');
    const s = (secs % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  });

  constructor() {
    this.counterApi.getCounters().subscribe({
      next: (items) => this.counters.set(items.map((c) => ({ id: c.id, counterNo: c.counterNo, counterName: c.counterName }))),
      error: () => this.counters.set([])
    });
  }

  ngOnInit(): void {
    // Always normalize queue view to counter-assigned scope when entering this page.
    this.state.loadAll('my-services');
  }

  private popup(title: string, text: string, icon: SweetAlertIcon): Promise<SweetAlertResult<unknown>> {
    return Swal.fire({
      icon,
      title,
      text,
      timer: 1800,
      showConfirmButton: false,
      customClass: {
        popup: 'staff-swal-popup',
        title: 'staff-swal-title',
        htmlContainer: 'staff-swal-text'
      }
    });
  }

  private modal(options: SweetAlertOptions): Promise<SweetAlertResult<any>> {
    return Swal.fire({
      ...options,
      customClass: {
        popup: 'staff-swal-popup',
        title: 'staff-swal-title',
        htmlContainer: 'staff-swal-text',
        confirmButton: 'staff-swal-confirm',
        cancelButton: 'staff-swal-cancel',
        validationMessage: 'staff-swal-validation'
      },
      buttonsStyling: false
    });
  }

  callNext(): void {
    if (this.busy()) return;
    this.busy.set(true);
    this.api.callNext(this.state.counterId()).subscribe({
      next: (res) => {
        this.state.loadAll();
        void this.popup(res.hasToken ? 'Token Called' : 'No Eligible Token', res.message, res.hasToken ? 'success' : 'info');
      },
      error: () => {
        this.busy.set(false);
        void this.popup('Action Failed', 'Could not call next token.', 'error');
      },
      complete: () => { this.busy.set(false); }
    });
  }
  complete(): void {
    const tokenId = this.active()?.tokenId;
    if (!tokenId) return;
    if (this.busy()) return;
    this.busy.set(true);
    this.api.complete(tokenId, this.state.counterId()).subscribe({
      next: (res) => {
        this.state.loadAll();
        void this.popup(res.success ? 'Service Completed' : 'Not Completed', res.message, res.success ? 'success' : 'info');
      },
      error: () => {
        this.busy.set(false);
        void this.popup('Action Failed', 'Complete action failed.', 'error');
      },
      complete: () => { this.busy.set(false); }
    });
  }
  startService(): void {
    const tokenId = this.active()?.tokenId;
    if (!tokenId) return;
    this.api.startService(tokenId, this.state.counterId()).subscribe(() => this.state.loadAll());
  }
  recall(): void {
    const tokenId = this.active()?.tokenId;
    if (!tokenId) return;
    if (this.busy()) return;
    this.busy.set(true);
    this.api.recall(tokenId, this.state.counterId()).subscribe({
      next: (res) => {
        this.state.loadAll();
        void this.popup(res.success ? 'Token Recalled' : 'Recall Skipped', res.message, res.success ? 'success' : 'info');
      },
      error: () => {
        this.busy.set(false);
        void this.popup('Action Failed', 'Recall action failed.', 'error');
      },
      complete: () => { this.busy.set(false); }
    });
  }
  noShow(): void {
    const tokenId = this.active()?.tokenId;
    if (!tokenId) return;
    if (this.busy()) return;
    this.busy.set(true);
    this.api.noShow(tokenId, this.state.counterId()).subscribe({
      next: (res) => {
        this.state.loadAll();
        void this.popup(res.success ? 'Marked as No Show' : 'No Show Not Applied', res.message, res.success ? 'warning' : 'info');
      },
      error: () => {
        this.busy.set(false);
        void this.popup('Action Failed', 'No show action failed.', 'error');
      },
      complete: () => { this.busy.set(false); }
    });
  }

  async transfer(): Promise<void> {
    const tokenId = this.active()?.tokenId;
    if (!tokenId || this.busy()) return;
    this.busy.set(true);
    this.api.getTransferOptions().subscribe({
      next: async (opts) => {
        const serviceOptions = opts.services.map((s) => `<option value="${s.id}">${s.name}</option>`).join('');
        const subOptions = opts.subServices.map((s) => `<option value="${s.id}" data-service="${s.serviceId}">${s.name}</option>`).join('');
        const counterOptions = ['<option value="">Auto assign later</option>']
          .concat(opts.counters.map((c) => `<option value="${c.id}">${c.counterName}</option>`))
          .join('');

        const result = await this.modal({
          title: 'Transfer Token',
          html: `
            <div style="display:grid;gap:10px;text-align:left">
              <label>Transfer To Service</label>
              <select id="swal-service" class="swal2-input" style="margin:0">${serviceOptions}</select>
              <label>Sub-Service</label>
              <select id="swal-subservice" class="swal2-input" style="margin:0">${subOptions}</select>
              <label>Counter (Optional)</label>
              <select id="swal-counter" class="swal2-input" style="margin:0">${counterOptions}</select>
              <label>Reason</label>
              <textarea id="swal-reason" class="swal2-textarea" placeholder="Reason for transfer"></textarea>
            </div>
          `,
          showCancelButton: true,
          confirmButtonText: 'Transfer Token',
          cancelButtonText: 'Cancel',
          preConfirm: () => {
            const serviceId = Number((document.getElementById('swal-service') as HTMLSelectElement)?.value);
            const subServiceId = Number((document.getElementById('swal-subservice') as HTMLSelectElement)?.value);
            const counterRaw = (document.getElementById('swal-counter') as HTMLSelectElement)?.value;
            const reason = (document.getElementById('swal-reason') as HTMLTextAreaElement)?.value ?? '';

            const selectedSub = opts.subServices.find((x) => x.id === subServiceId);
            if (!serviceId || !subServiceId || !selectedSub || selectedSub.serviceId !== serviceId) {
              Swal.showValidationMessage('Please choose matching service and sub-service.');
              return null;
            }
            return {
              targetServiceId: serviceId,
              targetSubServiceId: subServiceId,
              targetCounterId: counterRaw ? Number(counterRaw) : null,
              reason
            };
          }
        });

        if (!result.isConfirmed || !result.value) {
          this.busy.set(false);
          return;
        }

        this.api.transfer(tokenId, this.state.counterId(), result.value).subscribe({
          next: (res) => {
            this.state.loadAll();
            void this.popup(res.success ? 'Token Transferred' : 'Transfer Not Applied', res.message, res.success ? 'success' : 'info');
          },
          error: () => {
            this.busy.set(false);
            void this.popup('Action Failed', 'Transfer action failed.', 'error');
          },
          complete: () => { this.busy.set(false); }
        });
      },
      error: () => {
        this.busy.set(false);
        void this.popup('Action Failed', 'Could not load transfer options.', 'error');
      }
    });
  }

  statusKind(status: string): 'ok' | 'warn' | 'danger' | 'neutral' {
    const s = status.toUpperCase();
    if (s.includes('WAIT')) return 'neutral';
    if (s.includes('VIP')) return 'ok';
    if (s.includes('SKIP') || s.includes('OVER')) return 'danger';
    return 'warn';
  }

  onCounterChange(event: Event): void {
    const selected = Number((event.target as HTMLSelectElement).value);
    this.state.setCounter(selected, 'my-services');
  }
}
