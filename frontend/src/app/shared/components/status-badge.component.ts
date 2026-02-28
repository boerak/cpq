import { Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [NgClass, MatChipsModule, MatIconModule],
  template: `
    <span class="status-badge" [ngClass]="statusClass">
      <mat-icon class="status-icon">{{ statusIcon }}</mat-icon>
      {{ statusLabel }}
    </span>
  `,
  styles: [`
    .status-badge {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 4px 10px;
      border-radius: 12px;
      font-size: 0.8rem;
      font-weight: 500;

      .status-icon {
        font-size: 14px;
        width: 14px;
        height: 14px;
      }

      &.status-draft {
        background-color: #e0e0e0;
        color: #424242;
      }

      &.status-valid {
        background-color: #c8e6c9;
        color: #1b5e20;
      }

      &.status-invalid {
        background-color: #ffcdd2;
        color: #b71c1c;
      }

      &.status-finalized {
        background-color: #bbdefb;
        color: #0d47a1;
      }
    }
  `]
})
export class StatusBadgeComponent {
  @Input() status: 'draft' | 'valid' | 'invalid' | 'finalized' = 'draft';

  get statusClass(): string {
    return `status-${this.status}`;
  }

  get statusLabel(): string {
    const labels: Record<string, string> = {
      draft: 'Concept',
      valid: 'Geldig',
      invalid: 'Ongeldig',
      finalized: 'Afgerond'
    };
    return labels[this.status] ?? this.status;
  }

  get statusIcon(): string {
    const icons: Record<string, string> = {
      draft: 'edit',
      valid: 'check_circle',
      invalid: 'error',
      finalized: 'lock'
    };
    return icons[this.status] ?? 'info';
  }
}
