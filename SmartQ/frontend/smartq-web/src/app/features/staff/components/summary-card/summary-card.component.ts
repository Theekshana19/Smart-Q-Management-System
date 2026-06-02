import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-summary-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './summary-card.component.html',
  styleUrl: './summary-card.component.scss'
})
export class SummaryCardComponent {
  @Input({ required: true }) title = '';
  @Input({ required: true }) value = '';
  @Input() hint = '';
  @Input() progressPercent?: number;
}
