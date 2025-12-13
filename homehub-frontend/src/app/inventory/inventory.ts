import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService, InventoryItem as ApiInventoryItem } from '../services/api.service';
import { InventoryItemModal, InventoryItemFormData } from './inventory-item-modal';
import { ToastService } from '../services/toast.service';

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
  imports: [CommonModule, RouterLink, InventoryItemModal],
  templateUrl: './inventory.html',
  styleUrl: './inventory.css',
})
export class Inventory implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly toastService = inject(ToastService);

  inventoryItems: InventoryItem[] = [];
  isLoading = false;
  hasError = false;
  isModalOpen = false;
  selectedItem: InventoryItem | null = null;

  ngOnInit(): void {
    this.loadInventoryItems();
  }

  loadInventoryItems(): void {
    this.isLoading = true;
    this.hasError = false;

    this.apiService.getInventoryItems(1, 100).subscribe({
      next: (response) => {
        console.log('API Response:', response);
        console.log('Response items:', response.items);
        console.log('Is array?', Array.isArray(response.items));

        if (response.items && Array.isArray(response.items)) {
          this.inventoryItems = response.items.map(item => this.mapApiItemToComponentItem(item));
          console.log('Mapped items:', this.inventoryItems);
          console.log('Items count:', this.inventoryItems.length);
        } else {
          console.warn('Response items is not valid:', response.items);
          this.inventoryItems = [];
        }

        this.isLoading = false;
        this.hasError = false;
        console.log('isLoading set to:', this.isLoading);
        console.log('Final inventoryItems:', this.inventoryItems);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading inventory items:', err);
        this.toastService.error('An error occurred while loading inventory items. Please try again later.');
        this.isLoading = false;
        this.hasError = true;
        this.cdr.detectChanges();
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
        this.toastService.error('Failed to delete item. Please try again.');
        console.error('Error deleting inventory item:', err);
      }
    });
  }

  openAddModal(): void {
    this.selectedItem = null;
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  openEditModal(item: InventoryItem): void {
    console.log('Opening edit modal for item:', item);
    // Close modal first if it's open, then set item and open
    if (this.isModalOpen) {
      this.isModalOpen = false;
      this.selectedItem = null;
      this.cdr.detectChanges();
    }

    // Set item and open modal
    this.selectedItem = { ...item };
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.selectedItem = null;
  }

  saveItem(formData: InventoryItemFormData): void {
    if (this.selectedItem) {
      // Update existing item
      console.log('Updating item:', this.selectedItem.id, formData);
      this.apiService.updateInventoryItem(this.selectedItem.id, formData).subscribe({
        next: (updatedItem) => {
          console.log('Update successful:', updatedItem);
          const index = this.inventoryItems.findIndex(item => item.id === this.selectedItem!.id);
          if (index !== -1) {
            this.inventoryItems[index] = this.mapApiItemToComponentItem(updatedItem);
          }
          this.closeModal();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.toastService.error('Failed to update item. Please try again.');
          console.error('Error updating inventory item:', err);
          console.error('Error details:', JSON.stringify(err, null, 2));
        }
      });
    } else {
      // Create new item
      this.apiService.createInventoryItem(formData).subscribe({
        next: () => {
          this.loadInventoryItems();
          this.closeModal();
        },
        error: (err) => {
          this.toastService.error('Failed to create item. Please try again.');
          console.error('Error creating inventory item:', err);
        }
      });
    }
  }

}
