import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService, QueryResponse } from '../../../core/services/api.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-chat-interface',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './chat-interface.component.html',
  styleUrl: './chat-interface.component.css'
})
export class ChatInterfaceComponent {
  question = '';
  isLoading = false;
  response: QueryResponse | null = null;
  error = '';

  constructor(
    private apiService: ApiService,
    private storeService: StoreService
  ) { }

  askQuestion() {
    if (!this.question.trim()) return;

    const currentStore = this.storeService.getCurrentStore();
    if (!currentStore) {
      this.error = 'Please select a store first.';
      return;
    }

    this.isLoading = true;
    this.error = '';
    this.response = null;

    this.apiService.query({
      question: this.question,
      fileSearchStoreName: currentStore.name
    }).subscribe({
      next: (res) => {
        this.response = res;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to get answer. Please try again.';
        this.isLoading = false;
      }
    });
  }
}
