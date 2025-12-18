import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-delete-inventory-item-modal',
  imports: [CommonModule],
  templateUrl: './delete-inventory-item-modal.html',
  standalone: true
})
export class DeleteInventoryItemModal {
  @Input() isOpen = false;
  @Input() itemName: string = '';
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<void>();

  onClose(): void {
    this.close.emit();
  }

  onConfirm(): void {
    this.confirm.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (target.classList.contains('modal-backdrop')) {
      this.onClose();
    }
  }
}
