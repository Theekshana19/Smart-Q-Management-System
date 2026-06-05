import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { StaffTokenDetails } from '../../../../core/models/staff-console.models';

@Component({
  selector: 'app-token-details-drawer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './token-details-drawer.component.html',
  styleUrl: './token-details-drawer.component.scss'
})
export class TokenDetailsDrawerComponent {
  readonly open = input(false);
  readonly loading = input(false);
  readonly details = input<StaffTokenDetails | null>(null);
  readonly showActions = input(true);

  readonly closed = output<void>();
  readonly recall = output<number>();
  readonly transfer = output<number>();
  readonly cancel = output<number>();

  onClose(): void {
    this.closed.emit();
  }

  onRecall(): void {
    const id = this.details()?.tokenId;
    if (id) this.recall.emit(id);
  }

  onTransfer(): void {
    const id = this.details()?.tokenId;
    if (id) this.transfer.emit(id);
  }

  onCancel(): void {
    const id = this.details()?.tokenId;
    if (id) this.cancel.emit(id);
  }

  isVip(priority: string): boolean {
    return priority.toUpperCase() !== 'STANDARD';
  }

  priorityLabel(priority: string): string {
    return this.isVip(priority) ? 'VIP Priority' : 'Regular';
  }

  formatStatus(status: string): string {
    const normalized = status.replace(/_/g, ' ').toLowerCase();
    return normalized.charAt(0).toUpperCase() + normalized.slice(1);
  }

  formatCreatedTime(value: string): string {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '--';
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  queuePositionLabel(position: number): string {
    return `${position}`.padStart(2, '0');
  }

  canAct(status: string): boolean {
    const s = status.toUpperCase();
    return !['COMPLETED', 'SKIPPED', 'CANCELLED', 'TRANSFERRED'].includes(s);
  }

  canRecall(status: string): boolean {
    const s = status.toUpperCase();
    return s === 'CALLED' || s === 'SERVING';
  }

  canTransfer(status: string): boolean {
    const s = status.toUpperCase();
    return s === 'WAITING' || s === 'CALLED' || s === 'SERVING';
  }

  canCancel(status: string): boolean {
    const s = status.toUpperCase();
    return s === 'WAITING' || s === 'CALLED' || s === 'SERVING';
  }
}
