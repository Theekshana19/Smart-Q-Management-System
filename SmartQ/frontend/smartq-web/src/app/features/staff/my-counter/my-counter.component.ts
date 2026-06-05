import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import Swal, { SweetAlertIcon, SweetAlertOptions, SweetAlertResult } from 'sweetalert2';
import { catchError, forkJoin, of, switchMap } from 'rxjs';
import {
  StaffMyCounter,
  StaffMyCounterActiveDetails,
  StaffMyCounterEfficiency,
  StaffMyCounterUpcomingToken,
  StaffQueueItem,
  StaffTokenDetails
} from '../../../core/models/staff-console.models';
import { StaffConsoleApiService } from '../services/staff-console-api.service';
import { StaffStateService } from '../services/staff-state.service';

@Component({
  selector: 'app-my-counter',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-counter.component.html',
  styleUrl: './my-counter.component.scss'
})
export class MyCounterComponent implements OnInit, OnDestroy {
  readonly state = inject(StaffStateService);
  private readonly api = inject(StaffConsoleApiService);

  readonly data = signal<StaffMyCounter | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  private readonly busy = signal(false);
  readonly actionBusy = computed(() => this.busy());
  private tickTimer?: ReturnType<typeof setInterval>;

  readonly elapsedDisplay = computed(() => {
    const secs = this.data()?.activeSession?.elapsedSeconds ?? 0;
    const m = Math.floor(secs / 60).toString().padStart(2, '0');
    const s = (secs % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  });

  ngOnInit(): void {
    this.state.loadAll('my-services');
    this.load();
    this.tickTimer = setInterval(() => {
      const current = this.data();
      if (!current?.activeSession) return;
      this.data.set({
        ...current,
        activeSession: {
          ...current.activeSession,
          elapsedSeconds: current.activeSession.elapsedSeconds + 1
        }
      });
    }, 1000);
  }

  ngOnDestroy(): void {
    if (this.tickTimer) clearInterval(this.tickTimer);
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.getMyCounter().pipe(
      catchError(() => this.loadMyCounterFallback())
    ).subscribe({
      next: (res) => this.applyMyCounter(res),
      error: () => {
        this.error.set('Unable to load My Counter workspace.');
        this.loading.set(false);
      }
    });
  }

  private applyMyCounter(res: StaffMyCounter): void {
    this.data.set(res);
    this.state.context.set(res.context);
    this.state.summary.set(res.summary);
    this.state.activeSession.set(res.activeSession);
    this.loading.set(false);
  }

  /** Uses existing staff-console endpoints when /my-counter is not yet deployed. */
  private loadMyCounterFallback() {
    return forkJoin({
      context: this.api.getContext(),
      summary: this.api.getSummary(),
      activeSession: this.api.getActiveSession(),
      queue: this.api.getQueue('my-services'),
      performance: this.api.getPerformance('today')
    }).pipe(
      switchMap((parts) => {
        const tokenId = parts.activeSession?.tokenId;
        if (!tokenId) return of(this.composeMyCounter(parts, null));
        return this.api.getTokenDetails(tokenId).pipe(
          catchError(() => of(null)),
          switchMap((details) => of(this.composeMyCounter(parts, details)))
        );
      })
    );
  }

  private composeMyCounter(
    parts: {
      context: StaffMyCounter['context'];
      summary: StaffMyCounter['summary'];
      activeSession: StaffMyCounter['activeSession'];
      queue: StaffQueueItem[];
      performance: StaffMyCounter['performance'];
    },
    details: StaffTokenDetails | null
  ): StaffMyCounter {
    const msgs = parts.context.displayMessages ?? {};
    const threshold = Number(msgs['STAFF_QUEUE_PRESSURE_HIGH_THRESHOLD']) || 12;
    const upcomingLimit = Number(msgs['STAFF_MY_COUNTER_UPCOMING_COUNT']) || 3;
    const pressurePercent = threshold <= 0
      ? 0
      : Math.min(100, Math.round((parts.summary.waiting / threshold) * 100));

    const pressureLabel =
      parts.summary.queuePressure === 'HIGH'
        ? msgs['STAFF_QUEUE_PRESSURE_HIGH'] ?? 'Queue pressure is high. Avoid taking breaks at this time.'
        : parts.summary.queuePressure === 'NORMAL'
          ? msgs['STAFF_QUEUE_PRESSURE_NORMAL'] ?? 'Queue pressure is moderate.'
          : msgs['STAFF_QUEUE_PRESSURE_LOW'] ?? 'Queue pressure is low.';

    const upcoming: StaffMyCounterUpcomingToken[] = parts.queue.slice(0, upcomingLimit).map((q) => ({
      tokenId: q.tokenId,
      tokenNo: q.tokenNo,
      tokenPrefixBadge: this.extractPrefixBadge(q.tokenNo),
      subServiceName: q.subServiceName,
      waitMinutes: q.waitMinutes
    }));

    let activeDetails: StaffMyCounterActiveDetails | null = null;
    if (parts.activeSession && details) {
      const idFormat = msgs['STAFF_TOKEN_ID_FORMAT'] ?? 'TK-{id}';
      const isPriority = details.priority.toUpperCase() !== 'STANDARD';
      activeDetails = {
        tokenIdLabel: idFormat.replace('{id}', String(details.tokenId)),
        customerLabel: isPriority
          ? msgs['STAFF_CUSTOMER_LABEL_VIP'] ?? 'Priority Member'
          : msgs['STAFF_CUSTOMER_LABEL_STANDARD'] ?? 'Regular Member',
        waitTimeDisplay: `${details.waitingMinutes}m`,
        waitMinutes: details.waitingMinutes
      };
    }

    const breakAllowance = Number(msgs['STAFF_BREAK_ALLOWANCE_MINUTES']) || 60;
    const breakUsed = Number(msgs['STAFF_BREAK_USED_MINUTES']) || 15;
    const efficiency: StaffMyCounterEfficiency = {
      efficiencyPercent: Math.round(parts.performance.completionRate),
      efficiencyTrend: msgs['STAFF_EFFICIENCY_TREND'] ?? '+2% since last hour',
      breakTimeDisplay: `${breakUsed}m / ${breakAllowance}m`,
      successRateDisplay: `${parts.performance.completionRate}%`,
      shiftEndsInDisplay: '--'
    };

    return {
      context: parts.context,
      summary: parts.summary,
      activeSession: parts.activeSession,
      activeDetails,
      upcomingTokens: upcoming,
      performance: parts.performance,
      efficiency,
      queuePressurePercent: pressurePercent,
      queuePressureLabel: pressureLabel
    };
  }

  private extractPrefixBadge(tokenNo: string): string {
    const idx = tokenNo.indexOf('-');
    if (idx <= 0) return tokenNo.slice(0, 2).toUpperCase();
    return tokenNo.slice(0, idx).toUpperCase();
  }

  msg(key: string, fallback: string): string {
    return this.data()?.context.displayMessages?.[key] ?? fallback;
  }

  greeting(d: StaffMyCounter): string {
    const staffName = d.context.staff?.fullName?.split(' ')[0] ?? 'Officer';
    const hour = new Date().getHours();
    const part = hour < 12 ? 'morning' : hour < 17 ? 'afternoon' : 'evening';
    const services = d.context.assignedServices.map((s) => s.name).join(', ') || 'assigned services';
    const template = this.msg('STAFF_MY_COUNTER_GREETING', 'You are currently handling the priority queue for assigned services.');
    return `Good ${part}, ${staffName}. ${template.replace('assigned services', services)}`;
  }

  statusLabel(d: StaffMyCounter): string {
    const status = d.summary.currentStatus?.toUpperCase() ?? '';
    if (d.activeSession) return 'In Service';
    if (status === 'AVAILABLE') return 'Available';
    if (status === 'OFFLINE') return 'Offline';
    if (status === 'MAINTENANCE') return 'Busy';
    return status || 'Active';
  }

  isInService(d: StaffMyCounter): boolean {
    return !!d.activeSession || d.summary.currentStatus === 'SERVING';
  }

  displayTokenNo(tokenNo: string): string {
    const raw = tokenNo.includes('(') ? tokenNo.split('(')[0].trim() : tokenNo;
    return raw.replace('-', ' ');
  }

  priorityIconFill(priority: string): string {
    return priority.toUpperCase() === 'VIP' ? "'FILL' 1" : "'FILL' 0";
  }

  setCounterStatus(status: 'AVAILABLE' | 'BUSY' | 'BREAK' | 'OFFLINE'): void {
    if (this.busy()) return;
    this.busy.set(true);
    this.api.updateCounterStatus({ status }).subscribe({
      next: (res) => {
        void this.popup(res.success ? 'Status Updated' : 'Status Not Updated', res.message, res.success ? 'success' : 'info');
        this.load();
        this.state.loadAll('my-services');
      },
      error: (err) => {
        this.busy.set(false);
        const notFound = err?.status === 404;
        void this.popup(
          'Action Failed',
          notFound
            ? 'Counter status API is not available yet. Restart the backend API and try again.'
            : 'Could not update counter status.',
          'error'
        );
      },
      complete: () => this.busy.set(false)
    });
  }

  complete(): void {
    const tokenId = this.data()?.activeSession?.tokenId;
    if (!tokenId || this.busy()) return;
    this.busy.set(true);
    this.api.complete(tokenId).subscribe({
      next: (res) => {
        void this.popup(res.success ? 'Service Completed' : 'Not Completed', res.message, res.success ? 'success' : 'info');
        this.load();
        this.state.loadAll('my-services');
      },
      error: () => {
        this.busy.set(false);
        void this.popup('Action Failed', 'Complete action failed.', 'error');
      },
      complete: () => this.busy.set(false)
    });
  }

  noShow(): void {
    const tokenId = this.data()?.activeSession?.tokenId;
    if (!tokenId || this.busy()) return;
    this.busy.set(true);
    this.api.noShow(tokenId).subscribe({
      next: (res) => {
        void this.popup(res.success ? 'Marked as No Show' : 'No Show Not Applied', res.message, res.success ? 'warning' : 'info');
        this.load();
        this.state.loadAll('my-services');
      },
      error: () => {
        this.busy.set(false);
        void this.popup('Action Failed', 'No show action failed.', 'error');
      },
      complete: () => this.busy.set(false)
    });
  }

  async transfer(): Promise<void> {
    const tokenId = this.data()?.activeSession?.tokenId;
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

        this.api.transfer(tokenId, result.value).subscribe({
          next: (res) => {
            void this.popup(res.success ? 'Token Transferred' : 'Transfer Not Applied', res.message, res.success ? 'success' : 'info');
            this.load();
            this.state.loadAll('my-services');
          },
          error: () => {
            this.busy.set(false);
            void this.popup('Action Failed', 'Transfer action failed.', 'error');
          },
          complete: () => this.busy.set(false)
        });
      },
      error: () => {
        this.busy.set(false);
        void this.popup('Action Failed', 'Could not load transfer options.', 'error');
      }
    });
  }

  private popup(title: string, text: string, icon: SweetAlertIcon): Promise<SweetAlertResult<unknown>> {
    return Swal.fire({
      icon,
      title,
      text,
      timer: 1800,
      showConfirmButton: false,
      customClass: { popup: 'staff-swal-popup', title: 'staff-swal-title', htmlContainer: 'staff-swal-text' }
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
}
