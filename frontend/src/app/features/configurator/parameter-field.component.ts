import { Component, Input, Output, EventEmitter } from '@angular/core';
import { NgIf, NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Parameter } from '../../core/models/product-parameters.model';
import { AvailableOption } from '../../core/models/available-options.model';
import { ValidationError } from '../../core/models/validation-result.model';
import { DimensionFieldComponent } from './dimension-field.component';
import { SelectFieldComponent } from './select-field.component';
import { MultiSelectFieldComponent } from './multi-select-field.component';

@Component({
  selector: 'app-parameter-field',
  standalone: true,
  imports: [
    NgIf,
    NgSwitch,
    NgSwitchCase,
    NgSwitchDefault,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    DimensionFieldComponent,
    SelectFieldComponent,
    MultiSelectFieldComponent,
  ],
  template: `
    <div class="parameter-field" [ngSwitch]="parameter.dataType">

      <!-- Integer / numeric dimension field -->
      <app-dimension-field
        *ngSwitchCase="'integer'"
        [parameter]="parameter"
        [value]="value"
        [errors]="errors"
        (valueChange)="onValueChange($event)">
      </app-dimension-field>

      <!-- Single select dropdown -->
      <app-select-field
        *ngSwitchCase="'select'"
        [parameter]="parameter"
        [value]="value"
        [errors]="errors"
        [availableOptions]="availableOptions"
        (valueChange)="onValueChange($event)">
      </app-select-field>

      <!-- Multi-select checkboxes -->
      <app-multi-select-field
        *ngSwitchCase="'multi_select'"
        [parameter]="parameter"
        [value]="value"
        [errors]="errors"
        [availableOptions]="availableOptions"
        (valueChange)="onValueChange($event)">
      </app-multi-select-field>

      <!-- Boolean checkbox -->
      <div *ngSwitchCase="'boolean'" class="boolean-field">
        <mat-checkbox
          [checked]="!!value"
          (change)="onValueChange($event.checked)">
          {{ parameter.name }}
          <span *ngIf="parameter.isRequired" class="required-mark">*</span>
        </mat-checkbox>
        <div class="error-message" *ngIf="currentError">{{ currentError }}</div>
      </div>

      <!-- Text input -->
      <mat-form-field *ngSwitchDefault class="full-width" appearance="outline">
        <mat-label>
          {{ parameter.name }}
          <span *ngIf="parameter.isRequired" class="required-mark">*</span>
        </mat-label>
        <input
          matInput
          [value]="value ?? ''"
          (change)="onTextChange($event)">
        <mat-error *ngIf="currentError">{{ currentError }}</mat-error>
      </mat-form-field>

    </div>
  `,
  styles: [`
    .parameter-field {
      margin-bottom: 16px;
    }

    .full-width {
      width: 100%;
    }

    .required-mark {
      color: #f44336;
      margin-left: 2px;
    }

    .boolean-field {
      margin-bottom: 8px;
    }

    .error-message {
      font-size: 0.75rem;
      color: #f44336;
      margin-top: 4px;
    }
  `]
})
export class ParameterFieldComponent {
  @Input() parameter!: Parameter;
  @Input() value: unknown = null;
  @Input() errors: ValidationError[] = [];
  @Input() availableOptions: AvailableOption[] | null = null;
  @Output() valueChange = new EventEmitter<unknown>();

  get currentError(): string | null {
    const err = this.errors.find(e => e.parameter === this.parameter.code);
    return err?.message ?? null;
  }

  onValueChange(val: unknown): void {
    this.valueChange.emit(val);
  }

  onTextChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.valueChange.emit(input.value);
  }
}
