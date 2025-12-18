import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GeneratedRecipeResponse } from '../services/api.service';

@Component({
  selector: 'app-generated-recipes-modal',
  imports: [CommonModule],
  templateUrl: './generated-recipes-modal.html',
  standalone: true
})
export class GeneratedRecipesModal {
  @Input() isOpen = false;
  @Input() recipes: GeneratedRecipeResponse[] = [];
  @Output() close = new EventEmitter<void>();
  @Output() saveRecipe = new EventEmitter<GeneratedRecipeResponse>();

  onClose(): void {
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.onClose();
    }
  }

  onSaveRecipe(recipe: GeneratedRecipeResponse): void {
    this.saveRecipe.emit(recipe);
  }

  getSortedSteps(steps: GeneratedRecipeResponse['steps']): GeneratedRecipeResponse['steps'] {
    return [...steps].sort((a, b) => a.order - b.order);
  }
}
