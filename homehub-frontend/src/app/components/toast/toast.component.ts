import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../services/toast.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.css'
})
export class ToastComponent implements OnInit, OnDestroy {
  private readonly toastService = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();

  toasts: Toast[] = [];

  ngOnInit(): void {
    this.toastService.toasts$
      .pipe(takeUntil(this.destroy$))
      .subscribe(toast => {
        // Defer update to avoid ExpressionChangedAfterItHasBeenCheckedError
        setTimeout(() => {
          this.toasts.push(toast);
          this.autoRemove(toast);
          this.cdr.detectChanges();
        }, 0);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  remove(toast: Toast): void {
    this.toasts = this.toasts.filter(t => t.id !== toast.id);
    this.cdr.detectChanges();
  }

  private autoRemove(toast: Toast): void {
    const duration = toast.duration ?? 5000;
    setTimeout(() => {
      this.remove(toast);
    }, duration);
  }
}