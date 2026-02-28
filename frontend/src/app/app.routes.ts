import { Routes } from '@angular/router';
import { ProductSelectorComponent } from './features/product-selector/product-selector.component';
import { ConfiguratorComponent } from './features/configurator/configurator.component';
import { BomViewerComponent } from './features/bom-viewer/bom-viewer.component';
import { ConfigurationListComponent } from './features/configuration-list/configuration-list.component';

export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products', component: ProductSelectorComponent },
  { path: 'configure/:configId', component: ConfiguratorComponent },
  { path: 'configure/:configId/bom', component: BomViewerComponent },
  { path: 'configurations', component: ConfigurationListComponent },
];
