import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { roleGuard } from './core/guards/role.guard';
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
        path: 'dashboard/reservas',
        loadChildren: () =>
          import('./features/reservas/reservas.routes').then((m) => m.RESERVAS_ROUTES)
      },
      {
        path: 'dashboard/libros/:id/reservas',
        loadComponent: () =>
          import('./features/reservas/pages/book-queue-page/book-queue-page.component').then(
            (m) => m.BookQueuePageComponent
          ),
        canActivate: [roleGuard(['LIBRARIAN', 'ADMIN'])],
        title: 'Cola de Reservas | Biblioteca UDEM'
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
      },
      {
        path: 'dashboard/devoluciones',
        loadComponent: () =>
          import('./features/devoluciones/pages/returns-page/returns-page.component').then(
            (m) => m.ReturnsPageComponent
          ),
        title: 'Devoluciones | Biblioteca UDEM'
      },
      {
        path: 'dashboard/devoluciones/nueva',
        loadComponent: () =>
          import('./features/devoluciones/pages/new-return-page/new-return-page.component').then(
            (m) => m.NewReturnPageComponent
          ),
        title: 'Nueva Devolución | Biblioteca UDEM'
      },
      {
        path: 'dashboard/multas',
        loadComponent: () =>
          import('./features/multas/pages/fines-page/fines-page.component').then(
            (m) => m.FinesPageComponent
          ),
        title: 'Multas | Biblioteca UDEM'
      },
      {
        path: 'dashboard/multas/:id',
        loadComponent: () =>
          import('./features/multas/pages/fine-detail-page/fine-detail-page.component').then(
            (m) => m.FineDetailPageComponent
          ),
        title: 'Detalle Multa | Biblioteca UDEM'
      },
      {
        path: 'dashboard/configuracion/multas',
        loadComponent: () =>
          import('./features/multas/pages/fine-config-page/fine-config-page.component').then(
            (m) => m.FineConfigPageComponent
          ),
        title: 'Configuración Multas | Biblioteca UDEM'
      },
      {
        path: 'dashboard/reportes',
        loadComponent: () =>
          import('./features/reportes/pages/reports-page/reports-page.component').then(
            (m) => m.ReportsPageComponent
          ),
        title: 'Reportes | Biblioteca UDEM'
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
