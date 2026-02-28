import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { NgFor, NgIf } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProductService } from '../../core/services/product.service';
import { ConfigurationService } from '../../core/services/configuration.service';
import { ProductFamily } from '../../core/models/product-family.model';
import { ProductType } from '../../core/models/product-type.model';
import { ProductCardComponent } from './product-card.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner.component';

@Component({
  selector: 'app-product-selector',
  standalone: true,
  imports: [
    NgFor,
    NgIf,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    ProductCardComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './product-selector.component.html',
  styleUrl: './product-selector.component.scss'
})
export class ProductSelectorComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly configurationService = inject(ConfigurationService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  families = signal<ProductFamily[]>([]);
  productsByFamily = signal<Record<string, ProductType[]>>({});
  selectedFamilyIndex = signal<number>(0);
  isLoadingFamilies = signal<boolean>(false);
  isLoadingProducts = signal<boolean>(false);
  isCreatingConfig = signal<boolean>(false);
  error = signal<string | null>(null);

  selectedFamily = computed(() => this.families()[this.selectedFamilyIndex()]);
  selectedProducts = computed(() => {
    const family = this.selectedFamily();
    if (!family) return [];
    return this.productsByFamily()[family.code] ?? [];
  });

  ngOnInit(): void {
    this.loadFamilies();
  }

  private loadFamilies(): void {
    this.isLoadingFamilies.set(true);
    this.error.set(null);

    this.productService.getFamilies().subscribe({
      next: (families) => {
        const activeFamilies = families.filter(f => f.isActive);
        this.families.set(activeFamilies);
        this.isLoadingFamilies.set(false);

        if (activeFamilies.length > 0) {
          this.loadProductsForFamily(activeFamilies[0]);
        }
      },
      error: (err) => {
        this.error.set('Fout bij laden van productfamilies.');
        this.isLoadingFamilies.set(false);
        console.error('Error loading families:', err);
      }
    });
  }

  private loadProductsForFamily(family: ProductFamily): void {
    if (this.productsByFamily()[family.code]) {
      return; // Already loaded
    }

    this.isLoadingProducts.set(true);

    this.productService.getProductsByFamily(family.code).subscribe({
      next: (products) => {
        this.productsByFamily.update(current => ({
          ...current,
          [family.code]: products
        }));
        this.isLoadingProducts.set(false);
      },
      error: (err) => {
        this.isLoadingProducts.set(false);
        console.error('Error loading products:', err);
        this.snackBar.open('Fout bij laden van producten.', 'Sluiten', { duration: 3000 });
      }
    });
  }

  onTabChange(index: number): void {
    this.selectedFamilyIndex.set(index);
    const family = this.families()[index];
    if (family) {
      this.loadProductsForFamily(family);
    }
  }

  onConfigureProduct(product: ProductType): void {
    this.isCreatingConfig.set(true);

    this.configurationService.createConfiguration({ productTypeCode: product.code }).subscribe({
      next: (config) => {
        this.isCreatingConfig.set(false);
        this.router.navigate(['/configure', config.id]);
      },
      error: (err) => {
        this.isCreatingConfig.set(false);
        console.error('Error creating configuration:', err);
        this.snackBar.open('Fout bij aanmaken configuratie.', 'Sluiten', { duration: 3000 });
      }
    });
  }
}
