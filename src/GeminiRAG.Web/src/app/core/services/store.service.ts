import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { StoreInfo } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class StoreService {
  private selectedStoreSubject = new BehaviorSubject<StoreInfo | null>(null);
  selectedStore$ = this.selectedStoreSubject.asObservable();

  constructor() { }

  selectStore(store: StoreInfo) {
    this.selectedStoreSubject.next(store);
  }

  clearSelection() {
    this.selectedStoreSubject.next(null);
  }

  getCurrentStore(): StoreInfo | null {
    return this.selectedStoreSubject.value;
  }
}
