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
    loadComponent: () => import('./shopping-list/shopping-list').then(m => m.ShoppingList),
  },
  {
    path: 'recipes',
    loadComponent: () => import('./recipes/recipes').then(m => m.Recipes),
  },
];
