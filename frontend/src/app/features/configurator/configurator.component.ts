import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgFor, NgIf } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ConfiguratorStore } from './configurator.store';
import { WizardStepComponent } from './wizard-step.component';
import { ValidationPanelComponent } from './validation-panel.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { AvailableOption } from '../../core/models/available-options.model';

@Component({
  selector: 'app-configurator',
  standalone: true,
  imports: [
    NgFor,
    NgIf,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressBarModule,
    MatTooltipModule,
    WizardStepComponent,
    ValidationPanelComponent,
    LoadingSpinnerComponent,
    StatusBadgeComponent,
  ],
  providers: [ConfiguratorStore],
  templateUrl: './configurator.component.html',
  styleUrl: './configurator.component.scss'
})
export class ConfiguratorComponent implements OnInit {
  readonly store = inject(ConfiguratorStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  ngOnInit(): void {
    const configId = this.route.snapshot.paramMap.get('configId');
    if (configId) {
      this.store.loadConfiguration(configId);
    }
  }

  onTabChange(index: number): void {
    this.store.goToStep(index);
  }

  onFieldChange(event: { code: string; value: unknown }): void {
    this.store.updateField(event.code, event.value);
  }

  getAvailableOptionsMap(): Record<string, AvailableOption[]> {
    const config = this.store.configuration();
    return (config?.availableOptions as Record<string, AvailableOption[]>) ?? {};
  }

  navigateBack(): void {
    this.router.navigate(['/products']);
  }

  get progressValue(): number {
    const steps = this.store.steps();
    const current = this.store.currentStepIndex();
    if (steps.length === 0) return 0;
    return Math.round(((current + 1) / steps.length) * 100);
  }
}
