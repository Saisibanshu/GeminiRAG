import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ApiService, QueryHistory } from '../../../core/services/api.service';

@Component({
  selector: 'app-history-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatExpansionModule, MatButtonModule, MatIconModule],
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

  exportJson() {
    this.apiService.exportJson().subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `query-history-${new Date().toISOString().split('T')[0]}.json`;
      link.click();
      URL.revokeObjectURL(url);
    });
  }

  exportMarkdown() {
    this.apiService.exportMarkdown().subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `query-history-${new Date().toISOString().split('T')[0]}.md`;
      link.click();
      URL.revokeObjectURL(url);
    });
  }
}
