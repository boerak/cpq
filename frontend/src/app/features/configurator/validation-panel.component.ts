import { Component, Input } from '@angular/core';
import { NgFor, NgIf, NgClass } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatListModule } from '@angular/material/list';
import { ValidationResult } from '../../core/models/validation-result.model';

@Component({
  selector: 'app-validation-panel',
  standalone: true,
  imports: [NgFor, NgIf, NgClass, MatIconModule, MatExpansionModule, MatListModule],
  template: `
    <div class="validation-panel"
         [ngClass]="{
           'has-errors': validation?.errors?.length,
           'has-warnings': validation?.warnings?.length && !validation?.errors?.length
         }"
         *ngIf="hasMessages">

      <mat-expansion-panel [expanded]="!isCollapsible" [disabled]="!isCollapsible">
        <mat-expansion-panel-header>
          <mat-panel-title>
            <mat-icon class="panel-icon error-icon" *ngIf="validation?.errors?.length">error</mat-icon>
            <mat-icon class="panel-icon warning-icon" *ngIf="!validation?.errors?.length && validation?.warnings?.length">warning</mat-icon>
            <span *ngIf="validation?.errors?.length">
              {{ validation?.errors?.length }} fout(en)
              <span *ngIf="validation?.warnings?.length"> en {{ validation?.warnings?.length }} waarschuwing(en)</span>
            </span>
            <span *ngIf="!validation?.errors?.length && validation?.warnings?.length">
              {{ validation?.warnings?.length }} waarschuwing(en)
            </span>
          </mat-panel-title>
        </mat-expansion-panel-header>

        <div class="validation-content">
          <!-- Errors -->
          <div *ngIf="validation?.errors?.length" class="error-section">
            <h4 class="section-title error-title">
              <mat-icon>error</mat-icon>
              Fouten
            </h4>
            <mat-list>
              <mat-list-item *ngFor="let error of validation?.errors" class="error-item">
                <mat-icon matListItemIcon class="error-icon">cancel</mat-icon>
                <span matListItemTitle class="error-param">{{ error.parameter }}</span>
                <span matListItemLine class="error-message">{{ error.message }}</span>
              </mat-list-item>
            </mat-list>
          </div>

          <!-- Warnings -->
          <div *ngIf="validation?.warnings?.length" class="warning-section">
            <h4 class="section-title warning-title">
              <mat-icon>warning</mat-icon>
              Waarschuwingen
            </h4>
            <mat-list>
              <mat-list-item *ngFor="let warning of validation?.warnings" class="warning-item">
                <mat-icon matListItemIcon class="warning-icon">warning_amber</mat-icon>
                <span matListItemTitle class="warning-param">{{ warning.parameter }}</span>
                <span matListItemLine class="warning-message">{{ warning.message }}</span>
              </mat-list-item>
            </mat-list>
          </div>
        </div>
      </mat-expansion-panel>
    </div>
  `,
  styles: [`
    .validation-panel {
      margin: 16px 0;
      border-radius: 4px;
      overflow: hidden;

      &.has-errors {
        border-left: 4px solid #f44336;
      }

      &.has-warnings {
        border-left: 4px solid #ff9800;
      }
    }

    .panel-icon {
      margin-right: 8px;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .error-icon {
      color: #f44336;
    }

    .warning-icon {
      color: #ff9800;
    }

    .validation-content {
      padding: 0 16px 8px;
    }

    .section-title {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 8px 0;
      font-size: 0.9rem;
      font-weight: 500;
    }

    .error-title {
      color: #d32f2f;
    }

    .warning-title {
      color: #f57c00;
    }

    .error-item,
    .warning-item {
      font-size: 0.85rem;
    }

    .error-param,
    .warning-param {
      font-weight: 500;
    }

    .error-message,
    .warning-message {
      color: #616161;
    }
  `]
})
export class ValidationPanelComponent {
  @Input() validation: ValidationResult | undefined | null;
  @Input() isCollapsible = true;

  get hasMessages(): boolean {
    return !!(
      (this.validation?.errors && this.validation.errors.length > 0) ||
      (this.validation?.warnings && this.validation.warnings.length > 0)
    );
  }
}
