import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatExpansionModule } from '@angular/material/expansion';
import { ApiService, QueryHistory } from '../../../core/services/api.service';

@Component({
  selector: 'app-history-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatExpansionModule],
  templateUrl: './history-list.component.html',
  styleUrl: './history-list.component.css'
})
export class HistoryListComponent implements OnInit {
  history: QueryHistory[] = [];

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory() {
    this.apiService.getHistory().subscribe(data => {
      this.history = data.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
    });
  }
}
