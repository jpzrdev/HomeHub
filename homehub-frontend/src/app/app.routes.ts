import { Routes } from '@angular/router';
import { Home } from './home/home';

export const routes: Routes = [
  {
    path: '',
    component: Home,
  },
  {
    path: 'inventory',
    loadComponent: () => import('./inventory/inventory').then(m => m.Inventory),
  },
  {
    path: 'shopping-list',
    loadComponent: () => import('./home/home').then(m => m.Home), // Placeholder - will be replaced with actual shopping list component
  },
];
