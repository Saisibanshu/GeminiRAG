import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface StoreInfo {
  name: string;
  displayName: string;
  createTime?: Date;
  updateTime?: Date;
}

export interface DocumentInfo {
  name: string;
  displayName: string;
  mimeType: string;
  uploadDate?: Date;
  status: string;
}

export interface QueryRequest {
  question: string;
  fileSearchStoreName: string;
}

export interface QueryResponse {
  answer: string;
  citations: string[];
  responseTime: number;
  isFound: boolean;
}

export interface QueryHistory {
  timestamp: Date;
  question: string;
  answer: string;
  citations: string[];
  responseTime: number;
  isFound: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  // private apiUrl = 'http://localhost:5109/api'; // Updated port
  private apiUrl = 'https://localhost:44386/api'; // Updated port for IIS Express

  constructor(private http: HttpClient) { }

  // Stores
  getStores(): Observable<StoreInfo[]> {
    return this.http.get<StoreInfo[]>(`${this.apiUrl}/stores`);
  }

  createStore(displayName: string): Observable<{ name: string }> {
    return this.http.post<{ name: string }>(`${this.apiUrl}/stores`, { displayName });
  }

  deleteStore(storeName: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/stores`, { params: { storeName } });
  }

  // Documents
  getDocuments(storeName: string): Observable<DocumentInfo[]> {
    return this.http.get<DocumentInfo[]>(`${this.apiUrl}/documents`, { params: { storeName } });
  }

  uploadDocument(storeName: string, file: File): Observable<{ operation: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ operation: string }>(`${this.apiUrl}/documents/upload`, formData, { params: { storeName } });
  }

  deleteDocument(fileName: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/documents`, { params: { fileName } });
  }

  deleteStoreWithFiles(storeName: string, force: boolean = false): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/stores`, { params: { storeName, force: force.toString() } });
  }

  // Query
  query(request: QueryRequest): Observable<QueryResponse> {
    return this.http.post<QueryResponse>(`${this.apiUrl}/query`, request);
  }

  // History & Export
  getHistory(): Observable<QueryHistory[]> {
    return this.http.get<QueryHistory[]>(`${this.apiUrl}/export/history`);
  }

  exportJson(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export/json`, { responseType: 'blob' });
  }

  exportMarkdown(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export/markdown`, { responseType: 'blob' });
  }
}
