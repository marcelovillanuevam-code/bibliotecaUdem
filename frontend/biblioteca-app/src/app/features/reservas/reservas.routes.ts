import { Routes } from '@angular/router';
import { MyReservationsPageComponent } from './pages/my-reservations-page/my-reservations-page.component';

export const RESERVAS_ROUTES: Routes = [
  {
    path: '',
    component: MyReservationsPageComponent,
    title: 'Mis Reservas | Biblioteca UDEM'
  }
];
