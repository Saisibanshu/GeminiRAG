import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { HistoryListComponent } from './features/history/history-list/history-list.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: '', component: HomeComponent, canActivate: [authGuard] },
    { path: 'history', component: HistoryListComponent, canActivate: [authGuard] },
    { path: '**', redirectTo: '/login' }
];
