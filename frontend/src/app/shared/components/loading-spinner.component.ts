import { Component, Input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [MatProgressSpinnerModule, NgIf],
  template: `
    <div class="spinner-container" *ngIf="loading">
      <mat-spinner [diameter]="diameter"></mat-spinner>
      <p *ngIf="message" class="spinner-message">{{ message }}</p>
    </div>
  `,
  styles: [`
    .spinner-container {
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      padding: 32px;
      gap: 16px;
    }
    .spinner-message {
      color: #757575;
      margin: 0;
    }
  `]
})
export class LoadingSpinnerComponent {
  @Input() loading = false;
  @Input() message = '';
  @Input() diameter = 48;
}
