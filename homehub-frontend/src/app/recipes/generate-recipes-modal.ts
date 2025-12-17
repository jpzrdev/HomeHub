import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, InventoryItem } from '../services/api.service';

export interface GenerateRecipesFormData {
  inventoryItemIds: string[];
  userDescription: string;
}

@Component({
  selector: 'app-generate-recipes-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './generate-recipes-modal.html',
  standalone: true
})
export class GenerateRecipesModal implements OnInit, OnChanges {
  @Input() isOpen = false;
  @Input() isGenerating = false;
  @Output() close = new EventEmitter<void>();
  @Output() generate = new EventEmitter<GenerateRecipesFormData>();

  private readonly apiService = inject(ApiService);

  formData: GenerateRecipesFormData = {
    inventoryItemIds: [],
    userDescription: ''
  };

  inventoryItems: InventoryItem[] = [];
  isLoadingInventory = false;
  errors: { [key: string]: string } = {};

  ngOnInit(): void {
    this.loadInventoryItems();
  }

  ngOnChanges(changes: SimpleChanges): void {
    const isOpening = changes['isOpen']?.currentValue === true &&
      (changes['isOpen']?.previousValue === false || changes['isOpen']?.previousValue === undefined);

    if (isOpening) {
      this.resetForm();
      if (this.inventoryItems.length === 0) {
        this.loadInventoryItems();
      }
    }
  }

  loadInventoryItems(): void {
    this.isLoadingInventory = true;
    this.apiService.getInventoryItems(1, 100).subscribe({
      next: (response) => {
        if (response.items && Array.isArray(response.items)) {
          this.inventoryItems = response.items;
        } else {
          this.inventoryItems = [];
        }
        this.isLoadingInventory = false;
      },
      error: (err) => {
        console.error('Error loading inventory items:', err);
        this.inventoryItems = [];
        this.isLoadingInventory = false;
      }
    });
  }

  resetForm(): void {
    this.formData = {
      inventoryItemIds: [],
      userDescription: ''
    };
    this.errors = {};
  }

  toggleInventoryItem(itemId: string): void {
    const index = this.formData.inventoryItemIds.indexOf(itemId);
    if (index > -1) {
      this.formData.inventoryItemIds.splice(index, 1);
    } else {
      this.formData.inventoryItemIds.push(itemId);
    }
  }

  isItemSelected(itemId: string): boolean {
    return this.formData.inventoryItemIds.includes(itemId);
  }

  validateForm(): boolean {
    this.errors = {};

    if (this.formData.inventoryItemIds.length === 0) {
      this.errors['inventoryItemIds'] = 'Please select at least one inventory item';
    }

    return Object.keys(this.errors).length === 0;
  }

  onGenerate(): void {
    if (!this.validateForm()) {
      return;
    }

    this.generate.emit({ ...this.formData });
  }

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.onClose();
    }
  }
}
