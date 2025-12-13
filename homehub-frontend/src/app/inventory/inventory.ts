import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService, InventoryItem as ApiInventoryItem } from '../services/api.service';

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
export class Inventory implements OnInit {
  private readonly apiService = inject(ApiService);

  inventoryItems: InventoryItem[] = [];
  isLoading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.loadInventoryItems();
  }

  private loadInventoryItems(): void {
    this.isLoading = true;
    this.error = null;

    this.apiService.getInventoryItems(1, 100).subscribe({
      next: (response) => {
        this.inventoryItems = response.items.map(this.mapApiItemToComponentItem);
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load inventory items. Please try again later.';
        this.isLoading = false;
        console.error('Error loading inventory items:', err);
      }
    });
  }

  private mapApiItemToComponentItem(apiItem: ApiInventoryItem): InventoryItem {
    return {
      id: apiItem.id,
      name: apiItem.name,
      quantityAvailable: apiItem.quantityAvailable,
      minimumQuantity: apiItem.minimumQuantity,
      notifyOnBelowMinimumQuantity: apiItem.notifyOnBelowMinimumQuantity,
      createdAt: new Date(apiItem.createdAt),
      updatedAt: apiItem.updatedAt ? new Date(apiItem.updatedAt) : undefined,
    };
  }

  isBelowMinimum(item: InventoryItem): boolean {
    return item.quantityAvailable < item.minimumQuantity;
  }

  getStatusClass(item: InventoryItem): string {
    return this.isBelowMinimum(item) ? 'status-low' : 'status-ok';
  }

  getStatusText(item: InventoryItem): string {
    return this.isBelowMinimum(item) ? 'Low Stock' : 'In Stock';
  }

  deleteItem(id: string): void {
    if (!confirm('Are you sure you want to delete this item?')) {
      return;
    }

    this.apiService.deleteInventoryItem(id).subscribe({
      next: () => {
        this.inventoryItems = this.inventoryItems.filter(item => item.id !== id);
      },
      error: (err) => {
        this.error = 'Failed to delete item. Please try again.';
        console.error('Error deleting inventory item:', err);
      }
    });
  }

}
