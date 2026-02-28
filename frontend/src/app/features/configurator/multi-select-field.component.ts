import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgFor, NgIf } from '@angular/common';
import { Parameter } from '../../core/models/product-parameters.model';
import { AvailableOption } from '../../core/models/available-options.model';
import { ValidationError } from '../../core/models/validation-result.model';

interface DisplayOption {
  code: string;
  displayName: string;
  isDisabled: boolean;
  reason: string | null;
  checked: boolean;
}

@Component({
  selector: 'app-multi-select-field',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatTooltipModule,
    NgFor,
    NgIf,
  ],
  template: `
    <div class="multi-select-field">
      <label class="field-label">
        {{ parameter.name }}
        <span *ngIf="parameter.isRequired" class="required-mark">*</span>
      </label>
      <div class="checkboxes-container">
        <div
          *ngFor="let opt of displayOptions"
          class="checkbox-item"
          [matTooltip]="opt.reason ?? ''"
          [matTooltipDisabled]="!opt.reason">
          <mat-checkbox
            [checked]="opt.checked"
            [disabled]="opt.isDisabled"
            (change)="onCheckboxChange(opt.code, $event.checked)">
            {{ opt.displayName }}
          </mat-checkbox>
        </div>
      </div>
      <div class="error-message" *ngIf="currentError">{{ currentError }}</div>
    </div>
  `,
  styles: [`
    .multi-select-field {
      margin-bottom: 16px;
    }

    .field-label {
      display: block;
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.6);
      margin-bottom: 8px;
      font-weight: 500;
    }

    .required-mark {
      color: #f44336;
      margin-left: 2px;
    }

    .checkboxes-container {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .checkbox-item {
      min-width: 150px;
    }

    .error-message {
      font-size: 0.75rem;
      color: #f44336;
      margin-top: 4px;
    }
  `]
})
export class MultiSelectFieldComponent implements OnInit, OnChanges {
  @Input() parameter!: Parameter;
  @Input() value: unknown = null;
  @Input() errors: ValidationError[] = [];
  @Input() availableOptions: AvailableOption[] | null = null;
  @Output() valueChange = new EventEmitter<string[]>();

  displayOptions: DisplayOption[] = [];
  selectedValues: Set<string> = new Set();

  get currentError(): string | null {
    const err = this.errors.find(e => e.parameter === this.parameter.code);
    return err?.message ?? null;
  }

  ngOnInit(): void {
    this.initSelectedValues();
    this.buildDisplayOptions();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['availableOptions'] && !changes['availableOptions'].firstChange) {
      this.buildDisplayOptions();
    }
    if (changes['value'] && !changes['value'].firstChange) {
      this.initSelectedValues();
      this.buildDisplayOptions();
    }
  }

  private initSelectedValues(): void {
    if (Array.isArray(this.value)) {
      this.selectedValues = new Set(this.value as string[]);
    } else {
      this.selectedValues = new Set();
    }
  }

  private buildDisplayOptions(): void {
    const staticOptions = this.parameter.options ?? [];

    if (this.availableOptions && this.availableOptions.length > 0) {
      this.displayOptions = staticOptions.map(opt => {
        const apiOpt = this.availableOptions!.find(ao => ao.code === opt.code);
        return {
          code: opt.code,
          displayName: opt.displayName,
          isDisabled: apiOpt ? !apiOpt.available : false,
          reason: apiOpt?.reason ?? null,
          checked: this.selectedValues.has(opt.code),
        };
      });
    } else {
      this.displayOptions = staticOptions
        .filter(opt => opt.isActive)
        .map(opt => ({
          code: opt.code,
          displayName: opt.displayName,
          isDisabled: false,
          reason: null,
          checked: this.selectedValues.has(opt.code),
        }));
    }
  }

  onCheckboxChange(code: string, checked: boolean): void {
    if (checked) {
      this.selectedValues.add(code);
    } else {
      this.selectedValues.delete(code);
    }

    // Update display options
    this.displayOptions = this.displayOptions.map(opt => ({
      ...opt,
      checked: this.selectedValues.has(opt.code)
    }));

    this.valueChange.emit(Array.from(this.selectedValues));
  }
}
