import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgIf } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Parameter } from '../../core/models/product-parameters.model';
import { ValidationError } from '../../core/models/validation-result.model';

@Component({
  selector: 'app-dimension-field',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    NgIf,
  ],
  template: `
    <mat-form-field class="full-width" appearance="outline">
      <mat-label>
        {{ parameter.name }}
        <span *ngIf="parameter.isRequired" class="required-mark">*</span>
      </mat-label>
      <input
        matInput
        type="number"
        [formControl]="control"
        [placeholder]="placeholder"
        [min]="minValue"
        [max]="maxValue">
      <span matSuffix *ngIf="parameter.unit" class="unit-suffix">{{ parameter.unit }}</span>
      <mat-hint *ngIf="hintText">{{ hintText }}</mat-hint>
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
    .unit-suffix {
      color: #757575;
      font-size: 0.85rem;
    }
  `]
})
export class DimensionFieldComponent implements OnInit, OnDestroy {
  @Input() parameter!: Parameter;
  @Input() value: unknown = null;
  @Input() errors: ValidationError[] = [];
  @Output() valueChange = new EventEmitter<number | null>();

  control = new FormControl<number | null>(null);
  private destroy$ = new Subject<void>();

  get minValue(): number | null {
    return (this.parameter.metadata?.['min'] as number) ?? null;
  }

  get maxValue(): number | null {
    return (this.parameter.metadata?.['max'] as number) ?? null;
  }

  get placeholder(): string {
    const min = this.minValue;
    const max = this.maxValue;
    if (min !== null && max !== null) {
      return `${min} - ${max}`;
    }
    return '';
  }

  get hintText(): string {
    const min = this.minValue;
    const max = this.maxValue;
    if (min !== null && max !== null) {
      const unit = this.parameter.unit ? ` ${this.parameter.unit}` : '';
      return `Min: ${min}${unit}, Max: ${max}${unit}`;
    }
    return '';
  }

  get currentError(): string | null {
    const err = this.errors.find(e => e.parameter === this.parameter.code);
    return err?.message ?? null;
  }

  ngOnInit(): void {
    // Set initial value
    if (this.value !== null && this.value !== undefined) {
      this.control.setValue(this.value as number, { emitEvent: false });
    }

    // Subscribe with 400ms debounce for numeric inputs
    this.control.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(val => {
      this.valueChange.emit(val);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
