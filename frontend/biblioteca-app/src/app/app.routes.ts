import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { AppShellComponent } from './core/layout/app-shell/app-shell.component';
import { LoginPageComponent } from './features/auth/pages/login-page/login-page.component';
import { BooksPageComponent } from './features/books/pages/books-page/books-page.component';
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page/dashboard-page.component';
import { UsersPageComponent } from './features/users/pages/users-page/users-page.component';
import { ReturnsPageComponent } from './features/devoluciones/pages/returns-page/returns-page.component';
import { NewReturnPageComponent } from './features/devoluciones/pages/new-return-page/new-return-page.component';
import { FinesPageComponent } from './features/multas/pages/fines-page/fines-page.component';
import { FineDetailPageComponent } from './features/multas/pages/fine-detail-page/fine-detail-page.component';
import { FineConfigPageComponent } from './features/multas/pages/fine-config-page/fine-config-page.component';

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
      },
      {
        path: 'dashboard/devoluciones',
        component: ReturnsPageComponent,
        title: 'Devoluciones | Biblioteca UDEM'
      },
      {
        path: 'dashboard/devoluciones/nueva',
        component: NewReturnPageComponent,
        title: 'Nueva Devolución | Biblioteca UDEM'
      },
      {
        path: 'dashboard/multas',
        component: FinesPageComponent,
        title: 'Multas | Biblioteca UDEM'
      },
      {
        path: 'dashboard/multas/:id',
        component: FineDetailPageComponent,
        title: 'Detalle Multa | Biblioteca UDEM'
      },
      {
        path: 'dashboard/configuracion/multas',
        component: FineConfigPageComponent,
        title: 'Configuración Multas | Biblioteca UDEM'
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
