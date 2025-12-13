import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface InventoryItem {
  id: string;
  name: string;
  quantityAvailable: number;
  minimumQuantity: number;
  notifyOnBelowMinimumQuantity: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ShoppingListItem {
  id: string;
  shoppingListId: string;
  inventoryItemId: string;
  inventoryItem: InventoryItem;
  quantityToBuy: number;
  isPurchased: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ShoppingList {
  id: string;
  items: ShoppingListItem[];
  isCompleted: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateInventoryItemRequest {
  name: string;
  quantityAvailable: number;
  minimumQuantity: number;
  notifyOnBelowMinimumQuantity: boolean;
}

export interface UpdateInventoryItemRequest {
  name?: string;
  quantityAvailable?: number;
  minimumQuantity?: number;
  notifyOnBelowMinimumQuantity?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  // Inventory endpoints
  getInventoryItems(pageNumber: number = 1, pageSize: number = 100): Observable<PaginatedResponse<InventoryItem>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<InventoryItem>>(`${this.apiUrl}/Inventory`, { params });
  }

  getInventoryItemById(id: string): Observable<InventoryItem> {
    return this.http.get<InventoryItem>(`${this.apiUrl}/Inventory/${id}`);
  }

  createInventoryItem(item: CreateInventoryItemRequest): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/Inventory`, item);
  }

  updateInventoryItem(id: string, item: UpdateInventoryItemRequest): Observable<InventoryItem> {
    return this.http.put<InventoryItem>(`${this.apiUrl}/Inventory/${id}`, item);
  }

  deleteInventoryItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Inventory/${id}`);
  }

  // Shopping List endpoints
  getShoppingLists(pageNumber: number = 1, pageSize: number = 100): Observable<PaginatedResponse<ShoppingList>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<ShoppingList>>(`${this.apiUrl}/ShoppingList`, { params });
  }

  getShoppingListById(id: string): Observable<ShoppingList> {
    return this.http.get<ShoppingList>(`${this.apiUrl}/ShoppingList/${id}`);
  }

  generateShoppingList(): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/ShoppingList/generate`, {});
  }
}
