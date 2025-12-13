import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InventoryItem } from './inventory';

export interface InventoryItemFormData {
  name: string;
  quantityAvailable: number;
  minimumQuantity: number;
  notifyOnBelowMinimumQuantity: boolean;
}

@Component({
  selector: 'app-inventory-item-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory-item-modal.html',
  styleUrl: './inventory-item-modal.css',
  standalone: true
})
export class InventoryItemModal implements OnInit, OnChanges {
  @Input() isOpen = false;
  @Input() item: InventoryItem | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<InventoryItemFormData>();

  formData: InventoryItemFormData = {
    name: '',
    quantityAvailable: 0,
    minimumQuantity: 0,
    notifyOnBelowMinimumQuantity: false
  };

  isEditMode = false;
  errors: { [key: string]: string } = {};

  ngOnInit(): void {
    this.initializeForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log('Modal ngOnChanges:', {
      isOpen: this.isOpen,
      item: this.item,
      changes: Object.keys(changes)
    });

    // When modal opens (isOpen changes from false to true), initialize the form
    const isOpening = changes['isOpen']?.currentValue === true &&
      (changes['isOpen']?.previousValue === false || changes['isOpen']?.previousValue === undefined);

    // If item changes, we should also update
    const itemChanged = changes['item'] &&
      changes['item'].currentValue !== changes['item'].previousValue;

    if (isOpening || (itemChanged && this.isOpen)) {
      console.log('Initializing form:', { isOpening, itemChanged, isOpen: this.isOpen, item: this.item });
      this.initializeForm();
    }
  }

  private initializeForm(): void {
    if (!this.isOpen) {
      console.log('Modal not open, skipping initialization');
      return;
    }

    if (this.item) {
      console.log('Initializing form in edit mode with item:', this.item);
      this.isEditMode = true;
      this.formData = {
        name: this.item.name,
        quantityAvailable: this.item.quantityAvailable,
        minimumQuantity: this.item.minimumQuantity,
        notifyOnBelowMinimumQuantity: this.item.notifyOnBelowMinimumQuantity
      };
    } else {
      console.log('Initializing form in add mode');
      this.isEditMode = false;
      this.resetForm();
    }
    this.errors = {};
  }

  resetForm(): void {
    this.formData = {
      name: '',
      quantityAvailable: 0,
      minimumQuantity: 0,
      notifyOnBelowMinimumQuantity: false
    };
    this.errors = {};
  }

  validateForm(): boolean {
    this.errors = {};

    if (!this.formData.name || this.formData.name.trim().length === 0) {
      this.errors['name'] = 'Name is required';
    }

    if (this.formData.quantityAvailable < 0) {
      this.errors['quantityAvailable'] = 'Quantity available cannot be negative';
    }

    if (this.formData.minimumQuantity < 0) {
      this.errors['minimumQuantity'] = 'Minimum quantity cannot be negative';
    }

    return Object.keys(this.errors).length === 0;
  }

  onSave(): void {
    if (!this.validateForm()) {
      return;
    }

    this.save.emit({ ...this.formData });
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
