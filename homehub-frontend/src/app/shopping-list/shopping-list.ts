import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, ShoppingList as ApiShoppingList, ShoppingListItem as ApiShoppingListItem } from '../services/api.service';

export interface ShoppingListItem {
  id: string;
  inventoryItemId: string;
  inventoryItemName: string;
  quantityToBuy: number;
  isPurchased: boolean;
}

export interface ShoppingListData {
  id: string;
  items: ShoppingListItem[];
  isCompleted: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

@Component({
  selector: 'app-shopping-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './shopping-list.html',
  styleUrl: './shopping-list.css',
})
export class ShoppingList implements OnInit {
  private readonly apiService = inject(ApiService);

  shoppingLists: ShoppingListData[] = [];
  isLoading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.loadShoppingLists();
  }

  loadShoppingLists(): void {
    this.isLoading = true;
    this.error = null;

    console.log('Loading shopping lists from API...');
    this.apiService.getShoppingLists(1, 100).subscribe({
      next: (response) => {
        console.log('Shopping lists API response:', response);
        this.shoppingLists = response.items.map((list) => this.mapApiShoppingListToComponentShoppingList(list));
        console.log('Mapped shopping lists:', this.shoppingLists);
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load shopping lists. Please try again later.';
        this.isLoading = false;
        console.error('Error loading shopping lists:', err);
      }
    });
  }

  private mapApiShoppingListToComponentShoppingList(apiList: ApiShoppingList): ShoppingListData {
    return {
      id: apiList.id,
      isCompleted: apiList.isCompleted,
      createdAt: new Date(apiList.createdAt),
      updatedAt: apiList.updatedAt ? new Date(apiList.updatedAt) : undefined,
      items: apiList.items?.map((item) => this.mapApiItemToComponentItem(item)) || [],
    };
  }

  private mapApiItemToComponentItem(apiItem: ApiShoppingListItem): ShoppingListItem {
    return {
      id: apiItem.id,
      inventoryItemId: apiItem.inventoryItemId,
      inventoryItemName: apiItem.inventoryItem?.name || 'Unknown Item',
      quantityToBuy: apiItem.quantityToBuy,
      isPurchased: apiItem.isPurchased,
    };
  }

  getProgressPercentage(list: ShoppingListData): number {
    if (list.items.length === 0) return 0;
    const purchasedCount = list.items.filter(item => item.isPurchased).length;
    return Math.round((purchasedCount / list.items.length) * 100);
  }

  getPurchasedCount(list: ShoppingListData): number {
    return list.items.filter(item => item.isPurchased).length;
  }

  formatDate(date: Date): string {
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    }).format(new Date(date));
  }

  generateNewShoppingList(): void {
    this.isLoading = true;
    this.error = null;

    this.apiService.generateShoppingList().subscribe({
      next: () => {
        // Reload shopping lists after generating a new one
        this.loadShoppingLists();
      },
      error: (err) => {
        this.error = 'Failed to generate shopping list. Please try again.';
        this.isLoading = false;
        console.error('Error generating shopping list:', err);
      }
    });
  }

}
