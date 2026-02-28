import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { ProductType } from '../../core/models/product-type.model';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
  ],
  template: `
    <mat-card class="product-card">
      <mat-card-header>
        <mat-icon mat-card-avatar class="product-icon">construction</mat-icon>
        <mat-card-title>{{ product.name }}</mat-card-title>
        <mat-card-subtitle>{{ product.variant }}</mat-card-subtitle>
      </mat-card-header>

      <mat-card-content>
        <p class="product-description">{{ product.description }}</p>
        <mat-chip-set>
          <mat-chip class="family-chip">{{ product.family.name }}</mat-chip>
        </mat-chip-set>
      </mat-card-content>

      <mat-card-actions align="end">
        <button mat-raised-button color="primary" (click)="configure.emit(product)">
          <mat-icon>play_arrow</mat-icon>
          Configureren
        </button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .product-card {
      cursor: default;
      transition: box-shadow 0.3s ease;
      height: 100%;
      display: flex;
      flex-direction: column;

      &:hover {
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15) !important;
      }
    }

    .product-icon {
      background-color: #e3f2fd;
      color: #1976d2;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      width: 40px;
      height: 40px;
    }

    .product-description {
      color: #757575;
      font-size: 0.9rem;
      line-height: 1.4;
      min-height: 40px;
    }

    .family-chip {
      font-size: 0.75rem;
    }

    mat-card-content {
      flex: 1;
    }
  `]
})
export class ProductCardComponent {
  @Input() product!: ProductType;
  @Output() configure = new EventEmitter<ProductType>();
}
