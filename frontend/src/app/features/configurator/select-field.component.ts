import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgFor, NgIf } from '@angular/common';
import { Parameter, Option } from '../../core/models/product-parameters.model';
import { AvailableOption } from '../../core/models/available-options.model';
import { ValidationError } from '../../core/models/validation-result.model';

interface DisplayOption {
  code: string;
  displayName: string;
  isDisabled: boolean;
  reason: string | null;
}

@Component({
  selector: 'app-select-field',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatTooltipModule,
    NgFor,
    NgIf,
  ],
  template: `
    <mat-form-field class="full-width" appearance="outline">
      <mat-label>
        {{ parameter.name }}
        <span *ngIf="parameter.isRequired" class="required-mark">*</span>
      </mat-label>
      <mat-select [formControl]="control">
        <mat-option
          *ngFor="let opt of displayOptions"
          [value]="opt.code"
          [disabled]="opt.isDisabled"
          [matTooltip]="opt.reason ?? ''"
          [matTooltipDisabled]="!opt.reason">
          {{ opt.displayName }}
          <span *ngIf="opt.isDisabled && opt.reason" class="unavailable-hint"> (niet beschikbaar)</span>
        </mat-option>
      </mat-select>
      <mat-error *ngIf="currentError">{{ currentError }}</mat-error>
    </mat-form-field>
  `,
  styles: [`
    .full-width {
      width: 100%;
    }
    .required-mark {
      color: #f44336;
      margin-left: 2px;
    }
    .unavailable-hint {
      font-size: 0.75rem;
      color: #9e9e9e;
    }
  `]
})
export class SelectFieldComponent implements OnInit, OnChanges {
  @Input() parameter!: Parameter;
  @Input() value: unknown = null;
  @Input() errors: ValidationError[] = [];
  @Input() availableOptions: AvailableOption[] | null = null;
  @Output() valueChange = new EventEmitter<string | null>();

  control = new FormControl<string | null>(null);
  displayOptions: DisplayOption[] = [];

  get currentError(): string | null {
    const err = this.errors.find(e => e.parameter === this.parameter.code);
    return err?.message ?? null;
  }

  ngOnInit(): void {
    this.buildDisplayOptions();

    if (this.value !== null && this.value !== undefined) {
      this.control.setValue(this.value as string, { emitEvent: false });
    }

    this.control.valueChanges.subscribe(val => {
      this.valueChange.emit(val);
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['availableOptions'] && !changes['availableOptions'].firstChange) {
      this.buildDisplayOptions();
    }
    if (changes['value'] && !changes['value'].firstChange) {
      const newVal = changes['value'].currentValue;
      if (newVal !== this.control.value) {
        this.control.setValue(newVal as string | null, { emitEvent: false });
      }
    }
  }

  private buildDisplayOptions(): void {
    const staticOptions = this.parameter.options ?? [];

    if (this.availableOptions && this.availableOptions.length > 0) {
      // Use API-provided availability data
      this.displayOptions = staticOptions.map(opt => {
        const apiOpt = this.availableOptions!.find(ao => ao.code === opt.code);
        return {
          code: opt.code,
          displayName: opt.displayName,
          isDisabled: apiOpt ? !apiOpt.available : false,
          reason: apiOpt?.reason ?? null,
        };
      });
    } else {
      // Fall back to static options
      this.displayOptions = staticOptions
        .filter(opt => opt.available)
        .map(opt => ({
          code: opt.code,
          displayName: opt.displayName,
          isDisabled: false,
          reason: null,
        }));
    }
  }
}
