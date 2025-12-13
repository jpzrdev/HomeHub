import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

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
export class ShoppingList {
  shoppingLists: ShoppingListData[] = [
    // Mock data - will be replaced with API call
    {
      id: '1',
      isCompleted: false,
      createdAt: new Date('2024-01-20'),
      items: [
        {
          id: '1-1',
          inventoryItemId: '1',
          inventoryItemName: 'Milk',
          quantityToBuy: 2.0,
          isPurchased: false,
        },
        {
          id: '1-2',
          inventoryItemId: '2',
          inventoryItemName: 'Bread',
          quantityToBuy: 2.0,
          isPurchased: true,
        },
        {
          id: '1-3',
          inventoryItemId: '3',
          inventoryItemName: 'Eggs',
          quantityToBuy: 12.0,
          isPurchased: false,
        },
      ],
    },
    {
      id: '2',
      isCompleted: true,
      createdAt: new Date('2024-01-15'),
      updatedAt: new Date('2024-01-16'),
      items: [
        {
          id: '2-1',
          inventoryItemId: '4',
          inventoryItemName: 'Bananas',
          quantityToBuy: 1.5,
          isPurchased: true,
        },
        {
          id: '2-2',
          inventoryItemId: '5',
          inventoryItemName: 'Apples',
          quantityToBuy: 2.0,
          isPurchased: true,
        },
      ],
    },
    {
      id: '3',
      isCompleted: false,
      createdAt: new Date('2024-01-22'),
      items: [
        {
          id: '3-1',
          inventoryItemId: '6',
          inventoryItemName: 'Cheese',
          quantityToBuy: 1.0,
          isPurchased: false,
        },
      ],
    },
  ];

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
}
