import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';
import { LoanDetailPageComponent } from './pages/loan-detail-page/loan-detail-page.component';
import { LoansPageComponent } from './pages/loans-page/loans-page.component';
import { NewLoanPageComponent } from './pages/new-loan-page/new-loan-page.component';

export const PRESTAMOS_ROUTES: Routes = [
  {
    path: '',
    component: LoansPageComponent,
    title: 'Prestamos | Biblioteca UDEM'
  },
  {
    path: 'nuevo',
    component: NewLoanPageComponent,
    canActivate: [roleGuard(['LIBRARIAN', 'ADMIN'])],
    title: 'Nuevo Prestamo | Biblioteca UDEM'
  },
  {
    path: ':id',
    component: LoanDetailPageComponent,
    title: 'Detalle Prestamo | Biblioteca UDEM'
  }
];
