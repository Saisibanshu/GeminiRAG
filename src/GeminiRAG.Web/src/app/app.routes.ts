import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { HistoryListComponent } from './features/history/history-list/history-list.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'history', component: HistoryListComponent },
    { path: '**', redirectTo: '' }
];
