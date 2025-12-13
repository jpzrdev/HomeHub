import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

export interface InventoryItem {
  id: string;
  name: string;
  quantityAvailable: number;
  minimumQuantity: number;
  notifyOnBelowMinimumQuantity: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

@Component({
  selector: 'app-inventory',
  imports: [CommonModule, RouterLink],
  templateUrl: './inventory.html',
  styleUrl: './inventory.css',
})
export class Inventory {
  inventoryItems: InventoryItem[] = [
    // Mock data - will be replaced with API call
    {
      id: '1',
      name: 'Milk',
      quantityAvailable: 2.5,
      minimumQuantity: 1.0,
      notifyOnBelowMinimumQuantity: true,
      createdAt: new Date('2024-01-15'),
      updatedAt: new Date('2024-01-20'),
    },
    {
      id: '2',
      name: 'Bread',
      quantityAvailable: 0.5,
      minimumQuantity: 2.0,
      notifyOnBelowMinimumQuantity: true,
      createdAt: new Date('2024-01-10'),
    },
    {
      id: '3',
      name: 'Eggs',
      quantityAvailable: 12.0,
      minimumQuantity: 6.0,
      notifyOnBelowMinimumQuantity: false,
      createdAt: new Date('2024-01-18'),
    },
  ];

  isBelowMinimum(item: InventoryItem): boolean {
    return item.quantityAvailable < item.minimumQuantity;
  }

  getStatusClass(item: InventoryItem): string {
    return this.isBelowMinimum(item) ? 'status-low' : 'status-ok';
  }

  getStatusText(item: InventoryItem): string {
    return this.isBelowMinimum(item) ? 'Low Stock' : 'In Stock';
  }
}
