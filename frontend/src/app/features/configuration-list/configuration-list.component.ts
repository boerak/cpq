import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { ConfigurationService } from '../../core/services/configuration.service';
import { Configuration } from '../../core/models/configuration.model';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  selector: 'app-configuration-list',
  standalone: true,
  imports: [
    NgFor,
    NgIf,
    DatePipe,
    FormsModule,
    RouterLink,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTooltipModule,
    MatChipsModule,
    LoadingSpinnerComponent,
    StatusBadgeComponent,
  ],
  template: `
    <div class="configuration-list">
      <div class="page-header">
        <h1>Configuraties</h1>
        <button mat-raised-button color="primary" routerLink="/products">
          <mat-icon>add</mat-icon>
          Nieuwe configuratie
        </button>
      </div>

      <!-- Status filter -->
      <mat-card class="filter-card">
        <mat-card-content>
          <mat-form-field appearance="outline" class="filter-field">
            <mat-label>Filter op status</mat-label>
            <mat-select [(ngModel)]="statusFilter" (ngModelChange)="applyFilter()">
              <mat-option value="">Alle statussen</mat-option>
              <mat-option value="draft">Concept</mat-option>
              <mat-option value="valid">Geldig</mat-option>
              <mat-option value="invalid">Ongeldig</mat-option>
              <mat-option value="finalized">Afgerond</mat-option>
            </mat-select>
          </mat-form-field>
        </mat-card-content>
      </mat-card>

      <app-loading-spinner [loading]="isLoading()" message="Configuraties laden..."></app-loading-spinner>

      <div *ngIf="!isLoading() && error()" class="error-message">
        {{ error() }}
      </div>

      <mat-card *ngIf="!isLoading() && !error()">
        <mat-card-content>
          <table mat-table [dataSource]="filteredConfigurations()" class="config-table">

            <!-- Product type column -->
            <ng-container matColumnDef="productType">
              <th mat-header-cell *matHeaderCellDef>Product</th>
              <td mat-cell *matCellDef="let config">
                <div class="product-cell">
                  <span class="product-name">{{ config.productType.name }}</span>
                  <span class="product-variant">{{ config.productType.variant }}</span>
                </div>
              </td>
            </ng-container>

            <!-- Status column -->
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let config">
                <app-status-badge [status]="config.status"></app-status-badge>
              </td>
            </ng-container>

            <!-- Updated at column -->
            <ng-container matColumnDef="updatedAt">
              <th mat-header-cell *matHeaderCellDef>Bijgewerkt</th>
              <td mat-cell *matCellDef="let config">
                {{ config.updatedAt | date:'dd-MM-yyyy HH:mm' }}
              </td>
            </ng-container>

            <!-- Created at column -->
            <ng-container matColumnDef="createdAt">
              <th mat-header-cell *matHeaderCellDef>Aangemaakt</th>
              <td mat-cell *matCellDef="let config">
                {{ config.createdAt | date:'dd-MM-yyyy HH:mm' }}
              </td>
            </ng-container>

            <!-- Actions column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Acties</th>
              <td mat-cell *matCellDef="let config">
                <div class="action-buttons">
                  <button
                    mat-icon-button
                    color="primary"
                    [routerLink]="['/configure', config.id]"
                    matTooltip="Bewerken">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button
                    mat-icon-button
                    color="primary"
                    *ngIf="config.status === 'finalized'"
                    [routerLink]="['/configure', config.id, 'bom']"
                    matTooltip="BOM bekijken">
                    <mat-icon>receipt_long</mat-icon>
                  </button>
                  <button
                    mat-icon-button
                    (click)="cloneConfiguration(config)"
                    matTooltip="Kopiëren">
                    <mat-icon>content_copy</mat-icon>
                  </button>
                  <button
                    mat-icon-button
                    color="warn"
                    (click)="deleteConfiguration(config)"
                    matTooltip="Verwijderen">
                    <mat-icon>delete</mat-icon>
                  </button>
                </div>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;" class="config-row"></tr>

          </table>

          <div *ngIf="filteredConfigurations().length === 0 && !isLoading()" class="no-configurations">
            <mat-icon class="no-data-icon">inbox</mat-icon>
            <p>Geen configuraties gevonden.</p>
            <button mat-raised-button color="primary" routerLink="/products">
              Maak eerste configuratie
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .configuration-list {
      max-width: 1200px;
      margin: 0 auto;
      padding: 24px;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;

      h1 {
        margin: 0;
        font-size: 1.8rem;
        font-weight: 300;
        color: #212121;
      }
    }

    .filter-card {
      margin-bottom: 16px;

      mat-card-content {
        padding: 16px 16px 0 !important;
      }
    }

    .filter-field {
      min-width: 200px;
    }

    .config-table {
      width: 100%;
    }

    .product-cell {
      display: flex;
      flex-direction: column;

      .product-name {
        font-weight: 500;
      }

      .product-variant {
        font-size: 0.8rem;
        color: #757575;
      }
    }

    .action-buttons {
      display: flex;
      gap: 4px;
    }

    .config-row {
      &:hover {
        background-color: #f5f5f5;
      }
    }

    .no-configurations {
      text-align: center;
      padding: 48px;
      color: #9e9e9e;

      .no-data-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        margin-bottom: 16px;
      }

      p {
        margin-bottom: 24px;
      }
    }

    .error-message {
      color: #f44336;
      padding: 24px;
      text-align: center;
    }
  `]
})
export class ConfigurationListComponent implements OnInit {
  private readonly configService = inject(ConfigurationService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  configurations = signal<Configuration[]>([]);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);
  statusFilter = '';

  displayedColumns = ['productType', 'status', 'updatedAt', 'createdAt', 'actions'];

  filteredConfigurations = computed(() => {
    if (!this.statusFilter) {
      return this.configurations();
    }
    return this.configurations().filter(c => c.status === this.statusFilter);
  });

  ngOnInit(): void {
    this.loadConfigurations();
  }

  private loadConfigurations(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.configService.listConfigurations().subscribe({
      next: (configs) => {
        // Sort by updatedAt descending
        const sorted = [...configs].sort((a, b) =>
          new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
        );
        this.configurations.set(sorted);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Fout bij laden van configuraties.');
        this.isLoading.set(false);
        console.error('Error loading configurations:', err);
      }
    });
  }

  applyFilter(): void {
    // filteredConfigurations computed signal handles this automatically
  }

  cloneConfiguration(config: Configuration): void {
    this.configService.cloneConfiguration(config.id).subscribe({
      next: (cloned) => {
        this.snackBar.open('Configuratie gekopieerd.', 'Sluiten', { duration: 3000 });
        this.router.navigate(['/configure', cloned.id]);
      },
      error: (err) => {
        console.error('Error cloning configuration:', err);
        this.snackBar.open('Fout bij kopiëren.', 'Sluiten', { duration: 3000 });
      }
    });
  }

  deleteConfiguration(config: Configuration): void {
    if (!confirm(`Weet u zeker dat u de configuratie wilt verwijderen?`)) {
      return;
    }

    this.configService.deleteConfiguration(config.id).subscribe({
      next: () => {
        this.configurations.update(list => list.filter(c => c.id !== config.id));
        this.snackBar.open('Configuratie verwijderd.', 'Sluiten', { duration: 3000 });
      },
      error: (err) => {
        console.error('Error deleting configuration:', err);
        this.snackBar.open('Fout bij verwijderen.', 'Sluiten', { duration: 3000 });
      }
    });
  }
}
