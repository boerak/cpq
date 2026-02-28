import { Component, Input, Output, EventEmitter } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { Step, Parameter } from '../../core/models/product-parameters.model';
import { AvailableOption } from '../../core/models/available-options.model';
import { ValidationError } from '../../core/models/validation-result.model';
import { ParameterFieldComponent } from './parameter-field.component';

@Component({
  selector: 'app-wizard-step',
  standalone: true,
  imports: [NgFor, NgIf, ParameterFieldComponent],
  template: `
    <div class="wizard-step">
      <h2 class="step-title">{{ step.stepName }}</h2>

      <div class="parameters-grid">
        <div
          *ngFor="let parameter of visibleParameters"
          class="parameter-wrapper">
          <app-parameter-field
            [parameter]="parameter"
            [value]="getFieldValue(parameter.code)"
            [errors]="getFieldErrors(parameter.code)"
            [availableOptions]="getAvailableOptions(parameter.code)"
            (valueChange)="onFieldChange(parameter.code, $event)">
          </app-parameter-field>
        </div>
      </div>

      <div *ngIf="visibleParameters.length === 0" class="no-parameters">
        <p>Geen parameters beschikbaar voor deze stap.</p>
      </div>
    </div>
  `,
  styles: [`
    .wizard-step {
      padding: 8px 0;
    }

    .step-title {
      font-size: 1.2rem;
      font-weight: 400;
      color: #424242;
      margin: 0 0 24px;
    }

    .parameters-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 8px 24px;
    }

    .parameter-wrapper {
      min-width: 0;
    }

    .no-parameters {
      text-align: center;
      padding: 24px;
      color: #9e9e9e;
    }
  `]
})
export class WizardStepComponent {
  @Input() step!: Step;
  @Input() configValues: Record<string, unknown> = {};
  @Input() errors: ValidationError[] = [];
  @Input() availableOptionsMap: Record<string, AvailableOption[]> = {};
  @Output() fieldChange = new EventEmitter<{ code: string; value: unknown }>();

  get visibleParameters(): Parameter[] {
    return this.step.parameters.filter(p => this.isVisible(p));
  }

  private isVisible(parameter: Parameter): boolean {
    const visibleWhen = parameter.metadata?.['visibleWhen'] as string | undefined;
    if (!visibleWhen) return true;

    return this.evaluateVisibleWhen(visibleWhen);
  }

  /**
   * Evaluates simple visibleWhen expressions like "driveType == 'motor'"
   * Does NOT use eval(). Supports == and != operators.
   */
  private evaluateVisibleWhen(expression: string): boolean {
    // Support "fieldCode == 'value'" or "fieldCode != 'value'"
    const eqMatch = expression.match(/^(\w+)\s*==\s*['"]?([^'"]+)['"]?$/);
    if (eqMatch) {
      const fieldCode = eqMatch[1].trim();
      const expectedValue = eqMatch[2].trim();
      const currentValue = this.configValues[fieldCode];
      return String(currentValue ?? '') === expectedValue;
    }

    const neqMatch = expression.match(/^(\w+)\s*!=\s*['"]?([^'"]+)['"]?$/);
    if (neqMatch) {
      const fieldCode = neqMatch[1].trim();
      const expectedValue = neqMatch[2].trim();
      const currentValue = this.configValues[fieldCode];
      return String(currentValue ?? '') !== expectedValue;
    }

    // Unknown expression format - show by default
    return true;
  }

  getFieldValue(code: string): unknown {
    return this.configValues[code] ?? null;
  }

  getFieldErrors(code: string): ValidationError[] {
    return this.errors.filter(e => e.parameter === code);
  }

  getAvailableOptions(code: string): AvailableOption[] | null {
    return this.availableOptionsMap[code] ?? null;
  }

  onFieldChange(code: string, value: unknown): void {
    this.fieldChange.emit({ code, value });
  }
}
