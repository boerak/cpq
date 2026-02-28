import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgFor, NgIf, DatePipe, DecimalPipe, SlicePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { BomService } from '../../core/services/bom.service';
import { ConfigurationService } from '../../core/services/configuration.service';
import { BomResponse, BomLine } from '../../core/models/bom.model';
import { Configuration } from '../../core/models/configuration.model';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  selector: 'app-bom-viewer',
  standalone: true,
  imports: [
    NgFor,
    NgIf,
    DatePipe,
    DecimalPipe,
    RouterLink,
    SlicePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDividerModule,
    MatTooltipModule,
    MatChipsModule,
    LoadingSpinnerComponent,
    StatusBadgeComponent,
  ],
  template: `
    <div class="bom-viewer">

      <!-- Loading -->
      <app-loading-spinner [loading]="isLoading()" message="BOM laden..."></app-loading-spinner>

      <!-- Error -->
      <div *ngIf="!isLoading() && error()" class="error-container">
        <p class="error-message">{{ error() }}</p>
        <button mat-raised-button (click)="goBack()">
          <mat-icon>arrow_back</mat-icon>
          Terug
        </button>
      </div>

      <!-- BOM Content -->
      <div *ngIf="!isLoading() && !error() && bom() as bom" class="bom-content">

        <!-- Header -->
        <div class="bom-header">
          <button mat-icon-button (click)="goBack()" matTooltip="Terug naar configurator">
            <mat-icon>arrow_back</mat-icon>
          </button>
          <div class="header-info">
            <h1>Stuklijst (BOM)</h1>
            <p *ngIf="configuration()" class="product-info">
              {{ configuration()?.productType?.name }} &mdash; {{ configuration()?.productType?.variant }}
            </p>
          </div>
          <span class="spacer"></span>
          <app-status-badge *ngIf="configuration()" [status]="configuration()!.status"></app-status-badge>
        </div>

        <!-- Summary -->
        <mat-card class="summary-card">
          <mat-card-content>
            <div class="summary-grid">
              <div class="summary-item">
                <span class="summary-label">Totaal onderdelen</span>
                <span class="summary-value">{{ bom.lines.length }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Totaalgewicht</span>
                <span class="summary-value">{{ bom.totalWeight | number:'1.2-2' }} kg</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Gegenereerd op</span>
                <span class="summary-value">{{ bom.generatedAt | date:'dd-MM-yyyy HH:mm' }}</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- BOM Lines table -->
        <mat-card>
          <mat-card-header>
            <mat-card-title>Onderdelen</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="sortedLines(bom.lines)" class="bom-table">

              <!-- Sort Order / # -->
              <ng-container matColumnDef="sortOrder">
                <th mat-header-cell *matHeaderCellDef>#</th>
                <td mat-cell *matCellDef="let line; let i = index">{{ i + 1 }}</td>
              </ng-container>

              <!-- SKU -->
              <ng-container matColumnDef="partSku">
                <th mat-header-cell *matHeaderCellDef>Artikelnr.</th>
                <td mat-cell *matCellDef="let line">
                  <code class="sku">{{ line.partSku }}</code>
                </td>
              </ng-container>

              <!-- Name -->
              <ng-container matColumnDef="partName">
                <th mat-header-cell *matHeaderCellDef>Omschrijving</th>
                <td mat-cell *matCellDef="let line">{{ line.partName }}</td>
              </ng-container>

              <!-- Category -->
              <ng-container matColumnDef="category">
                <th mat-header-cell *matHeaderCellDef>Categorie</th>
                <td mat-cell *matCellDef="let line">
                  <mat-chip class="category-chip">{{ line.category }}</mat-chip>
                </td>
              </ng-container>

              <!-- Quantity -->
              <ng-container matColumnDef="quantity">
                <th mat-header-cell *matHeaderCellDef>Aantal</th>
                <td mat-cell *matCellDef="let line">
                  {{ line.quantity | number:'1.0-3' }} {{ line.unit }}
                </td>
              </ng-container>

              <!-- Cut Length -->
              <ng-container matColumnDef="cutLengthMm">
                <th mat-header-cell *matHeaderCellDef>Zaaglengte</th>
                <td mat-cell *matCellDef="let line">
                  <span *ngIf="line.cutLengthMm; else nocut">{{ line.cutLengthMm }} mm</span>
                  <ng-template #nocut><span class="muted">—</span></ng-template>
                </td>
              </ng-container>

              <!-- Notes -->
              <ng-container matColumnDef="notes">
                <th mat-header-cell *matHeaderCellDef>Notities</th>
                <td mat-cell *matCellDef="let line">
                  <span *ngIf="line.notes; else nonotes" [matTooltip]="line.notes">
                    {{ line.notes | slice:0:40 }}<span *ngIf="line.notes.length > 40">...</span>
                  </span>
                  <ng-template #nonotes><span class="muted">—</span></ng-template>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>

            </table>

            <div *ngIf="bom.lines.length === 0" class="no-lines">
              <p>Geen onderdelen in stuklijst.</p>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Action buttons -->
        <div class="action-buttons">
          <button mat-stroked-button (click)="goBack()">
            <mat-icon>edit</mat-icon>
            Terug naar configurator
          </button>
          <button mat-stroked-button routerLink="/configurations">
            <mat-icon>list</mat-icon>
            Alle configuraties
          </button>
          <button mat-raised-button color="primary" routerLink="/products">
            <mat-icon>add</mat-icon>
            Nieuwe configuratie
          </button>
        </div>

      </div>

    </div>
  `,
  styles: [`
    .bom-viewer {
      max-width: 1200px;
      margin: 0 auto;
      padding: 24px;
    }

    .error-container {
      text-align: center;
      padding: 48px;

      .error-message {
        color: #f44336;
        margin-bottom: 24px;
      }
    }

    .bom-content {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .bom-header {
      display: flex;
      align-items: center;
      gap: 12px;

      .header-info {
        h1 {
          margin: 0;
          font-size: 1.4rem;
          font-weight: 400;
        }

        .product-info {
          margin: 2px 0 0;
          font-size: 0.85rem;
          color: #757575;
        }
      }

      .spacer {
        flex: 1;
      }
    }

    .summary-card {
      mat-card-content {
        padding: 16px !important;
      }
    }

    .summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 16px;
    }

    .summary-item {
      display: flex;
      flex-direction: column;
      gap: 4px;

      .summary-label {
        font-size: 0.75rem;
        color: #757575;
        text-transform: uppercase;
        letter-spacing: 0.5px;
      }

      .summary-value {
        font-size: 1.1rem;
        font-weight: 500;
        color: #212121;
      }
    }

    .bom-table {
      width: 100%;
    }

    .sku {
      font-family: monospace;
      font-size: 0.85rem;
      background: #f5f5f5;
      padding: 2px 6px;
      border-radius: 3px;
    }

    .category-chip {
      font-size: 0.75rem;
    }

    .muted {
      color: #bdbdbd;
    }

    .no-lines {
      text-align: center;
      padding: 32px;
      color: #9e9e9e;
    }

    .action-buttons {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    }
  `]
})
export class BomViewerComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bomService = inject(BomService);
  private readonly configService = inject(ConfigurationService);

  bom = signal<BomResponse | null>(null);
  configuration = signal<Configuration | null>(null);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  displayedColumns = ['sortOrder', 'partSku', 'partName', 'category', 'quantity', 'cutLengthMm', 'notes'];

  ngOnInit(): void {
    const configId = this.route.snapshot.paramMap.get('configId');
    if (configId) {
      this.loadBom(configId);
    }
  }

  private loadBom(configId: string): void {
    this.isLoading.set(true);
    this.error.set(null);

    // Load both BOM and configuration in parallel
    this.configService.getConfiguration(configId).subscribe({
      next: (config) => {
        this.configuration.set(config);
      },
      error: (err) => console.error('Error loading configuration:', err)
    });

    this.bomService.getBom(configId).subscribe({
      next: (bom) => {
        if (bom.lines.length > 0) {
          this.bom.set(bom);
          this.isLoading.set(false);
        } else {
          // No BOM lines yet — generate on demand
          this.generateBom(configId);
        }
      },
      error: () => {
        // GET failed — try to generate
        this.generateBom(configId);
      }
    });
  }

  private generateBom(configId: string): void {
    this.bomService.generateBom(configId).subscribe({
      next: (bom) => {
        this.bom.set(bom);
        this.isLoading.set(false);
      },
      error: (genErr) => {
        this.error.set('Fout bij laden van stuklijst. Controleer of de configuratie afgerond is.');
        this.isLoading.set(false);
        console.error('Error generating BOM:', genErr);
      }
    });
  }

  sortedLines(lines: BomLine[]): BomLine[] {
    return [...lines].sort((a, b) => a.sortOrder - b.sortOrder);
  }

  goBack(): void {
    const configId = this.route.snapshot.paramMap.get('configId');
    if (configId) {
      this.router.navigate(['/configure', configId]);
    } else {
      this.router.navigate(['/configurations']);
    }
  }
}
