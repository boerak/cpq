import { Injectable, inject, signal, computed } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ConfigurationService } from '../../core/services/configuration.service';
import { ProductService } from '../../core/services/product.service';
import { Configuration } from '../../core/models/configuration.model';
import { Step } from '../../core/models/product-parameters.model';
import { AvailableOption } from '../../core/models/available-options.model';
import { ValidationError } from '../../core/models/validation-result.model';

@Injectable()
export class ConfiguratorStore {
  private readonly configService = inject(ConfigurationService);
  private readonly productService = inject(ProductService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  // State signals
  configId = signal<string>('');
  configuration = signal<Configuration | null>(null);
  steps = signal<Step[]>([]);
  currentStepIndex = signal<number>(0);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  error = signal<string | null>(null);

  // Computed signals
  currentStep = computed(() => this.steps()[this.currentStepIndex()]);

  currentStepErrors = computed((): ValidationError[] => {
    const config = this.configuration();
    const step = this.currentStep();
    if (!config?.validation?.errors || !step) return [];

    const paramCodes = new Set(step.parameters.map(p => p.code));
    return config.validation.errors.filter(e => paramCodes.has(e.parameter));
  });

  canGoNext = computed(() =>
    this.currentStepIndex() < this.steps().length - 1 &&
    this.currentStepErrors().length === 0
  );

  canGoPrev = computed(() => this.currentStepIndex() > 0);

  canFinalize = computed(() => this.configuration()?.canFinalize ?? false);

  allErrors = computed((): ValidationError[] =>
    this.configuration()?.validation?.errors ?? []
  );

  // Load configuration and its parameters
  loadConfiguration(configId: string): void {
    this.configId.set(configId);
    this.isLoading.set(true);
    this.error.set(null);

    this.configService.getConfiguration(configId).subscribe({
      next: (config) => {
        this.configuration.set(config);

        // Load parameters for the product type
        this.productService.getParameters(config.productType.code).subscribe({
          next: (parametersResponse) => {
            this.steps.set(parametersResponse.steps);
            this.isLoading.set(false);
          },
          error: (err) => {
            this.error.set('Fout bij laden van parameters.');
            this.isLoading.set(false);
            console.error('Error loading parameters:', err);
          }
        });
      },
      error: (err) => {
        this.error.set('Fout bij laden van configuratie.');
        this.isLoading.set(false);
        console.error('Error loading configuration:', err);
      }
    });
  }

  // Update a field value and PATCH to API
  updateField(code: string, value: unknown): void {
    const config = this.configuration();
    if (!config) return;

    this.isSaving.set(true);

    this.configService.updateConfiguration(config.id, {
      selections: { [code]: value },
      expectedVersion: config.version
    }).subscribe({
      next: (updatedConfiguration) => {
        // Handle resetFields notification
        if (updatedConfiguration.resetFields && updatedConfiguration.resetFields.length > 0) {
          const resetList = updatedConfiguration.resetFields.join(', ');
          this.snackBar.open(
            `Veld ${resetList} is gereset vanwege wijziging in ${code}`,
            'Sluiten',
            { duration: 5000, panelClass: ['warning-snackbar'] }
          );
        }

        this.configuration.set(updatedConfiguration);
        this.isSaving.set(false);
      },
      error: (err) => {
        this.isSaving.set(false);
        console.error('Error updating field:', err);
      }
    });
  }

  // Navigate to next step
  nextStep(): void {
    if (this.canGoNext()) {
      this.currentStepIndex.update(i => i + 1);
    }
  }

  // Navigate to previous step
  prevStep(): void {
    if (this.canGoPrev()) {
      this.currentStepIndex.update(i => i - 1);
    }
  }

  // Go to specific step index
  goToStep(index: number): void {
    if (index >= 0 && index < this.steps().length) {
      this.currentStepIndex.set(index);
    }
  }

  // Finalize configuration
  finalize(): void {
    const config = this.configuration();
    if (!config || !this.canFinalize()) return;

    this.isSaving.set(true);

    this.configService.finalizeConfiguration(config.id).subscribe({
      next: (finalizedConfig) => {
        this.configuration.set(finalizedConfig);
        this.isSaving.set(false);
        this.router.navigate(['/configure', config.id, 'bom']);
      },
      error: (err) => {
        this.isSaving.set(false);
        console.error('Error finalizing configuration:', err);
        this.snackBar.open('Fout bij afronden van configuratie.', 'Sluiten', { duration: 3000 });
      }
    });
  }

  // Get available options for a parameter
  getAvailableOptions(paramCode: string): AvailableOption[] | null {
    const config = this.configuration();
    if (!config?.availableOptions) return null;
    return config.availableOptions[paramCode] ?? null;
  }
}
