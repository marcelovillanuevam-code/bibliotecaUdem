import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { AppShellComponent } from './core/layout/app-shell/app-shell.component';
import { LoginPageComponent } from './features/auth/pages/login-page/login-page.component';
import { BooksPageComponent } from './features/books/pages/books-page/books-page.component';
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page/dashboard-page.component';
import { UsersPageComponent } from './features/users/pages/users-page/users-page.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    component: LoginPageComponent,
    canActivate: [guestGuard],
    title: 'Login | Biblioteca UDEM'
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardPageComponent,
        title: 'Dashboard | Biblioteca UDEM'
      },
      {
        path: 'dashboard/catalogo',
        component: BooksPageComponent,
        data: { mode: 'catalog' },
        title: 'Buscar Libros | Biblioteca UDEM'
      },
      {
        path: 'dashboard/libros',
        component: BooksPageComponent,
        data: { mode: 'manage' },
        title: 'Gestion Libros | Biblioteca UDEM'
      },
      {
        path: 'dashboard/prestamos',
        loadChildren: () =>
          import('./features/prestamos/prestamos.routes').then((m) => m.PRESTAMOS_ROUTES)
      },
      {
        path: 'dashboard/perfil',
        component: DashboardPageComponent,
        title: 'Mi Perfil | Biblioteca UDEM'
      },
      {
        path: 'usuarios',
        component: UsersPageComponent,
        title: 'Usuarios | Biblioteca UDEM'
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
